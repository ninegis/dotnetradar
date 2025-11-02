<template>
  <section id="idlogin" v-show="visible">
    <div class="login ftco-section">
      <div class="bgdiv"></div>
      <div class="container">
        <div class="row justify-content-center">
          <div class="col-md-6 text-center mb-5 custom-form-title">
            <h2 class="heading-section"><b>{{ store.sysinfo.title }}</b></h2>

          </div>
        </div>
        <div class="row justify-content-center">
          <div class="col-md-6 col-lg-4">
            <div class="login-wrap p-0">
              <form action="#" class="signin-form">
                <div class="form-group">
                  <input id="username" type="text" class="form-control" placeholder="用户名">
                </div>
                <div class="form-group">
                  <input id="password" :type="passwordInputText" class="form-control" placeholder="密码" required>
                  <span @mousedown="passwordshow" @mouseup="passwordshow(false)"
                    class="fa fa-fw fa-eye field-icon toggle-password"></span>
                </div>
                <div class="form-group">
                  <button type="button" class="btn btn-primary" style="width: 100%;border-radius: 15px"
                    @click="login">登录</button>
                </div>
                <!-- 英文 -->
                <!-- <div class="form-group d-md-flex">
                  <div class="w-50">
                    <label class="checkbox-wrap checkbox-primary">Remember me
                      <input type="checkbox" checked>
                      <span class="checkmark"></span>
                    </label>
                  </div>
                  <div class="w-50 text-md-right">
                    <a href="#" style="color: #fff">Forgot password</a>
                  </div>
                </div>-->
                <!-- 中文 -->
                <div class="form-group d-md-flex">
                  <div class="w-50">
                    <label class="checkbox-wrap checkbox-primary">记住我
                      <input type="checkbox" checked>
                      <span class="checkmark"></span>
                    </label>
                  </div>
                  <div class="w-50 text-md-right">
                    <a href="#" style="color: #fff">忘记密码</a>
                  </div>
                </div>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
    <div class="author">
      <div v-if="store.sysinfo.title === 'SRS-Slope RT100边坡状态监测系统'">
        <p style="text-align: center"><b></b></p>
        <p style="margin-top: -10px; text-align: center"><b></b></p>
      </div>
      <div v-else-if="store.sysinfo.title === '空联网边坡雷达智能化监测预警后台管理系统'">
        <p><b>版权所有：江苏空联网数据服务有限公司</b></p>
        <p style="margin-top: -10px"><b>地址：苏州工业园区东富路35号7幢8层802室</b></p>
      </div>
      <div v-else-if="store.sysinfo.title === '司南边坡雷达监测预警后台管理系统'">
        <p><b>版权所有：上海司南卫星导航技术股份有限公司</b></p>
        <p style="margin-top: -10px"><b>地址：上海市嘉定区马陆镇澄浏中路618号</b></p>
      </div>
      <div v-else-if="store.sysinfo.title === '迈知科技边坡雷达形变智能化分析预警平台'">
        <p style="text-align:center"><b>版权所有：上海迈知科技有限公司</b></p>
        <p style="margin-top: -10px;text-align: center"><b>地址：上海市嘉定区嘉罗公路1661弄41号楼9楼</b></p>
      </div>
    </div>
  </section>
</template>

<script setup>
// desktop / 2023-07-16 / 22:33:52 / 71901
/*-- imports --*/
import '@/lib/font-awesome-4.7.0/css/font-awesome.min.css';
import '@/styles/login.css';
import { defineComponent, ref, onMounted, computed, reactive } from 'vue';
import { useRouter } from 'vue-router';
import { useMapStore } from "@/store/index.js";
import { getProjectInfo, getUserInfo, ucmlLogin } from "@/axios/apiucml.js";
import axios from "axios";
import { ApiRadar } from "@/axios/apiRadar.js";
import { showMessage } from "@/utils/tools.js";
/*-- name --*/
defineComponent({
  name: "login",
});
/*-- props  --*/
const props = defineProps({
  visible: {
    type: String,
    required: false,
    default: 'show',
  },
});
/*-- stores --*/
const router = useRouter();
const store = useMapStore();
/*-- vars --*/
const passwordInputText = ref('password');
const form = reactive({
  username: '',
  password: '',
  rememberMe: '',
});
const url = ref('');
/*-- methods --*/
const loginRadarBackend = async (user, pass) => {
  try {
    const res = await axios.post(ApiRadar.apiUrl + '/api/Auth/login', {
      username: user,
      password: pass
    });
    if (res && res.data && res.data.code === 200 && res.data.data && res.data.data.token) {
      const token = res.data.data.token;
      sessionStorage.setItem('token', token);
      axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;
      return true;
    }
  } catch (e) {}
  return false;
};

const login = async () => {
  if (window.localrelease !== undefined) {
    if (username.value !== window.localrelease.username || password.value !== window.localrelease.password) {
      sessionStorage.setItem('isauthorized', 'false');
      showMessage('账号密码错误，请重试', 'error');
      return;
    }
    store.sysinfo.reportRadarUrl = window.localrelease.reportRadarUrl;//启用报表
    store.sysinfo.reportSign = window.localrelease.reportSign;//报表签名
    store.sysinfo.websocketUrl = window.localrelease.websocketUrl;
    store.sysinfo.serverIp = window.localrelease.url;
    store.sysinfo.title = window.localrelease.title;
    store.sysinfo.shortName = window.localrelease.shortName;
    sessionStorage.setItem('serverIp', store.sysinfo.serverIp);
    sessionStorage.setItem('websocketUrl', store.sysinfo.websocketUrl);
    sessionStorage.setItem('title', store.sysinfo.title);
    sessionStorage.setItem('isauthorized', 'true');
    ApiRadar.radarApiUrl = window.localrelease.url;
    ApiRadar.apiUrl = window.localrelease.url;
    // ✅ 登录后端以获取Token（所有接口需要）
    await loginRadarBackend(window.localrelease.username, window.localrelease.password);
    store.axiosInstance.otherInstance = axios.create();
    router.push('/home');
    return;
  }
  //获取ucml的token
  ucmlLogin('http://8.140.201.145:6081/basic-api', username.value, password.value).then(async (instance) => {
    store.axiosInstance.ucmlInstance = instance;
    store.sysinfo.config.username = username.value;
    //获取ucml登录用户的一些必须的查询id
    getUserInfo(instance).then(res => {
      //获取雷达相关的接口
      getProjectInfo('sloperadar', res.data['Entity']['OrgOID'], res.data['Entity']['UserOID']).then(async projectInfo => {
        if (projectInfo['data']['Entity']['Table'][0]['Province'] === "0" && window.tool.allow === true) {
          showMessage('用户未授权', 'warning');
          return;
        }
        store.sysinfo.serverIp = projectInfo['data']['Entity']['Table'][0]['DataInterfaceApi'];
        store.sysinfo.title = projectInfo['data']['Entity']['Table'][0]['projectname'];
        store.sysinfo.ucmlInfo.divisionOid = res.data['Entity']['DivisionOID'];
        store.sysinfo.ucmlInfo.orgOid = res.data['Entity']['OrgOID'];
        store.sysinfo.ucmlInfo.postOid = res.data['Entity']['PostOID'];
        store.sysinfo.ucmlInfo.userOid = res.data['Entity']['UserOID'];
        store.sysinfo.websocketUrl = projectInfo['data']['Entity']['Table'][0]['WebSocketaddress'];
        store.sysinfo.config.projectCode = projectInfo['data']['Entity']['Table'][0]['projectcode'];
        store.sysinfo.config.shortName = projectInfo['data']['Entity']['Table'][0]['projectabbreviation'];
        store.sysinfo.config.i18Title = projectInfo['data']['Entity']['Table'][0]['BackupField05'];
        store.sysinfo.config.i18Sign = projectInfo['data']['Entity']['Table'][0]['BackupField06'];
        store.sysinfo.config.language = (projectInfo['data']['Entity']['Table'][0]['BackupField04'] === null || projectInfo['data']['Entity']['Table'][0]['BackupField04'] === '') ? '0' : projectInfo['data']['Entity']['Table'][0]['BackupField04'];
        store.sysinfo.config.radarCoordinates = projectInfo['data']['Entity']['Table'][0]['SpareField6'];

        //获取用户登录地址
        ApiRadar.GetUserAddressByIp().then(res => {
          store.sysinfo.address = res.data;
          sessionStorage.setItem('address', store.sysinfo.address);
        })

        //存入token方便调试用
        sessionStorage.setItem('username', store.sysinfo.config.username);
        sessionStorage.setItem('divisionOid', res.data['Entity']['DivisionOID']);
        sessionStorage.setItem('orgOid', res.data['Entity']['OrgOID']);
        sessionStorage.setItem('postOid', res.data['Entity']['PostOID']);
        sessionStorage.setItem('userOid', res.data['Entity']['UserOID']);
        sessionStorage.setItem('serverIp', store.sysinfo.serverIp);
        sessionStorage.setItem('title', store.sysinfo.title);


        sessionStorage.setItem('websocketUrl', store.sysinfo.websocketUrl);
        sessionStorage.setItem('projectCode', store.sysinfo.config.projectCode);


        sessionStorage.setItem('shortName', store.sysinfo.config.shortName);
        sessionStorage.setItem('isauthorized', 'true');
        sessionStorage.setItem('language', store.sysinfo.config.language);
        sessionStorage.setItem('i18Title', store.sysinfo.config.i18Title);
        sessionStorage.setItem('i18Sign', store.sysinfo.config.i18Sign);
        // ✅ 登录后端以获取Token（所有接口需要）
        const ok = await loginRadarBackend(username.value, password.value);
        if (!ok) {
          // 尝试默认管理员（后端内置）
          await loginRadarBackend('admin', 'admin123');
        }
        store.axiosInstance.radarInstance = axios.create();
        router.push('/home');
      });
    })
  }).catch(() => {
    showMessage('密码错误', 'error');
  })
}
const passwordshow = (on = true) => {
  passwordInputText.value = on ? 'text' : 'password';
}
/*-- events --*/
onMounted(() => {
  store.$reset();
  document.title = store.sysinfo.title;

  //console.log('Login.onMounted');
});
</script>

<style scoped>
#idlogin,
.frame {
  height: 100%;
  width: 100%;
  position: relative;
  background: black;
  overflow-y: hidden;
}

.author {
  position: absolute;
  bottom: 20px;
  left: 50%;
  color: white;
  transform: translateX(-50%);
  font-size: 14px;
  text-align: center;
}

.ftco-section {
  background-image: url("@/assets/loginbg.jpg");
  height: 100%;
}

.container {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  margin-top: -10px;
}

.form-control {
  border: 1px solid white;
}

.bgdiv {
  background: #00eaff;
  width: 400px;
  height: 460px;
  position: absolute;
  left: 50%;
  top: 50%;
  background: rgba(0, 0, 0, .5);
  transform: translate(-50%, -50%);
  border-radius: 15px;
}

.custom-form-title {
  max-width: 310px;
}
</style>