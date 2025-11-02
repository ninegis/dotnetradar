# Java圆弧雷达设备ID判断逻辑分析

## 核心代码（RadarServerHandler.java 第120-131行）

```java
private void handleData(byte[] msgBytes, ChannelHandlerContext ctx) {
    // 1. 字节数组转十六进制字符串
    String hexString = ByteUtil.bytes2Str(msgBytes).toUpperCase();
    
    // 2. 提取命令（位置12-16，4个字符）
    String mimoCommand = hexString.substring(12, 16);
    
    // 3. 提取SlaveId（位置4-12，8个字符）并转为int
    String slaveId = String.valueOf(ByteUtil.stringToInt(hexString.substring(4, 12)));
    
    // 4. 在映射表中查找DeviceId
    String deviceId = "";
    if (deviceIdMap.containsKey(slaveId)) {
        deviceId = deviceIdMap.get(slaveId);  // ← 找到映射
    } else {
        deviceInit();  // ← 重新加载设备列表
        deviceId = deviceIdMap.get(slaveId);
    }
}
```

## ByteUtil.stringToInt() 解析逻辑

### 步骤1: 十六进制字符串转字节数组
```java
// hexString2Bytes("14000000")
// 输入: "14000000" (8个字符)
// 输出: [0x14, 0x00, 0x00, 0x00] (4字节)

byte[] bytes = hexString2Bytes(hexString);
```

### 步骤2: 字节数组转int（LITTLE_ENDIAN）
```java
public static int toInt(byte[] byt, int a, int b) {
    int c = b - a + 1;  // 4
    byte[] bytes = new byte[c];
    bytes[0] = byt[a];      // byt[0] = 0x14
    bytes[1] = byt[a + 1];  // byt[1] = 0x00
    bytes[2] = byt[a + 2];  // byt[2] = 0x00
    bytes[3] = byt[b];      // byt[3] = 0x00
    
    // LITTLE_ENDIAN: 低字节在前
    // [0x14, 0x00, 0x00, 0x00] = 20
    int i = ByteBuffer.wrap(bytes).order(ByteOrder.LITTLE_ENDIAN).getInt();
    return i;  // 返回 20
}
```

## 数据包格式分析

### 示例：FactoryId=20的设备发送心跳

```
数据包（十六进制字符串）:
5A5A 14000000 0000 ...
│    │        │
│    │        └─ Command (位置12-16): "0000"
│    └─ SlaveId (位置4-12): "14000000"
│       解析: 0x14000000（小端）= 20
└─ Header: "5A5A"

字节数组形式:
[5A] [5A] [14] [00] [00] [00] [00] [00] ...
 0    1    2    3    4    5    6    7
 
位置: 0  1   2   3   4   5   6   7
```

## deviceIdMap 映射表

### 初始化（deviceInit方法，第71-89行）

```java
public void deviceInit(){
    deviceIdMap.clear();
    
    // 1. 从API获取项目列表
    String response = restTemplate.getForObject(
        "http://localhost:"+apiPort+"/api/project/list", String.class);
    
    // 2. 解析JSON
    JSONObject jsonObject = JSONObject.parseObject(response)
        .getJSONArray("data")
        .getJSONObject(0);
    
    projectId = jsonObject.getString("id");
    
    // 3. 遍历设备
    JSONArray jsonArray = jsonObject.getJSONArray("devices");
    for (int i = 0; i < jsonArray.size(); i++) {
        if (Objects.equals(
            jsonArray.getJSONObject(i).getString("type"), "ER")) {
            
            // 4. 建立映射：params.slaveId → devices[i].id
            String slaveId = jsonArray.getJSONObject(i)
                .getJSONObject("params")
                .getString("slaveId");  // ← 从params对象获取
            
            String deviceId = jsonArray.getJSONObject(i)
                .getString("id");
            
            deviceIdMap.put(slaveId, deviceId);
        }
    }
}
```

## 关键发现

### 1. API数据结构
```json
{
  "data": [{
    "id": "PROJECT001",
    "devices": [{
      "id": "RADAR_001",        // ← DeviceId
      "type": "ER",             // ← 设备类型（ER=圆弧雷达）
      "params": {
        "slaveId": "20"         // ← SlaveId（十进制字符串）
      }
    }]
  }]
}
```

### 2. 映射关系
```
deviceIdMap.put("20", "RADAR_001");
```
- **Key**: "20"（十进制字符串）
- **Value**: "RADAR_001"（设备ID）

## .NET代码修复要点

### 问题1: SlaveId解析错误

**原因**: 当前解析出933705501等大数字，说明：
1. 字节读取位置错误
2. 或字节序仍然错误
3. 或数据包本身格式不对

**需要验证实际数据包格式**！

### 问题2: API结构不匹配

Java从 `/api/project/list` 获取嵌套结构：
```
project.devices[].params.slaveId
```

.NET从 `/api/Device` 获取扁平结构：
```
devices[].factoryId
```

**两种结构不同！**

## 建议修复

### 方案1: 使用FactoryId直接映射（当前实现）
```csharp
// SQLite数据库中：
DeviceId = "RADAR_001"
FactoryId = "20"

// 直接映射
DeviceInfoCache.AddDevice("RADAR_001", "20", ...)
```

### 方案2: 验证数据包格式
需要查看实际接收的数据包十六进制内容，确认：
- 是否以5A5A开头
- SlaveId是否在位置4-12
- 字节序是否正确

## 下一步

1. 查看实际接收数据的HEX输出
2. 验证是否以5A5A开头
3. 手动解析SlaveId确认字节序
4. 如果数据格式正确，修复解析代码

