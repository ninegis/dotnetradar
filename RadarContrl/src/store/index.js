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
                websocketUrl: null,
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
                projectSelected: ''
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
            this.client = mqtt.connect(this.sysinfo.websocketUrl, this.options);

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
    }
})