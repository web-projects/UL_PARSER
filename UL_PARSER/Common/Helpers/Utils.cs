using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Helpers
{
    public enum InitialCrcValue { ZEROS = 0x0000, NONZERO1 = 0xffff, NONZERO2 = 0x1D0F }

    public static class Utils
    {
        public const int DeviceHealthKeyValueLength = 29;
        public const char DeviceHealthKeyValuePaddingCharacter = '_';
        public const int DeviceLogKeyValueLength = 43;
        public const char DeviceLogKeyValuePaddingCharacter = '_';

        public static IEnumerable<byte[]> Split(this byte[] value, int bufferLength)
        {
            int countOfArray = value.Length / bufferLength;

            if (value.Length % bufferLength > 0)
            {
                countOfArray++;
            }

            for (int i = 0; i < countOfArray; i++)
            {
                yield return value.Skip(i * bufferLength).Take(bufferLength).ToArray();
            }
        }

        public static IEnumerable<(int, T)> Enumerate<T>(this IEnumerable<T> input, int start = 0)
        {
            int i = start;
            foreach (var t in input)
            {
                yield return (i++, t);
            }
        }

        public static string GetUTCTimeStampToMinutes()
        {
            DateTime value = DateTime.UtcNow;
            return value.ToString("MMddyyyyHHmm");
        }

        public static string GetTimeStampToMinutes()
        {
            DateTime value = DateTime.Now;
            return value.ToString("MMddyyyyHHmm");
        }

        public static string GetTimeStampToSeconds()
        {
            DateTime value = DateTime.Now;
            return value.ToString("yyyyMMddHHmmss");
        }

        public static string GetTimeStamp()
        {
            DateTime value = DateTime.Now;
            return value.ToString("yyyyMMdd-HH:mm:ss.fff");
        }


        public static string FormatStringAsRequired(string input, int length = DeviceHealthKeyValueLength, char filler = DeviceHealthKeyValuePaddingCharacter)
        {
            return input.PadRight(length, filler);
        }

        private static string AssembleString(bool format, string valueMissing, params string[] items)
        {
            string result = "";
            foreach (string item in items)
            {
                result += $"{item ?? valueMissing}";
            }
            return format ? FormatStringAsRequired(result) : result;
        }
    }

    public class Crc16Ccitt
    {
        const ushort poly = 4129;
        ushort[] table = new ushort[256];
        ushort initialValue = 0;

        public Crc16Ccitt(InitialCrcValue initialValue)
        {
            this.initialValue = (ushort)initialValue;
            ushort temp, a;
            for (int i = 0; i < table.Length; ++i)
            {
                temp = 0;
                a = (ushort)(i << 8);
                for (int j = 0; j < 8; ++j)
                {
                    if (((temp ^ a) & 0x8000) != 0)
                    {
                        temp = (ushort)((temp << 1) ^ poly);
                    }
                    else
                    {
                        temp <<= 1;
                    }
                    a <<= 1;
                }
                table[i] = temp;
            }
        }

        public ushort ComputeChecksum(byte[] bytes)
        {
            ushort crc = this.initialValue;
            for (int i = 0; i < bytes.Length; ++i)
            {
                crc = (ushort)((crc << 8) ^ table[((crc >> 8) ^ (0xff & bytes[i]))]);
            }
            return crc;
        }

        public byte[] ComputeChecksumBytes(byte[] bytes)
        {
            ushort crc = ComputeChecksum(bytes);
            return BitConverter.GetBytes(crc);
        }
    }
}