import axios from "axios";
import {getUUID} from "@/utils/radartool.js";

export class ApiRadar {
    // ===== 本地C#后端配置 =====
    static apiUrl = (typeof window !== 'undefined' && window.localrelease && window.localrelease.url)
        || ((typeof window !== 'undefined' && window.location && window.location.origin) ? window.location.origin : '');
    static customApiUrl = (typeof window !== 'undefined' && window.localrelease && (window.localrelease.customApiUrl || window.localrelease.url))
        || ((typeof window !== 'undefined' && window.location && window.location.origin) ? window.location.origin : '');
    static kotiotApiUrl = (typeof window !== 'undefined' && window.localrelease && (window.localrelease.kotiotApiUrl || window.localrelease.url))
        || ((typeof window !== 'undefined' && window.location && window.location.origin) ? window.location.origin : '');
    static radarApiUrl = (typeof window !== 'undefined' && window.localrelease && (window.localrelease.radarApiUrl || window.localrelease.url))
        || ((typeof window !== 'undefined' && window.location && window.location.origin) ? window.location.origin : '');
    
    // ===== 以下为远程配置（已停用） =====
    // static customApiUrl = 'http://8.140.201.145:6086';
    // static kotiotApiUrl = 'http://218.4.141.234:25559';
    // static radarApiUrl = "http://218.4.141.234:25599";
    static getRadarData() {
        return new Promise(resolve => {
            axios.get(this.apiUrl + '/api/Project').then(res => {
                resolve(res);
            })
        })
    }
    
    // ✅ 新增：根据项目ID获取设备列表（包含雷达参数）
    static getDevicesByProjectId(projectId) {
        return new Promise(resolve => {
            axios.get(this.apiUrl + '/api/Device', {
                params: { projectId: projectId }
            }).then(res => {
                resolve(res);
            })
        })
    }
    
    // ✅ 新增：获取算法参数
    static getAlgorithmParam(projectId, deviceId) {
        return new Promise(resolve => {
            axios.get(this.apiUrl + '/api/protocol/query/algorithm/' + projectId + '/' + deviceId).then(res => {
                resolve(res);
            })
        })
    }
    
    // ✅ 新增：获取色条配置
    static getColorBar(projectId, mode) {
        return new Promise(resolve => {
            axios.get(this.apiUrl + '/api/protocol/colorBar/' + projectId + '/' + mode).then(res => {
                resolve(res);
            })
        })
    }
    static addMonitoringLocation(data,enableShieldArea){
        return new Promise(resolve => {
            axios.post(this.apiUrl+'/api/protocol/add/geo',{
                "id":data.id,//位置ID
                "projectId":data.projectId,//项目ID
                "alarmLevel":0,//0、1、2、3、4. 0为正常，1蓝色预警，2黄色预警，3橙色预警，4红色预警
                "visible":true,//是否可见
                "name":data.name,//监测名称
                "type":data.type,//GEO-POINT监测点 GEO-AREA监测区
                "devices":[data.deviceId],//设备
                "coordinates":data.coordinate,
                "defoComputingMethod":0,//形变计算方法。0为最大值，1为平均值 仅用于区域
                "enableData":!enableShieldArea,//是否分析数据 仅用于区域
                "enableAlarmArea":false,//是否用于兴趣区域 仅用于区域
                "enableSlope":false,//是否开启滑坡角度计算 仅用于区域
                "enableShieldArea":enableShieldArea,
                "slopeValue":2.0,//滑坡夹角(度)  仅用于区域
                "weightValue":50.0,//加权百分比，用于平均值  仅用于区域
                "direction":1//远离雷达(负数) 0， 靠近雷达(正数) 1    仅用于区域
                }
            ).then(data=>{
                resolve(data);
            })
        })
    }
    static deleteMonitor(id,projectid){
        return new Promise(resolve => {
            axios.get(this.apiUrl+'/api/protocol/remove/geo/'+id+'/'+projectid+'').then(data=>{
                resolve(data);
            })
        })
    }
    static controlRadar(projectId,deviceId,command,userName){
        return new Promise(resolve => {
            axios.get(this.apiUrl+'/api/'+(deviceId.substring(0,8)==='MIMOLITE'?'mimoLite':'arcsar')+'/command/'+projectId+'/'+deviceId+'/'+command+'/'+userName).then(data=>{
                resolve(data);
            })
        })
    }
    static getDiskStorage(){
        return new Promise(resolve => {
            axios.get(this.apiUrl+'/api/datastorage/query/discSpace').then(res=>{
                resolve(res);
            })
        })
    }
    static getDiskThreshold(){
        return new Promise(resolve => {
            axios.get(this.apiUrl+'/api/config/info').then(res=>{
                resolve(res);
            })
        })
    }
    static getAlarmRule(projectId){
        return new Promise(resolve => {
            axios.get(this.apiUrl+'/api/protocol/query/ruleBatch/'+projectId).then(res=>resolve(res));
        })
    }
    static addCameraParams(projectId,lon,lat,alt,heading,pitch,roll){
        return new Promise(resolve => {
            axios.post(this.apiUrl+'/api/protocol/set/project/view',{
                projectId,lon,lat,alt,heading,pitch,roll
            }).then(res=>resolve(res));
        })
    }
    static updateProjectInfo(projectId,name,description,contact,phone,email){
        return new Promise(resolve => {
            axios.post(this.apiUrl+'/api/custom/updateProjectInfo',{
                projectId,name,description,contact,phone,email
            }).then(res=>resolve(res))
                .catch(ex=>{resolve(ex)})
        })
    }
    static updateImageAnalysisConfig(projectId,imageDiffAnalysisConfig,imageAnalysisConfig){
        return new Promise((resolve,reject) => {
            axios.post(this.apiUrl+'/api/protocol/update/project/imageAnalysisConfig',{
                projectId,
                genImageType:imageAnalysisConfig['genImageType'],
                defoInterval:imageAnalysisConfig['followDefoInterval'],
                scatInterval:imageAnalysisConfig['scatInterval'],
                defoNumber:imageAnalysisConfig['followDefoNumber'],
                scatNumber:imageAnalysisConfig['scatNumber']
            }).then(res=>resolve(res))
                .catch(error=>reject(error));
        })
    }
    static updateRadarParams(params){
        return new Promise(resolve => {
            axios.post(this.apiUrl+'/api/protocol/update/radar/param',params).then(res=>resolve(res));
        })
    }
    static updatePushiRadarParams(params){
        return new Promise(resolve => {
            axios.post(this.apiUrl+'/api/protocol/update/radar/mimolite/param',params).then(res=>resolve(res));
        })
    }
    static queryAlarmRecordCount=(params)=>{
        return new Promise(resolve => {
            axios.post(this.apiUrl+'/api/alarmNotify/recordList/count',params).then(res=>resolve(res))
        })
    }
    static queryAlarmRecord=(params)=>{
        return new Promise(resolve => {
            axios.post(this.apiUrl+'/api/alarmNotify/recordList/count',params).then(res=>resolve(res))
        })
    }
    static queryImageCount=(projectId,deviceId,startDateTime,endDateTime,type,status)=>{
        return new Promise(resolve => {
            axios.post(this.apiUrl+'/api/sar/image/count',{
                "projectId": projectId,
                "devId": deviceId,
                "startDateTime": startDateTime,
                "endDateTime": endDateTime,
                "status":status,
                "type":type,
                "pageRowSize":5
            }).then(res=>{
                resolve(res);
            })
        })
    }
    static getImageResource=(url,filename)=>{
        return new Promise(resolve => {
            axios.get(this.apiUrl+url+filename).then(res=>{
                resolve(res);
            })
        })
    }
    static generateRadarImage=(deviceId,duration,fileName,projectId,sequence,status,timeUnit,ts,type)=>{
        return new Promise(resolve => {
            axios.post(this.apiUrl+'/api/sar/generate/image',{
                "deviceId": deviceId,
                "duration": duration,
                "fileName": fileName,
                "projectId": projectId,
                "sequence": sequence,
                "status": status,
                "timeUnit": timeUnit,
                "ts": ts,
                "type": type
            }).then(res=>{
                resolve(res);
            })
        })
    }
    static queryImageList=(projectId,deviceId,startDateTime,endDateTime,type,status,count)=>{
        return new Promise(resolve => {
            axios.post(this.apiUrl+'/api/sar/image/list',{
                "projectId": projectId,
                "devId": deviceId,
                "startDateTime": startDateTime,
                "endDateTime": endDateTime,
                "status":status,
                "type":type,
                "pageRowSize":count,
                "page":1
            }).then(res=>{
                resolve(res);
            })
        })
    }
    static updatePushiRadarAlgorithmParam(params){
        return new Promise((resolve,reject) => {
            axios.post(this.apiUrl+'/api/protocol/update/radar/mimolite/algoparam',params).then(res=>resolve(res))
                .catch(error=>{reject(error)});
        })
    }
    static updateRadarAlgorithmParam(params){
        return new Promise((resolve,reject) => {
            axios.post(this.apiUrl+'/api/protocol/update/radar/algoparam',params).then(res=>resolve(res))
                .catch(error=>{reject(error)});
        })
    }
    static updateSpeedTarget(projectId,timeUnit){
        return new Promise(resolve => {
            axios.post(this.apiUrl+'/api/protocol/update/speed/target',{
                projectId,timeUnit
            }).then(res=>resolve(res));
        })
    }
    static updateColorBar(params){
        return new Promise(resolve => {
            axios.post(this.apiUrl+'/api/protocol/update/colorBar',params).then(res=>{resolve(res)});
        })
    }
    static updateDangerArea(params){
        return new Promise(resolve => {
            axios.post(this.apiUrl+'/api/protocol/update/hidden/analysis',params).then(res=>resolve(res));
        })
    }
    static addAlarmRule(data){
        return new Promise(resolve => {
            axios.post(this.apiUrl+'/api/protocol/add/ruleBatch',{
                    "projectId":data.projectId,//项目ID
                    "id": getUUID(),//预警ID
                    "ruleName": data.name,//预警名
                    "ruleDescription": data.describe,//预警描述
                    "alarmRule": data.operator,//预警规则,现仅支持“>”,“<”,“>=”,“<=”
                    "enable": data.enable,//启用
                    "devices": data.deviceId,//设备ID
                    "geoMarkArray":data.geoMarkArray,//位置ID
                    "dataSource":"10",//数据来源,10连续形变,00原始形变。现仅有连续形变
                    "targetFlag":data.targetFlag,//数据值是否为绝对值
                    "alarmTargetThresholds":[       //多个指标的阈值
                        {
                            "name": "蓝色预警",
                            "level": 1, //预警等级   排列顺序请按现有顺序排列请勿错乱,即蓝色预警、黄色预警、橙色预警、红色预警
                            "flag": true,//启用。现仅支持启用，请勿修改
                            "targetCheckbox": [
                                {
                                    "label": "位移",//指标名称
                                    "value": data.displaceblue,//数值,若flag为true,请填写数值
                                    "flag": data.displacement,//是否启用，请与其他预警等级中的targetCheckbox的flag保持一致
                                    "timeUnit": "",//时间单位,位移无时间单位
                                    "target": "displacement" //指标名
                                },
                                {
                                    "label": "速度",
                                    "value": data.speedblue,//数值,若flag为true,请填写数值
                                    "flag": data.speed,//是否启用，请与其他预警等级中的targetCheckbox的flag保持一致
                                    "timeUnit": data.speedtimeunit,//时间单位,30分钟02,1小时03,1天04,1周05,1月06
                                    "target": "speed"//指标名
                                },
                                {
                                    "label": "加速度",
                                    "value": data.accelerateblue,//数值,若flag为true,请填写数值
                                    "flag": data.acceleration,//是否启用，请与其他预警等级中的targetCheckbox的flag保持一致
                                    "timeUnit": data.acceleratetimeunit,//时间单位,30分钟02,1小时03,1天04,1周05,1月06
                                    "target": "acceleration"//指标名
                                }
                            ]
                        },
                        {
                            "name": "黄色预警",//其他预警等级内容与蓝色预警等级一致.
                            "level": 2,
                            "flag": true,
                            "targetCheckbox": [
                                {
                                    "label": "位移",
                                    "value": data.displaceyellow,
                                    "flag": data.displacement,
                                    "timeUnit": "",
                                    "target": "displacement"
                                },
                                {
                                    "label": "速度",
                                    "value": data.speedyellow,
                                    "flag": data.speed,
                                    "timeUnit": data.speedtimeunit,
                                    "target": "speed"
                                },
                                {
                                    "label": "加速度",
                                    "value": data.accelerateyellow,
                                    "flag": data.acceleration,
                                    "timeUnit": data.acceleratetimeunit,
                                    "target": "acceleration"
                                }
                            ]
                        },
                        {
                            "name": "橙色预警",
                            "level": 3,
                            "flag": true,
                            "targetCheckbox": [
                                {
                                    "label": "位移",
                                    "value": data.displaceorange,
                                    "flag": data.displacement,
                                    "timeUnit": "",
                                    "target": "displacement"
                                },
                                {
                                    "label": "速度",
                                    "value": data.speedorange,
                                    "flag": data.speed,
                                    "timeUnit": data.speedtimeunit,
                                    "target": "speed"
                                },
                                {
                                    "label": "加速度",
                                    "value": data.accelerateorange,
                                    "flag": data.acceleration,
                                    "timeUnit": data.acceleratetimeunit,
                                    "target": "acceleration"
                                }
                            ]
                        },
                        {
                            "name": "红色预警",
                            "level": 4,
                            "flag": true,
                            "targetCheckbox": [
                                {
                                    "label": "位移",
                                    "value": data.displacered,
                                    "flag": data.displacement,
                                    "timeUnit": "",
                                    "target": "displacement"
                                },
                                {
                                    "label": "速度",
                                    "value": data.speedred,
                                    "flag": data.speed,
                                    "timeUnit": data.speedtimeunit,
                                    "target": "speed"
                                },
                                {
                                    "label": "加速度",
                                    "value": data.acceleratered,
                                    "flag": data.acceleration,
                                    "timeUnit": data.acceleratetimeunit,
                                    "target": "acceleration"
                                }
                            ]
                        }
                    ]
                }
            ).then(result=>{
                resolve(result);
            })
        })
    }
    static updateAlarmRule(data){
        return new Promise(resolve => {
            axios.post(this.apiUrl+'/api/protocol/update/ruleBatch',{
                    "projectId":data.projectId,//项目ID
                    "id": data.id,//预警ID
                    "ruleName": data.name,//预警名
                    "ruleDescription": data.describe,//预警描述
                    "alarmRule": data.operator,//预警规则,现仅支持“>”,“<”,“>=”,“<=”
                    "enable": data.enable,//启用
                    "devices": data.deviceId,//设备ID
                    "geoMarkArray":data.geoMarkArray,//位置ID
                    "dataSource":"10",//数据来源,10连续形变,00原始形变。现仅有连续形变
                    "targetFlag":data.targetFlag,//数据值是否为绝对值
                    "alarmTargetThresholds":[       //多个指标的阈值
                        {
                            "name": "蓝色预警",
                            "level": 1, //预警等级   排列顺序请按现有顺序排列请勿错乱,即蓝色预警、黄色预警、橙色预警、红色预警
                            "flag": true,//启用。现仅支持启用，请勿修改
                            "targetCheckbox": [
                                {
                                    "label": "位移",//指标名称
                                    "value": data.displaceblue,//数值,若flag为true,请填写数值
                                    "flag": data.displacement,//是否启用，请与其他预警等级中的targetCheckbox的flag保持一致
                                    "timeUnit": "",//时间单位,位移无时间单位
                                    "target": "displacement" //指标名
                                },
                                {
                                    "label": "速度",
                                    "value": data.speedblue,//数值,若flag为true,请填写数值
                                    "flag": data.speed,//是否启用，请与其他预警等级中的targetCheckbox的flag保持一致
                                    "timeUnit": data.speedtimeunit,//时间单位,30分钟02,1小时03,1天04,1周05,1月06
                                    "target": "speed"//指标名
                                },
                                {
                                    "label": "加速度",
                                    "value": data.accelerateblue,//数值,若flag为true,请填写数值
                                    "flag": data.acceleration,//是否启用，请与其他预警等级中的targetCheckbox的flag保持一致
                                    "timeUnit": data.acceleratetimeunit,//时间单位,30分钟02,1小时03,1天04,1周05,1月06
                                    "target": "acceleration"//指标名
                                }
                            ]
                        },
                        {
                            "name": "黄色预警",//其他预警等级内容与蓝色预警等级一致.
                            "level": 2,
                            "flag": true,
                            "targetCheckbox": [
                                {
                                    "label": "位移",
                                    "value": data.displaceyellow,
                                    "flag": data.displacement,
                                    "timeUnit": "",
                                    "target": "displacement"
                                },
                                {
                                    "label": "速度",
                                    "value": data.speedyellow,
                                    "flag": data.speed,
                                    "timeUnit": data.speedtimeunit,
                                    "target": "speed"
                                },
                                {
                                    "label": "加速度",
                                    "value": data.accelerateyellow,
                                    "flag": data.acceleration,
                                    "timeUnit": data.acceleratetimeunit,
                                    "target": "acceleration"
                                }
                            ]
                        },
                        {
                            "name": "橙色预警",
                            "level": 3,
                            "flag": true,
                            "targetCheckbox": [
                                {
                                    "label": "位移",
                                    "value": data.displaceorange,
                                    "flag": data.displacement,
                                    "timeUnit": "",
                                    "target": "displacement"
                                },
                                {
                                    "label": "速度",
                                    "value": data.speedorange,
                                    "flag": data.speed,
                                    "timeUnit": data.speedtimeunit,
                                    "target": "speed"
                                },
                                {
                                    "label": "加速度",
                                    "value": data.accelerateorange,
                                    "flag": data.acceleration,
                                    "timeUnit": data.acceleratetimeunit,
                                    "target": "acceleration"
                                }
                            ]
                        },
                        {
                            "name": "红色预警",
                            "level": 4,
                            "flag": true,
                            "targetCheckbox": [
                                {
                                    "label": "位移",
                                    "value": data.displacered,
                                    "flag": data.displacement,
                                    "timeUnit": "",
                                    "target": "displacement"
                                },
                                {
                                    "label": "速度",
                                    "value": data.speedred,
                                    "flag": data.speed,
                                    "timeUnit": data.speedtimeunit,
                                    "target": "speed"
                                },
                                {
                                    "label": "加速度",
                                    "value": data.acceleratered,
                                    "flag": data.acceleration,
                                    "timeUnit": data.acceleratetimeunit,
                                    "target": "acceleration"
                                }
                            ]
                        }
                    ]
                }
            ).then(result=>{
                resolve(result);
            })
        })
    }
    static deleteAlarmRule(id,projectId){
        return new Promise(resolve => {
            axios.get(this.apiUrl+'/api/protocol/remove/ruleBatch/'+id+'/'+projectId+'')
                .then(data=>{
                    resolve(data);
                })
        })
    }
    static addAlarmContact(name,email,phone,alarmlevel,enable,projectId){
        return new Promise(resolve => {
            axios.post(this.apiUrl+'/api/protocol/add/contact',{
                "id": getUUID(), //推送ID
                "name": name,//联络人姓名
                "email": email,//“邮箱”
                "phone": phone,//“手机号”
                "alarmLevel": alarmlevel,//“预警等级” 0正常运行 1蓝色预警 2黄色预警 3橙色预警 4红色预警
                "enable": enable, //启用
                "projectId": projectId    //项目ID
            }).then(res=>{
                resolve(res);
            })
        })
    }
    static addAllowPeople(name,phone,project_code){
        return new Promise(resolve => {
            axios.get(this.kotiotApiUrl+'/api/server/addAllowPeople?name='+name+'&phone='+phone+'&project_code='+project_code).then(res=>{
                resolve(res);
            })
        })
    }
    static updateAllowPeople(name,phone,project_code){
        return new Promise(resolve => {
            axios.get(this.kotiotApiUrl+'/api/server/addAllowPeople?name'+name+'&phone='+phone+'&project_code='+project_code).then(res=>{
                resolve(res);
            })
        })
    }
    static updateAlarmContact(id,name,email,phone,alarmlevel,enable,projectId){
        return new Promise(resolve => {
            axios.post(this.apiUrl+'/api/protocol/update/contact',{
                "id": id, //推送ID
                "name": name,//联络人姓名
                "email": email,//“邮箱”
                "phone": phone,//“手机号”
                "alarmLevel": alarmlevel,//“预警等级” 0正常运行 1蓝色预警 2黄色预警 3橙色预警 4红色预警
                "enable": enable, //启用
                "projectId": projectId    //项目ID
            }).then(res=>{
                resolve(res);
            })
        })
    }
    static getAlarmContact(projectId){
        return new Promise(resolve => {
            axios.get(this.apiUrl+'/api/protocol/query/contact/'+projectId)
                .then(data=>{
                    resolve(data);
                })
        })
    }
    static getAllowPeople(project_code){
        return new Promise(resolve => {
            axios.get(this.kotiotApiUrl+'/api/server/getAllowPeople?project_code='+project_code)
                .then(data=>{
                    resolve(data);
                })
        })
    }
    static deleteAllowPeople(id){
        return new Promise(resolve => {
            axios.get(this.kotiotApiUrl+'/api/server/delAllowPeople?id='+id)
                .then(data=>{
                    resolve(data);
                })
        })
    }
    static deleteAlarmContact(id,projectId){
        return new Promise(resolve => {
            axios.get(this.apiUrl+'/api/protocol/remove/contact/'+id+'/'+projectId+'')
                .then(data=>{
                    resolve(data);
                })
        })
    }
    static addAlarmMessage(params){
        return new Promise(resolve => {
            axios.post(this.apiUrl+'/api/protocol/update/smsConfig',params).then(res=>resolve(res));
        })
    }
    static updateDiskStorage(discSpacePercentage,deleteFile){
        return new Promise((resolve,reject) => {
            axios.post(this.apiUrl+'/api/custom/updateDiskStorage',{
                discSpacePercentage,deleteFile
            }).then(res=>resolve(res))
                .catch(error=>{
                    reject(error);
                });
        })
    }
    static updateTiltMotorPitch(projectId,deviceId,pitch){
        return new Promise((resolve,reject) => {
            axios.post(this.apiUrl+'/api/custom/updateTiltMotorPitch',{
                projectId,deviceId,pitch
            }).then(res=>resolve(res))
                .catch(error=>{reject(error)});
        })
    }
    static setParamControl(projectId,deviceId){
        return new Promise(resolve => {
            axios.post(this.apiUrl+'/api/arcsar/command/'+projectId+'/'+deviceId+'/11/qingqiangjia').then(res=>{
                resolve(res);
            })
        })
    }
    static setPushiRadarParamControl(projectId,deviceId){
        return new Promise(resolve => {
            axios.post(this.apiUrl+'/api/mimoLite/command/'+projectId+'/'+deviceId+'/11/qingqiangjia').then(res=>{
                resolve(res);
            })
        })
    }
    static addLayer(oid,name,type,url,userid,postid,divisionid,orgid,treeid){
        return new Promise(resolve => {
            axios.get(this.customApiUrl+'/sloperadar/api/addlayer?oid='+oid+'&name='+name+'&type='+type
                +'&url='+url+'&userid='+userid+'&postid='+postid+'&divisionid='+divisionid+'&orgid='+orgid
                +'&treeid='+treeid).then(res=>{
                    resolve(res);
            })
        })
    }
    static deleteLayer(oid){
        return new Promise(resolve => {
            axios.get(this.customApiUrl+'/sloperadar/api/deletelayer?oid='+oid).then(res=>resolve(res));
        })
    }
    static enableLayer(oid,enable){
        return new Promise(resolve => {
            axios.get(this.customApiUrl+'/sloperadar/api/enablelayer?oid='+oid+'&enable='+enable).then(res=>resolve(res));
        })
    }
    static showLayer(oid,show){
        return new Promise(resolve => {
            axios.get(this.customApiUrl+'/sloperadar/api/showlayer?oid='+oid+'&show='+show).then(res=>resolve(res));
        })
    }
    static getLayer(orgid){
        return new Promise(resolve => {
            axios.get(this.customApiUrl+'/sloperadar/api/getlayer?orgid='+orgid).then(res=>resolve(res));
        })
    }
    static addDevice(projectId,deviceName,deviceId,factoryId,orientation,type,lon,lat,alt,ipv4,port,mqttTopic,status,description){
        return new Promise(resolve => {
            // ✅ 映射雷达类型到后端期望的DeviceTypeCode（int）
            const typeCodeMap = {
                'ER': 0,           // 圆弧雷达/边坡雷达
                'MIMOLITE': 7,     // MIMO雷达
                'ARCSAR': 0,       // 圆弧雷达
                'MIMO': 7          // MIMO雷达
            };
            
            axios.post(this.apiUrl+'/api/Device',{
                projectId: projectId,
                deviceName: deviceName,
                deviceId: deviceId,
                deviceType: type,
                deviceTypeCode: typeCodeMap[type] || 0,
                ipAddress: ipv4 || '127.0.0.1',
                port: port || 8888,
                // ✅ 独立的坐标字段
                longitude: parseFloat(lon) || 0,
                latitude: parseFloat(lat) || 0,
                elevation: parseFloat(alt) || 0,
                location: `经度:${lon},纬度:${lat},高度:${alt}`,  // 保留用于显示
                // ✅ 雷达特有信息
                factoryId: factoryId || '',
                orientation: parseFloat(orientation) || 0,
                mqttTopic: mqttTopic || `radar/${deviceId}`,
                description: description || ''
            }).then(res=>resolve(res))
        })
    }
    static addProject(projectId,projectName,projectDescribe,contact,phone,email,lon,lat,alt){
        return new Promise(resolve => {
            axios.post(this.apiUrl+'/api/Project',{
                projectId: projectId,
                projectName: projectName,
                description: projectDescribe,
                contactPerson: contact,
                contactPhone: phone,
                contactEmail: email,
                longitude: lon,
                latitude: lat,
                elevation: alt
            }).then(res=>resolve(res))
        })
    }
    static DeleteDevice(deviceId){
        return new Promise(resolve => {
            axios.delete(this.apiUrl+'/api/Device/'+encodeURIComponent(deviceId)).then(res=>{
                resolve(res);
            })
        })
    }
    static DeleteProject(projectId){
        return new Promise(resolve => {
            axios.delete(this.apiUrl+'/api/Project/'+encodeURIComponent(projectId)).then(res=>resolve(res));
        })
    }

    // === 新增：更新项目信息（REST - PUT） ===
    static UpdateProject(projectId, { projectName, projectDescribe, contact, phone, email, lon, lat, alt }){
        return new Promise(resolve => {
            axios.put(this.apiUrl+'/api/Project/'+encodeURIComponent(projectId),{
                projectId: projectId,
                projectName: projectName,
                description: projectDescribe,
                contactPerson: contact,
                contactPhone: phone,
                contactEmail: email,
                longitude: lon,
                latitude: lat,
                elevation: alt
            }).then(res=>resolve(res))
        })
    }

    // === 新增：更新设备信息（REST - PUT） ===
    static UpdateDevice(deviceId, { projectId, deviceName, type, ipv4, location, deviceTypeCode }){
        return new Promise(resolve => {
            axios.put(this.apiUrl+'/api/Device/'+encodeURIComponent(deviceId),{
                deviceId: deviceId,
                projectId: projectId,
                deviceName: deviceName,
                deviceType: type,
                deviceTypeCode: deviceTypeCode || type,
                ipAddress: ipv4,
                location: location
            }).then(res=>resolve(res))
        })
    }
    static GetUserAddressByIp(){
        return new Promise(resolve => {
            axios.post(this.kotiotApiUrl+'/api/server/getuseraddress').then(res=>resolve(res))
        })
    }
    static AddRadarLog(operate_content,operate_username,address,project_code,project_name){
        return new Promise(resolve => {
            if (window.localrelease!==undefined){
                resolve(200)
                return
            }
            axios.post(this.kotiotApiUrl+'/api/server/addradaroperatelog?operate_content='+operate_content+'&operate_username='+operate_username+'&address='+address
                +'&project_code='+project_code+'&project_name='+project_name)
                .then(res=>resolve(res))
        })
    }
    static DataRestore(projectId,deviceId,geoMaskId,geoMaskType,startTime,endTime){
        return new Promise(resolve => {
            axios.post(this.apiUrl+'/api/rollback/validate/geo/device',{
                projectId:projectId,
                deviceId:deviceId,
                geoMaskId:geoMaskId,
                geoMaskType:geoMaskType,
                startTime:startTime,
                endTime:endTime,
                rollbackStatus:'unstart',
                dataType:'10',
                deleteStatus:'false'
            }).then(res=>resolve(res))
        })
    }
    static DataGenerate(url,projectId,deviceId,startTime,endTime,interval,maxValue,minValue,markId,target,currentValue){
        return new Promise(resolve => {
            axios.post(this.radarApiUrl+'/api/radar/generatedatabyinterval',{
                url:url,
                projectId:projectId,
                deviceId:deviceId,
                startTime:startTime,
                endTime:endTime,
                interval:interval,
                maxValue:maxValue,
                minValue:minValue,
                markId:markId,
                target:target,
                currentValue:currentValue
            }).then(res=>resolve(res))
        })
    }
    static GetRadarOnlineStatusByTime(url,deviceId,datetime) {
        return new Promise(resolve => {
            axios.get(this.radarApiUrl+'/api/radar/lastonline?url='+url+'&deviceId='+deviceId+'&datetime='+datetime).then(res=>{
                resolve(res)
            })
        })
    }
    static GetRadarLastHeartbeatTime(url,deviceId) {
        return new Promise(resolve => {
            axios.get(this.radarApiUrl+'/api/radar/lastheartbeat?url='+url+'&deviceId='+deviceId).then(res=>{
                resolve(res)
            })
        })
    }
}
