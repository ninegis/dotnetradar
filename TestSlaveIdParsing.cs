// 测试SlaveId解析（验证与Java一致性）
using System;

// 模拟数据包：5A5A + 14000000 + ...
// SlaveId字段：14000000（小端）= 0x00000014 = 20

string hexString = "5A5A140000000302...";

// 提取SlaveId（8个字符，4字节）
string slaveIdHex = hexString.Substring(4, 8);
Console.WriteLine($"SlaveId Hex: {slaveIdHex}");

// 方式1：大端解析（错误）
int bigEndian = Convert.ToInt32(slaveIdHex, 16);
Console.WriteLine($"Big Endian: {bigEndian}"); // 335544320

// 方式2：小端解析（正确，与Java一致）
byte[] slaveIdBytes = new byte[4];
for (int i = 0; i < 4; i++)
{
    slaveIdBytes[i] = Convert.ToByte(slaveIdHex.Substring(i * 2, 2), 16);
}
int littleEndian = BitConverter.ToInt32(slaveIdBytes, 0);
Console.WriteLine($"Little Endian: {littleEndian}"); // 20 ✅

Console.WriteLine($"\n结论：FactoryId = {littleEndian}");

