<template>
  <div :class="isMobile?'basemaps-container-m':'basemaps-container'">
    <div class="basemaps-list flex">
      <div
          class="basemap-item flex-center flex-column"
          v-for="(item,index) of baseMapsList"
          :key="index"
      >
        <img
            class="basemap-image pointer"
            :class="{'activeBaseMap-image':index===activeBaseMap}"
            :src="item.imgSrc"
            @click="changeBaseMap(index,item.title)"
        />
        <span
            class="basemap-title"
            :class="{'activeBaseMap-title':index===activeBaseMap}"
        >{{item.title}}</span>
      </div>
    </div>
  </div>
</template>

<script>
import {useMapStore} from "@/store/index.js";
import {google_img,esriWorldImagery,bingAerial,tdt_img,tdt_vec,tdt_ter,osm,mapboxStreets,
  bdcdark,bdcgrayscale,mapboxSatellite,offline} from "@/assets/load.js";
import {CesiumUtils} from "@/utils/CesiumUtils.js";
import worldimg from "@/assets/world.jpg";
import {useI18n} from "vue-i18n";

export default {
  name: "BaseLayer",
  data() {
    return {
      activeBaseMap: 2,
      baseMapsList: [],
      store:null
    };
  },
  computed: {
    isMobile() {
      return false
    },
  },
  created() {
    this.store = new useMapStore();
    const { t } = useI18n();
    const allLayers = [
      {
        "pid": 10,
        "name": t('backend.googleSatellite'),
        "icon": google_img,
        "type": "www_google",
        "crs": "wgs84",
        "layer": "img_d",
        "visible": true
      },
      {
        "pid": 10,
        "name": t('backend.arcgisSatellite'),
        "icon": esriWorldImagery,
        "type": "arcgis",
        "url": "https://services.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer",
        "enablePickFeatures": false
      },
      {
        "pid": 10,
        "name": t('backend.bingSatellite'),
        "icon": bingAerial,
        "type": "www_bing",
        "layer": "Aerial"
      },
      {
        "pid": 10,
        "name": t('backend.tdtSatellite'),
        "icon": tdt_img,
        "type": "www_tdt",
        "layer": "img_d",
      },
      {
        "pid": 10,
        "name": t('backend.tdtVector'),
        "icon": tdt_vec,
        "type": "group",
      },
      {
        "pid": 10,
        "name": t('backend.tdtTerrain'),
        "icon": tdt_ter,
        "type": "group",
      },
      {
        "pid": 10,
        "name": t('backend.osmMap'),
        "type": "xyz",
        "icon": osm,
        "url": "http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
        "subdomains": "abc"
      },
      {
        "pid": 10,
        "name": t('backend.mapStreet'),
        "icon": mapboxStreets,
        "type": "mapbox",
        "mapId": "mapbox.streets"
      },
      {
        "pid": 10,
        "name": t('backend.blackMap'),
        "icon": bdcdark,
        "type": "mapbox",
        "mapId": "mapbox.dark"
      },
      {
        "pid": 10,
        "name": t('backend.darkMap'),
        "icon": bdcgrayscale,
        "type": "mapbox",
        "mapId": "mapbox.light"
      },
      {
        "pid": 10,
        "name": t('backend.offlineMap'),
        "type": "xyz",
        "icon": mapboxSatellite,
        "url": "http://data.marsgis.cn/maptile/wgs3857img/{z}/{x}/{y}.jpg",
      },
      {
        "pid": 10,
        "name": t('backend.singleMap'),
        "icon": offline,
        "type": "image",
        "url": "img/world/world.jpg"
      }
    ];
    allLayers.forEach((layer) => {
      this.baseMapsList.push({
        title: layer.name,
        imgSrc: layer.icon,
      });
    });
  },
  methods: {
    changeBaseMap(index,title) {
      this.activeBaseMap = index;
      CesiumUtils.LayerImageryRemoveAll();
      switch (title){
        case 'Google Imagery':
        case '谷歌卫星':
          CesiumUtils.LayerImageryGoogleSatelliteAdd();
          break;
        case 'ArcGIS Imagery':
        case 'ArcGIS卫星':
          CesiumUtils.LayerImageryArcGISSatelliteAdd();
          break;
        case 'Bing Imagery':
        case '微软卫星':
          CesiumUtils.LayerImageryBingSatelliteAdd();
          break;
        case 'TDT Imagery':
        case '天地图卫星':
          CesiumUtils.LayerImageryTDTSatelliteAdd();
          break;
        case 'TDT Vector':
        case '天地图电子':
          CesiumUtils.LayerImageryTDTVectorAdd();
          break;
        case 'TDT Terrain':
        case '天地图地形':
          CesiumUtils.LayerImageryTDTTerrainAdd();
          break;
        case 'OSM Map':
        case 'OSM地图':
          CesiumUtils.LayerImageryOSMAdd();
          break;
        case 'Street':
        case '街道图':
          CesiumUtils.LayerImageryMapBoxStreetAdd();
          break;
        case 'Black Map':
        case '黑色底图':
          CesiumUtils.LayerImageryMapBoxDarkAdd();
          break;
        case 'Dark Map':
        case '灰色底图':
          CesiumUtils.LayerImageryMapBoxGrayAdd();
          break;
        case 'Offline Map':
        case '离线地图':
          CesiumUtils.LayerImageryTMSAdd('https://data.kotiot.cn/昆山数据/202301月处理昆山电子底图/{z}/{x}/{y}.png');
          break;
        case 'Single Map':
        case '单张图片':
          CesiumUtils.LayerImageryPhotoAdd(worldimg);
          break;
      }
    },
  },
};
</script>

<style lang="scss" scoped>
@use "./mobile.scss";

.basemaps-container {
  background-color: rgba(0,0,0,.5);
  width: 276px;
  overflow: auto;
  max-height: 500px;

  .basemaps-list {
    justify-content: space-around;
    flex-wrap: wrap;
    padding: 15px 5px 10px 15px;
  }

  .basemap-item {
    width: 47%;
    float: left;
  }

  .basemap-image {
    width: 70px;
    height: 70px;
    border: 2px #fff solid;
  }

  .basemap-title {
    font-size: 14px;
    font-weight: bold;
    margin: 5px 0;
  }
}

.activeBaseMap-image {
  border: 2px red solid !important;
}

.activeBaseMap-title {
  color: red;
}
.flex-column {
  flex-direction: column;
}
.flex-center {
  display: flex;
  justify-content: center;
  align-items: center;
}
</style>