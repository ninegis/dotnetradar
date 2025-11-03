import { useMapStore } from "@/store/index.js";
import { CesiumUtils } from "@/utils/CesiumUtils.js";
import { MonitorPoint, MyLocation } from "@/assets/load.js";
import { CallbackProperty, Color, Entity, HeightReference, ImageMaterialProperty, Rectangle } from "cesium";
import { showMessage } from "@/utils/tools.js";
import { CommonUtils } from "@/utils/CommonUtils.js";
import { ApiRadar } from "@/axios/apiRadar.js";
import * as d3 from "d3";
import { getGPSLayerTree } from "@/axios/apiucml.js";
import axios from "axios";
import { Decimal } from 'decimal.js'

export const monitorLoad = (data) => {
    const store = useMapStore();
    store.shieldEntities = [];
    for (let i = 0; i < data.length; i++) {
        if (data[i]['type'] === 'GEO-POINT') {
            CesiumUtils.EntityPointAdd(data[i]['coordinates'][0][0], data[i]['coordinates'][0][1],
                data[i]['coordinates'][0][2], MonitorPoint, data[i].name, -58, HeightReference.NONE).then(entity => {
                    store.monitorDevice.treeData[0].children.push({ title: data[i].name, key: entity.id, radarId: data[i]['devices'][0] });
                    store.monitorDevice.monitorEntityMap[entity.id] = data[i].id;
                })
        } else {
            CesiumUtils.EntityPolygonAdd(data[i]['coordinates'], data[i].name, data[i]['enableShieldArea'] ? Color.MEDIUMVIOLETRED.withAlpha(0.2) : Color.MEDIUMSPRINGGREEN.withAlpha(0.2)).then(entity => {
                let title = data[i].name;
                if (title.substring(0, 4) === "KOT_" && !window.tool.visible) {
                    entity.show = false;
                    entity.properties = { geoId: data[i].id };
                    store.shieldEntities.push(entity.id);
                } else {
                    store.monitorDevice.treeData[1].children.push({ title: title, key: entity.id, radarId: data[i]['devices'][0] });
                    store.monitorDevice.monitorEntityMap[entity.id] = data[i].id;
                }
            })
        }
    }
    store.monitorDevice.treeSpinning = false;
}
export const getColorRule = (data) => {
    let content = '';
    for (let i = 0; i < data.length; i++) {
        if (data[i].flag) {
            content += data[i].label + '阈值' + data[i].value + 'mm,'
        }
    }
    return content.substring(0, content.length - 1);
}
export const getTarget = (data) => {
    let content = '';
    for (let i = 0; i < data.length; i++) {
        if (data[i].flag) {
            content += data[i].label + ',';
        }
    }
    return content.substring(0, content.length - 1);
}
export const getTargetStatus = (data) => {
    let arr = [];
    for (let i = 0; i < data.length; i++) {
        if (data[i].flag) {
            arr.push(data[i].label);
        }
    }
    return arr;
}
export const loadLayer = (orgId) => {
    return new Promise(resolve => {
        if (window.localrelease !== undefined) {
            resolve(200)
            return;
        }
        getGPSLayerTree("", orgId).then(res => {
            if (res.status !== 200) { return; }
            // 检查 res、res.data 和 res.data.Entity 是否存在，且 res.data.Entity 不是空对象
            if (!res || !res.data || !res.data.Entity || Object.keys(res.data.Entity).length === 0) {
                return;
            }
            const data = res.data.Entity[Object.keys(res.data.Entity)[0]];
            const store = useMapStore();
            for (let i = 0; i < data.length; i++) {
                if (data[i]['type'] === 'mapGroup') store.layerOid = data[i]['oid'];
                if (data[i]['initDisplay'] === 'True') {
                    switch (data[i]['ServiceType']) {
                        case '3dtile':
                            // CesiumUtils.LayerPrimitive3dtileAdd('http://127.0.0.1:7777/3dtile/tileset.json').then(primitive=>{
                            CesiumUtils.LayerPrimitive3dtileAdd(data[i]['ServiceAddress']).then(primitive => {
                                store.layerCheckedKeys.push(data[i]['oid']);
                                store.layerList[data[i]['oid']] = primitive;
                            });
                            break;
                        case 'geojson':
                            CesiumUtils.LayerGeoJsonAdd(data[i]['ServiceAddress']).then(ds => {
                                store.layerCheckedKeys.push(data[i]['oid']);
                                store.layerList[data[i]['oid']] = ds;
                            })
                            break;
                        case 'tms':
                            CesiumUtils.LayerImageryTMSAdd(data[i]['ServiceAddress']).then(layer => {
                                store.layerCheckedKeys.push(data[i]['oid']);
                                store.layerList[data[i]['oid']] = layer;
                            })
                            break;
                        default:
                            showMessage('该地图格式未接入系统：' + data[i]['ServiceType'], 'warning');
                            break;
                    }
                }
            }
        })
        resolve(200)
    })
}
export const layerLoad = (data) => {
    const store = useMapStore();
    return new Promise(resolve => {
        for (let i = 0; i < data.length; i++) {
            if (data[i].type === "MODEL-3DTILES") {
                CesiumUtils.LayerPrimitive3dtileAdd(store.sysinfo.apiUrl + data[i].url);
            }
        }
    })
}
export const getValue = (data, index) => {
    let arr = [];
    for (let i = 0; i < data.length; i++) {
        if (data[i].targetCheckbox[index].flag) {
            arr.push(data[i].targetCheckbox[index].value);
        }
    }
    return arr;
}
export const getTimeUnit = (data, index) => {
    return data[0].targetCheckbox[index].timeUnit;
}
export const addMonitorPoint = () => {
    CesiumUtils.DrawPoint('请在地图上选择一个点并使用鼠标左键确认').then(result => {
        CesiumUtils.EntityRemove(result[1]);
        CommonUtils.ShowInputDialog('是否确认新增该监测点?', '在下方输入监测点名称并点击确认提交按钮').then(response => {
            if (response === 'reject') {
                showMessage('操作已取消', 'message');
                return;
            }
            CesiumUtils.EntityPointAdd(result[0][0], result[0][1], result[0][2], MonitorPoint, response, -58, HeightReference.NONE).then(entity => {
                const uuid = getUUID();
                const store = useMapStore();
                ApiRadar.addMonitoringLocation({
                    id: uuid,
                    projectId: store.radarInfo.projectId,
                    name: response,
                    type: 'GEO-POINT',
                    deviceId: store.radarInfo.deviceId,
                    coordinate: [result[0]]
                }).then(data => {
                    if (data.data.msg === '操作成功') {
                        showMessage('新增成功', 'success');
                        store.monitorDevice.treeData[0].children.push({ title: response, key: entity.id });
                        store.monitorDevice.monitorEntityMap[entity.id] = uuid;
                    }
                })
            })
        })
    })
}
export const addMonitorPolygon = (enableShield) => {
    CesiumUtils.DrawPolygon('请绘制一个面并鼠标右键结束绘制').then(result => {
        CesiumUtils.EntityRemove(result[1]);
        CommonUtils.ShowInputDialog('是否确认新增该监测面?', '在下方输入监测面名称并点击确认提交按钮').then(response => {
            if (response === 'reject') {
                showMessage('操作已取消', 'message');
                return;
            }
            const uuid = getUUID();
            const store = useMapStore();
            CesiumUtils.EntityPolygonAdd(result[0], response, enableShield ? Color.MEDIUMVIOLETRED.withAlpha(0.2) : Color.MEDIUMSPRINGGREEN.withAlpha(0.2)).then((entity) => {
                ApiRadar.addMonitoringLocation({
                    id: uuid,
                    projectId: store.radarInfo.projectId,
                    name: response,
                    type: 'GEO-AREA',
                    deviceId: store.radarInfo.deviceId,
                    coordinate: result[0]
                }, enableShield).then(() => {
                    showMessage('新增成功', 'success');
                    store.monitorDevice.treeData[1].children.push({ title: response, key: entity.id });
                    store.monitorDevice.monitorEntityMap[entity.id] = uuid;
                })
            })
        });
    })
}
export const staticDataBind = () => {
    const store = useMapStore();
    console.log('staticDataBind 被调用');
    console.log('当前projectSelected:', store.projectInfo.projectSelected);
    console.log('projectData:', store.projectInfo.projectData);
    
    // ✅ 修复：使用 projectId 而不是 id 来查找项目
    const data = CommonUtils.FindObjectOfArray('projectId', store.projectInfo.projectSelected, store.projectInfo.projectData);
    
    if (!data) {
        console.error('找不到项目数据，projectId:', store.projectInfo.projectSelected);
        return;
    }
    
    console.log('找到项目:', data.projectName, '设备数量:', data.devices?.length || 0);
    
    store.radarInfo.projectId = data.projectId;

    for (let i = store.projectInfo.deviceData.length - 1; i >= 0; i--) {
        CesiumUtils.EntityRemoveById(store.projectInfo.deviceData[i]['entityId']);
        store.projectInfo.deviceData.splice(i, 1);
    }
    for (let i = store.boundaryEntityIds.length - 1; i >= 0; i--) {
        CesiumUtils.EntityRemoveById(store.boundaryEntityIds[i]['entityId']);
        store.boundaryEntityIds.splice(i, 1);
    }
    store.radarSelected = [];
    
    // ✅ 添加安全检查
    if (!data.devices || data.devices.length === 0) {
        console.warn('当前项目没有设备');
        return;
    }
    
    // ✅ 只在第一个设备时设置radarInfo，避免被最后一个设备覆盖
    let isFirstDevice = true;
    
    for (let i = 0; i < data.devices.length; i++) {
        if (data.devices[i]['type'] === 'ER' || data.devices[i]['type'] === "MIMOLITE") {
            store.radarSelected.push(data.devices[i]['id']);

            // ✅ 只在第一个设备或没有选中设备时设置
            if (isFirstDevice || !store.radarInfo.deviceId) {
                store.radarInfo.deviceId = data.devices[i]['id'];
                store.radarInfo.deviceName = data.devices[i]['name'];
                
                // 判断项目配置是否有虚拟坐标
                const inDeviceConfig = store.sysinfo.config.radarCoordinates?.indexOf(data.devices[i].id.substring(7)) > -1;
                if (inDeviceConfig) {
                    //雷达坐标跟实际不符合 特定坐标
                    const startPos = store.sysinfo.config.radarCoordinates.indexOf(
                        data.devices[i].id.substring(7)
                    );
                    const endPos = store.sysinfo.config.radarCoordinates.indexOf(
                        ";",
                        startPos + 1
                    );
                    const newPt = store.sysinfo.config.radarCoordinates
                        .substring(startPos, endPos)
                        .split(",")
                        .map(Number);
                    store.radarInfo.coordinates = newPt.slice(1, 4);
                } else {
                    store.radarInfo.coordinates = data.devices[i].coordinates;
                }
                //雷达参数
                store.radarInfo.params = data.devices[i].params;
                store.radarInfo.algorithmParam = data.devices[i].algorithmParam;
                //俯仰电机配置
                store.radarInfo.tiltMotor = data.devices[1];
                
                isFirstDevice = false;
            }
            
            // ✅ 用于绘制雷达扇区的坐标（每个设备都需要）
            const coordinatesTemp = data.devices[i]['coordinates'];
            data.devices[i]['params']['radarOri'] = parseFloat(data.devices[i]['params']['radarOri']);
            data.devices[i]['params']['ImgAngleEnd'] = parseFloat(data.devices[i]['params']['ImgAngleEnd']);
            data.devices[i]['params']['AnteBeam_half'] = parseFloat(data.devices[i]['params']['AnteBeam_half']);
            data.devices[i]['params']['ImgAngleStart'] = parseFloat(data.devices[i]['params']['ImgAngleStart']);
            if (data.devices[i]['type'] === 'MIMOLITE') {
                CesiumUtils.DrawSector({
                    lon: coordinatesTemp[0],
                    lat: coordinatesTemp[1],
                    height: coordinatesTemp[2],
                    d1: data.devices[i]['params']['radarOri'] + data.devices[i]['params']['ImgAngleStart'] >= 30 ? data.devices[i]['params']['radarOri'] + data.devices[i]['params']['ImgAngleStart'] : 30,
                    d2: data.devices[i]['params']['radarOri'] + data.devices[i]['params']['ImgAngleEnd'],
                    color: Color.GHOSTWHITE.withAlpha(0.36),
                    radius: parseFloat(data.devices[i]['params']['RngMax']) * 1.3
                }).then(entity => {
                    entity.show = false;
                    store.boundaryEntityIds.push({ deviceId: data.devices[i]['id'], entityId: entity.id });
                });
            } else {
                CesiumUtils.DrawSector({
                    lon: coordinatesTemp[0],
                    lat: coordinatesTemp[1],
                    height: coordinatesTemp[2],
                    d2: data.devices[i]['params']['radarOri'] + data.devices[i]['params']['AnteBeam_half'] / 2 +
                        data.devices[i]['params']['ImgAngleEnd'] - data.devices[i]['params']['AnteBeam_half'],
                    d1: data.devices[i]['params']['radarOri'] + data.devices[i]['params']['ImgAngleStart'] +
                        data.devices[i]['params']['AnteBeam_half'] / 2,
                    color: Color.GHOSTWHITE.withAlpha(0.36),
                    radius: parseFloat(data.devices[i]['params']['RngMax']) * 1.3
                }).then(entity => {
                    entity.show = false;
                    store.boundaryEntityIds.push({ deviceId: data.devices[i]['id'], entityId: entity.id });
                });
            }
            CesiumUtils.EntityPointAdd(coordinatesTemp[0], coordinatesTemp[1], coordinatesTemp[2], MyLocation, data.devices[i]['name']).then(entity => {
                data.devices[i]['entityId'] = entity.id;
                store.projectInfo.deviceData.push(data.devices[i]);
            })
        }
    }
    if (data.devices.length === 0) {
        store.radarInfo.deviceId = null;
        store.radarInfo.coordinates = [120, 30, 100];
        //雷达参数
        store.radarInfo.params = [];
        store.radarInfo.algorithmParam = [];
    }

    //项目配置
    store.radarInfo.projectConfig['name'] = data.projectName || data.name;
    store.radarInfo.projectConfig['description'] = data.description;
    store.radarInfo.projectConfig['contact'] = data.contactPerson || data.contact;
    store.radarInfo.projectConfig['phone'] = data.contactPhone || data.phone;
    store.radarInfo.projectConfig['email'] = data.contactEmail || data.email;
    //雷达图像配置
    store.radarInfo.imageAnalysisConfig = data.imageAnalysisConfig;
    //雷达图像差分配置
    store.radarInfo.imageDiffAnalysisConfig = data.imageDiffAnalysisConfig;
    //色条配置
    store.radarInfo.defoColorBarSetting = data.defoColorBarSetting;
    store.radarInfo.scatColorBarSetting = data.scatColorBarSetting;
    //隐患区域分析配置
    store.radarInfo.autoAnalysisHiddenAreaConfig = data.autoAnalysisHiddenAreaConfig;
    //短信推送配置
    store.radarInfo.smsNotifyConfig = data.smsNotifyConfig;
    //预警联系人配置
    store.radarInfo.ruleContact = data.contactsAlarm;
    
    // ✅ 加载项目场景并定位到项目位置（总是调用，内部会处理默认值）
    console.log('[staticDataBind] 加载项目场景:', {
        projectId: data.projectId,
        sceneLongitude: data.sceneLongitude,
        sceneLatitude: data.sceneLatitude,
        sceneHeight: data.sceneHeight,
        longitude: data.longitude,
        latitude: data.latitude
    });
    store.loadProjectScene(data);
}
export function getUUID(a) {
    return a ? (a ^ Math.random() * 16 >> a / 4).toString(16) : ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, getUUID);
}
export const renderColorBar = (data, id, title) => {
    let valuearr = [];
    const interval = (data.maxValue - data.minValue) / (data.colorArray.length - 1);
    for (let i = 0; i < data.colorArray.length; i++) {
        valuearr.push(data.minValue + i * interval);
    }
    const svg = Legend(d3.scaleSqrt(valuearr, data.colorArray), {
        title: title,
        width: 550
    });
    document.getElementById(id).innerHTML = '<svg width="550" height="50" viewBox="0,0,550,50" style="overflow: visible; display: block;">' + svg.innerHTML + '</svg>';
}
export const renderNewColorBar = (data, id, title) => {
    let valuearr = [];
    let valueList = [];
    const interval = (new Decimal(data.maxValue).sub(data.minValue)).div(data.colorList.length - 1).toNumber();
    const interval1 = (new Decimal(data.maxValue).sub(data.minValue)).div(data.colorList.length).toNumber();
    for (let i = 0; i < data.colorList.length; i++) {
        valuearr.push(data.minValue + i * interval);
        valueList.push(data.minValue + i * interval1)
    }
    valueList.push(data.maxValue);
    const svg = Legend(d3.scaleSqrt(valuearr, data.colorList), {
        title: title,
        width: 750,
        tickValues: valueList
    });
    document.getElementById(id).innerHTML = '<svg width="550" height="50" viewBox="0,0,550,50" style="overflow: visible; display: block;">' + svg.innerHTML + '</svg>';
}

export function createColorBar({
    container,
    colorStops = [
        { position: 0, color: '#FF0000', label: '低温' },
        { position: 0.5, color: '#FFFF00', label: '常温' },
        { position: 1, color: '#00FF00', label: '高温' }
    ],
    width = 750,
    height = 50,
    tickInterval,
    tickLength
}) {
    const canvas = document.getElementById('colorBar');;
    canvas.width = width;
    canvas.height = height;
    container.appendChild(canvas);

    const ctx = canvas.getContext('2d');
    const barHeight = 40;
    const padding = { top: 20, bottom: 40 };

    // 绘制渐变色条
    const gradient = ctx.createLinearGradient(0, 0, width, 0);
    colorStops.forEach(stop => gradient.addColorStop(stop.position, stop.color));

    ctx.fillStyle = gradient;
    ctx.fillRect(0, padding.top, width, barHeight);

    // 绘制刻度
    ctx.strokeStyle = '#333';
    ctx.lineWidth = 1;
    ctx.font = '12px Arial';
    ctx.textAlign = 'center';

    for (let x = 0; x <= width; x += tickInterval) {
        const yBase = padding.top + barHeight;
        ctx.beginPath();
        ctx.moveTo(x, yBase);
        ctx.lineTo(x, yBase + tickLength);
        ctx.stroke();
        ctx.fillText(x, x, yBase + tickLength + 15);
    }

    // 绘制分段标签
    ctx.font = 'bold 14px Arial';
    colorStops.forEach(stop => {
        ctx.fillText(stop.label, stop.position * width, padding.top - 5);
    });
}
export function getAlarmLevel(value) {
    let result = '';
    switch (value) {
        case '0':
            result = '正常运行';
            break;
        case '1':
            result = '蓝色预警';
            break;
        case '2':
            result = '黄色预警';
            break;
        case '3':
            result = '橙色预警';
            break;
        case '4':
            result = '红色预警';
            break;
    }
    return result;
}
export function Legend(color, {
    title,
    tickSize = 6,
    width = 320,
    height = 44 + tickSize,
    marginTop = 18,
    marginRight = 0,
    marginBottom = 16 + tickSize,
    marginLeft = 0,
    ticks = width / 64,
    tickFormat,
    tickValues
} = {}) {

    function ramp(color, n = 256) {
        const canvas = document.createElement("canvas");
        canvas.width = n;
        canvas.height = 1;
        const context = canvas.getContext("2d");
        for (let i = 0; i < n; ++i) {
            context.fillStyle = color(i / (n - 1));
            context.fillRect(i, 0, 1, 1);
        }
        return canvas;
    }

    const svg = d3.create("svg")
        .attr("width", width)
        .attr("height", height)
        .attr("viewBox", [0, 0, width, height])
        .style("overflow", "visible")
        .style("display", "block");

    let tickAdjust = g => g.selectAll(".tick line").attr("y1", marginTop + marginBottom - height);
    let x;

    // Continuous
    if (color.interpolate) {
        const n = Math.min(color.domain().length, color.range().length);

        x = color.copy().rangeRound(d3.quantize(d3.interpolate(marginLeft, width - marginRight), n));

        svg.append("image")
            .attr("x", marginLeft)
            .attr("y", marginTop)
            .attr("width", width - marginLeft - marginRight)
            .attr("height", height - marginTop - marginBottom)
            .attr("preserveAspectRatio", "none")
            .attr("xlink:href", ramp(color.copy().domain(d3.quantize(d3.interpolate(0, 1), n))).toDataURL());
    }

    // Sequential
    else if (color.interpolator) {
        x = Object.assign(color.copy()
            .interpolator(d3.interpolateRound(marginLeft, width - marginRight)),
            { range() { return [marginLeft, width - marginRight]; } });

        svg.append("image")
            .attr("x", marginLeft)
            .attr("y", marginTop)
            .attr("width", width - marginLeft - marginRight)
            .attr("height", height - marginTop - marginBottom)
            .attr("preserveAspectRatio", "none")
            .attr("xlink:href", ramp(color.interpolator()).toDataURL());

        // scaleSequentialQuantile doesn’t implement ticks or tickFormat.
        if (!x.ticks) {
            if (tickValues === undefined) {
                const n = Math.round(ticks + 1);
                tickValues = d3.range(n).map(i => d3.quantile(color.domain(), i / (n - 1)));
            }
            if (typeof tickFormat !== "function") {
                tickFormat = d3.format(tickFormat === undefined ? ",f" : tickFormat);
            }
        }
    }

    // Threshold
    else if (color.invertExtent) {
        const thresholds
            = color.thresholds ? color.thresholds() // scaleQuantize
                : color.quantiles ? color.quantiles() // scaleQuantile
                    : color.domain(); // scaleThreshold

        const thresholdFormat
            = tickFormat === undefined ? d => d
                : typeof tickFormat === "string" ? d3.format(tickFormat)
                    : tickFormat;

        x = d3.scaleLinear()
            .domain([-1, color.range().length - 1])
            .rangeRound([marginLeft, width - marginRight]);

        svg.append("g")
            .selectAll("rect")
            .data(color.range())
            .join("rect")
            .attr("x", (d, i) => x(i - 1))
            .attr("y", marginTop)
            .attr("width", (d, i) => x(i) - x(i - 1))
            .attr("height", height - marginTop - marginBottom)
            .attr("fill", d => d);

        tickValues = d3.range(thresholds.length);
        tickFormat = i => thresholdFormat(thresholds[i], i);
    }

    // Ordinal
    else {
        x = d3.scaleBand()
            .domain(color.domain())
            .rangeRound([marginLeft, width - marginRight]);

        svg.append("g")
            .selectAll("rect")
            .data(color.domain())
            .join("rect")
            .attr("x", x)
            .attr("y", marginTop)
            .attr("width", Math.max(0, x.bandwidth() - 1))
            .attr("height", height - marginTop - marginBottom)
            .attr("fill", color);

        tickAdjust = () => { };
    }

    svg.append("g")
        .attr("transform", `translate(0,${height - marginBottom})`)
        .call(d3.axisBottom(x)
            .ticks(ticks, typeof tickFormat === "string" ? tickFormat : undefined)
            .tickFormat(typeof tickFormat === "function" ? tickFormat : undefined)
            .tickSize(tickSize)
            .tickValues(tickValues))
        .call(tickAdjust)
        .call(g => g.select(".domain").remove())
        .call(g => g.append("text")
            .attr("x", marginLeft)
            .attr("y", marginTop + marginBottom - height - 6)
            .attr("fill", "currentColor")
            .attr("text-anchor", "start")
            .attr("font-weight", "bold")
            .attr("class", "title")
            .text(title));

    return svg.node();
}
export const parseRaraType = (value) => {
    switch (value) {
        case '01':
        case "61":
            return '复散射';
        case '10':
            return '连续形变';
        case '04':
            return '连续形变速度';
        case '05':
            return '区间形变差值';
        case '00':
            return '原始形变';
        case '02':
            return '置信度';
        default:
            return value;
    }
}
export const parseRaraStatus = (value) => {
    switch (value) {
        case 'success':
            return '成功';
        case 'fail':
            return '失败';
        case 'skip':
            return '跳过';
        case 'unstart':
            return '未开始';
    }
}
export function getRadarImgTimeUnit(value) {
    let result = '';
    switch (value) {
        case '00':
            result = '默认';
            break;
        case '04':
            result = '4小时';
            break;
        case '05':
            result = '24小时';
            break;
        default:
            result = value;
    }
    return result;
}
export function url2blob(url) {
    return new Promise(resolve => {
        const xhr = new XMLHttpRequest();
        xhr.open('get', url, true);
        xhr.responseType = 'blob';
        xhr.onload = function (e) {
            if (this.status === 200) {
                const blob = this.response;
                const image = new Image();
                image.src = URL.createObjectURL(blob);
                image.onload = function () {
                    URL.revokeObjectURL(image.src);
                    resolve(image);
                }
            }
        }
        xhr.send();
    })
}

/**
 * 围绕中心点旋转矩形坐标
 * @param {Array} rectBounds - 原始矩形边界 [west, south, east, north]
 * @param {Array} centerPoint - 旋转中心点 [lon, lat]
 * @param {Number} angleRad - 旋转角度（弧度）
 * @returns {Array} 旋转后的矩形边界 [west, south, east, north]
 */
export function rotateRectangleAroundPoint(rectBounds, centerPoint, angleRad) {
    if (angleRad === 0) {
        return rectBounds;
    }

    const [west, south, east, north] = rectBounds;
    const [centerLon, centerLat] = centerPoint;

    // 计算矩形的四个角点
    const corners = [
        [west, south],
        [east, south],
        [east, north],
        [west, north]
    ];

    // 旋转每个角点
    const rotatedCorners = corners.map(([lon, lat]) => {
        // 计算相对于中心点的偏移（使用近似的平面坐标系统）
        const deltaLon = lon - centerLon;
        const deltaLat = lat - centerLat;

        // 应用旋转矩阵
        const cosAngle = Math.cos(angleRad);
        const sinAngle = Math.sin(angleRad);

        const rotatedDeltaLon = deltaLon * cosAngle - deltaLat * sinAngle;
        const rotatedDeltaLat = deltaLon * sinAngle + deltaLat * cosAngle;

        // 计算旋转后的绝对坐标
        return [
            centerLon + rotatedDeltaLon,
            centerLat + rotatedDeltaLat
        ];
    });

    // 找到新的边界框
    const lons = rotatedCorners.map(c => c[0]);
    const lats = rotatedCorners.map(c => c[1]);

    return [
        Math.min(...lons),  // west
        Math.min(...lats),  // south
        Math.max(...lons),  // east
        Math.max(...lats)   // north
    ];
}
export function addRadarLayer(viewer, url, count, alpha = 1, rotation = 0, radarCenter = null) {
    return new Promise(resolve => {
        const store = useMapStore();
        for (let i = 0; i < count; i++) {
            axios.get(url + i + '.json')
                .then(res => {
                    url2blob(url + i + '_1024.png').then(image => {
                        const originalPosition = res.data; // [west, south, east, north]

                        const entity = new Entity({
                            rectangle: {
                                // 如果提供了雷达中心点，则动态计算旋转后的矩形位置
                                coordinates: radarCenter && radarCenter.length >= 2
                                    ? new CallbackProperty(function () {
                                        const angleRad = store.radarImageRotation || rotation;
                                        if (angleRad === 0) {
                                            return Rectangle.fromDegrees(
                                                originalPosition[0], originalPosition[1],
                                                originalPosition[2], originalPosition[3]
                                            );
                                        }

                                        // 计算矩形中心点
                                        const centerLon = (originalPosition[0] + originalPosition[2]) / 2;
                                        const centerLat = (originalPosition[1] + originalPosition[3]) / 2;

                                        // 计算矩形中心相对于雷达中心的偏移
                                        const deltaLon = centerLon - radarCenter[0];
                                        const deltaLat = centerLat - radarCenter[1];

                                        // 应用旋转矩阵，计算旋转后的中心位置
                                        const cosAngle = Math.cos(angleRad);
                                        const sinAngle = Math.sin(angleRad);
                                        const rotatedDeltaLon = deltaLon * cosAngle - deltaLat * sinAngle;
                                        const rotatedDeltaLat = deltaLon * sinAngle + deltaLat * cosAngle;

                                        // 计算新的矩形中心
                                        const newCenterLon = radarCenter[0] + rotatedDeltaLon;
                                        const newCenterLat = radarCenter[1] + rotatedDeltaLat;

                                        // 计算矩形的半宽和半高
                                        const halfWidth = (originalPosition[2] - originalPosition[0]) / 2;
                                        const halfHeight = (originalPosition[3] - originalPosition[1]) / 2;

                                        // 返回新的矩形边界
                                        return Rectangle.fromDegrees(
                                            newCenterLon - halfWidth,
                                            newCenterLat - halfHeight,
                                            newCenterLon + halfWidth,
                                            newCenterLat + halfHeight
                                        );
                                    }, false)
                                    : Rectangle.fromDegrees(
                                        originalPosition[0], originalPosition[1],
                                        originalPosition[2], originalPosition[3]
                                    ),
                                material: new ImageMaterialProperty({
                                    image: image,
                                    transparent: true,
                                    color: Color.WHITE.withAlpha(alpha)
                                }),
                                // 使用 rotation 属性实现弧度旋转（旋转矩形本身）
                                rotation: new CallbackProperty(function () {
                                    return store.radarImageRotation || rotation;
                                }, false),
                                // 使用 stRotation 属性实现纹理旋转（旋转图像纹理）
                                stRotation: new CallbackProperty(function () {
                                    return store.radarImageRotation || rotation;
                                }, false)
                            }
                        });
                        viewer.entities.add(entity);
                        store.radarImageEntityIds.push(entity.id)
                        if (i === count - 1) {
                            resolve(200);
                        }
                    })
                })
        }
    })
}
export const getRadarType = (arr) => {
    if (arr.length === 0) return '';
    let str = '';
    for (let i = 0; i < arr.length; i++) {
        switch (arr[i]) {
            case '复散射':
                str += '01,';
                break;
            case '连续形变':
                str += '10,';
                break;
            case '连续形变速度':
                str += '04,';
                break;
            case '区间形变差值':
                str += '05,'
                break;
        }
    }
    return str.substring(0, str.length - 1);
}
export const getRadarStatus = (arr) => {
    if (arr.length === 0) return '';
    let str = '';
    for (let i = 0; i < arr.length; i++) {
        switch (arr[i]) {
            case '成功':
                str += 'success,';
                break;
            case '失败':
                str += 'fail,'
                break;
            case '跳过':
                str += 'skip,';
                break;
            case '未开始':
                str += 'unstart,';
                break;
        }
    }
    return str.substring(0, str.length - 1);
}
export const projectDataInit = () => {
    const store = useMapStore();
    ApiRadar.getRadarData().then(res => {
        // ✅ 映射后端字段到前端期望的格式（同时保留原始字段名和映射字段名）
        const projects = res.data.data || [];
        store.projectInfo.projectData = projects.map(p => ({
            // 保留原始字段名（用于新组件）
            projectId: p.projectId,
            projectName: p.projectName,
            // 映射字段名（用于旧组件兼容）
            id: p.projectId,
            name: p.projectName,
            description: p.description,
            contact: p.contactPerson,
            phone: p.contactPhone,
            email: p.contactEmail,
            contactPerson: p.contactPerson,
            contactPhone: p.contactPhone,
            contactEmail: p.contactEmail,
            longitude: p.longitude,
            latitude: p.latitude,
            elevation: p.elevation,
            devices: (p.devices || []).map(d => {
                // ✅ 映射设备类型代码到类型字符串
                let deviceTypeStr = 'ER'; // 默认为边坡雷达
                if (typeof d.deviceType === 'string') {
                    deviceTypeStr = d.deviceType;
                } else {
                    // 根据设备类型代码映射
                    switch(d.deviceType) {
                        case 0: deviceTypeStr = 'ER'; break;        // 边坡雷达
                        case 4: deviceTypeStr = 'ER'; break;        // 建筑物雷达
                        case 5: deviceTypeStr = 'ER'; break;        // 边坡雷达Mini
                        case 6: deviceTypeStr = 'ER'; break;        // 建筑物雷达2D
                        case 7: deviceTypeStr = 'MIMOLITE'; break;  // MIMO雷达
                        case 8: deviceTypeStr = 'MIMOLITE'; break;  // 普适雷达
                        default: deviceTypeStr = 'ER'; break;
                    }
                }
                
                // ✅ 确保params有默认值
                const params = d.params || {};
                if (!params.radarOri && d.orientation !== undefined) params.radarOri = d.orientation || 0;
                if (!params.ImgAngleStart) params.ImgAngleStart = 0;
                if (!params.ImgAngleEnd) params.ImgAngleEnd = 360;
                if (!params.RngMax) params.RngMax = 1000;
                if (!params.RngMin) params.RngMin = 0;
                if (!params.AnteBeam_half) params.AnteBeam_half = 10;
                if (!params.FreqBand) params.FreqBand = 0;
                
                return {
                    // 保留原始字段名
                    deviceId: d.deviceId,
                    deviceName: d.deviceName,
                    // 映射字段名（兼容旧代码）
                    id: d.deviceId,
                    name: d.deviceName,
                    type: deviceTypeStr,
                    status: d.status,
                    coordinates: [d.longitude || 0, d.latitude || 0, d.elevation || 0],
                    // ✅ 添加独立的地理位置字段
                    longitude: d.longitude || 0,
                    latitude: d.latitude || 0,
                    elevation: d.elevation || 0,
                    // ✅ 添加雷达特有字段
                    slaveId: d.slaveId || '',
                    orientation: d.orientation || 0,
                    ipAddress: d.ipAddress,
                    port: d.port,
                    params: params,
                    dataVersion: d.dataVersion || '0',
                    algorithmParam: d.algorithmParam || {}
                };
            })
        }));
        staticDataBind();
    });
}

window.tool = {
    allow: true,
    visible: false,
    showShield() {
        const store = useMapStore();
        for (let i = 0; i < store.shieldEntities.length; i++) {
            const entity = CesiumUtils.FindEntityById(store.shieldEntities[i]);
            entity.show = !entity.show;
        }
    },
    deleteShield(name) {
        const store = useMapStore();
        for (let i = 0; i < store.shieldEntities.length; i++) {
            const entity = CesiumUtils.FindEntityById(store.shieldEntities[i]);
            if (entity.label.text._value === name) {
                ApiRadar.deleteMonitor(entity.properties.geoId._value, store.radarInfo.projectId).then(() => {
                    CommonUtils.ShowMessage("删除成功", 'success');
                    CesiumUtils.EntityRemove(entity);
                })
            }
        }
    }
}