using System;
using System.Text;

namespace RadarSystem.Communication.Utilities
{
    /// <summary>
    /// 字节处理工具类 - 对应 Java 的 ByteUtil
    /// </summary>
    public static class ByteUtil
    {
        /// <summary>
        /// 字节数组转十六进制字符串
        /// </summary>
        public static string Bytes2Str(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                sb.AppendFormat("{0:X2}", b);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 十六进制字符串转字节数组
        /// </summary>
        public static byte[] HexString2Bytes(string hexString)
        {
            if (string.IsNullOrEmpty(hexString))
                return Array.Empty<byte>();

            hexString = hexString.Replace(" ", "");
            if (hexString.Length % 2 != 0)
                throw new ArgumentException("Hex string must have even length");

            byte[] bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return bytes;
        }

        /// <summary>
        /// 从字节数组中提取整数（大端序）
        /// </summary>
        public static int ToInt(byte[] data, int startIndex, int endIndex)
        {
            if (data == null || startIndex < 0 || endIndex >= data.Length || startIndex > endIndex)
                return 0;

            int result = 0;
            for (int i = startIndex; i <= endIndex; i++)
            {
                result = (result << 8) | (data[i] & 0xFF);
            }
            return result;
        }

        /// <summary>
        /// 整数转十六进制字符串
        /// </summary>
        public static string IntToHexString(int value, int byteCount)
        {
            return value.ToString($"X{byteCount * 2}");
        }

        /// <summary>
        /// 整数转小端序十六进制字符串
        /// </summary>
        public static string IntToLittleEndianHex(int value, int byteCount)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (bytes.Length > byteCount)
            {
                Array.Resize(ref bytes, byteCount);
            }
            return Bytes2Str(bytes);
        }

        /// <summary>
        /// 字符串转整数
        /// </summary>
        public static int StringToInt(string hexString)
        {
            if (string.IsNullOrEmpty(hexString))
                return 0;

            return Convert.ToInt32(hexString, 16);
        }

        /// <summary>
        /// 浮点数转十六进制字符串
        /// </summary>
        public static string Float2HexString(float value, int byteCount)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (bytes.Length > byteCount)
            {
                Array.Resize(ref bytes, byteCount);
            }
            return Bytes2Str(bytes);
        }

        /// <summary>
        /// 十六进制字符串转浮点数
        /// </summary>
        public static float HexString2Float(string hexString)
        {
            if (string.IsNullOrEmpty(hexString))
                return 0f;

            byte[] bytes = HexString2Bytes(hexString);
            if (bytes.Length < 4)
            {
                Array.Resize(ref bytes, 4);
            }
            return BitConverter.ToSingle(bytes, 0);
        }

        /// <summary>
        /// 字符串转十六进制字符串
        /// </summary>
        public static string String2HexString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            byte[] bytes = Encoding.UTF8.GetBytes(str);
            return Bytes2Str(bytes);
        }

        /// <summary>
        /// 十六进制字符串转字符串
        /// </summary>
        public static string HexString2String(string hexString)
        {
            if (string.IsNullOrEmpty(hexString))
                return string.Empty;

            byte[] bytes = HexString2Bytes(hexString);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// 反向填充字符串
        /// </summary>
        public static string FillReverse(string str, int totalLength, char fillChar)
        {
            if (str.Length >= totalLength)
                return str;

            return str.PadRight(totalLength, fillChar);
        }

        /// <summary>
        /// 反转字符串（每两个字符为一组）
        /// </summary>
        public static string ReverseString(string hexString)
        {
            if (string.IsNullOrEmpty(hexString) || hexString.Length % 2 != 0)
                return hexString;

            StringBuilder sb = new StringBuilder(hexString.Length);
            for (int i = hexString.Length - 2; i >= 0; i -= 2)
            {
                sb.Append(hexString.Substring(i, 2));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 提取子字节数组
        /// </summary>
        public static byte[] SubBytes(byte[] source, int startIndex, int length)
        {
            if (source == null || startIndex < 0 || startIndex + length > source.Length)
                return Array.Empty<byte>();

            byte[] result = new byte[length];
            Array.Copy(source, startIndex, result, 0, length);
            return result;
        }

        /// <summary>
        /// 计算 MD5 哈希
        /// </summary>
        public static string CalculateMD5(byte[] data)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hash = md5.ComputeHash(data);
                return Bytes2Str(hash);
            }
        }
    }
}

