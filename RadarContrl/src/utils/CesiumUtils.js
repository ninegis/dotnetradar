import {
    Ion,
    Viewer,
    Terrain,
    Camera,
    Rectangle,
    Cartesian3,
    HorizontalOrigin,
    VerticalOrigin,
    HeightReference,
    Color,
    Cartesian2,
    LabelStyle,
    Math as CesiumMath,
    Cesium3DTileset,
    DistanceDisplayCondition,
    EasingFunction,
    Transforms,
    Matrix4,
    Cartographic,
    Matrix3,
    PolygonHierarchy,
    Entity,
    ScreenSpaceEventHandler,
    ScreenSpaceEventType,
    CallbackProperty,
    EllipsoidTerrainProvider,
    Cesium3DTileFeature,
    Model,
    ArcGisMapServerImageryProvider,
    UrlTemplateImageryProvider,
    WebMercatorTilingScheme,
    BingMapsImageryProvider,
    BingMapsStyle,
    WebMapTileServiceImageryProvider,
    MapboxStyleImageryProvider,
    SingleTileImageryProvider,
    GeoJsonDataSource,
    WebMapServiceImageryProvider,
    KmlDataSource, WebMercatorProjection
} from "cesium";
import {point, center, featureCollection, distance} from '@turf/turf';
import {c32lonlat} from "@/utils/tools.js";
import {TurfUtils} from "@/utils/TurfUtils.js";

export class CesiumUtils{
    static viewer = null;
    static CameraFlyToPostion(longitude,latitude,altitude,heading,pitch,roll){
        this.viewer.camera.flyTo({
            destination : Cartesian3.fromDegrees(parseFloat(longitude),parseFloat(latitude),parseFloat(altitude)),
            orientation : {
                heading : CesiumMath.toRadians(parseFloat(heading)),
                pitch : CesiumMath.toRadians(parseFloat(pitch)),
                roll : CesiumMath.toRadians(parseFloat(roll))
            }
        });
    }
    static Cartesian2ToCartesian3(cartesian2){
        return new Promise(resolve => {
            let earthPosition;
            const pick = this.viewer.scene.drillPick(cartesian2)[0];
            if (pick===undefined){
                //     拾取为空 不是模型，地形和地球二选一
                if (this.viewer.terrainProvider instanceof EllipsoidTerrainProvider){
                    //         地球
                    earthPosition = this.viewer.camera.pickEllipsoid(cartesian2,this.viewer.scene.globe.ellipsoid);
                }else{
                    //         地形
                    earthPosition = this.viewer.scene.globe.pick(this.viewer.camera.getPickRay(cartesian2),this.viewer.scene);
                }
            }else{
                //拾取存在 不是地球，模型和地形二选一
                if (pick && pick.primitive instanceof Cesium3DTileFeature
                    || pick && pick.primitive instanceof Cesium3DTileset
                    || pick && pick.primitive instanceof Model
                    // || pick && pick.primitive instanceof GroundPolylinePrimitive
                ){
                    //模型
                    this.viewer.scene.pick(cartesian2);
                    earthPosition  = this.viewer.scene.pickPosition(cartesian2);
                }else{
                    //地形
                    earthPosition = this.viewer.scene.globe.pick(this.viewer.camera.getPickRay(cartesian2),this.viewer.scene);
                }
            }
            resolve(earthPosition);
        })
    }
    static Cartesian3ToLonlatalt(cartesian3){
        let xyz_Carto = this.viewer.scene.globe.ellipsoid.cartesianToCartographic(cartesian3);
        let xyz_lat = CesiumMath.toDegrees(xyz_Carto.latitude);
        let xyz_lng = CesiumMath.toDegrees(xyz_Carto.longitude);
        return [xyz_lng,xyz_lat,xyz_Carto.height];
    }
    static CesiumInit(){
        return new Promise(resolve => {
            // window.CESIUM_BASE_URL = './node_modules/cesium/Build/Cesium/';
            // window.CESIUM_BASE_URL = './assets/Cesium/';
            Camera.DEFAULT_VIEW_RECTANGLE = Rectangle.fromDegrees(90, -20, 110, 90);//缩放至大陆地球
            Ion.defaultAccessToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiI4YzQ3NDAyOS0wNWNkLTRkOTItOTkzNS0yNGEzMTVjOGI2ZjEiLCJpZCI6MzA3MDg2LCJpYXQiOjE3NDg0MjU5ODR9.SUMizikOCj3aEzADHtjCAvJcT5LMNiDElx9G8bUntS0';
            //地球初始化
            const viewer =new Viewer('cesiumContainer',{
                terrain: Terrain.fromWorldTerrain(),
                animation: false,
                timeline: false,
                navigationHelpButton:false,
                fullscreenButton:false,
                infoBox:false,
                imageryProviderViewModels:[],
                baseLayerPicker:false,
                homeButton:false,
                geocoder:false,
                sceneModePicker:false,
                selectionIndicator:false,
            })
            viewer.cesiumWidget.creditContainer.style.display = 'none';
            //关闭地形深度检测
            viewer.scene.globe.depthTestAgainstTerrain = false;
            viewer.cesiumWidget.screenSpaceEventHandler.removeInputAction(
                ScreenSpaceEventType.LEFT_DOUBLE_CLICK
            );//移除双击后trackEntity被锁定的功能
            document.oncontextmenu = new Function("event.returnValue=false");//去除默认的右击事件
            this.viewer = viewer;
            resolve(viewer);
        })
    }
    static ComputeCenterByPoints(points){
        return new Promise(resolve => {
            const arr = [];
            for (let i = 0; i < points.length; i++) {
                arr.push(point([points[i][0],points[i][1]]));
            }
            resolve(center(featureCollection(arr)));
        })
    }
    static GetPointByProjection(lon, lat, height, direction, radius) {
        // 观察点
        let cartesian = Cartesian3.fromDegrees(lon, lat, height)
        // 世界坐标转为投影坐标
        let webMercatorProjection = new WebMercatorProjection(
            this.viewer.scene.globe.ellipsoid
        )
        let viewPointWebMercator = webMercatorProjection.project(
            Cartographic.fromCartesian(cartesian)
        )
        // 计算目标点
        let toPoint = new Cartesian3(
            viewPointWebMercator.x + radius * Math.cos(direction),
            viewPointWebMercator.y + radius * Math.sin(direction),
            height
        )
        // 投影坐标转为世界坐标
        let cartographic = webMercatorProjection.unproject(toPoint)
        let point = [
            CesiumMath.toDegrees(cartographic.longitude),
            CesiumMath.toDegrees(cartographic.latitude),
        ]
        return point
    }
    static GenerateHierarchy(lon, lat, height, d1, d2, radius) {
        let list = [Number(lon), Number(lat), Number(height)]
        //获取 航偏角d1 至 航偏角d2 弧段的点位信息
        for (let i = d1; i < d2; i += 1) {
            let point = this.GetPointByProjection(lon, lat, height, (90 - i) * (Math.PI / 180), radius)
            list.push(Number(point[0]))
            list.push(Number(point[1]))
            list.push(height)
        }
        list.push(Number(lon))
        list.push(Number(lat))
        list.push(Number(height))
        return Cartesian3.fromDegreesArrayHeights(list)
    }
    static DrawSector(params){
        /**
         * @description 画扇形（从正北开始顺时针旋转）
         * @param {Number} d1 扇形开始角度
         * @param {Number} d2 扇形结束角度
         * @param {Color} color 扇形颜色
         * @param {Number} radius 扇形半径
         * @param {Float} lat 经度
         * @param {Float} lon 纬度
         * @param {Float} height 高度
         */
        return new Promise(resolve => {
            let { d1,d2, color, radius, lon,lat,height } = params;
            resolve(this.viewer.entities.add({
                polygon: {
                    show: true,
                    hierarchy: this.GenerateHierarchy(lon, lat, height, d1, d2, radius),
                    material: color,
                    zIndex: Math.floor(1000 / radius)
                }
            }))
        })
    }
    static DrawPolygon(drawTooltipText,polylineMaterial=Color.AQUA){
        return new Promise(resolve => {
            let positions = [],points=[], polygon = new PolygonHierarchy(),point=new Cartesian3(), _polygonEntity = new Entity(), polyObj = null;
            const handle = new ScreenSpaceEventHandler(this.viewer.scene.canvas);
            const that = this;
            handle.setInputAction(function (e){
                that.Cartesian2ToCartesian3(e.position).then(cartesian3=>{
                    if (positions.length === 0) {
                        polygon.positions.push(cartesian3.clone())
                        positions.push(cartesian3.clone());
                    }
                    polygon.positions.push(cartesian3.clone())
                    positions.push(cartesian3.clone());
                    points.push(that.Cartesian3ToLonlatalt(cartesian3.clone()));
                })
            },ScreenSpaceEventType.LEFT_CLICK);
            handle.setInputAction(function (e){
                that.Cartesian2ToCartesian3(e.endPosition).then(cartesian3=>point = cartesian3);
                that.Cartesian2ToCartesian3(e.endPosition).then(cartesian3=> {
                    if (positions.length >= 2) {
                        if (cartesian3 && cartesian3.x) {
                            positions.pop()
                            positions.push(cartesian3);
                            polygon.positions.pop()
                            polygon.positions.push(cartesian3);
                        }
                    }
                });
            },ScreenSpaceEventType.MOUSE_MOVE);
            handle.setInputAction(function (e){
                positions.pop();
                positions.push(positions[0]);
                handle.removeInputAction(ScreenSpaceEventType.MOUSE_MOVE);
                handle.removeInputAction(ScreenSpaceEventType.LEFT_CLICK);
                handle.removeInputAction(ScreenSpaceEventType.RIGHT_CLICK);
                polyObj.label = undefined;
                polyObj.point = undefined;
                polyObj.position = undefined;
                setTimeout(()=>{
                    resolve([points,polyObj]);
                },150)
            },ScreenSpaceEventType.RIGHT_CLICK);
            const create=()=>{
                _polygonEntity.polyline = {
                    width: 3
                    , material: polylineMaterial
                    , clampToGround: true
                }
                _polygonEntity.polyline.positions = new CallbackProperty(function () {
                    return positions
                }, false);
                _polygonEntity.polygon = {
                    hierarchy: new CallbackProperty(function () {
                        return polygon
                    }, false),
                    material: Color.WHITE.withAlpha(0.3)
                };
                _polygonEntity.position = new CallbackProperty(()=>{
                    return point;
                },false);
                _polygonEntity.label = {
                    text:drawTooltipText,
                    font:'18px 微软雅黑',
                    fillColor:Color.WHEAT,
                    backgroundColor:new Color(0, 0, 0, 0.5),
                    pixelOffset: new Cartesian2(0, -38), //偏移量
                    showBackground:true,
                    style:LabelStyle.FILL,
                    verticalOrigin:VerticalOrigin.TOP,
                    horizontalOrigin:HorizontalOrigin.CENTER,
                    heightReference:HeightReference.NONE,
                    disableDepthTestDistance: Number.POSITIVE_INFINITY
                };
                _polygonEntity.point = {
                    color: Color.WHITE,
                    pixelSize: 10,
                    heightReference: HeightReference.NONE,
                    disableDepthTestDistance: Number.POSITIVE_INFINITY
                };
                polyObj = this.viewer.entities.add(_polygonEntity);
            }
            create();
        })
    }
    static DrawPoint(drawTooltipText,billboardImage){
        return new Promise(resolve => {
            const handle = new ScreenSpaceEventHandler(this.viewer.scene.canvas);
            const that = this;
            let point = new Cartesian3(),entity = new Entity();
            handle.setInputAction(function(event){
                that.Cartesian2ToCartesian3(event.position).then(cartesian3=>{
                    entity.label.text = undefined;
                    entity.position = cartesian3.clone();
                    handle.removeInputAction(ScreenSpaceEventType.LEFT_CLICK);
                    setTimeout(()=>{
                        resolve([that.Cartesian3ToLonlatalt(cartesian3.clone()),entity]);
                    },150)
                })
            },ScreenSpaceEventType.LEFT_CLICK);
            handle.setInputAction(function (e){
                that.Cartesian2ToCartesian3(e.endPosition).then(cartesian3=>point = cartesian3);
            },ScreenSpaceEventType.MOUSE_MOVE);

            entity.position = new CallbackProperty(()=>{
                return point;
            },false);
            entity.billboard = {
                image:billboardImage,
                horizontalOrigin:HorizontalOrigin.CENTER,
                verticalOrigin:VerticalOrigin.BOTTOM,
                heightReference:HeightReference.NONE,
                disableDepthTestDistance: Number.POSITIVE_INFINITY
            };
            entity.label = {
                text:drawTooltipText,
                font:'18px 微软雅黑',
                fillColor:Color.WHEAT,
                backgroundColor:new Color(0, 0, 0, 0.5),
                pixelOffset: new Cartesian2(0, -38), //偏移量
                showBackground:true,
                style:LabelStyle.FILL,
                verticalOrigin:VerticalOrigin.TOP,
                horizontalOrigin:HorizontalOrigin.CENTER,
                heightReference:HeightReference.NONE,
                disableDepthTestDistance: Number.POSITIVE_INFINITY
            };
            that.viewer.entities.add(entity);
        })
    }
    static EntityPointAdd(positionLon,positionLat,positionAlt,billboardImage,labelText='',labelPixelOffset=-58,billboardHF=HeightReference.NONE,properties={},customProps = {}){
        return new Promise(resolve => {
            const entity = this.viewer.entities.add({
                position:Cartesian3.fromDegrees(parseFloat(positionLon),parseFloat(positionLat),parseFloat(positionAlt)),
                billboard:{
                    image:billboardImage,
                    horizontalOrigin:HorizontalOrigin.CENTER,
                    verticalOrigin:VerticalOrigin.BOTTOM,
                    heightReference:billboardHF,
                    disableDepthTestDistance: Number.POSITIVE_INFINITY
                },
                label:{
                    text:labelText,
                    font:'18px 微软雅黑',
                    fillColor:Color.WHITE,
                    backgroundColor:new Color(0, 0, 0, 0.5),
                    pixelOffset: new Cartesian2(0, labelPixelOffset), //偏移量
                    showBackground:true,
                    style:LabelStyle.FILL,
                    verticalOrigin:VerticalOrigin.TOP,
                    horizontalOrigin:HorizontalOrigin.CENTER,
                    heightReference:billboardHF,
                    disableDepthTestDistance: Number.POSITIVE_INFINITY
                },
                properties,
                customprops:customProps
            });
            resolve(entity);
        })
    }
    static EntityPolygonAdd(points,labelText,polygonMaterial=Color.MEDIUMSPRINGGREEN.withAlpha(0.2),labelHeightReference = HeightReference.NONE){
        return new Promise(resolve => {
            this.ComputeCenterByPoints(points).then(center=>{
                const entity = this.viewer.entities.add({
                    position:Cartesian3.fromDegrees(center.geometry.coordinates[0],center.geometry.coordinates[1],points[0][2]+10),
                    polygon:{
                        hierarchy: Cartesian3.fromDegreesArrayHeights(points.flat()),
                        material:polygonMaterial,
                        distanceDisplayCondition:new DistanceDisplayCondition(0.0, 5000.0),
                    },
                    label:{
                        text:labelText,
                        font:'18px 微软雅黑',
                        fillColor:Color.WHITE,
                        backgroundColor:new Color(0, 0, 0, 0.5),
                        pixelOffset: new Cartesian2(0, 0), //偏移量
                        showBackground:true,
                        style:LabelStyle.FILL,
                        verticalOrigin:VerticalOrigin.TOP,
                        horizontalOrigin:HorizontalOrigin.CENTER,
                        // distanceDisplayCondition:new DistanceDisplayCondition(0.0, 5000.0),
                        heightReference:labelHeightReference,
                        disableDepthTestDistance: Number.POSITIVE_INFINITY
                    }
                })
                resolve(entity);
            })
        })
    }
    static EntityRemove(entity){
        this.viewer.entities.remove(entity);
    }
    static EntityRemoveAll(){
        this.viewer.entities.removeAll();
    }
    static EntityRemoveById(id){
        this.viewer.entities.removeById(id);
    }
    static FindEntityById(id){
        try {
            return this.viewer.entities.getById(id);
        }catch (ex){
            return null;
        }
    }
    static GetCameraParams(){
        const coordinates = this.Cartesian3ToLonlatalt(Cartesian3.clone(this.viewer.camera.positionWC));
        return {
            longitude:coordinates[0],
            latitude:coordinates[1],
            altitude:this.viewer.camera.positionCartographic.height,
            heading:CesiumMath.toDegrees(this.viewer.scene.camera.heading),
            pitch:CesiumMath.toDegrees(this.viewer.scene.camera.pitch),
            roll:CesiumMath.toDegrees(this.viewer.scene.camera.roll)
        }
    }
    static LayerGeoJsonAdd(url){
        return new Promise(resolve => {
            const dataSource = GeoJsonDataSource.load(
                url, {
                    stroke: Color.HOTPINK,
                    fill: Color.PINK,
                    strokeWidth: 3,
                    markerSymbol: '?'
                });
            this.viewer.dataSources.add(dataSource);
            resolve(dataSource);
        })
    }
    static LayerImageryArcGISSatelliteAdd(url='https://services.arcgisonline.com/arcgis/rest/services/World_Imagery/MapServer') {
        return new Promise(async resolve => {
            const imagerylayer = await ArcGisMapServerImageryProvider.fromUrl(url);
            this.viewer.imageryLayers.addImageryProvider(imagerylayer);
            resolve(imagerylayer);
        })
    }
    static LayerImageryBingSatelliteAdd(){
        return new Promise(async resolve => {
            const imagerylayer = await BingMapsImageryProvider.fromUrl(
                "https://dev.virtualearth.net", {
                    key: "Aqv18akwoLQNGJbvlLae7yyg3bFvG0lWzKO1DASX5mtHwGV4xrJmSaXjzHUfGwIi",
                    mapStyle: BingMapsStyle.AERIAL
                });
            this.viewer.imageryLayers.addImageryProvider(imagerylayer);
            resolve(imagerylayer);
        })
    }
    static LayerImageryGoogleSatelliteAdd(){
        return new Promise(resolve => {
            const imagerylayer=new UrlTemplateImageryProvider({
                url:'https://gac-geo.googlecnapps.cn/maps/vt?lyrs=s&x={x}&y={y}&z={z}',
                tilingScheme:new WebMercatorTilingScheme(),
                minimumLevel:1,
                maximumLevel:20
            });
            this.viewer.imageryLayers.addImageryProvider(imagerylayer);
            resolve(imagerylayer);
        })
    }
    static LayerImageryMapBoxDarkAdd(){
        return new Promise(resolve => {
            const mapxtoken = 'pk.eyJ1IjoiamlhcWluZ3FpYW5nIiwiYSI6ImNsZnJ2NmtocDAwMmszdHFrdGdhdTAwM2YifQ.j4Sc00mVuv4tyQHc4Udq1w';
            let options = {
                url: "https://api.mapbox.com/styles/v1/",
                styleId: 'clfrzqwyb002m01mh5fd8nov0',//'streets-v11'
                accessToken: mapxtoken,
                username: "jiaqingqiang",
                credit: "mapboxAPI"
            };
            const mapboxlayer = new MapboxStyleImageryProvider(options);
            this.viewer.imageryLayers.addImageryProvider(mapboxlayer);
            resolve(mapboxlayer);
        })
    }
    static LayerImageryMapBoxGrayAdd(){
        return new Promise(resolve => {
            const mapxtoken = 'pk.eyJ1IjoiamlhcWluZ3FpYW5nIiwiYSI6ImNsZnJ2NmtocDAwMmszdHFrdGdhdTAwM2YifQ.j4Sc00mVuv4tyQHc4Udq1w';
            let options = {
                url: "https://api.mapbox.com/styles/v1/",
                styleId: 'clfrzuqg6000c01tcwft2w380',//'streets-v11'
                accessToken: mapxtoken,
                username: "jiaqingqiang",
                credit: "mapboxAPI"
            };
            const mapboxlayer = new MapboxStyleImageryProvider(options);
            this.viewer.imageryLayers.addImageryProvider(mapboxlayer);
            resolve(mapboxlayer);
        })
    }
    static LayerImageryMapBoxStreetAdd(){
        return new Promise(resolve => {
            const mapxtoken = 'pk.eyJ1IjoiamlhcWluZ3FpYW5nIiwiYSI6ImNsZnJ2NmtocDAwMmszdHFrdGdhdTAwM2YifQ.j4Sc00mVuv4tyQHc4Udq1w';
            const options = {
                url: "https://api.mapbox.com/styles/v1/",
                styleId: 'clfrwbkvy006x01t6fvnnwd43',//'streets-v11'
                accessToken: mapxtoken,
                username: "jiaqingqiang",
                credit: "mapboxAPI"
            };
            const mapboxlayer = new MapboxStyleImageryProvider(options);
            this.viewer.imageryLayers.addImageryProvider(mapboxlayer);
            resolve(mapboxlayer);
        })

    }
    static LayerImageryOSMAdd(){
        return new Promise(resolve => {
            const layer = new UrlTemplateImageryProvider({
                url: 'https://tile-{s}.openstreetmap.fr/hot/{z}/{x}/{y}.png',
                subdomains: ["a", "b", "c", "d"],
            });
            this.viewer.imageryLayers.addImageryProvider(layer);
            resolve(layer);
        })
    }
    static LayerImageryPhotoAdd(url){
        return new Promise(resolve => {
            const rectangle=Rectangle.fromDegrees(-180,-90,180,90);
            const imageryLayer = new SingleTileImageryProvider({
                url,rectangle,
                tileWidth:256,
                tileHeight:256
            });
            this.viewer.imageryLayers.addImageryProvider(imageryLayer);
            resolve(imageryLayer);
        })
    }
    static LayerImageryWMSAdd(url,layers){
        return new Promise(resolve => {
            const imageryLayer = new WebMapServiceImageryProvider({
                url:url,
                layers:layers,
                parameters: {
                    service: 'WMS',
                    format: 'image/png',
                    transparent: true
                }
            });
            this.viewer.imageryLayers.addImageryProvider(imageryLayer);
            resolve(imageryLayer);
        })
    }
    static LayerImageryWMTSAdd(url,layer){
        return new Promise(resolve => {
            const imageryLayer = new WebMapTileServiceImageryProvider({
                url:url,
                layer:layer,
                style : 'default',
                format : 'image/jpeg',
                tileMatrixSetID : 'GoogleMapsCompatible',
                maximumLevel: 19,
            });
            this.viewer.imageryLayers.addImageryProvider(imageryLayer);
            resolve(imageryLayer);
        })
    }
    static LayerKMLAdd(url){
        return new Promise(resolve => {
            const dataSource = KmlDataSource.load(url,
                {
                    camera: this.viewer.scene.camera,
                    canvas: this.viewer.scene.canvas
                });
            this.viewer.dataSources.add(dataSource);
            resolve(dataSource);
        })

    }
    static async LayerPrimitive3dtileAdd(url) {
        const tileset = await Cesium3DTileset.fromUrl(url, {
                skipLevelOfDetail: false,
                loadSiblings: true,
                cullRequestsWhileMovingMultiplier: 10,
                dynamicScreenSpaceErrorDensity: 0.1,
                dynamicScreenSpaceError: true,
                preferLeaves: false,
            }
        );
        this.viewer.scene.primitives.add(tileset);
        return tileset;
    }
    static LayerPrimitiveRemove(primitive){
        this.viewer.scene.primitives.remove(primitive);
    }
    static LayerPrimitiveRemoveAll(){
        this.viewer.scene.primitives.removeAll();
    }
    static LayerImageryTDTSatelliteAdd(){
        return new Promise(resolve => {
            const tianditutk = 'a1bbc51b8f9cf61c028f4e4d9548648a';
            const imgLayer =  new WebMapTileServiceImageryProvider({
                url: "http://t{s}.tianditu.com/img_w/wmts?service=wmts&request=GetTile&version=1.0.0&LAYER=img&tileMatrixSet=w&TileMatrix={TileMatrix}&TileRow={TileRow}&TileCol={TileCol}&style=default.jpg&tk="+tianditutk,
                subdomains: ['0','1','2','3','4','5','6','7'],
                layer: "tdtImgLayer",
                style: "default",
                format: "image/jpeg",
                tileMatrixSetID: "GoogleMapsCompatible",
                show: true
            });
            const ciaLayer = new WebMapTileServiceImageryProvider({
                url: "http://t{s}.tianditu.com/cia_w/wmts?service=wmts&request=GetTile&version=1.0.0&LAYER=cia&tileMatrixSet=w&TileMatrix={TileMatrix}&TileRow={TileRow}&TileCol={TileCol}&style=default.jpg&tk="+tianditutk,
                subdomains: ['0','1','2','3','4','5','6','7'],
                layer: "tdtCiaLayer",
                style: "default",
                format: "image/jpeg",
                tileMatrixSetID: "GoogleMapsCompatible",
                show: true
            });
            this.viewer.imageryLayers.addImageryProvider(imgLayer);
            this.viewer.imageryLayers.addImageryProvider(ciaLayer);
            resolve([imgLayer,ciaLayer]);
        })
    }
    static LayerImageryTDTTerrainAdd(){
        return new Promise(resolve => {
            const tianditutk = 'a1bbc51b8f9cf61c028f4e4d9548648a';
            const ctaLayer = new WebMapTileServiceImageryProvider({
                url: "http://t{s}.tianditu.com/cta_w/wmts?service=wmts&request=GetTile&version=1.0.0&LAYER=cta&tileMatrixSet=w&TileMatrix={TileMatrix}&TileRow={TileRow}&TileCol={TileCol}&style=default.jpg&tk="+tianditutk,
                subdomains: ['0','1','2','3','4','5','6','7'],
                layer: "tdtCtaLayer",
                style: "default",
                format: "image/jpeg",
                tileMatrixSetID: "GoogleMapsCompatible",
                show: true
            });
            const terrainLayer = new WebMapTileServiceImageryProvider({
                url: "http://t{s}.tianditu.com/ter_w/wmts?service=wmts&request=GetTile&version=1.0.0&LAYER=ter&tileMatrixSet=w&TileMatrix={TileMatrix}&TileRow={TileRow}&TileCol={TileCol}&style=default.jpg&tk="+tianditutk,
                subdomains: ['0','1','2','3','4','5','6','7'],
                layer: "tdtTerLayer",
                style: "default",
                format: "image/jpeg",
                tileMatrixSetID: "GoogleMapsCompatible",
                show: true
            });
            this.viewer.imageryLayers.addImageryProvider(ctaLayer);
            this.viewer.imageryLayers.addImageryProvider(terrainLayer);
            resolve([terrainLayer,ctaLayer]);
        })
    }
    static LayerImageryTDTVectorAdd(){
        return new Promise(resolve => {
            const tianditutk = 'a1bbc51b8f9cf61c028f4e4d9548648a';
            const vectorLayer = new WebMapTileServiceImageryProvider({
                url: "http://t{s}.tianditu.com/vec_w/wmts?service=wmts&request=GetTile&version=1.0.0&LAYER=vec&tileMatrixSet=w&TileMatrix={TileMatrix}&TileRow={TileRow}&TileCol={TileCol}&style=default.jpg&tk="+tianditutk,
                subdomains: ['0','1','2','3','4','5','6','7'],
                layer: "tdtVecLayer",
                style: "default",
                format: "image/jpeg",
                tileMatrixSetID: "GoogleMapsCompatible",
                show: true
            });
            const cvaLayer = new WebMapTileServiceImageryProvider({
                url: "http://t{s}.tianditu.com/cva_w/wmts?service=wmts&request=GetTile&version=1.0.0&LAYER=cva&tileMatrixSet=w&TileMatrix={TileMatrix}&TileRow={TileRow}&TileCol={TileCol}&style=default.jpg&tk="+tianditutk,
                subdomains: ['0','1','2','3','4','5','6','7'],
                layer: "tdtCvaLayer",
                style: "default",
                format: "image/jpeg",
                tileMatrixSetID: "GoogleMapsCompatible",
                show: true
            });
            this.viewer.imageryLayers.addImageryProvider(vectorLayer);
            this.viewer.imageryLayers.addImageryProvider(cvaLayer);
            resolve([vectorLayer,cvaLayer]);
        })
    }
    static LayerImageryTMSAdd(url){
        return new Promise(resolve => {
            const imageryLayer = new UrlTemplateImageryProvider({
                url: url,
            });
            this.viewer.imageryLayers.addImageryProvider(imageryLayer);
            resolve(imageryLayer);
        })
    }
    static LayerImageryRemoveAll(){
        this.viewer.imageryLayers.removeAll();
    }
    static MeasureArea(){
        return new Promise(resolve => {
            let positions = [], polygon = new PolygonHierarchy(), _polygonEntity = new Entity(), polyObj = null,entityArr = [];
            const handler = new ScreenSpaceEventHandler(this.viewer.scene.canvas);
            handler.setInputAction(function (e){
                CesiumUtils.Cartesian2ToCartesian3(e.position).then(cartesian3=> {
                    if (positions.length === 0) {
                        polygon.positions.push(cartesian3.clone())
                        positions.push(cartesian3.clone());
                    }
                    polygon.positions.push(cartesian3.clone())
                    positions.push(cartesian3.clone());
                    if (!polyObj) create();
                });
            },ScreenSpaceEventType.LEFT_CLICK);
            handler.setInputAction(function (e){
                if (positions.length >= 2) {
                    CesiumUtils.Cartesian2ToCartesian3(e.endPosition).then(cartesian3=>{
                        if (cartesian3 && cartesian3.x) {
                            positions.pop()
                            positions.push(cartesian3);
                            polygon.positions.pop()
                            polygon.positions.push(cartesian3);
                        }
                    })
                }
            },ScreenSpaceEventType.MOUSE_MOVE);
            handler.setInputAction(function (e){
                positions.pop();
                positions.push(positions[0]);
                const arr = [];
                for (let i = 0; i < positions.length; i++) {
                    arr.push(CesiumUtils.Cartesian3ToLonlatalt(positions[i]));
                }
                const area = TurfUtils.ComputeAreaByPoints(arr);
                const position = TurfUtils.ComputeCenterByPoints(arr);
                const entity = CesiumUtils.viewer.entities.add({
                    position:Cartesian3.fromDegrees(position.geometry.coordinates[0],position.geometry.coordinates[1],arr[0][2]+100),
                    label:{
                        text:'面积为:【'+area+'】',
                        font:'18px 微软雅黑',
                        fillColor:Color.WHITE,
                        backgroundColor:new Color(0, 0, 0, 0.5),
                        pixelOffset: new Cartesian2(0, -58), //偏移量
                        showBackground:true,
                        style:LabelStyle.FILL,
                        verticalOrigin:VerticalOrigin.TOP,
                        horizontalOrigin:HorizontalOrigin.CENTER,
                    }
                });
                entityArr.push(entity.id);
                resolve(entityArr);
                handler.removeInputAction(ScreenSpaceEventType.MOUSE_MOVE);
                handler.removeInputAction(ScreenSpaceEventType.LEFT_CLICK);
                handler.removeInputAction(ScreenSpaceEventType.RIGHT_CLICK);
            },ScreenSpaceEventType.RIGHT_CLICK);
            const create=()=>{
                _polygonEntity.polyline = {
                    width: 3
                    , material: Color.AQUA
                    , clampToGround: true
                }
                _polygonEntity.polyline.positions = new CallbackProperty(function () {
                    return positions
                }, false)
                _polygonEntity.polygon = {
                    hierarchy: new CallbackProperty(function () {
                        return polygon
                    }, false),
                    material: Color.WHITE.withAlpha(0.4)
                    , clampToGround: true
                }
                polyObj = this.viewer.entities.add(_polygonEntity);
                entityArr.push(polyObj.id);
            }
        })
    }
    static MeasureCoordinate(){
        return new Promise(resolve => {
            const entityArr = [];
            const handler = new ScreenSpaceEventHandler(this.viewer.scene.canvas);
            handler.setInputAction((e)=>{
                this.Cartesian2ToCartesian3( e.position).then(cartesian=>{
                    const coordinate = CesiumUtils.Cartesian3ToLonlatalt(cartesian);
                    const entityObj = CesiumUtils.viewer.entities.add({
                        position:cartesian,
                        point:{
                            color:Color.WHITE,
                            pixelSize:5,
                        },
                        label:{
                            text:'经度：'+coordinate[0].toFixed(6)+'\n纬度：'+coordinate[1].toFixed(6)+'\n高度：'+coordinate[2].toFixed(6),
                            pixelOffset: new Cartesian2(0, -56), //偏移量
                            font:'18px 微软雅黑'
                        }
                    })
                    entityArr.push(entityObj.id);
                })
            },ScreenSpaceEventType.LEFT_CLICK);
            handler.setInputAction((e)=>{
                handler.removeInputAction(ScreenSpaceEventType.LEFT_CLICK);
                handler.removeInputAction(ScreenSpaceEventType.RIGHT_CLICK);
                resolve(entityArr);
            },ScreenSpaceEventType.RIGHT_CLICK);
        })
    }
    static MeasureLineDistance(){
        return new Promise(resolve => {
            let entityObj = null,positions = [],linepositions = [],entityArr = [];
            const handler = new ScreenSpaceEventHandler(this.viewer.scene.canvas);
            handler.setInputAction((e)=>{
                this.Cartesian2ToCartesian3(e.position).then(cartesian=>{
                    if (positions.length===0)positions.push(cartesian.clone());
                    positions.push(cartesian.clone());
                    linepositions.push(cartesian.clone());
                    if (linepositions.length>=2){
                        const lastpoint = linepositions[linepositions.length - 2];
                        const currentpoint =  linepositions[linepositions.length - 1];
                        const pt = CesiumUtils.Cartesian3ToLonlatalt(currentpoint);
                        const currentdistance = (distance(CesiumUtils.Cartesian3ToLonlatalt(lastpoint), pt)*1000).toFixed(2);
                        const labelEntity = this.viewer.entities.add({
                            position:Cartesian3.fromDegrees(pt[0],pt[1],pt[2]+50),
                            label:{
                                text:'距离：'+currentdistance+'米',
                                pixelOffset: new Cartesian2(0, -20),
                                font:'18px 微软雅黑',
                                horizontalOrigin: HorizontalOrigin.LEFT,
                                verticalOrigin: VerticalOrigin.TOP,
                            }
                        })
                        entityArr.push(labelEntity.id);
                    }
                })
                if (!entityObj){
                    entityObj = this.viewer.entities.add({
                        polyline:{
                            width:3,
                            material:Color.AQUA,
                            positions:new CallbackProperty(()=>{
                                return positions
                            },false)
                        }
                    })
                    entityArr.push(entityObj.id);
                }
            },ScreenSpaceEventType.LEFT_CLICK);
            handler.setInputAction(function (e){
                if (positions.length >= 2) {
                    CesiumUtils.Cartesian2ToCartesian3(e.endPosition).then(cartesian3=>{
                        if (cartesian3 && cartesian3.x) {
                            positions.pop();
                            positions.push(cartesian3);
                        }
                    })
                }
            },ScreenSpaceEventType.MOUSE_MOVE);
            handler.setInputAction(function (e){
                positions.pop();
                handler.removeInputAction(ScreenSpaceEventType.MOUSE_MOVE);
                handler.removeInputAction(ScreenSpaceEventType.LEFT_CLICK);
                resolve(entityArr);
            },ScreenSpaceEventType.RIGHT_CLICK);
        })
    }
    static ZoomToEntity(entity){
        this.viewer.zoomTo(entity);
    }
    static ZoomToPointSoftly(longitude,latitude,altitude){
        const positionCartographic = new Cartographic(
            longitude,
            latitude,
            altitude * 0.5
        );
        const position = this.viewer.scene.globe.ellipsoid.cartographicToCartesian(positionCartographic);

        const camera = this.viewer.scene.camera;
        const heading = camera.heading;
        const pitch = camera.pitch;

        const offset = offsetFromHeadingPitchRange(
            heading,
            pitch,
            altitude * 2.0
        );

        const transform = Transforms.eastNorthUpToFixedFrame(position);
        Matrix4.multiplyByPoint(transform, offset, position);

        camera.flyTo({
            destination: position,
            orientation: {
                heading: heading,
                pitch: pitch,
            },
            easingFunction: EasingFunction.QUADRATIC_OUT,
        });
        function offsetFromHeadingPitchRange(heading, pitch, range) {
            pitch = CesiumMath.clamp(
                pitch,
                -CesiumMath.PI_OVER_TWO,
                CesiumMath.PI_OVER_TWO
            );
            heading = CesiumMath.zeroToTwoPi(heading) - CesiumMath.PI_OVER_TWO;

            const pitchQuat = Cesium.Quaternion.fromAxisAngle(
                Cartesian3.UNIT_Y,
                -pitch
            );
            const headingQuat = Cesium.Quaternion.fromAxisAngle(
                Cartesian3.UNIT_Z,
                -heading
            );
            const rotQuat = Cesium.Quaternion.multiply(
                headingQuat,
                pitchQuat,
                headingQuat
            );
            const rotMatrix = Matrix3.fromQuaternion(rotQuat);
            const offset = Cartesian3.clone(Cartesian3.UNIT_X);
            Matrix3.multiplyByVector(rotMatrix, offset, offset);
            Cartesian3.negate(offset, offset);
            Cartesian3.multiplyByScalar(offset, range, offset);
            return offset;
        }
    }
}