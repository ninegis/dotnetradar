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
        },
        startRadarMQTT() {
            // 动态获取WebSocket地址
            const wsUrl = 'ws://' + window.location.hostname + ':8083/mqtt';  // EMQX
            console.log('[WebSocket] 连接地址:', wsUrl);
            this.client = mqtt.connect(wsUrl, this.options);

            let that = this;
            const heartbeatEvent = () => {
                let newDate = new Date(new Date().getTime() - that.sysinfo.config.radarHeart);
                for (let i = 0; i < that.projectData.deviceData.length; i++) {
                    ApiRadar.GetRadarOnlineStatusByTime(that.sysinfo.config.url, that.projectData.deviceData[i].id, DateTimeToStr(newDate)).then(res => {
                        that.projectInfo.deviceData[i]['online'] = res.data.data;
                    })
                }
            }
            this.client.on('connect', (e) => {
                console.log('[MQTT] 已连接到EMQX');
                this.client.subscribe('/dev/device/status', {}, (error) => { });  // ✅ 订阅设备状态
                this.client.subscribe('/dev/radar/mimoLite/defo/command', {}, (error) => { });
                this.client.subscribe('/dev/radar/mimo/defo/command/reponse', {}, (error) => { });
                this.client.subscribe('/dev/radar/mimoLite/defo/command', {}, (error) => { });
                this.client.subscribe('/dev/radar/defo/nAlgorithmParam', {}, (error) => { });
                this.client.subscribe('/dev/image', {}, (error) => { });
                this.client.subscribe('/dev/real/online', {}, (error) => { });
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
            // 断开发起重连
            this.client.on('reconnect', (error) => {
                console.log('正在重连:', error)
            })
            // 链接异常处理
            this.client.on('error', (error) => {
                console.log('连接失败:', error)
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
            const { deviceId, factoryId, status, timestamp } = statusData;
            
            console.log('[MQTT收到设备状态]', statusData);
            
            // 查找设备并更新状态（支持多种匹配方式）
            let device = this.projectData.deviceData.find(d => d.deviceId === deviceId);
            
            if (!device && factoryId) {
                // 尝试通过factoryId查找
                device = this.projectData.deviceData.find(d => d.factoryId === factoryId);
            }
            
            if (!device) {
                // 尝试通过设备ID的不同格式查找
                device = this.projectData.deviceData.find(d => 
                    d.id === deviceId || d.id === factoryId
                );
            }
            
            if (device) {
                // 更新状态（兼容不同字段名）
                if (device.status !== undefined) device.status = status;
                if (device.online !== undefined) device.online = status === 'online';
                if (device.lastHeartbeat !== undefined) device.lastHeartbeat = timestamp;
                
                console.log(`[设备状态更新成功] ${device.deviceName || deviceId}: ${status}`);
                
                // 显示通知
                if (status === 'online') {
                    showMessage(`设备上线: ${device.deviceName || deviceId}`, 'success');
                } else {
                    showMessage(`设备离线: ${device.deviceName || deviceId}`, 'warning');
                }
                
                // 强制更新视图
                this.projectData = {...this.projectData};
            } else {
                console.warn('[设备状态] 未找到设备:', deviceId, factoryId);
                console.log('[当前设备列表]', this.projectData.deviceData.map(d => ({id: d.id, deviceId: d.deviceId, factoryId: d.factoryId})));
            }
        }
    }
})