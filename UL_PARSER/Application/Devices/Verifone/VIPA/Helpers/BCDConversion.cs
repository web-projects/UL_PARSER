using System;
using System.Buffers;
using System.Text.RegularExpressions;

namespace UL_PARSER.Devices.Verifone.VIPA.Helpers
{
    public static class BCDConversion
    {
        private static readonly byte nineKey = 0x27;
        private static readonly byte zeroKey = 0x1e;

        public static byte IntToBCDByte(int intValue)
        {
            byte bcdValue = (byte)((intValue / 10) * 16);
            bcdValue += (byte)(intValue % 10);

            return bcdValue;
        }

        public static byte[] LongToBCD(long numericValue, int byteSize = 6)
        {
            byte[] bcd = ArrayPool<byte>.Shared.Rent(byteSize);

            Array.Clear(bcd, 0, bcd.Length);

            for (int index = 0; index < (byteSize << 1); index++)
            {
                uint hexpart = (uint)(numericValue % 10);
                bcd[index / 2] |= (byte)(hexpart << ((index % 2) * 4));
                numericValue /= 10;
            }

            return bcd;
        }

        public static byte[] IntToBCD(int numericValue, int byteSize = 6)
        {
            byte[] bcd = ArrayPool<byte>.Shared.Rent(byteSize);

            Array.Clear(bcd, 0, bcd.Length);

            for (int index = 0; index < (byteSize << 1); index++)
            {
                uint hexpart = (uint)(numericValue % 10);
                bcd[index / 2] |= (byte)(hexpart << ((index % 2) * 4));
                numericValue /= 10;
            }

            return bcd;
        }

        public static byte[] StringToBCD(string bcdString, bool isLittleEndian = false)
        {
            if (string.IsNullOrEmpty(bcdString))
            {
                return null;
            }

            byte[] bytes = null;

            // Check that the string is made up of sets of two numbers (e.g. "01", "3456" or "AA")
            if (Regex.IsMatch(bcdString, "^(([0-9]{2})|([A]{2}))+$"))
            {
                char[] chars = bcdString.ToCharArray();
                int len = chars.Length / 2;

                bytes = new byte[len];

                if (isLittleEndian)
                {
                    for (int i = 0; i < len; i++)
                    {
                        byte highNibble = 0x00;
                        byte lowNibble = 0x00;

                        if (Char.IsNumber(chars[2 * i]))
                        {
                            highNibble = byte.Parse(chars[2 * (len - 1) - 2 * i].ToString());
                            lowNibble = byte.Parse(chars[2 * (len - 1) - 2 * i + 1].ToString());
                        }
                        else
                        {
                            highNibble = (byte)((byte)Char.ToUpper(chars[2 * (len - 1) - 2 * i]) - 0x37);
                            lowNibble = (byte)((byte)Char.ToUpper(chars[2 * (len - 1) - 2 * i + 1]) - 0x37);
                        }

                        bytes[i] = (byte)((byte)(highNibble << 4) | lowNibble);
                    }
                }
                else
                {
                    for (int i = 0; i < len; i++)
                    {
                        byte highNibble = 0x00;
                        byte lowNibble = 0x00;

                        if (Char.IsNumber(chars[2 * i]))
                        {
                            highNibble = byte.Parse(chars[2 * i].ToString());
                            lowNibble = byte.Parse(chars[2 * i + 1].ToString());
                        }
                        else
                        {
                            highNibble = (byte)((byte)Char.ToUpper(chars[2 * i]) - 0x37);
                            lowNibble = (byte)((byte)Char.ToUpper(chars[2 * i + 1]) - 0x37);
                        }

                        bytes[i] = (byte)((byte)(highNibble << 4) | lowNibble);
                    }
                }
            }

            return bytes;
        }

        public static int BCDToInt(byte[] bcd)
        {
            int result = 0;

            foreach (byte index in bcd)
            {
                result *= 0x100;
                result += (0x10 * (index >> 4) + (index & 0x0F));
            }

            return result;
        }

        public static string StringFromByteData(byte[] byteData)
        {
            if ((byteData?.Length ?? 0) == 0)
                return string.Empty;

            string[] byteStrings = new string[byteData.Length];

            for (int i = 0; i < byteData.Length; i++)
            {
                byteStrings[i] = KeyStringFromByte(byteData[i]);
            }

            return string.Join(",", byteStrings);
        }

        public static string KeyStringFromByte(byte byteValue)
        {
            if (zeroKey <= byteValue && byteValue <= nineKey)
                return $"KEY_{(int)(byteValue - zeroKey)}";
            return byteValue switch
            {
                (byte)0x8C => "KEY_INFO",
                (byte)0x8B => "KEY_HASH",
                (byte)0x8A => "KEY_STAR",
                (byte)0x88 => "KEY_DOWN",
                (byte)0x86 => "KEY_UP",
                (byte)0x1B => "KEY_STOP",
                (byte)0x0D => "KEY_OK",
                (byte)0x08 => "KEY_CORR",
                _ => "KEY_Unknown"
            };
        }
    }

}
