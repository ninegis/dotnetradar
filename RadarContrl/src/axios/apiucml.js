import axios from "axios";
import {md5} from "js-md5";
import {useMapStore} from "@/store/index.js";

export const ucmlLogin=(url,username,password)=>{
    return new Promise((resolve,reject) => {
        const instance = axios.create({
            baseURL:url
        });
        instance.post( '/ServiceEntry',{
            "BPOName": 'UserMenuApi',
            "ClassFullName": "",
            "GloalServiceName": "",
            "MethodName": 'LoginAndAuth',
            "PUrl": {},
            "Parameters": {
                username: username,
                password: md5(password)
            }
        }).then(res => {
            if (res.data['Entity']['ErrorCode']===-1){
                reject(500);
            }else{
                instance.defaults.headers.common['Authorization'] = res.data['Entity']['Token'];
                sessionStorage.setItem('ucmlToken',res.data['Entity']['Token']);
                resolve(instance);
            }
        })
    })
}
export const getUserInfo=()=>{
    return new Promise(resolve => {
        ucmlPost('UserMenuApi','CurrentUser',{}).then(res=>{
            resolve(res);
        })
    })
}
export const ucmlPost=(BPOName,MethodName,Parameters)=>{
    return new Promise(resolve => {
        const store = useMapStore();
        const execute =()=>{
            store.axiosInstance.ucmlInstance.post('/ServiceEntry', {
                "BPOName": BPOName,
                "ClassFullName": "",
                "GloalServiceName": "",
                "MethodName": MethodName,
                "PUrl": {},
                "Parameters": Parameters
            }).then(res => {
                resolve(res);
            });
        }
        if (!store.axiosInstance.ucmlInstance){
            ucmlTokenReset().then(instance=>{
                store.axiosInstance.ucmlInstance = instance;
                store.sysinfo.ucmlInfo.divisionOid = sessionStorage.getItem("divisionOid");
                store.sysinfo.ucmlInfo.orgOid = sessionStorage.getItem("orgOid");
                store.sysinfo.ucmlInfo.postOid = sessionStorage.getItem("postOid");
                store.sysinfo.ucmlInfo.userOid = sessionStorage.getItem("userOid");
                execute();
            })
        }else{
            execute();
        }
    })
}
export const ucmlTokenReset=()=>{
    return new Promise(resolve => {
        // 使用环境变量或本地地址（可配置）
        const ucmlBaseUrl = import.meta.env.VITE_UCML_API_URL || 'http://8.140.201.145:6081/basic-api';
        const instance = axios.create({
            baseURL: ucmlBaseUrl
        });
        instance.defaults.headers.common['Authorization'] = sessionStorage.getItem('ucmlToken');
        resolve(instance);
    })
}
export const getGPSLayerTree = (prjOID,orgOID) => {
    return new Promise(resolve => {
        ucmlPost('BPO_M2023001','GetCurrentUserMapTree',{
            "prjOID":prjOID,
            "orgOID":orgOID
        }).then(res=>{
            resolve(res);
        })
    })
}
export const getProjectInfo=(prjtype,orgOID,userOID)=>{
    return new Promise(resolve => {
        ucmlPost('BPO_CommonfunBpo','GetPrjInfo',{
            prjtype,orgOID,userOID
        }).then(res=>resolve(res));
    })
}
export const instanceReset=(userOID)=>{
    return new Promise(resolve => {
        if (userOID===null){
            const store = useMapStore();
            store.sysinfo.ucmlInfo.divisionOid = sessionStorage.getItem("divisionOid");
            store.sysinfo.ucmlInfo.orgOid = sessionStorage.getItem("orgOid");
            store.sysinfo.ucmlInfo.postOid = sessionStorage.getItem("postOid");
            store.sysinfo.ucmlInfo.userOid = sessionStorage.getItem("userOid");
            store.sysinfo.serverIp = sessionStorage.getItem("serverIp");
            store.sysinfo.title = sessionStorage.getItem("title");
            store.sysinfo.websocketUrl = sessionStorage.getItem("websocketUrl");
            store.sysinfo.address = sessionStorage.getItem("address");
            store.sysinfo.config.username = sessionStorage.getItem("username");
            store.sysinfo.config.projectCode = sessionStorage.getItem("projectCode");
            store.sysinfo.config.shortName = sessionStorage.getItem("shortName");
            store.sysinfo.config.language = sessionStorage.getItem("language");
            store.sysinfo.config.i18Title = sessionStorage.getItem("i18Title");
            store.sysinfo.config.i18Sign = sessionStorage.getItem("i18Sign");
            store.axiosInstance.radarInstance = axios.create({
                baseURL:store.sysinfo.serverIp
            });
            resolve(200);
        }else{
            resolve(200);
        }
    })
}