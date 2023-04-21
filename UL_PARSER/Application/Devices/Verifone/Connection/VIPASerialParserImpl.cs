using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using UL_PARSER.Devices.Common;
using UL_PARSER.Devices.Verifone.VIPA;
using UL_PARSER.Devices.Verifone.VIPA.TagLengthValue;

namespace UL_PARSER.Devices.Verifone.Connection
{
    internal class VIPASerialParserImpl : IVIPASerialParser, IDisposable
    {
        #region --- Attributes ---

        private const int headerProtoLen = 4;   //  NAD, PCB, LEN and LRC
        private const int maxPacketLen = 254;
        private const int maxPacketProtoLen = maxPacketLen + headerProtoLen;

        private enum ReadErrorLevel
        {
            None,
            Length,
            Invalid_NAD,
            Invalid_PCB,
            Invalid_CombinedBytes,
            Missing_LRC,
            CombinedBytes_MisMatch
        }

        private DeviceLogHandler deviceLogHandler;

        private readonly ArrayPool<byte> arrayPool;
        private readonly List<Tuple<int, byte[]>> addedComponentBytes;
        private readonly object combinedResponseBytesLock = new object();

        private static readonly List<byte> validNADValues = new List<byte> { 0x01, 0x02, 0x11 };
        private static readonly List<byte> validPCBValues = new List<byte> { 0x00, 0x01, 0x02, 0x03, 0x40, 0x41, 0x42, 0x43 };
        private static readonly List<uint> nestedTagTags = new List<uint> { 0xEE, 0xEF, 0xF0, 0xE0, 0xE4, 0xE7, 0xFF7C, 0xFF7F };

        private byte[] combinedResponseBytes;
        private int combinedResponseLength;

        private readonly bool trackErrors;
        private readonly string comPort;
        private ReadErrorLevel readErrorLevel = ReadErrorLevel.None;

        private static ConcurrentDictionary<string, int> numReadErrors;

        #endregion ---Attributes ---

        public VIPASerialParserImpl(DeviceLogHandler deviceLogHandler, string comPort)
        {
            this.deviceLogHandler = deviceLogHandler;

            arrayPool = ArrayPool<byte>.Create();
            addedComponentBytes = new List<Tuple<int, byte[]>>();
            combinedResponseBytes = null;
            numReadErrors ??= new ConcurrentDictionary<string, int>();

            this.comPort = comPort;
            trackErrors = !string.IsNullOrWhiteSpace(comPort);
            if (trackErrors && !numReadErrors.ContainsKey(comPort))
            {
                numReadErrors[comPort] = 0;
            }
        }

        public void BytesRead(byte[] chunk, int chunkLength = 0)
        {
            if (chunk is null)
            {
                throw new ArgumentException(nameof(chunk));
            }
            if (chunkLength > chunk.Length)
            {
                throw new ArgumentException(nameof(chunkLength));
            }
            if (chunk.Length == 0)
            {
                return;
            }
            if (chunkLength <= 0)
            {
                chunkLength = chunk.Length;
            }

            lock (combinedResponseBytesLock)
            {
                if (combinedResponseLength + chunkLength > (combinedResponseBytes?.Length ?? 0))        //Expand current buffer to accomodate larger chunks
                {
                    byte[] tempArray = arrayPool.Rent(combinedResponseLength + chunkLength);

                    if (combinedResponseBytes is { })
                    {
                        Buffer.BlockCopy(combinedResponseBytes, 0, tempArray, 0, combinedResponseLength);
                        arrayPool.Return(combinedResponseBytes);
                    }

                    combinedResponseBytes = tempArray;
                }
            }
            Buffer.BlockCopy(chunk, 0, combinedResponseBytes, combinedResponseLength, chunkLength);
            combinedResponseLength += chunkLength;
        }

        private bool CheckForResponseErrors(ref bool addedResponseComponent, ref int consumedResponseBytesLength, ref int responseCode, bool isChainedMessageResponse)
        {
            bool isChainedCommand = (combinedResponseBytes[1] & 0x01) == 0x01;

            // Validate NAD, PCB, and LEN values
            if (combinedResponseLength < headerProtoLen)
            {
                readErrorLevel = ReadErrorLevel.Length;
                return true;
            }

            if (!isChainedMessageResponse)
            {
                if (!validNADValues.Contains(combinedResponseBytes[0]))
                {
                    readErrorLevel = ReadErrorLevel.Invalid_NAD;
                    return true;
                }
                else if (!validPCBValues.Contains(combinedResponseBytes[1]))
                {
                    readErrorLevel = ReadErrorLevel.Invalid_PCB;
                    return true;
                }
                else if (combinedResponseBytes[2] > (combinedResponseLength - headerProtoLen) && !isChainedCommand)  // command is not chained
                {
                    deviceLogHandler?.Invoke(Types.LogLevel.Info,
                        string.Format("VIPA-RRCBADD [{0}]: EXPECTED-LEN=0x{1:X2} - CALCULATED-LEN=0x{2:X2}", comPort, combinedResponseBytes[2], (combinedResponseLength - headerProtoLen)));
                    readErrorLevel = ReadErrorLevel.Invalid_CombinedBytes;
                    return true;
                }
            }

            int maxPacketLen = isChainedCommand ? combinedResponseBytes.Length - 1 : combinedResponseBytes[2] + 3;

            if (!isChainedMessageResponse)
            {
                // Validate LRC
                byte lrc = CalculateLRCFromByteArray(combinedResponseBytes);

                // offset from length to LRC is 3
                if (!isChainedCommand && combinedResponseBytes[maxPacketLen] != lrc)
                {
                    deviceLogHandler?.Invoke(Types.LogLevel.Error,
                        string.Format("VIPA: message LRC error - Expected=[0x{0:X2}] != Calculated=[0x{1:X2}]", combinedResponseBytes[maxPacketLen], lrc));
                    readErrorLevel = ReadErrorLevel.Missing_LRC;
                    return true;
                }
            }

            if (isChainedMessageResponse || isChainedCommand)  // Command is chained (VIPA section 2.4)
            {
                // reassemble chained message response
                if (isChainedMessageResponse)
                {
                    if (ProcessChainedMessageResponse())
                    {
                        return true;
                    }
                }
                else
                {
                    int componentBytesLength = (int)combinedResponseBytes[2];
                    byte[] componentBytes = arrayPool.Rent(componentBytesLength);

                    try
                    {
                        // set maximum component copy length
                        componentBytesLength = Math.Min(componentBytesLength, combinedResponseBytes.Length - 0x03);

                        // copy component bytes: skip NAD PCB LEN from Frame 
                        Buffer.BlockCopy(combinedResponseBytes, 0x03, componentBytes, 0, componentBytesLength);

                        addedComponentBytes.Add(new Tuple<int, byte[]>(componentBytesLength, componentBytes));
                        consumedResponseBytesLength = componentBytesLength + headerProtoLen;

                        Debug.WriteLineIf(SerialConnection.LogSerialBytes, $"VIPA-RRCBADD [{comPort}]: {BitConverter.ToString(componentBytes, 0, componentBytesLength)}");
                        deviceLogHandler?.Invoke(Types.LogLevel.Info, $"VIPA-RRCBADD [{comPort}]: {BitConverter.ToString(componentBytes, 0, componentBytesLength)}");
                    }
                    catch (Exception ex)
                    {
                        deviceLogHandler?.Invoke(Types.LogLevel.Error, $"Error processing chained-message response({ex})");
                        PoolReturnIfNotNull(arrayPool, componentBytes);
                        return !isChainedMessageResponse;
                    }
                }

                // 1st packet      : NAD PCB(bit 0 set) LEN CLA INS P1 P2 Lc Data… LRC
                // 2nd – nth packet: NAD PCB(bit 0 set) LEN Data… LRC
                // Last packet     : NAD PCB(bit 0 unset) LEN Data… LRC
                readErrorLevel = ReadErrorLevel.CombinedBytes_MisMatch;
                addedResponseComponent = true;

                if (isChainedMessageResponse)
                {
                    int componentBytesLength = CalculateByteArrayLength(combinedResponseBytes, combinedResponseLength - 1);
                    int sw1Offset = componentBytesLength - 2;
                    responseCode = (combinedResponseBytes[sw1Offset] << 8) + combinedResponseBytes[sw1Offset + 1];
                    readErrorLevel = ReadErrorLevel.None;
                }

                return !isChainedMessageResponse;
            }
            else
            {
                int sw1Offset = combinedResponseBytes[2] + 1;  // Offset to SW1 is forward 3, back 2 (back 1 for SW2)
                responseCode = (combinedResponseBytes[sw1Offset] << 8) + combinedResponseBytes[sw1Offset + 1];
                readErrorLevel = ReadErrorLevel.None;
            }

            return false;
        }

        /// <summary>
        /// Length Expected (Le) byte
        /// For specific commands, there is an Le byte (the “expected” length of data to be returned).
        /// This would mean that the following packet structure could occur:
        /// 
        /// [NAD, PCB, LEN]
        /// [CLA, INS, P1, P2, Lc]
        /// [Data, Le]
        /// [LRC]
        /// 
        /// </summary>
        /// <param name="responseTagsHandler"></param>
        /// <param name="responseTaglessHandler"></param>
        /// <param name="responseContactlessHandler"></param>
        /// <param name="isChainedMessageResponse"></param>
        public void ReadAndExecute(VIPAImpl.ResponseTagsHandlerDelegate responseTagsHandler, VIPAImpl.ResponseTaglessHandlerDelegate responseTaglessHandler, VIPAImpl.ResponseCLessHandlerDelegate responseContactlessHandler, bool isChainedMessageResponse = false)
        {
            bool addedResponseComponent = true;

            lock (combinedResponseBytesLock)
            {
                while (addedResponseComponent && combinedResponseLength > 0 && combinedResponseBytes != null)
                {
                    int consumedResponseBytesLength = 0;
                    int responseCode = 0;
                    addedResponseComponent = false;

                    // Check for errors or extra responses.
                    bool errorFound = CheckForResponseErrors(ref addedResponseComponent, ref consumedResponseBytesLength, ref responseCode, isChainedMessageResponse);

                    if (!errorFound)
                    {
                        int totalDecodeSize = combinedResponseBytes[2] - 2;        // Use LEN of final response packet

                        foreach (Tuple<int, byte[]> component in addedComponentBytes)
                        {
                            totalDecodeSize += component.Item1;
                        }

                        byte[] totalDecodeBytes = arrayPool.Rent(totalDecodeSize);
                        Array.Clear(totalDecodeBytes, 0, totalDecodeBytes.Length);

                        int totalDecodeOffset = 0;

                        foreach (Tuple<int, byte[]> component in addedComponentBytes)
                        {
                            Buffer.BlockCopy(component.Item2, 0, totalDecodeBytes, totalDecodeOffset, component.Item1);
                            totalDecodeOffset += component.Item1;
                            arrayPool.Return(component.Item2);
                        }

                        if (isChainedMessageResponse)
                        {
                            // Skip final response header and use LEN of final response
                            if (totalDecodeBytes[0] == 0x01 && totalDecodeBytes[2] == 0xFE)
                            {
                                int count = totalDecodeOffset - 3;  // NAD + PCB + LEN
                                Buffer.BlockCopy(totalDecodeBytes, 3, totalDecodeBytes, 0, count);
                                // Remove trailing bytes
                                for (int i = count; i < totalDecodeOffset; i++)
                                {
                                    totalDecodeBytes[i] = 0x00;
                                }
                                totalDecodeSize = totalDecodeOffset - 3;    // NAD + PCB + LEN
                            }
                            totalDecodeSize -= 2;                       // SW1 + SW2
                            totalDecodeSize = CalculateByteArrayLength(totalDecodeBytes, totalDecodeSize - 1);
                            consumedResponseBytesLength = combinedResponseLength = totalDecodeSize;
                            // Reset response code from the newly updated final block
                            if (responseCode != (int)VipaSW1SW2Codes.Success)
                            {
                                responseCode = (totalDecodeBytes[totalDecodeSize] << 8) + totalDecodeBytes[totalDecodeSize + 1];
                            }
                        }
                        else
                        {
                            // Skip final response header and use LEN of final response (not including the SW1, SW2, and LRC bytes)
                            Buffer.BlockCopy(combinedResponseBytes, 3, totalDecodeBytes, totalDecodeOffset, combinedResponseBytes[2] - 2);
                        }

                        addedComponentBytes.Clear();

                        if (responseTagsHandler != null || responseContactlessHandler != null)
                        {
                            List<TLV> tags = null;

                            if (responseCode == (int)VipaSW1SW2Codes.Success)
                            {
                                tags = TLV.Decode(totalDecodeBytes, 0, totalDecodeSize, nestedTagTags.ToArray());
                                Debug.WriteLineIf(SerialConnection.LogSerialBytes, $"VIPA-DECODED [{comPort}]: {BitConverter.ToString(totalDecodeBytes, 0, isChainedMessageResponse ? totalDecodeSize + 1 : totalDecodeSize)}");
                            }

                            if (responseTagsHandler != null)
                            {
                                responseTagsHandler.Invoke(tags, responseCode);
                            }
                            else if (responseContactlessHandler != null)
                            {
                                responseContactlessHandler.Invoke(tags, responseCode, combinedResponseBytes[1]);
                            }
                        }
                        else if (responseTaglessHandler != null)
                        {
                            Debug.WriteLineIf(SerialConnection.LogSerialBytes, $"VIPA-TAGLESS DECODED [{comPort}]: {(responseCode == (int)VipaSW1SW2Codes.Success ? string.Empty : "NOTSUCCESS")} {BitConverter.ToString(totalDecodeBytes, 0, totalDecodeBytes.Length)}");
                            responseTaglessHandler.Invoke(totalDecodeBytes, totalDecodeSize, responseCode);
                        }
                        arrayPool.Return(totalDecodeBytes, false);

                        consumedResponseBytesLength = combinedResponseBytes[2] + headerProtoLen;

                        addedResponseComponent = (combinedResponseLength - consumedResponseBytesLength) > 0;
                    }
                    else if (readErrorLevel != ReadErrorLevel.CombinedBytes_MisMatch)
                    {
                        // allows for debugging of VIPA read issues
                        Debug.WriteLineIf(SerialConnection.LogSerialBytes, $"VIPA-READ [{comPort}]: ERROR LEVEL: '{readErrorLevel}'");
                        if (combinedResponseBytes is null || combinedResponseLength == 0)
                        {
                            deviceLogHandler?.Invoke(Types.LogLevel.Error, $"Error reading vipa-byte stream({readErrorLevel}): 0 || <null>");
                        }
                        else
                        {
                            deviceLogHandler?.Invoke(Types.LogLevel.Error, $"Error reading vipa-byte stream({readErrorLevel}): " + BitConverter.ToString(combinedResponseBytes, 0, combinedResponseLength));
                        }

                        // When the device is in a indeterminate state due to a parsing error, we must notify the requestor of the error to quickly release the response event handler. 
                        // Thus helping with faster device recovery (i.e. don't wait for the CancellationToken to expire).
                        responseTagsHandler?.Invoke(null, (int)VipaSW1SW2Codes.Failure);
                    }

                    if (consumedResponseBytesLength >= combinedResponseLength || isChainedMessageResponse)
                    {
                        // All bytes consumed.  Leave a null array for later
                        if (combinedResponseBytes is { })
                        {
                            arrayPool.Return(combinedResponseBytes, false);
                            combinedResponseBytes = null;
                            combinedResponseLength = 0;
                        }
                    }
                    else if (consumedResponseBytesLength > 0)
                    {
                        // Remove consumed bytes and leave remaining bytes for later consumption
                        int updatedLength = combinedResponseLength - consumedResponseBytesLength;
                        byte[] tempArray = arrayPool.Rent(updatedLength);
                        Buffer.BlockCopy(combinedResponseBytes, consumedResponseBytesLength, tempArray, 0, updatedLength);
                        arrayPool.Return(combinedResponseBytes, false);
                        combinedResponseBytes = tempArray;
                        combinedResponseLength = updatedLength;
                    }
                }
            }
        }

        public bool SanityCheck()
        {
            bool sane = true;

            if (combinedResponseLength > 0 || combinedResponseBytes is { })
            {
                sane = false;
                if (combinedResponseBytes is { })
                {
                    //deviceLogHandler?.Invoke(Types.LogLevel.Warn, $"VIPA-PARSE[{comPort}]: SanityCheckFailed-(LEN={combinedResponseLength})-{BitConverter.ToString(combinedResponseBytes, 0, combinedResponseBytes.Length)}");
                    arrayPool.Return(combinedResponseBytes);
                    combinedResponseBytes = null;
                }
                else
                {
                    //deviceLogHandler?.Invoke(Types.LogLevel.Warn, $"VIPA-PARSE[{comPort}]: SanityCheckFailed-(LEN={combinedResponseLength})");
                    combinedResponseLength = 0;
                }
            }

            // chained command answer: component bytes should be assembled into a single packet
            if (addedComponentBytes.Count > 0)
            {
                sane = false;
                deviceLogHandler?.Invoke(Types.LogLevel.Warn, $"VIPA-PARSE[{comPort}]: SanityCheckFailedComponentCheck-{addedComponentBytes.Count}");
                foreach (Tuple<int, byte[]> component in addedComponentBytes)
                {
                    deviceLogHandler?.Invoke(Types.LogLevel.Error, $"VIPA-PARSE[{comPort}]: SanityCheckFailed-StoredComponent-{BitConverter.ToString(component.Item2, 0, component.Item1)}");
                    arrayPool.Return(component.Item2);
                }
                addedComponentBytes.Clear();
            }
            if (ReadErrorLevel.None != readErrorLevel)
            {
                sane = false;
                //deviceLogHandler?.Invoke(Types.LogLevel.Error, $"VIPA-PARSE[{comPort}]: SanityCheckFailedStateCheck-{readErrorLevel}");
            }
            return sane;
        }

        public void Dispose()
        {
            if (combinedResponseBytes is { })
            {
                arrayPool.Return(combinedResponseBytes);
            }
        }

        private void PoolReturnIfNotNull(ArrayPool<byte> pool, byte[] buffer)
        {
            if (buffer != null)
            {
                pool.Return(buffer);
            }
        }

        private bool ProcessChainedMessageResponse()
        {
            ArrayPool<byte> workerPool = ArrayPool<byte>.Create();

            byte[] workerBuffer = null;
            byte[] componentBytes = null;

            try
            {
                // obtain proper length from payload
                int messageLength = CalculateByteArrayLength(combinedResponseBytes, combinedResponseLength - 1);

                componentBytes = arrayPool.Rent(messageLength);

                int offset = 0;
                int frame = 1;

                // Chained message response packet could be less than the expected multi-message response size
                int packetLenWithProto = (int)combinedResponseBytes[2] + headerProtoLen;
                int packetProtoLen = (packetLenWithProto < maxPacketProtoLen) ? packetLenWithProto : maxPacketProtoLen;

                // VIPA Specification: the maximum possible LEN byte value is 0xFE (254 bytes) + headerProtoLen
                // 0xFE + 0x04 = 0x102 (dec: 258)
                // 1st – nth packet: NAD PCB(bit 0 set)   LEN Data… LRC
                // Last packet     : NAD PCB(bit 0 unset) LEN Data… SW1 - SW2 LRC
                for (int i = 0; i < messageLength; i += packetProtoLen)
                {
                    // copy MAX of 258 block sizes (NAD+PCB+LEN+LRC+FE_BYTES_DATA_MAX) or LESS depending on last packet with PCB = 0
                    int blockCopyLength = CalculateByteArrayLength(combinedResponseBytes, i + maxPacketProtoLen - 1) - i + 1;
                    workerBuffer = workerPool.Rent(blockCopyLength);
                    Buffer.BlockCopy(combinedResponseBytes, i, workerBuffer, 0, blockCopyLength);

                    // assume the buffer length is correct: NAD+PCB+LEN+LRC
                    int workerBufferLen = workerBuffer[2] + headerProtoLen;

                    // last message in chained response PCB bit is set to 0: total length = LEN + SW1 + SW2 + LRC
                    byte lrc = CalculateLRCFromByteArray(workerBuffer, (workerBuffer[1] == 0x00 ? workerBuffer[2] + 0x03 : 0));
                    workerBufferLen = (workerBuffer[1] == 0x00) ? workerBuffer[2] + 0x03 : workerBufferLen - 1;

                    if (workerBuffer[workerBufferLen] != lrc)
                    {
                        Debug.WriteLineIf(SerialConnection.LogSerialBytes, $"VIPA-RRCBADD [{comPort}]|FRAME#{frame++}: {BitConverter.ToString(workerBuffer, 0, workerBufferLen)}");
                        workerPool.Return(workerBuffer);
                        deviceLogHandler?.Invoke(Types.LogLevel.Error,
                           string.Format("VIPA: message LRC error - Current=[0x{0:X2}] != Calculated=[0x{1:X2}]", workerBuffer[workerBufferLen], lrc));
                        readErrorLevel = ReadErrorLevel.Missing_LRC;
                        return true;
                    }

                    // remove LRC
                    int workerBufferReportLen = workerBufferLen + 1;

                    // last packet?
                    if (workerBuffer[1] == 0x00 && (i + workerBufferLen >= messageLength))
                    {
                        workerBufferReportLen--;
                        Buffer.BlockCopy(workerBuffer, 0, componentBytes, offset, workerBufferLen);
                    }
                    else
                    {
                        workerBufferLen -= i > 0 ? 3 : 0;
                        Buffer.BlockCopy(workerBuffer, ((i > 0) ? 3 : 0), componentBytes, offset, workerBufferLen);
                    }

                    offset += workerBufferLen;

                    //byte[] workerReportBuffer = AddByteToArray(workerBuffer, lrc);
                    deviceLogHandler?.Invoke(Types.LogLevel.Info, $"VIPA-RRCBADD [{comPort}]|FRAME#{frame}: {BitConverter.ToString(workerBuffer, 0, workerBufferReportLen)}");
                    Debug.WriteLineIf(SerialConnection.LogSerialBytes, $"VIPA-RRCBADD [{comPort}]|FRAME#{frame++}: {BitConverter.ToString(workerBuffer, 0, workerBufferReportLen)}");

                    workerPool.Return(workerBuffer);
                }

                int componentBytesLength = offset;
                addedComponentBytes.Add(new Tuple<int, byte[]>(componentBytesLength, componentBytes));
                Debug.WriteLineIf(SerialConnection.LogSerialBytes, $"VIPA-COMMAND: {BitConverter.ToString(addedComponentBytes[0].Item2, 0, addedComponentBytes[0].Item1)}");
                deviceLogHandler?.Invoke(Types.LogLevel.Info, $"VIPA-COMMAND: {BitConverter.ToString(addedComponentBytes[0].Item2, 0, addedComponentBytes[0].Item1)}");
            }
            catch (Exception ex)
            {
                deviceLogHandler?.Invoke(Types.LogLevel.Error, $"Error processing chained-message response({ex.Message})");
                PoolReturnIfNotNull(workerPool, workerBuffer);
                PoolReturnIfNotNull(arrayPool, componentBytes);
            }

            return false;
        }

        //private byte[] AddByteToArray(byte[] _input_bArray, byte _newByte, Boolean _add_to_start_of_array = false)
        //{
        //    byte[] newArray;
        //    if (_add_to_start_of_array)
        //    {
        //        newArray = new byte[_input_bArray.Length + 1];
        //        _input_bArray.CopyTo(newArray, 1);
        //        newArray[0] = _newByte;
        //    }
        //    else
        //    {
        //        newArray = new byte[_input_bArray.Length + 1];
        //        _input_bArray.CopyTo(newArray, 0);
        //        newArray[_input_bArray.Length] = _newByte;
        //    }
        //    return newArray;
        //}

        /// <summary>
        /// This method provides the length of array of bytes to find the location of the LRC byte.
        /// Because the LRC byte can be 0, we need to ensure that at least two bytes past the candidate LRC byte
        /// are indeed non-zero byte values.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="startPosition"></param>
        /// <returns></returns>
        private int CalculateByteArrayLength(byte[] array, int startPosition)
        {
            // array length returns the size of the array instead of the length of its contents
            int length = startPosition;

            try
            {
                // check for possible buffer overrun on last copied block
                for (length = Math.Min(startPosition, array.Length - 1); length > 0; length--)
                {
                    // LRC Byte can be 0x00
                    //if (array[length] != 0x00 || array[length - 1] != 0x00)
                    if (array[length] != 0x00 || (array[length - 1] != 0x00 && array[length - 2] != 0x00))
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                deviceLogHandler?.Invoke(Types.LogLevel.Error, $"Error calculating array length - '{ex}'");
            }

            return length;
        }

        // For specific commands, there is an Le byte (the “expected” length of data to be returned).
        // This would mean that the following packet structure could occur:
        // [NAD, PCB, LEN]
        // [CLA, INS, P1, P2, Lc]
        // [Data, Le]
        // [LRC]
        private byte CalculateLRCFromByteArray(byte[] array, int packetOffset = 0)
        {
            // Packet length (LEN) byte
            // The LEN byte is the length of the packet.It includes the CLA, INS, P1, P2 bytes(but not for subsequent
            // packets in Chained commands), includes the Lc and data field (if present) bytes, and includes the Le
            // byte(if present), includes the SW1 - SW2 bytes for responses, but excludes the LRC byte.

            // VIPA Specification: the maximum possible LEN byte value is 0xFE (254 bytes)
            int maxPacketLen = Math.Min((packetOffset > 0 ? packetOffset : (array[2] + 3)), array.Length - 1);
            byte lrc = 0x00;

            for (int index = 0; index < maxPacketLen; index++)
            {
                lrc ^= array[index];
                //Debug.Write(string.Format("0x{0:X2}, ", lrc));
            }

            //Debug.WriteLine("0xFF");
            Debug.WriteLine(string.Format("LRC BYTE: [0x{0:X2}]", lrc));

            return lrc;
        }
    }
}
