import { defineStore } from 'pinia'
import mqtt from "mqtt";
import { DateTimeToStr, showMessage } from "@/utils/tools.js";
import { CommonUtils } from "@/utils/CommonUtils.js";
import { ApiRadar } from "@/axios/apiRadar.js";

export const useMapStore = defineStore("sloperadarControl", {
    state: () => {
        return ({
            sysinfo: {

                //title:"SRS-Slope RT100边坡状态监测系统",
                //title:"ComNav Slope Radar Backend Management Platform",
                title: '空联网边坡雷达智能化监测预警后台管理系统',
                //  title:'司南边坡雷达监测预警后台管理系统',
                // title:'IMSE-100井场环境安全智能监测系统',
                // title:'迈知科技边坡雷达形变智能化分析预警平台',
                fontsize: 'font-size:34px',
                serverIp: null,
                ucmlInfo: {
                    divisionOid: null,
                    orgOid: null,
                    postOid: null,
                    userOid: null
                },
                websocketUrl: 'ws://' + window.location.hostname + ':8099/wss',  // 动态获取
                address: null,
                config: {
                    radarHeart: 0,
                    username: null,
                    projectCode: null,
                    shortName: null,
                    language: "0",  // ✅ 修复：默认中文，使用字符串
                    i18Title: null,
                    i18Sign: null
                }
            },
            monitorDevice: {
                treeSpinning: true,
                treeData: [{ title: '监测点', key: '0-0', children: [] }, { title: '监测面', key: '0-1', children: [] }],
                monitorEntityMap: {}
            },
            shieldEntities: [],
            radarImageEntityIds: [],
            radarSelected: [],
            projectInfo: {
                deviceData: [],
                projectData: [],
                projectSelected: '',
                currentScene: null  // ✅ 当前项目场景信息
            },
            radarInfo: {
                entityId: null,
                projectId: null,
                deviceId: null,
                deviceName: null,
                params: {},
                coordinates: [],
                algorithmParam: {},
                projectConfig: {},
                imageAnalysisConfig: {},
                tiltMotor: {},
                imageDiffAnalysisConfig: {},
                defoColorBarSetting: {},
                scatColorBarSetting: {},
                autoAnalysisHiddenAreaConfig: {},
                smsNotifyConfig: {},
            },
            toolbarcontent: '',
            alarmRuleInfo: {},
            alarmContactInfo: {},
            Layers: {},
            deviceOnlineStatus: {},
            deviceHeartbeatTimestamps: {}, // ✅ 设备心跳时间戳记录 {deviceId: timestamp}
            heartbeatCheckInterval: null, // ✅ 心跳检测定时器
            layerCheckedKeys: [],
            layerList: [],
            layerOid: null,
            boundaryEntityIds: [],
            radarEntityIds: [],
            dragContainer: {
                width: 500
            },
            axiosInstance: { ucmlInstance: null, radarInstance: null, otherInstance: null },
            client: null,
            paramLoading: true,
            imageData: [],
        })
    },
    getters: {
        dynamicTreeData(state) {
            if (state.sysinfo.config.language === '0') {
                state.monitorDevice.treeData[0].title = '监测点';
                state.monitorDevice.treeData[1].title = '监测面';
            } else {
                state.monitorDevice.treeData[0].title = 'Monitoring Point';
                state.monitorDevice.treeData[1].title = 'Monitoring Polygon';
            }
            return state.monitorDevice.treeData
        }
    },
    actions: {
        componentSlotDestroy() {
            this.toolbarcontent = '';
            // ✅ 组件销毁时停止心跳检测
            this.stopHeartbeatCheck();
        },
        startRadarMQTT() {
            // ✅ 如果已有连接，先断开
            if (this.client) {
                try {
                    this.client.end();
                    this.client = null;
                } catch (e) {
                    console.warn('[MQTT] 断开旧连接失败:', e);
                }
            }
            
            // 动态获取WebSocket地址
            const hostname = window.location.hostname;
            const wsUrl = `ws://${hostname}:8083/mqtt`;  // EMQX WebSocket
            
            console.log('[MQTT] 开始连接:', wsUrl);
            
            // ✅ MQTT连接选项
            const clientId = 'radar-frontend-' + Math.random().toString(16).substr(2, 8);
            const mqttOptions = {
                clientId: clientId,
                keepalive: 60, // 60秒心跳
                connectTimeout: 10000, // 10秒连接超时
                reconnectPeriod: 5000, // 5秒重连间隔
                clean: true,
                protocolVersion: 4, // MQTT 3.1.1
                rejectUnauthorized: false, // 允许自签名证书
                will: {
                    topic: '/dev/frontend/status',
                    payload: JSON.stringify({ status: 'offline', clientId: clientId }),
                    qos: 1,
                    retain: false
                }
            };
            
            this.client = mqtt.connect(wsUrl, mqttOptions);

            let that = this;
            const heartbeatEvent = () => {
                let newDate = new Date(new Date().getTime() - that.sysinfo.config.radarHeart);
                for (let i = 0; i < that.projectData.deviceData.length; i++) {
                    ApiRadar.GetRadarOnlineStatusByTime(that.sysinfo.config.url, that.projectData.deviceData[i].id, DateTimeToStr(newDate)).then(res => {
                        that.projectInfo.deviceData[i]['online'] = res.data.data;
                    })
                }
            }
            
            // ✅ 保存clientId供回调使用
            const savedClientId = clientId;
            
            this.client.on('connect', (e) => {
                console.log('[MQTT] ✅ 已连接到EMQX', e);
                
                // ✅ 订阅所有需要的主题
                const topics = [
                    '/dev/device/status',           // 设备状态
                    '/dev/radar/mimoLite/defo/command',
                    '/dev/radar/mimo/defo/command/reponse',
                    '/dev/radar/defo/nAlgorithmParam',
                    '/dev/image',
                    '/dev/real/online'
                ];
                
                topics.forEach(topic => {
                    this.client.subscribe(topic, { qos: 1 }, (error) => {
                        if (error) {
                            console.error(`[MQTT] 订阅失败 [${topic}]:`, error);
                        } else {
                            console.log(`[MQTT] ✅ 已订阅 [${topic}]`);
                        }
                    });
                });
                
                // ✅ 发布前端上线状态
                this.client.publish('/dev/frontend/status', JSON.stringify({
                    status: 'online',
                    clientId: savedClientId,
                    timestamp: new Date().toISOString()
                }), { qos: 1 });
                
                // ✅ 启动30秒心跳检测
                this.startHeartbeatCheck();
                
                if (that.sysinfo.config.radarHeart !== 0) {
                    setTimeout(() => {
                        heartbeatEvent();
                        setInterval(heartbeatEvent, that.sysinfo.config.radarHeart);
                    }, 5000)
                }
            })
            // 接收消息处理
            this.client.on('message', (topic, message) => {
                const msg = message.toString()
                const obj = JSON.parse(msg);
                
                // ✅ 处理设备状态更新
                if (topic === '/dev/device/status') {
                    console.log('[DeviceStatus]', obj);
                    this.updateDeviceOnlineStatus(obj);
                    return;
                }
                
                switch (topic) {
                    case '/dev/image': {
                        const index = CommonUtils.FindIndexOfArray('time', obj['ts'], this.imageData);
                        if (index === -1) {
                            showMessage('有新图像已更新');
                            break;
                        }
                        ApiRadar.queryImageList(obj['projectId'], obj['deviceId'], obj['ts'], obj['ts'], obj['deviceType'], 'success', 1).then(res => {
                            if (res.data.data.count === 1) {
                                this.imageData[index]['filedir'] = res.data.data.dataset[0][5];
                                this.imageData[index]['status'] = '成功';
                                showMessage('图像已更新');
                            }
                        })
                        break;
                    }
                    case '/dev/real/online': {
                        if (this.sysinfo.config.radarHeart !== 0) return;

                        const index = CommonUtils.FindIndexOfArray('id', obj['deviceId'], this.projectInfo.deviceData);
                        this.projectInfo.deviceData[index]['online'] = obj['status'];
                        showMessage('设备在线状态更新');
                        break;
                    }
                    case '/dev/radar/defo/nAlgorithmParam': {
                        // ✅ 更新新32字段算法参数（如果需要从MQTT接收）
                        // 注意：MQTT消息可能仍使用旧字段名，需要根据实际情况调整
                        this.paramLoading = false;
                        break;
                    }
                    case '/dev/radar/mimo/defo/command/reponse': {
                        if (obj['result'] === '0') {
                            showMessage('设置成功', 'success');
                        } else {
                            showMessage('操作失败，请重试', 'error');
                        }
                    }
                }
            })
            // ✅ 连接错误处理
            this.client.on('error', (error) => {
                console.error('[MQTT] ❌ 连接错误:', error);
                if (error.message && error.message.includes('timeout')) {
                    console.warn('[MQTT] 连接超时，请检查EMQX是否运行在端口8083');
                }
            })
            
            // ✅ 正在重连
            this.client.on('reconnect', () => {
                console.log('[MQTT] 🔄 正在重连...');
            })
            
            // ✅ 断开连接时停止心跳检测
            this.client.on('close', () => {
                console.log('[MQTT] ⚠️ 连接已关闭');
                this.stopHeartbeatCheck();
            })
            
            this.client.on('offline', () => {
                console.log('[MQTT] ⚠️ 连接已离线');
                this.stopHeartbeatCheck();
            })
            
            // ✅ 连接结束
            this.client.on('end', () => {
                console.log('[MQTT] 🔚 连接已结束');
                this.stopHeartbeatCheck();
            })
        },
        // ✅ 加载项目场景并定位
        loadProjectScene(projectData) {
            if (!projectData) {
                console.warn('[场景定位] 项目数据为空');
                return;
            }
            
            // ✅ 优先使用场景配置，如果没有则使用项目位置或默认值
            const sceneLongitude = projectData.sceneLongitude ?? projectData.longitude ?? 120.0;
            const sceneLatitude = projectData.sceneLatitude ?? projectData.latitude ?? 30.0;
            const sceneHeight = projectData.sceneHeight ?? projectData.elevation ?? 500.0;
            const sceneHeading = projectData.sceneHeading ?? 0.0;
            const scenePitch = projectData.scenePitch ?? -45.0;
            const sceneRoll = projectData.sceneRoll ?? 0.0;
            
            // 保存场景信息
            this.projectInfo.currentScene = {
                longitude: sceneLongitude,
                latitude: sceneLatitude,
                height: sceneHeight,
                heading: sceneHeading,
                pitch: scenePitch,
                roll: sceneRoll
            };
            
            console.log('[场景定位] 项目:', projectData.projectId || projectData.projectName, this.projectInfo.currentScene);
            
            // ✅ 获取viewer（优先window.viewer，其次CesiumUtils.viewer）
            const getViewer = () => {
                if (window.viewer) return window.viewer;
                if (window.CesiumUtils && window.CesiumUtils.viewer) return window.CesiumUtils.viewer;
                return null;
            };
            
            const getCesium = () => {
                if (window.Cesium) return window.Cesium;
                if (window.CesiumUtils && window.CesiumUtils.Cesium) return window.CesiumUtils.Cesium;
                return null;
            };
            
            // ✅ 等待Cesium初始化完成后再定位（增加最大重试次数）
            let retryCount = 0;
            const maxRetries = 50; // 最多重试5秒
            
            const tryFlyTo = () => {
                const viewer = getViewer();
                const Cesium = getCesium();
                
                if (viewer && Cesium) {
                    console.log('[场景定位] 执行飞行到项目位置:', {
                        lon: sceneLongitude,
                        lat: sceneLatitude,
                        height: sceneHeight,
                        heading: sceneHeading,
                        pitch: scenePitch
                    });
                    
                    try {
                        viewer.camera.flyTo({
                            destination: Cesium.Cartesian3.fromDegrees(
                                sceneLongitude,
                                sceneLatitude,
                                sceneHeight
                            ),
                            orientation: {
                                heading: Cesium.Math.toRadians(sceneHeading),
                                pitch: Cesium.Math.toRadians(scenePitch),
                                roll: Cesium.Math.toRadians(sceneRoll)
                            },
                            duration: 2.0
                        });
                        console.log('[场景定位] 飞行命令已执行');
                    } catch (error) {
                        console.error('[场景定位] 飞行执行失败:', error);
                    }
                } else {
                    retryCount++;
                    if (retryCount < maxRetries) {
                        console.log(`[场景定位] Cesium未就绪，100ms后重试 (${retryCount}/${maxRetries})`, {
                            hasViewer: !!viewer,
                            hasCesium: !!Cesium,
                            windowViewer: !!window.viewer,
                            cesiumUtilsViewer: !!(window.CesiumUtils && window.CesiumUtils.viewer)
                        });
                        setTimeout(tryFlyTo, 100);
                    } else {
                        console.error('[场景定位] 超过最大重试次数，定位失败');
                    }
                }
            };
            
            // 立即尝试或延迟执行
            const viewer = getViewer();
            const Cesium = getCesium();
            if (viewer && Cesium) {
                tryFlyTo();
            } else {
                console.log('[场景定位] Cesium未就绪，开始重试...');
                setTimeout(tryFlyTo, 100);
            }
            
            // 同时发送事件通知（兼容旧代码）
            window.dispatchEvent(new CustomEvent('project-scene-loaded', {
                detail: this.projectInfo.currentScene
            }));
        },
        
        // ✅ 更新设备在线状态
        updateDeviceOnlineStatus(statusData) {
            const { deviceId, slaveId, status, timestamp } = statusData;
            
            // ✅ 使用时间戳（如果有）或当前时间
            const heartbeatTime = timestamp ? new Date(timestamp).getTime() : Date.now();
            
            console.log('[MQTT收到设备状态]', statusData, '心跳时间:', new Date(heartbeatTime).toLocaleString());
            
            // ✅ 在所有项目的设备中查找（遍历projectData）
            let device = null;
            let foundProject = null;
            
            for (const project of this.projectInfo.projectData) {
                if (!project.devices || project.devices.length === 0) continue;
                
                // 优先通过deviceId查找
                device = project.devices.find(d => d.deviceId === deviceId);
                
                if (!device && slaveId) {
                    // 尝试通过slaveId查找
                    device = project.devices.find(d => d.slaveId === slaveId);
                }
                
                if (!device) {
                    // 尝试通过设备ID的不同格式查找
                    device = project.devices.find(d => 
                        d.id === deviceId || d.id === slaveId
                    );
                }
                
                if (device) {
                    foundProject = project;
                    break;
                }
            }
            
            if (device) {
                // ✅ 记录心跳时间戳
                const key = deviceId || slaveId || device.id;
                this.deviceHeartbeatTimestamps[key] = heartbeatTime;
                
                // 更新状态（兼容不同字段名）
                const isOnline = status === 'online' || status === true;
                if (device.status !== undefined) device.status = isOnline ? 'online' : 'offline';
                if (device.online !== undefined) device.online = isOnline;
                if (device.lastHeartbeat !== undefined) device.lastHeartbeat = heartbeatTime;
                
                console.log(`[设备状态更新] ${device.deviceName || deviceId}: ${isOnline ? '在线' : '离线'}`, {
                    deviceId,
                    slaveId,
                    status,
                    heartbeatTime: new Date(heartbeatTime).toLocaleString()
                });
                
                // 强制更新视图
                this.projectInfo.projectData = [...this.projectInfo.projectData];
            } else {
                console.warn('[设备状态] 未找到设备:', deviceId, slaveId);
            }
        },
        
        // ✅ 启动30秒心跳检测
        startHeartbeatCheck() {
            // 清除旧的定时器
            if (this.heartbeatCheckInterval) {
                clearInterval(this.heartbeatCheckInterval);
            }
            
            const HEARTBEAT_TIMEOUT = 30 * 1000; // 30秒超时
            
            this.heartbeatCheckInterval = setInterval(() => {
                const now = Date.now();
                let hasChanges = false;
                
                // ✅ 遍历所有项目的所有设备
                for (const project of this.projectInfo.projectData) {
                    if (!project.devices || project.devices.length === 0) continue;
                    
                    for (const device of project.devices) {
                        const key = device.deviceId || device.id || device.slaveId;
                        if (!key) continue;
                        
                        const lastHeartbeat = this.deviceHeartbeatTimestamps[key];
                        
                        if (lastHeartbeat) {
                            const timeSinceHeartbeat = now - lastHeartbeat;
                            
                            // ✅ 如果超过30秒没有心跳，标记为离线
                            if (timeSinceHeartbeat > HEARTBEAT_TIMEOUT) {
                                const wasOnline = device.status === 'online' || device.online === true;
                                
                                if (wasOnline) {
                                    device.status = 'offline';
                                    device.online = false;
                                    hasChanges = true;
                                    
                                    console.log(`[心跳检测] 设备超时离线: ${device.deviceName || key}`, {
                                        lastHeartbeat: new Date(lastHeartbeat).toLocaleString(),
                                        timeout: Math.round(timeSinceHeartbeat / 1000) + '秒'
                                    });
                                }
                            } else {
                                // ✅ 如果在线但在30秒内有心跳，确保状态是在线
                                const shouldBeOnline = device.status !== 'offline';
                                if (shouldBeOnline && (device.status !== 'online' || device.online !== true)) {
                                    device.status = 'online';
                                    device.online = true;
                                    hasChanges = true;
                                }
                            }
                        } else {
                            // ✅ 没有心跳记录，默认离线
                            if (device.status === 'online' || device.online === true) {
                                device.status = 'offline';
                                device.online = false;
                                hasChanges = true;
                            }
                        }
                    }
                }
                
                // ✅ 如果有变化，强制更新视图
                if (hasChanges) {
                    this.projectInfo.projectData = [...this.projectInfo.projectData];
                }
            }, 30000); // 每30秒检查一次
            
            console.log('[心跳检测] 已启动，每30秒检查一次设备心跳状态');
        },
        
        // ✅ 停止心跳检测
        stopHeartbeatCheck() {
            if (this.heartbeatCheckInterval) {
                clearInterval(this.heartbeatCheckInterval);
                this.heartbeatCheckInterval = null;
                console.log('[心跳检测] 已停止');
            }
        }
    }
})