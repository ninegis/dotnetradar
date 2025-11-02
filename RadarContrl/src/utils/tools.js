import worldimg from '@/assets/world.jpg'
import {h} from "vue";
import {ElMessage, ElNotification} from "element-plus";
import {useMapStore} from "@/store/index.js";
import pt from '@/assets/entities/pt.png';
import {destination, distance, point, rhumbBearing,center,featureCollection,polygon,area} from "@turf/turf";
import * as Cesium from 'cesium';
import {Point,MyLocation,MeasureCoordinate} from "@/assets/load.js";
import axios from 'axios';
import {Base64} from "js-base64";
import * as d3 from "d3";

let trackdentityCollection = [];
let trackDataSource = null;
export function getIndexOfArray(primarykey,value,array){
    for (let i=0;i<array.length;i++){
        if (array[i][primarykey]===value){
            return i;
        }
    }
    return -1;
}
function getHeadingPitchRollByMatrix(matrix, ellipsoid, fixedFrameTransform, result) {
    return Cesium.Transforms.fixedFrameToHeadingPitchRoll(
        matrix,
        ellipsoid,
        fixedFrameTransform,
        result
    );
}
function getHeadingPitchRollByOrientation(position, orientation, ellipsoid, fixedFrameTransform) {
    if (!Cesium.defined(orientation) || !Cesium.defined(position))
        return new Cesium.HeadingPitchRoll();
    const matrix = Cesium.Matrix4.fromRotationTranslation(
        Cesium.Matrix3.fromQuaternion(orientation, new Cesium.Matrix3()),
        position,
        new Cesium.Matrix4()
    );
    return getHeadingPitchRollByMatrix(matrix, ellipsoid, fixedFrameTransform);
}
function getHeadingPitchRoll(entity, time) {
    time = time || currentTime();
    const position = entity.position.getValue(time,new Cesium.Cartesian3())
    const orientation = entity.orientation.getValue(time,new Cesium.Quaternion());
    return getHeadingPitchRollByOrientation(position, orientation);
}

export function ZoomToSamplePositionProperty(viewer,entity){
    let time = viewer.clock.currentTime;
    let position = entity.position.getValue(time,new Cesium.Cartesian3());
    let hpr = getHeadingPitchRoll(entity, time);
    let heading = Cesium.Math.toDegrees(hpr.heading) + 90;
    const newhdr = new Cesium.HeadingPitchRoll(Cesium.Math.toRadians(heading), Cesium.Math.toRadians(-50), hpr.roll);
    viewer.camera.flyToBoundingSphere(new Cesium.BoundingSphere(position, 1000),{
        offset:newhdr
    });
}
export const showMessage=(message,type='success',duration=3000)=>{
    ElMessage({
        message: message,
        type: type,
        duration:duration
    });
}
export function FormatJsonToLayerTreeData(originData,pk,pfk,title){
    let treeData = [];
    let data = originData[Object.keys(originData)[0]];
    let map = {};
    data.forEach(function(item){
        item['title'] = item[title];
        item['key'] = item[pk];
        map[item[pk]] = item;
    })
    data.forEach(function(item){
        let parent = map[item[pfk]];
        if (parent){
            (parent.children||(parent.children = [])).push(item);
        }else{
            treeData.push(item);
        }
    })
    return treeData;
}
export function FormatJsonToTreeData(data){
    let treeData = [];
    let map = {};
    data.forEach(function(item){
        item['title'] = item['name'];
        item['key'] = item['oid'];
        item['imei'] = item['DeviceSN'];
        map[item['oid']] = item;
    })
    data.forEach(function(item){
        let parent = map[item.pfk];
        if (parent){
            (parent.children||(parent.children = [])).push(item);
        }else{
            treeData.push(item);
        }
    })
    return treeData;
}
export function CustomFormatDate(date,Format='yyyy-MM-dd'){
    return date.getFullYear()+'-'+
        (date.getMonth()>9?date.getMonth()+1:'0'+(date.getMonth()+1))+'-'+
        (date.getDate()>9?date.getDate():'0'+date.getDate());
}
export function FormatDate(date){
    return date.getFullYear()+'-'+
        (date.getMonth()>9?date.getMonth()+1:'0'+(date.getMonth()+1))+'-'+
        (date.getDate()>9?date.getDate():'0'+date.getDate())+' '+
        (date.getHours()>9?date.getHours():'0'+date.getHours())+':'+
        (date.getMinutes()>9?date.getMinutes():'0'+date.getMinutes())+':'+
        (date.getSeconds()>9?date.getSeconds():'0'+date.getSeconds());
}

export function foreachTree (viewer,tree,add=true) {
    tree.forEach(item => {
        if (!add&&item.DeviceSN){
            const store = useMapStore();
            let id = getIndexOfArray('id',item.DeviceSN,store.trackEntityCollection);
            store.deleteDevice(item.DeviceSN);
            store.trackEntityCollection.splice(id,1);
        }else{
            const store = useMapStore();
            store.webSocket.send('{\'cmd\':1005,\'tid\':\''+item.DeviceSN+'\'}');
            // addTrackSingleEntity(viewer,item);
        }
        if (item.children) {
            foreachTree(viewer,item.children,add)
        }
    })
}

export function addTrackSingleEntity(viewer,data){
    if (data['latitude']===''||data['longitude']===''||data['imei']===''){
        return;
    }
    const store = useMapStore();
    let entity = viewer.entities.add({
        id:data['imei'],
        position:Cesium.Cartesian3.fromDegrees(parseFloat(data['longitude']),parseFloat(data['latitude']),0),
        billboard:{
            image:pt,
            horizontalOrigin:Cesium.HorizontalOrigin.CENTER,
            verticalOrigin:Cesium.VerticalOrigin.BOTTOM
        },
        properties:{
            speed:data['speed'],
            gpstime:data['gpstime'],
            carstatus:data['carstatus'],
            onlinestatus:data['onlinestatus'],
            alarmstatus:data['alarmstatus'],
            address:data['address']
        }
    });
    store.treeEntityNum++;
    store.trackEntityCollection.push(entity);
    return entity;
}
export function DateTimeToStr(date){
    return date.getFullYear()+'-'+
        (date.getMonth()>9?date.getMonth()+1:'0'+(date.getMonth()+1))+'-'+
        (date.getDate()>9?date.getDate():'0'+date.getDate())+' '+
        (date.getHours()>9?date.getHours():'0'+date.getHours())+':'+
        (date.getMinutes()>9?date.getMinutes():'0'+date.getMinutes())+':'+
        (date.getSeconds()>9?date.getSeconds():'0'+date.getSeconds());
}
export const parseBase64=(value)=>{
    if (value===undefined||value===''){
        return '无';
    }
    if(!isNaN(Number(value,10))){
        if (value>1000000000){
            return DateTimeToStr(new Date(value));
        }
        return value;
    }
    if (!isBase64(value))return value;
    return Base64.decode(value);
}
function isBase64(str) {
    if (str ==='' || str.trim() ===''){ return false; }
    try {
        return btoa(atob(str)) == str;
    } catch (err) {
        return false;
    }
}
export function addTrackEntity(viewer,data){
    let arr = [];
    viewer.clock.shouldAnimate = false;
    for (let i=0;i<data.length;i++){
        arr.push(viewer.entities.add({
            id:data[i].car_id,
            position:Cesium.Cartesian3.fromDegrees(data[i].lng,data[i].lat,0),
            billboard:{
                image:pt,
                horizontalOrigin:Cesium.HorizontalOrigin.CENTER,
                verticalOrigin:Cesium.VerticalOrigin.BOTTOM,
                heightReference:Cesium.HeightReference.CLAMP_TO_GROUND
            },
            properties:{
                speed:data[i]['speed'],
                gpstime:DateTimeToStr(new Date(data[i]['gpstime'])),
                carstatus:(data[i]['status_str']),
                onlinestatus:'离线',
                alarmstatus:(data[i]['alarm_str']),
                address:(data[i]['address'])
            }
        }));
    }
    viewer.zoomTo(viewer.entities);
    return arr;
}

export function getGPXDataSource(){
    return trackDataSource;
}

export function addDynamicGPXTrack(viewer,data){
    viewer.dataSources.remove(trackDataSource);
    let parser = new DOMParser();
    return viewer.dataSources.add(
        Cesium.GpxDataSource.load(
            parser.parseFromString(data,"application/xml"),{
                clampToGround: true,
                trackColor:Cesium.Color.AQUA
            }
        )
    ).then(function (dataSource) {
        trackDataSource = dataSource;
        dataSource.clustering.show = false;
        viewer.flyTo(dataSource.entities);
        return viewer.clock.multiplier;
    });
}

export function addDynamicTrack(viewer,data,carid){
    trackdentityCollection.forEach(item=>{
        viewer.entities.remove(item);
    })
    if (data.length === 0){
        return;
    }
    let start;
    let stop;
    let property = new Cesium.SampledPositionProperty();
    let lastPostion = null;
    let lastTime = null;

    for (let i = 0, len = data.length; i < len; i++) {
        let item = data[i];
        let lng = Number(item.lng.toFixed(6));
        let lat = Number(item.lat.toFixed(6));
        let time = item.gpstime;
        let speed = item.speed;
        let mileage = item.mileage;

        let position = null;
        if (lng && lat) position = Cesium.Cartesian3.fromDegrees(lng, lat, 0);
        let juliaDate = null;
        if (time)
            juliaDate = Cesium.JulianDate.fromIso8601(time);
        console.log(time);
        console.log(juliaDate);
        if (position && juliaDate)
            property.addSample(juliaDate, position);

        if (i === 0)
            start = juliaDate;
        else if (i === len - 1)
            stop = juliaDate;
        let color = Cesium.Color.BLUE;
        let speedText = "";
        if (speed < 60) {
            color = Cesium.Color.BLUE;
        } else if (speed >= 60 && speed < 70) {
            color = Cesium.Color.GREEN;
        } else if (speed >= 70 && speed < 80) {
            color = Cesium.Color.YELLOW;
        } else if (speed >= 80) {
            color = Cesium.Color.RED;
        }
        speedText = "速度:" + speed.toFixed(2) + "km/h";
        trackdentityCollection.push(viewer.entities.add({
            show: true,
            position: position,
            popup: "时间:" + time + '<br/>' + speedText+'<br/>里程:'+mileage+'km',
            point: {
                pixelSize: 5,
                color: color,
            }
        }));

        lastPostion = position;
        lastTime = new Date(time);
    }

    viewer.clock.startTime = start.clone();
    viewer.clock.stopTime = stop.clone();
    viewer.clock.currentTime = start.clone();
    viewer.clock.multiplier = 5;

    if (viewer.timeline)
        viewer.timeline.zoomTo(start, stop);

    let entity = viewer.entities.add({
        availability: new Cesium.TimeIntervalCollection([new Cesium.TimeInterval({
            start: start,
            stop: stop
        })]),
        position: property,
        orientation: new Cesium.VelocityOrientationProperty(property),
        label: {
            text: carid,
            font: "normal small-caps normal 19px 楷体",
            style: Cesium.LabelStyle.FILL_AND_OUTLINE,
            fillColor: Cesium.Color.AZURE,
            outlineColor: Cesium.Color.BLACK,
            outlineWidth: 2,
            horizontalOrigin: Cesium.HorizontalOrigin.CENTER,
            verticalOrigin: Cesium.VerticalOrigin.BOTTOM,
            pixelOffset: new Cesium.Cartesian2(10, -25), //偏移量
        },
        model: {
            uri: 'src/assets/dynamictrackicon/wajueji.glb',
            scale: 1,
            minimumPixelSize: 20
        },
        path: {
            resolution: 1,
            leadTime: 0,
            trailTime: 3600,
            material: Cesium.Color.FORESTGREEN,
            width: 3
        }
    });
    trackdentityCollection.push(entity);
    return entity;
}
export const addPolygonEntity=async (viewer,boundary,material=Cesium.Color.MEDIUMSPRINGGREEN.withAlpha(0.5))=>{
    let position = [];
    for (let i = 0; i < boundary.length; i++) {
        position.push(boundary[i][0],boundary[i][1]);
    }
    const entity = viewer.entities.add({
        polygon:{
            hierarchy: Cesium.Cartesian3.fromDegreesArray(position),
            material:material,
            heightReference:Cesium.HeightReference.CLAMP_TO_GROUND,
            height:0
        }
    })
    return(entity);
}
export const getIP=()=>{
    return new Promise(resolve => {
        delete axios.defaults.headers['Authorization'];
        axios.get('https://forge.speedtest.cn/api/location/info').then(res=>{
            resolve(res.data);
        })
    })
}
export const addEntityPoint=async (viewer,coordinate)=>{
    return viewer.entities.add({
        position:Cesium.Cartesian3.fromDegrees(coordinate.lon,coordinate.lat),
        billboard:{
            image:MyLocation
        },
    })
}
export const zoomToIP=(viewer)=>{
    const store = useMapStore();
    getIP().then(res=>{
        addEntityPoint(viewer,{lon:parseFloat(res.lon),lat:parseFloat(res.lat)}).then(entity=>{
            viewer.zoomTo(entity);
            setTimeout(()=>{
                viewer.entities.remove(entity);
            },3000);
        })
    })
}
export function c2ToXYZ(viewer,e){
    return new Promise(resolve => {
        let xyz_Carto=null,earthPosition=null;
        earthPosition = c2ToC3(viewer,e.position);
        xyz_Carto= Cesium.Cartographic.fromCartesian(earthPosition);
        const xyz_lat = Cesium.Math.toDegrees(xyz_Carto.latitude);
        const xyz_lng = Cesium.Math.toDegrees(xyz_Carto.longitude);
        resolve({longitude:xyz_lng,latitude:xyz_lat,height:xyz_Carto.height});
    })
}
export const getParentKey = (key, tree) => {
    let parentKey;
    for (let i = 0; i < tree.length; i++) {
        const node = tree[i];
        if (node.children) {
            if (node.children.some(item => item.key === key)) {
                parentKey = node.key;
            } else if (getParentKey(key, node.children)) {
                parentKey = getParentKey(key, node.children);
            }
        }
    }
    return parentKey;
};
export function c32lonlat(viewer,c3,needHeight=false){
    let xyz_Carto = viewer.scene.globe.ellipsoid.cartesianToCartographic(c3);
    let xyz_lat = Cesium.Math.toDegrees(xyz_Carto.latitude);
    let xyz_lng = Cesium.Math.toDegrees(xyz_Carto.longitude);
    return needHeight?[xyz_lng,xyz_lat,xyz_Carto.height]:[xyz_lng,xyz_lat];
}
export const getZoomLevel=(viewer)=>{
    return viewer.scene.globe._surface._tilesToRender[0].level;
}
export const getViewExtent=(viewer)=>{
    const coordinates = c32lonlat(viewer,viewer.camera.position);
    return {
        x:coordinates[0],
        y:coordinates[1],
        z:viewer.camera.positionCartographic.height,
        zoom:getZoomLevel(viewer),
        heading:viewer.camera.heading,
        pitch:viewer.camera.pitch,
        roll:viewer.camera.roll,
        type:'cesium'
    }
}
export const restoreView=(viewer,data)=>{
    if (data.type==='cesium'){
        viewer.camera.flyTo({
            destination:Cesium.Cartesian3.fromDegrees(data.x,data.y,data.z),
            orientation:{
                heading:data.heading,
                pitch:data.pitch,
                roll:data.roll
            }
        })
    }else if (data.type==='arcgis2d'){
        viewer.camera.flyTo({
            destination:Cesium.Cartesian3.fromDegrees(data.x,data.y,zoomToAltitude(data.zoom)),
        })
    }else if (data.type==='arcgis3d'){
        viewer.camera.flyTo({
            destination:Cesium.Cartesian3.fromDegrees(data.x,data.y,data.z),
            orientation:{
                heading:Cesium.Math.toRadians(data.heading),
                pitch:Cesium.Math.toRadians(-30),
                roll:Cesium.Math.toRadians(data.tilt-58)
            }
        })
    }
}
export const zoomToAltitude=(zoom)=> {
    var A = 40487.57;
    var B = 0.00007096758;
    var C = 91610.74;
    var D = -40467.74;
    let alt = Math.pow((A - D) / (zoom - D) - 1, 1.0 / B) * C;
    return alt;
}
export const drawCircle=(viewer)=>{
    return new Promise(resolve => {
        const handler = new Cesium.ScreenSpaceEventHandler(viewer.scene.canvas);
        let center = [],nextpoint = [],entity = null;
        handler.setInputAction(function(event){
            const cartesian = c2ToC3(viewer,event.position);
            if (center.length===0){
                center = c32lonlat(viewer,cartesian);
                entity = viewer.entities.add({
                    position:cartesian,
                    ellipse: {
                        semiMajorAxis: new Cesium.CallbackProperty(() => {
                            if(nextpoint.length===2){
                                return distance(center,nextpoint )*1000;
                            }else{
                                return 0;
                            }
                        }, false),
                        semiMinorAxis: new Cesium.CallbackProperty(() => {
                            if(nextpoint.length===2){
                                return distance(center,nextpoint )*1000;
                            }else{
                                return 0;
                            }
                        }, false),
                        material: Cesium.Color.AQUA.withAlpha(0.5),
                        outline: true,
                        outlineColor:Cesium.Color.WHEAT.withAlpha(0.5),
                        heightReference:Cesium.HeightReference.CLAMP_TO_GROUND,
                        height:0
                    },
                })
            }
            if (nextpoint.length===2){
                resolve(entity.id);
                handler.removeInputAction(Cesium.ScreenSpaceEventType.LEFT_CLICK);
                handler.removeInputAction(Cesium.ScreenSpaceEventType.MOUSE_MOVE);
            }
        },Cesium.ScreenSpaceEventType.LEFT_CLICK);
        handler.setInputAction(function(event){
            if (center.length===2){
                nextpoint = c32lonlat(viewer,c2ToC3(viewer,event.endPosition));
            }
        },Cesium.ScreenSpaceEventType.MOUSE_MOVE);
    })
}
export const c2ToC3=(viewer,c2)=>{
    let earthPosition;
    if (Cesium.defined(viewer.terrain)){
        earthPosition  = viewer.scene.pickPosition(c2);
    }else{
        earthPosition = viewer.camera.pickEllipsoid(c2,viewer.scene.globe.ellipsoid);
    }
    if (!earthPosition){
        earthPosition = viewer.scene.globe.pick(viewer.camera.getPickRay(c2),viewer.scene);
    }
    return earthPosition;
}
export const drawRectangle=(viewer)=> {
    return new Promise(resolve => {
        let positions = [], polygon = new Cesium.PolygonHierarchy(), _polygonEntity = new Cesium.Entity(), polyObj = null;
        let points = [],coordinates=[],step = 0;
        const handler = new Cesium.ScreenSpaceEventHandler(viewer.scene.canvas);
        handler.setInputAction(function (e){
            let cartesian;
            if (Cesium.defined(viewer.terrain)){
                cartesian = viewer.scene.pickPosition(e.position);
            }else{
                cartesian = viewer.camera.pickEllipsoid(e.position,viewer.scene.globe.ellipsoid);
            }
            if (!Cesium.defined(cartesian)) {
                const ray = viewer.camera.getPickRay(e.position);
                cartesian = viewer.scene.globe.pick(ray, viewer.scene);
            }
            points[step] = cartesian;
            step++;
            if (step === 3) {
                positions.pop();
                positions.push(positions[0]);
                handler.removeInputAction(Cesium.ScreenSpaceEventType.MOUSE_MOVE);
                handler.removeInputAction(Cesium.ScreenSpaceEventType.LEFT_CLICK);
                resolve([coordinates,polyObj]);
            }
            if (positions.length === 0) {
                polygon.positions.push(cartesian.clone())
                positions.push(cartesian.clone());
            }
            polygon.positions.push(cartesian.clone())
            positions.push(cartesian.clone());
            if (!polyObj) create();
        },Cesium.ScreenSpaceEventType.LEFT_CLICK);
        handler.setInputAction(function (e){
            let cartesian;
            if (Cesium.defined(viewer.terrain)){
                cartesian = viewer.scene.pickPosition(e.startPosition);
            }else{
                cartesian = viewer.camera.pickEllipsoid(e.startPosition,viewer.scene.globe.ellipsoid);
            }
            if (!Cesium.defined(cartesian)) {
                const ray = viewer.camera.getPickRay(e.startPosition);
                cartesian = viewer.scene.globe.pick(ray, viewer.scene);
            }
            points[2] = cartesian;
            if (positions.length >= 2) {
                if (cartesian && cartesian.x) {
                    positions.pop()
                    positions.push(cartesian);
                    polygon.positions.pop()
                    polygon.positions.push(cartesian);
                }
            }
        },Cesium.ScreenSpaceEventType.MOUSE_MOVE);
        const create=()=>{
            _polygonEntity.polyline = {
                width: 3
                , material: Cesium.Color.AQUA
                , clampToGround: true
            }
            _polygonEntity.polyline.positions = new Cesium.CallbackProperty(function () {
                if (positions.length>2){
                    return [coordinates[0],coordinates[1],coordinates[2],coordinates[3],coordinates[0],];
                }else{
                    return positions;
                }
            }, false)
            _polygonEntity.polygon = {
                hierarchy: new Cesium.CallbackProperty(function () {
                    if (points[0] && points[1] && points[2]) {
                        const r0 = Cesium.Cartographic.fromCartesian(points[0])
                        const r1 = Cesium.Cartographic.fromCartesian(points[1]) // 辅助点
                        const r2 = Cesium.Cartographic.fromCartesian(points[2])

                        const p0 = point([r0.longitude * 180 / Math.PI, r0.latitude * 180 / Math.PI])
                        const p1 = point([r1.longitude * 180 / Math.PI, r1.latitude * 180 / Math.PI])
                        const p2 = point([r2.longitude * 180 / Math.PI, r2.latitude * 180 / Math.PI])

                        const bearing1 = rhumbBearing(p0, p1)
                        const bearing2 = rhumbBearing(p0, p2)
                        const angle1 = bearing2 - bearing1
                        // 对角长度
                        const length = distance(p0, p2, {units: 'miles'})

                        const len1 = Math.cos(angle1 / 180 * Math.PI) * length
                        const dest1 = destination(p0, len1, bearing1, {units: 'miles'})

                        const angle2 = 90 - angle1
                        const len2 = Math.cos(angle2 / 180 * Math.PI) * length
                        const dest2 = destination(p0, len2, 90 + bearing1, {units: 'miles'})

                        coordinates = [points[0], Cesium.Cartesian3.fromDegrees(...dest1.geometry.coordinates), points[2], Cesium.Cartesian3.fromDegrees(...dest2.geometry.coordinates)]

                        return new Cesium.PolygonHierarchy(coordinates)
                    }
                }, false),
                material: Cesium.Color.WHITE.withAlpha(0.4)
                , clampToGround: true
            }
            polyObj = viewer.entities.add(_polygonEntity);
        }
    })
}
export const drawPoint=(viewer)=>{
    return new Promise(resolve => {
        const handler = new Cesium.ScreenSpaceEventHandler(viewer.scene.canvas);
        handler.setInputAction(function(event){
            let earthPosition;
            if (Cesium.defined(viewer.terrain)){
                earthPosition  = viewer.scene.pickPosition(event.position);
            }else{
                earthPosition = viewer.camera.pickEllipsoid(event.position,viewer.scene.globe.ellipsoid);
            }
            const entity = viewer.entities.add({
                position:earthPosition,
                billboard:{
                    image:Point
                },
            })
            handler.removeInputAction(Cesium.ScreenSpaceEventType.LEFT_CLICK);
            resolve(entity);
        },Cesium.ScreenSpaceEventType.LEFT_CLICK);
    })
}
export const drawPolyline=(viewer)=>{
    return new Promise(resolve => {
        let positions = [], polygon = new Cesium.PolygonHierarchy(), _polygonEntity = new Cesium.Entity(), polyObj = null;
        const handler = new Cesium.ScreenSpaceEventHandler(viewer.scene.canvas);
        handler.setInputAction(function (e){
            c2ToXYZ(viewer,e).then(coordinate=>{
                const cartesian3 = Cesium.Cartesian3.fromDegrees(coordinate.longitude,coordinate.latitude,coordinate.height);
                if (positions.length === 0) {
                    polygon.positions.push(cartesian3.clone())
                    positions.push(cartesian3.clone());
                }
                polygon.positions.push(cartesian3.clone())
                positions.push(cartesian3.clone());
                if (!polyObj) create();
            })
        },Cesium.ScreenSpaceEventType.LEFT_CLICK);
        handler.setInputAction(function (e){
            if (positions.length >= 2) {
                c2ToXYZ(viewer,{position:e.endPosition}).then(coordinate=>{
                    const cartesian3 = Cesium.Cartesian3.fromDegrees(coordinate.longitude,coordinate.latitude,coordinate.height);
                    if (cartesian3 && cartesian3.x) {
                        positions.pop()
                        positions.push(cartesian3);
                        polygon.positions.pop()
                        polygon.positions.push(cartesian3);
                    }
                });
            }
        },Cesium.ScreenSpaceEventType.MOUSE_MOVE);
        handler.setInputAction(function (e){
            handler.removeInputAction(Cesium.ScreenSpaceEventType.MOUSE_MOVE);
            handler.removeInputAction(Cesium.ScreenSpaceEventType.LEFT_CLICK);
            resolve(polyObj);
        },Cesium.ScreenSpaceEventType.RIGHT_CLICK);
        const create=()=>{
            _polygonEntity.polyline = {
                width: 3
                , material: Cesium.Color.AQUA
                , clampToGround: true
            }
            _polygonEntity.polyline.positions = new Cesium.CallbackProperty(function () {
                return positions
            }, false)
            polyObj = viewer.entities.add(_polygonEntity);
        }
    })
}
export const drawMultiPoint=(viewer)=>{
    return new Promise(resolve => {
        let entityCollection = [];
        const handler = new Cesium.ScreenSpaceEventHandler(viewer.scene.canvas);
        handler.setInputAction(function(event){
            let earthPosition;
            if (Cesium.defined(viewer.terrain)){
                earthPosition  = viewer.scene.pickPosition(event.position);
            }else{
                earthPosition = viewer.camera.pickEllipsoid(event.position,viewer.scene.globe.ellipsoid);
            }
            const entity = viewer.entities.add({
                position:earthPosition,
                billboard:{
                    image:Point
                },
            })
            entityCollection.push(entity.id);
        },Cesium.ScreenSpaceEventType.LEFT_CLICK);
        handler.setInputAction((e)=>{
            handler.removeInputAction(Cesium.ScreenSpaceEventType.RIGHT_CLICK);
            handler.removeInputAction(Cesium.ScreenSpaceEventType.LEFT_CLICK);
            resolve(entityCollection);
        },Cesium.ScreenSpaceEventType.RIGHT_CLICK)
    })
}
export function drawPolygon(viewer){
    return new Promise(resolve => {
        let positions = [], polygon = new Cesium.PolygonHierarchy(), _polygonEntity = new Cesium.Entity(), polyObj = null;
        const handler = new Cesium.ScreenSpaceEventHandler(viewer.scene.canvas);
        handler.setInputAction(function (e){
            c2ToXYZ(viewer,e).then(coordinate=>{
                const cartesian3 = Cesium.Cartesian3.fromDegrees(coordinate.longitude,coordinate.latitude,coordinate.height);
                if (positions.length === 0) {
                    polygon.positions.push(cartesian3.clone())
                    positions.push(cartesian3.clone());
                }
                polygon.positions.push(cartesian3.clone())
                positions.push(cartesian3.clone());
                if (!polyObj) create();
            })
        },Cesium.ScreenSpaceEventType.LEFT_CLICK);
        handler.setInputAction(function (e){
            if (positions.length >= 2) {
                c2ToXYZ(viewer,{position:e.endPosition}).then(coordinate=>{
                    const cartesian3 = Cesium.Cartesian3.fromDegrees(coordinate.longitude,coordinate.latitude,coordinate.height);
                    if (cartesian3 && cartesian3.x) {
                        positions.pop()
                        positions.push(cartesian3);
                        polygon.positions.pop()
                        polygon.positions.push(cartesian3);
                    }
                });
            }
        },Cesium.ScreenSpaceEventType.MOUSE_MOVE);
        handler.setInputAction(function (e){
            positions.pop();
            positions.push(positions[0]);
            handler.removeInputAction(Cesium.ScreenSpaceEventType.MOUSE_MOVE);
            handler.removeInputAction(Cesium.ScreenSpaceEventType.LEFT_CLICK);
            resolve([positions,polyObj]);
        },Cesium.ScreenSpaceEventType.RIGHT_CLICK);
        const create=()=>{
            _polygonEntity.polyline = {
                width: 3
                , material: Cesium.Color.AQUA
                , clampToGround: true
            }
            _polygonEntity.polyline.positions = new Cesium.CallbackProperty(function () {
                return positions
            }, false)
            _polygonEntity.polygon = {
                hierarchy: new Cesium.CallbackProperty(function () {
                    return polygon
                }, false),
                material: Cesium.Color.WHITE.withAlpha(0.4)
                , clampToGround: true
            }
            polyObj = viewer.entities.add(_polygonEntity);
        }
    })
}
export function c32lonlataltArray(viewer,positionArr){
    let array = [];
    for (let i = 0; i < positionArr.length; i++) {
        let xyz_Carto = viewer.scene.globe.ellipsoid.cartesianToCartographic(positionArr[i]);
        let xyz_lat = Cesium.Math.toDegrees(xyz_Carto.latitude);
        let xyz_lng = Cesium.Math.toDegrees(xyz_Carto.longitude);
        array.push([xyz_lng,xyz_lat,xyz_Carto.height]);
    }
    return array;
}
export const clearEntityCollection =async (viewer,arr)=>{
    for (let i = 0; i < arr.length; i++) {
        viewer.entities.removeById(arr[i]);
    }
}
export const clearDataSource = async (viewer,arr)=>{
    for (let i = 0; i < arr.length; i++) {
        viewer.dataSources.remove(arr[i]);
    }
}

export function addNineGISGeoJsonLayer(viewer,url){
    let layerWork = new ninegis3d.layer.GeoJsonLayer(viewer, {
        "name": "江苏",
        "url": url,
        "symbol": {
            "styleOptions": {
                "fill": true,
                "randomColor": true,//随机色
                "opacity": 0.3,
                "outline": true,
                "outlineColor": "#FED976",
                "outlineWidth": 3,
                "outlineOpacity": 1,
                // "lineType": "dash", //虚线
                // "dashLength":16,
                "label": { //面中心点，显示文字的配置
                    "text": "{NAME_1}", //对应的属性名称
                    "opacity": 1,
                    "font_size": 40,
                    "color": "#ffffff",

                    "font_family": "楷体",
                    "border": true,
                    "border_color": "#000000",
                    "border_width": 3,

                    "background": false,
                    "background_color": "#000000",
                    "background_opacity": 0.1,

                    "font_weight": "normal",
                    "font_style": "normal",

                    "scaleByDistance": true,
                    "scaleByDistance_far": 20000000,
                    "scaleByDistance_farValue": 0.1,
                    "scaleByDistance_near": 1000,
                    "scaleByDistance_nearValue": 1,

                    "distanceDisplayCondition": false,
                    "distanceDisplayCondition_far": 10000,
                    "distanceDisplayCondition_near": 0
                }
            }
        },
        "popup": "{name}",
        // "tooltip": "{name}",
        "visible": true,
        "flyTo": true
    });
    return layerWork;
}
function offsetFromHeadingPitchRange(heading, pitch, range) {
    pitch = Cesium.Math.clamp(
        pitch,
        -Cesium.Math.PI_OVER_TWO,
        Cesium.Math.PI_OVER_TWO
    );
    heading = Cesium.Math.zeroToTwoPi(heading) - Cesium.Math.PI_OVER_TWO;

    const pitchQuat = Cesium.Quaternion.fromAxisAngle(
        Cesium.Cartesian3.UNIT_Y,
        -pitch
    );
    const headingQuat = Cesium.Quaternion.fromAxisAngle(
        Cesium.Cartesian3.UNIT_Z,
        -heading
    );
    const rotQuat = Cesium.Quaternion.multiply(
        headingQuat,
        pitchQuat,
        headingQuat
    );
    const rotMatrix = Cesium.Matrix3.fromQuaternion(rotQuat);

    const offset = Cesium.Cartesian3.clone(Cesium.Cartesian3.UNIT_X);
    Cesium.Matrix3.multiplyByVector(rotMatrix, offset, offset);
    Cesium.Cartesian3.negate(offset, offset);
    Cesium.Cartesian3.multiplyByScalar(offset, range, offset);
    return offset;
}
export const measureCoordinate=(viewer)=>{
    return new Promise(resolve => {
        let entityArr = [];
        const handler = new Cesium.ScreenSpaceEventHandler(viewer.scene.canvas);
        handler.setInputAction((e)=>{
            const cartesian = c2ToC3(viewer, e.position);
            const coordinate = c32lonlat(viewer,cartesian,true);
            const entityObj = viewer.entities.add({
                position:cartesian,
                billboard:{
                    image:MeasureCoordinate,
                },
                label:{
                    text:'经度：'+coordinate[0].toFixed(4)+'\n纬度：'+coordinate[1].toFixed(4)+'\n高度：'+coordinate[2].toFixed(2),
                    pixelOffset: new Cesium.Cartesian2(0, -56), //偏移量
                    font:'18px 微软雅黑'
                }
            })
            entityArr.push(entityObj.id);
        },Cesium.ScreenSpaceEventType.LEFT_CLICK);
        handler.setInputAction((e)=>{
            handler.removeInputAction(Cesium.ScreenSpaceEventType.LEFT_CLICK);
            handler.removeInputAction(Cesium.ScreenSpaceEventType.RIGHT_CLICK);
            resolve(entityArr);
        },Cesium.ScreenSpaceEventType.RIGHT_CLICK);
    })
}
export const measureDistance=(viewer)=>{
    return new Promise(resolve => {
        let entityObj = null,positions = [],linepositions = [],entityArr = [];
        const handler = new Cesium.ScreenSpaceEventHandler(viewer.scene.canvas);
        handler.setInputAction((e)=>{
            const cartesian = c2ToC3(viewer,e.position);
            if (positions.length===0)positions.push(cartesian.clone());
            positions.push(cartesian.clone());
            linepositions.push(cartesian.clone());
            if (linepositions.length>=2){
                const lastpoint = linepositions[linepositions.length - 2];
                const currentpoint =  linepositions[linepositions.length - 1];
                const currentdistance = (distance(c32lonlat(viewer,lastpoint), c32lonlat(viewer,currentpoint))*1000).toFixed(2);
                const labelEntity = viewer.entities.add({
                    position:currentpoint,
                    billboard:{
                        image:Point
                    },
                    label:{
                        text:'距离：'+currentdistance+'米',
                        pixelOffset: new Cesium.Cartesian2(90, 0), //偏移量
                        font:'18px 微软雅黑'
                    }
                })
                entityArr.push(labelEntity.id);
            }
            if (!entityObj){
                entityObj = viewer.entities.add({
                    polyline:{
                        width:3,
                        material:Cesium.Color.AQUA,
                        clampToGround:true,
                        positions:new Cesium.CallbackProperty(()=>{
                            return positions
                        },false)
                    }
                })
                entityArr.push(entityObj.id);
            }
        },Cesium.ScreenSpaceEventType.LEFT_CLICK);
        handler.setInputAction(function (e){
            if (positions.length >= 2) {
                const cartesian3 = c2ToC3(viewer,e.endPosition);
                if (cartesian3 && cartesian3.x) {
                    positions.pop();
                    positions.push(cartesian3);
                }
            }
        },Cesium.ScreenSpaceEventType.MOUSE_MOVE);
        handler.setInputAction(function (e){
            positions.pop();
            handler.removeInputAction(Cesium.ScreenSpaceEventType.MOUSE_MOVE);
            handler.removeInputAction(Cesium.ScreenSpaceEventType.LEFT_CLICK);
            resolve(entityArr);
        },Cesium.ScreenSpaceEventType.RIGHT_CLICK);
    })
}
export function measureArea(viewer){
    return new Promise(resolve => {
        let positions = [], polygon = new Cesium.PolygonHierarchy(), _polygonEntity = new Cesium.Entity(), polyObj = null,entityArr = [];
        const handler = new Cesium.ScreenSpaceEventHandler(viewer.scene.canvas);
        handler.setInputAction(function (e){
            const cartesian3 = c2ToC3(viewer,e.position)
            if (positions.length === 0) {
                polygon.positions.push(cartesian3.clone())
                positions.push(cartesian3.clone());
            }
            polygon.positions.push(cartesian3.clone())
            positions.push(cartesian3.clone());
            if (!polyObj) create();
        },Cesium.ScreenSpaceEventType.LEFT_CLICK);
        handler.setInputAction(function (e){
            if (positions.length >= 2) {
                const cartesian3 = c2ToC3(viewer,e.endPosition);
                if (cartesian3 && cartesian3.x) {
                    positions.pop()
                    positions.push(cartesian3);
                    polygon.positions.pop()
                    polygon.positions.push(cartesian3);
                }
            }
        },Cesium.ScreenSpaceEventType.MOUSE_MOVE);
        handler.setInputAction(function (e){
            positions.pop();
            positions.push(positions[0]);
            getArea(viewer,positions).then(area=>{
                getCenter(viewer,positions).then(position=>{
                    const entity = viewer.entities.add({
                        position:Cesium.Cartesian3.fromDegrees(position.geometry.coordinates[0],position.geometry.coordinates[1],100),
                        label:{
                            text:'面积为:【'+area+'】',
                            font:'18px 微软雅黑',
                            fillColor:Cesium.Color.WHITE,
                            backgroundColor:new Cesium.Color(0, 0, 0, 0.5),
                            pixelOffset: new Cesium.Cartesian2(0, -58), //偏移量
                            showBackground:true,
                            style:Cesium.LabelStyle.FILL,
                            verticalOrigin:Cesium.VerticalOrigin.TOP,
                            horizontalOrigin:Cesium.HorizontalOrigin.CENTER,
                        }
                    });
                    entityArr.push(entity.id);
                    resolve(entityArr);
                })
            })
            handler.removeInputAction(Cesium.ScreenSpaceEventType.MOUSE_MOVE);
            handler.removeInputAction(Cesium.ScreenSpaceEventType.LEFT_CLICK);
            handler.removeInputAction(Cesium.ScreenSpaceEventType.RIGHT_CLICK);
        },Cesium.ScreenSpaceEventType.RIGHT_CLICK);
        const create=()=>{
            _polygonEntity.polyline = {
                width: 3
                , material: Cesium.Color.AQUA
                , clampToGround: true
            }
            _polygonEntity.polyline.positions = new Cesium.CallbackProperty(function () {
                return positions
            }, false)
            _polygonEntity.polygon = {
                hierarchy: new Cesium.CallbackProperty(function () {
                    return polygon
                }, false),
                material: Cesium.Color.WHITE.withAlpha(0.4)
                , clampToGround: true
            }
            polyObj = viewer.entities.add(_polygonEntity);
            entityArr.push(polyObj.id);
        }
    })
}
export function getArea(viewer,positions){
    return new Promise(resolve => {
        let arr = [];
        for (let i = 0; i < positions.length; i++) {
            arr.push(c32lonlat(viewer,positions[i]));
        }
        let currentpolygon = polygon([arr]);
        let currentarea = area(currentpolygon);
        if (currentarea>=1000000){
            currentarea = (currentarea/1000000).toFixed(4)+'平方公里';
        }else{
            currentarea = currentarea.toFixed(4)+'平方米';
        }
        resolve(currentarea);
    })
}
export function getCenter(viewer,positions){
    return new Promise(resolve => {
        let arr = [];
        for (let i = 0; i < positions.length; i++) {
            if (positions[i].length===3){
                arr.push(point([positions[i][0],positions[i][1]]));
            }else{
                arr.push(point(c32lonlat(viewer,positions[i])));
            }
        }
        resolve(center(featureCollection(arr)));
    })
}
export const FormatTreeToJsonArray = tree => {
    return tree.reduce((prev, item) => {
        const { children = [], ...rest } = item
        return prev.concat( [{...rest}], FormatTreeToJsonArray(children) )
    }, [])
}