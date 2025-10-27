using System;
using System.Buffers.Binary;
using System.Linq;

namespace RadarSystem.Core.Utilities
{
    /// <summary>
    /// 字节转换工具类 - 完整实现Java ByteUtil所有功能
    /// </summary>
    public static class ByteConverter
    {
        /// <summary>
        /// 将字节数组转换为32位整数（小端序）
        /// </summary>
        public static int ToInt32LittleEndian(byte[] bytes, int startIndex, int endIndex)
        {
            if (bytes == null || bytes.Length < endIndex + 1)
                throw new ArgumentException("字节数组长度不足");

            var buffer = new byte[4];
            buffer[0] = bytes[startIndex];
            buffer[1] = bytes[startIndex + 1];
            buffer[2] = bytes[startIndex + 2];
            buffer[3] = bytes[endIndex];

            return BitConverter.IsLittleEndian
                ? BitConverter.ToInt32(buffer, 0)
                : BinaryPrimitives.ReadInt32LittleEndian(buffer);
        }

        /// <summary>
        /// 将字节数组转换为16位整数（小端序）
        /// </summary>
        public static short ToInt16LittleEndian(byte[] bytes, int startIndex, int endIndex)
        {
            if (bytes == null || bytes.Length < endIndex + 1)
                throw new ArgumentException("字节数组长度不足");

            var buffer = new byte[2];
            buffer[0] = bytes[startIndex];
            buffer[1] = bytes[endIndex];

            return BitConverter.IsLittleEndian
                ? BitConverter.ToInt16(buffer, 0)
                : BinaryPrimitives.ReadInt16LittleEndian(buffer);
        }

        /// <summary>
        /// 将字节数组转换为单精度浮点数（小端序）
        /// </summary>
        public static float ToSingleLittleEndian(byte[] bytes, int startIndex, int endIndex)
        {
            if (bytes == null || bytes.Length < endIndex + 1)
                throw new ArgumentException("字节数组长度不足");

            var buffer = new byte[4];
            buffer[0] = bytes[startIndex];
            buffer[1] = bytes[startIndex + 1];
            buffer[2] = bytes[startIndex + 2];
            buffer[3] = bytes[endIndex];

            return BitConverter.IsLittleEndian
                ? BitConverter.ToSingle(buffer, 0)
                : BinaryPrimitives.ReadSingleLittleEndian(buffer);
        }

        /// <summary>
        /// 将字节数组转换为双精度浮点数（小端序）
        /// </summary>
        public static double ToDoubleLittleEndian(byte[] bytes, int startIndex, int endIndex)
        {
            if (bytes == null || bytes.Length < endIndex + 1)
                throw new ArgumentException("字节数组长度不足");

            var buffer = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                buffer[i] = bytes[startIndex + i];
            }

            return BitConverter.IsLittleEndian
                ? BitConverter.ToDouble(buffer, 0)
                : BinaryPrimitives.ReadDoubleLittleEndian(buffer);
        }

        /// <summary>
        /// 将字节数组转换为半精度浮点数（16位）
        /// </summary>
        public static float ToHalfFloat(byte[] bytes, int startIndex, int endIndex)
        {
            if (bytes == null || bytes.Length < endIndex + 1)
                throw new ArgumentException("字节数组长度不足");

            var buffer = new byte[] { bytes[startIndex], bytes[endIndex] };
            var halfBits = BitConverter.ToInt16(buffer, 0);
            
            return HalfToFloat(halfBits);
        }

        /// <summary>
        /// 32位浮点转16位浮点字节数组
        /// </summary>
        public static byte[] Float32ToFloat16Bytes(float value)
        {
            if (value == 0.0f || Math.Abs(value) < 0.001f)
                return new byte[] { 0, 0 };

            var floatBits = BitConverter.SingleToInt32Bits(value);
            var halfBits = FloatToHalf(floatBits);
            
            return BitConverter.GetBytes((short)halfBits);
        }

        /// <summary>
        /// 半精度转全精度浮点（IEEE 754标准）
        /// </summary>
        private static float HalfToFloat(int halfBits)
        {
            int mant = halfBits & 0x03FF;
            int exp = halfBits & 0x7C00;

            if (exp == 0x7C00) // 无穷大或NaN
            {
                exp = 0x3FC00;
            }
            else if (exp != 0)
            {
                exp += 0x1C000;
                if (mant == 0 && exp > 0x1C400)
                {
                    return BitConverter.Int32BitsToSingle((halfBits & 0x8000) << 16 | exp << 13 | 0x3FF);
                }
            }
            else if (mant != 0)
            {
                exp = 0x1C400;
                do
                {
                    mant <<= 1;
                    exp -= 0x400;
                } while ((mant & 0x400) == 0);

                mant &= 0x3FF;
            }

            return BitConverter.Int32BitsToSingle((halfBits & 0x8000) << 16 | (exp | mant) << 13);
        }

        /// <summary>
        /// 全精度转半精度浮点
        /// </summary>
        private static int FloatToHalf(int floatBits)
        {
            return ((floatBits >> 16) & 0x8000) |
                   (((floatBits >> 23) - 127 + 15) & 0x1F) << 10 |
                   ((floatBits >> 13) & 0x3FF);
        }

        /// <summary>
        /// 整数转字节数组（小端序）
        /// </summary>
        public static byte[] Int32ToBytes(int value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        /// <summary>
        /// 单精度浮点转字节数组（小端序）
        /// </summary>
        public static byte[] SingleToBytes(float value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        /// <summary>
        /// 双精度浮点转字节数组（小端序）
        /// </summary>
        public static byte[] DoubleToBytes(double value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        /// <summary>
        /// 字节数组转十六进制字符串
        /// </summary>
        public static string ToHexString(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;

            return BitConverter.ToString(bytes).Replace("-", "");
        }

        /// <summary>
        /// 十六进制字符串转字节数组
        /// </summary>
        public static byte[] FromHexString(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return Array.Empty<byte>();

            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        /// <summary>
        /// 字符串转十六进制字符串
        /// </summary>
        public static string StringToHexString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            var bytes = System.Text.Encoding.UTF8.GetBytes(str);
            return ToHexString(bytes);
        }

        /// <summary>
        /// 十六进制字符串转字符串
        /// </summary>
        public static string HexStringToString(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return string.Empty;

            var bytes = FromHexString(hex);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// 合并字节数组
        /// </summary>
        public static byte[] Concat(params byte[][] arrays)
        {
            var totalLength = arrays.Sum(a => a?.Length ?? 0);
            var result = new byte[totalLength];
            var offset = 0;

            foreach (var array in arrays)
            {
                if (array != null && array.Length > 0)
                {
                    Buffer.BlockCopy(array, 0, result, offset, array.Length);
                    offset += array.Length;
                }
            }

            return result;
        }

        /// <summary>
        /// 大端序字节数组转短整数 - 对应Java byteToShortBig
        /// </summary>
        public static short ByteToShortBig(byte[] bytes, int start, int end)
        {
            if (bytes == null || bytes.Length < end + 1)
                throw new ArgumentException("字节数组长度不足");

            return (short)((bytes[start] << 8) | (bytes[start + 1] & 0xFF));
        }

        /// <summary>
        /// 十六进制字符串转整数 - 对应Java stringToInt
        /// </summary>
        public static int StringToInt(string hexString)
        {
            var bytes = FromHexString(hexString);
            if (bytes.Length == 2)
            {
                return ToInt16LittleEndian(bytes, 0, 1);
            }
            return ToInt32LittleEndian(bytes, 0, bytes.Length - 1);
        }

        /// <summary>
        /// 整数转指定长度字节数组 - 对应Java intToBytes
        /// </summary>
        public static byte[] IntToBytes(int value, int length)
        {
            var bytes = new byte[length];
            for (int i = 0; i < length; i++)
            {
                bytes[length - i - 1] = (byte)((value >> (8 * i)) & 0xFF);
            }
            return bytes;
        }

        /// <summary>
        /// 生成指定长度的零填充十六进制字符串 - 对应Java generateAssignLenHexString
        /// </summary>
        public static string GenerateZeroHexString(int length)
        {
            return new string('0', length * 2);
        }

        /// <summary>
        /// 浮点数转十六进制字符串 - 对应Java float2HexString
        /// </summary>
        public static string FloatToHexString(float value, int length)
        {
            var bytes = SingleToBytes(value);
            var hexString = ToHexString(bytes);
            return hexString.PadLeft(length, '0');
        }

        /// <summary>
        /// 字符转字节 - 对应Java char2Byte
        /// </summary>
        public static byte CharToByte(char character)
        {
            return (byte)character;
        }

        /// <summary>
        /// 整数转十六进制字符串 - 对应Java intToHexString
        /// </summary>
        public static string IntToHexString(int value, int length)
        {
            var hexString = value.ToString("X");
            return hexString.PadLeft(length * 2, '0');
        }

        /// <summary>
        /// 整数转小端序十六进制字符串 - 对应Java intToLittleEndianHex
        /// </summary>
        public static string IntToLittleEndianHex(int value, int byteLength)
        {
            byte[] bytes;
            switch (byteLength)
            {
                case 4:
                    bytes = Int32ToBytes(value);
                    break;
                case 2:
                    bytes = BitConverter.GetBytes((short)value);
                    if (!BitConverter.IsLittleEndian)
                        Array.Reverse(bytes);
                    break;
                default:
                    throw new ArgumentException("不支持的字节长度");
            }

            return ToHexString(bytes);
        }

        /// <summary>
        /// 十六进制字符串转十进制 - 对应Java hexToDecimal
        /// </summary>
        public static int HexToDecimal(string hexString)
        {
            return Convert.ToInt32(hexString, 16);
        }

        /// <summary>
        /// 十六进制字符串求和 - 对应Java hexStringAdd
        /// </summary>
        public static string HexStringAdd(string hexString)
        {
            long sum = 0;
            for (int i = 0; i < hexString.Length; i += 2)
            {
                var hex = hexString.Substring(i, 2);
                sum += Convert.ToInt64(hex, 16);
            }

            var result = sum.ToString("X");
            return result.Length == 1 ? "0" + result : result;
        }

        /// <summary>
        /// 两个十六进制字符串异或 - 对应Java xor
        /// </summary>
        public static string Xor(string hex1, string hex2)
        {
            var int1 = Convert.ToInt32(hex1, 16);
            var int2 = Convert.ToInt32(hex2, 16);
            var result = int1 ^ int2;
            return result.ToString("X").PadLeft(2, '0');
        }

        /// <summary>
        /// 字节数组转小写十六进制字符串 - 对应Java receiveHexToString
        /// </summary>
        public static string ReceiveHexToString(byte[] bytes)
        {
            try
            {
                return ToHexString(bytes).ToLower();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 左侧填充字符 - 对应Java fill
        /// </summary>
        public static string Fill(string input, int size, char symbol)
        {
            return input.PadLeft(size, symbol);
        }

        /// <summary>
        /// 右侧填充字符 - 对应Java fillReverse
        /// </summary>
        public static string FillReverse(string input, int size, char symbol)
        {
            return input.PadRight(size, symbol);
        }

        /// <summary>
        /// 十六进制字符串转浮点数 - 对应Java hexString2Float
        /// </summary>
        public static float HexStringToFloat(string hexString)
        {
            var bytes = FromHexString(hexString);
            return ToSingleLittleEndian(bytes, 0, 3);
        }

        /// <summary>
        /// 字节转位数组 - 对应Java byte2Int
        /// </summary>
        public static int[] ByteToIntArray(byte value)
        {
            var result = new int[8];
            for (int i = 7; i >= 0; i--)
            {
                result[i] = (value >> i) & 0x1;
            }
            return result;
        }
    }
}
