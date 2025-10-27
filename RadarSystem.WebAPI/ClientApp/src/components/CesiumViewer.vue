<template>
  <div class="cesium-container">
    <div ref="cesiumContainer" class="cesium-viewer"></div>
    
    <!-- 图层控制面板 -->
    <div class="layer-control" v-if="showLayerControl">
      <el-card>
        <template #header>
          <div class="card-header">
            <span>图层控制</span>
            <el-button text @click="showLayerControl = false">
              <el-icon><Close /></el-icon>
            </el-button>
          </div>
        </template>
        <div class="layer-list">
          <div class="layer-item">
            <el-checkbox v-model="layers.terrain" @change="toggleTerrain">地形</el-checkbox>
            <el-slider v-model="terrainOpacity" :min="0" :max="100" @change="updateTerrainOpacity" />
          </div>
          <div class="layer-item">
            <el-checkbox v-model="layers.imagery" @change="toggleImagery">影像底图</el-checkbox>
            <el-slider v-model="imageryOpacity" :min="0" :max="100" @change="updateImageryOpacity" />
          </div>
          <div class="layer-item">
            <el-checkbox v-model="layers.model" @change="toggleModel">三维模型</el-checkbox>
            <el-slider v-model="modelOpacity" :min="0" :max="100" @change="updateModelOpacity" />
          </div>
          <div class="layer-item">
            <el-checkbox v-model="layers.points" @change="togglePoints">监测点</el-checkbox>
          </div>
          <div class="layer-item">
            <el-checkbox v-model="layers.areas" @change="toggleAreas">监测面</el-checkbox>
          </div>
        </div>
      </el-card>
    </div>

    <!-- 工具栏 -->
    <div class="cesium-toolbar">
      <el-button-group>
        <el-tooltip content="图层控制">
          <el-button @click="showLayerControl = !showLayerControl">
            <el-icon><List /></el-icon>
          </el-button>
        </el-tooltip>
        <el-tooltip content="视角复位">
          <el-button @click="resetView">
            <el-icon><RefreshRight /></el-icon>
          </el-button>
        </el-tooltip>
        <el-tooltip content="测量工具">
          <el-button @click="toggleMeasure">
            <el-icon><Ruler /></el-icon>
          </el-button>
        </el-tooltip>
        <el-tooltip content="截图">
          <el-button @click="screenshot">
            <el-icon><Camera /></el-icon>
          </el-button>
        </el-tooltip>
      </el-button-group>
    </div>

    <!-- 坐标显示 -->
    <div class="coordinate-display" v-if="currentPosition">
      <span>经度: {{ currentPosition.longitude.toFixed(6) }}°</span>
      <span>纬度: {{ currentPosition.latitude.toFixed(6) }}°</span>
      <span>高程: {{ currentPosition.altitude.toFixed(2) }}m</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch } from 'vue';
import * as Cesium from 'cesium';
import type { MonitoringPoint, MonitoringArea } from '../types/monitoring';
import { Close, List, RefreshRight, Ruler, Camera } from '@element-plus/icons-vue';

// Cesium token (需要从 https://cesium.com/ion/tokens 获取)
Cesium.Ion.defaultAccessToken = 'your-cesium-ion-access-token-here';

const props = defineProps<{
  points?: MonitoringPoint[];
  areas?: MonitoringArea[];
  center?: { longitude: number; latitude: number; altitude: number };
}>();

const emit = defineEmits<{
  pointClick: [point: MonitoringPoint];
  areaClick: [area: MonitoringArea];
  viewChange: [view: any];
}>();

const cesiumContainer = ref<HTMLElement>();
let viewer: Cesium.Viewer | null = null;
const showLayerControl = ref(false);
const currentPosition = ref<{ longitude: number; latitude: number; altitude: number } | null>(null);

// 图层状态
const layers = ref({
  terrain: true,
  imagery: true,
  model: false,
  points: true,
  areas: true
});

const terrainOpacity = ref(100);
const imageryOpacity = ref(100);
const modelOpacity = ref(100);

// 监测点实体集合
let pointEntities: Cesium.Entity[] = [];
let areaEntities: Cesium.Entity[] = [];

onMounted(() => {
  initCesium();
  addMouseMoveHandler();
});

onUnmounted(() => {
  if (viewer) {
    viewer.destroy();
    viewer = null;
  }
});

// 初始化Cesium
const initCesium = () => {
  if (!cesiumContainer.value) return;

  viewer = new Cesium.Viewer(cesiumContainer.value, {
    animation: false,
    baseLayerPicker: false,
    fullscreenButton: false,
    geocoder: false,
    homeButton: false,
    infoBox: true,
    sceneModePicker: false,
    selectionIndicator: true,
    timeline: false,
    navigationHelpButton: false,
    scene3DOnly: false,
    shouldAnimate: false,
    terrainProvider: Cesium.createWorldTerrain(),
    imageryProvider: new Cesium.UrlTemplateImageryProvider({
      url: 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png',
      subdomains: ['a', 'b', 'c']
    })
  });

  // 设置初始视角
  const center = props.center || { longitude: 108.0, latitude: 34.0, altitude: 10000 };
  viewer.camera.setView({
    destination: Cesium.Cartesian3.fromDegrees(
      center.longitude,
      center.latitude,
      center.altitude
    ),
    orientation: {
      heading: Cesium.Math.toRadians(0),
      pitch: Cesium.Math.toRadians(-45),
      roll: 0.0
    }
  });

  // 启用深度测试
  viewer.scene.globe.depthTestAgainstTerrain = true;

  // 添加监测点和监测面
  if (props.points) {
    addMonitoringPoints(props.points);
  }
  if (props.areas) {
    addMonitoringAreas(props.areas);
  }

  // 点击事件
  viewer.selectedEntityChanged.addEventListener((entity) => {
    if (entity && entity.properties) {
      const data = entity.properties.getValue(Cesium.JulianDate.now());
      if (data.type === 'point') {
        emit('pointClick', data.data);
      } else if (data.type === 'area') {
        emit('areaClick', data.data);
      }
    }
  });
};

// 添加监测点
const addMonitoringPoints = (points: MonitoringPoint[]) => {
  if (!viewer) return;

  // 清除旧的点
  pointEntities.forEach(entity => viewer!.entities.remove(entity));
  pointEntities = [];

  points.forEach(point => {
    const color = getPointColor(point.status);
    const entity = viewer!.entities.add({
      position: Cesium.Cartesian3.fromDegrees(point.longitude, point.latitude, point.altitude),
      billboard: {
        image: createPointIcon(color, point.code),
        verticalOrigin: Cesium.VerticalOrigin.BOTTOM,
        heightReference: Cesium.HeightReference.CLAMP_TO_GROUND,
        scale: 1.0
      },
      label: {
        text: point.code,
        font: '14px sans-serif',
        fillColor: Cesium.Color.WHITE,
        outlineColor: Cesium.Color.BLACK,
        outlineWidth: 2,
        style: Cesium.LabelStyle.FILL_AND_OUTLINE,
        verticalOrigin: Cesium.VerticalOrigin.BOTTOM,
        pixelOffset: new Cesium.Cartesian2(0, -30),
        heightReference: Cesium.HeightReference.CLAMP_TO_GROUND
      },
      properties: {
        type: 'point',
        data: point
      }
    });
    pointEntities.push(entity);
  });
};

// 添加监测面
const addMonitoringAreas = (areas: MonitoringArea[]) => {
  if (!viewer) return;

  // 清除旧的面
  areaEntities.forEach(entity => viewer!.entities.remove(entity));
  areaEntities = [];

  areas.forEach(area => {
    const color = getAreaColor(area.status);
    const positions = area.coordinates.map(coord =>
      Cesium.Cartesian3.fromDegrees(coord.longitude, coord.latitude, coord.altitude || 0)
    );

    const entity = viewer!.entities.add({
      polygon: {
        hierarchy: new Cesium.PolygonHierarchy(positions),
        material: Cesium.Color.fromCssColorString(color).withAlpha(area.opacity || 0.3),
        outline: true,
        outlineColor: Cesium.Color.fromCssColorString(color),
        outlineWidth: 2,
        heightReference: Cesium.HeightReference.CLAMP_TO_GROUND
      },
      properties: {
        type: 'area',
        data: area
      }
    });
    areaEntities.push(entity);
  });
};

// 获取监测点颜色
const getPointColor = (status: string): string => {
  const colorMap: Record<string, string> = {
    normal: '#52c41a',
    warning: '#faad14',
    alarm: '#f5222d',
    offline: '#8c8c8c'
  };
  return colorMap[status] || '#1890ff';
};

// 获取监测面颜色
const getAreaColor = (status: string): string => {
  const colorMap: Record<string, string> = {
    normal: '#52c41a',
    warning: '#faad14',
    alarm: '#f5222d'
  };
  return colorMap[status] || '#1890ff';
};

// 创建监测点图标
const createPointIcon = (color: string, text: string): string => {
  const canvas = document.createElement('canvas');
  canvas.width = 48;
  canvas.height = 64;
  const ctx = canvas.getContext('2d')!;

  // 绘制图钉形状
  ctx.fillStyle = color;
  ctx.beginPath();
  ctx.arc(24, 20, 16, 0, Math.PI * 2);
  ctx.fill();
  ctx.beginPath();
  ctx.moveTo(24, 36);
  ctx.lineTo(20, 60);
  ctx.lineTo(28, 60);
  ctx.closePath();
  ctx.fill();

  // 绘制白色边框
  ctx.strokeStyle = '#fff';
  ctx.lineWidth = 2;
  ctx.beginPath();
  ctx.arc(24, 20, 16, 0, Math.PI * 2);
  ctx.stroke();

  return canvas.toDataURL();
};

// 图层控制
const toggleTerrain = () => {
  if (!viewer) return;
  viewer.scene.globe.show = layers.value.terrain;
};

const toggleImagery = () => {
  if (!viewer) return;
  viewer.imageryLayers.get(0).show = layers.value.imagery;
};

const toggleModel = () => {
  // TODO: 实现三维模型显示/隐藏
};

const togglePoints = () => {
  pointEntities.forEach(entity => {
    entity.show = layers.value.points;
  });
};

const toggleAreas = () => {
  areaEntities.forEach(entity => {
    entity.show = layers.value.areas;
  });
};

// 更新透明度
const updateTerrainOpacity = () => {
  // Cesium地形透明度需要特殊处理
};

const updateImageryOpacity = () => {
  if (!viewer) return;
  viewer.imageryLayers.get(0).alpha = imageryOpacity.value / 100;
};

const updateModelOpacity = () => {
  // TODO: 实现模型透明度调整
};

// 视角复位
const resetView = () => {
  if (!viewer) return;
  const center = props.center || { longitude: 108.0, latitude: 34.0, altitude: 10000 };
  viewer.camera.flyTo({
    destination: Cesium.Cartesian3.fromDegrees(center.longitude, center.latitude, center.altitude),
    orientation: {
      heading: Cesium.Math.toRadians(0),
      pitch: Cesium.Math.toRadians(-45),
      roll: 0.0
    },
    duration: 2.0
  });
};

// 测量工具
const toggleMeasure = () => {
  // TODO: 实现距离和面积测量
  ElMessage.info('测量工具开发中');
};

// 截图
const screenshot = () => {
  if (!viewer) return;
  viewer.render();
  const canvas = viewer.scene.canvas;
  canvas.toBlob((blob) => {
    if (blob) {
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `screenshot_${Date.now()}.png`;
      a.click();
      URL.revokeObjectURL(url);
    }
  });
};

// 鼠标移动事件 - 显示坐标
const addMouseMoveHandler = () => {
  if (!viewer) return;
  
  const handler = new Cesium.ScreenSpaceEventHandler(viewer.scene.canvas);
  handler.setInputAction((movement: any) => {
    const cartesian = viewer!.camera.pickEllipsoid(movement.endPosition, viewer!.scene.globe.ellipsoid);
    if (cartesian) {
      const cartographic = Cesium.Cartographic.fromCartesian(cartesian);
      currentPosition.value = {
        longitude: Cesium.Math.toDegrees(cartographic.longitude),
        latitude: Cesium.Math.toDegrees(cartographic.latitude),
        altitude: cartographic.height
      };
    }
  }, Cesium.ScreenSpaceEventType.MOUSE_MOVE);
};

// 飞到指定点
const flyToPoint = (point: MonitoringPoint) => {
  if (!viewer) return;
  viewer.camera.flyTo({
    destination: Cesium.Cartesian3.fromDegrees(
      point.longitude,
      point.latitude,
      point.altitude + 500
    ),
    duration: 2.0
  });
};

// 飞到指定区域
const flyToArea = (area: MonitoringArea) => {
  if (!viewer) return;
  const positions = area.coordinates.map(coord =>
    Cesium.Cartesian3.fromDegrees(coord.longitude, coord.latitude, coord.altitude || 0)
  );
  viewer.camera.flyToBoundingSphere(
    Cesium.BoundingSphere.fromPoints(positions),
    { duration: 2.0 }
  );
};

// 监听props变化
watch(() => props.points, (newPoints) => {
  if (newPoints && viewer) {
    addMonitoringPoints(newPoints);
  }
}, { deep: true });

watch(() => props.areas, (newAreas) => {
  if (newAreas && viewer) {
    addMonitoringAreas(newAreas);
  }
}, { deep: true });

// 暴露方法给父组件
defineExpose({
  flyToPoint,
  flyToArea,
  resetView,
  viewer
});
</script>

<style scoped>
.cesium-container {
  position: relative;
  width: 100%;
  height: 100%;
}

.cesium-viewer {
  width: 100%;
  height: 100%;
}

.layer-control {
  position: absolute;
  top: 20px;
  right: 20px;
  width: 280px;
  z-index: 1000;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.layer-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.layer-item {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.cesium-toolbar {
  position: absolute;
  top: 20px;
  left: 20px;
  z-index: 1000;
}

.coordinate-display {
  position: absolute;
  bottom: 20px;
  left: 20px;
  background: rgba(0, 0, 0, 0.7);
  color: white;
  padding: 8px 16px;
  border-radius: 4px;
  font-size: 12px;
  display: flex;
  gap: 16px;
  z-index: 1000;
}

/* Cesium样式覆盖 */
:deep(.cesium-viewer-toolbar),
:deep(.cesium-viewer-geocoderContainer),
:deep(.cesium-viewer-bottom) {
  display: none !important;
}

:deep(.cesium-widget-credits) {
  display: none !important;
}
</style>

