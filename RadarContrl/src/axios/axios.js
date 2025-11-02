import axios from "axios";
import {manVehicleSysApiUrl, ucmlSysUrl} from "@/axios/baseapi.js";
import {useMapStore} from "@/store/index.js";
import {FormatDate} from "@/utils/tools.js";
import {ucmlTokenReset} from "@/axios/apiucml.js";
let token = undefined;

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
export const getProvinceList=()=>{
    return new Promise(resolve=>{
        const store = useMapStore();
        delete axios.defaults.headers['Authorization'];
        return axios.get('https://restapi.amap.com/v3/config/district?key='+store.gaodekey).then(data=>{
            resolve(data);
        })
    })
}
export const getCityList=(adcode)=>{
    return new Promise(resolve=>{
        const store = useMapStore();
        delete axios.defaults.headers['Authorization'];
        return axios.get('https://restapi.amap.com/v3/config/district?key='+store.gaodekey+'&keywords='+adcode).then(data=>{
            resolve(data);
        })
    })
}
export const getStreetList=(adcode)=>{
    return new Promise(resolve=>{
        const store = useMapStore();
        delete axios.defaults.headers['Authorization'];
        return axios.get('https://restapi.amap.com/v3/config/district?key='+store.gaodekey+'&keywords='+adcode).then(data=>{
            resolve(data);
        })
    })
}
export const getTrackByIdATime=(carid,datestart,dateend,strstatic,strinvalid)=>{
    return new Promise(resolve => {
        carid = 'car_id='+carid;
        datestart = '&stime='+ FormatDate(datestart);
        dateend = '&etime='+FormatDate(dateend);
        strstatic = "&still="+(strstatic?'distinct':'');
        strinvalid = "&invalid="+strinvalid;
        const url = manVehicleSysApiUrl+'gettrackbyidatime?'+carid+datestart+dateend+strstatic+strinvalid;
        axios.get(url).then(res=>{
            resolve(res);
        })
    })

}
export const getOnlinenum=()=>{
    return new Promise(resolve => {
        delete axios.defaults.headers['Authorization'];
        axios.get(manVehicleSysApiUrl+'getonlinenum')
            .then(res=>{
                resolve(res);
            })
    })
}
export const getCurrentTrack=(activeTab)=>{
    return new Promise(resolve => {
        const param = activeTab?'?activetab='+activeTab:'';
        delete axios.defaults.headers.Authorization;
        axios.get(manVehicleSysApiUrl+'getcurrenttrack'+param).then((res)=>{
           resolve(res);
        });
    })
}

export const getMileageToday=()=>{
    return new Promise(resolve => {
        axios.get(manVehicleSysApiUrl+'getmileagetoday').then((res)=>{
            resolve(res);
        });
    })
}
export const getDrivingtimeToday=()=>{
    return new Promise(resolve => {
        axios.get(manVehicleSysApiUrl+'getdrivingtimetoday').then(res=>{
            resolve(res);
        })
    })
}
export const getMileageMonth=()=>{
    return new Promise(resolve => {
        axios.get(manVehicleSysApiUrl+'getmileagemonth')
            .then(res=>{
                resolve(res);
            })
    })
}
export const isExistedSTable=(stableName)=>{
    return new Promise(resolve => {
        delete axios.defaults.headers['Authorization'];
        axios.get(manVehicleSysApiUrl+'isexistedstable?stable='+stableName)
            .then(res=>{
                resolve(res);
            })
    })
}
export const createSTable=(stablename,fieldstr)=>{
    delete axios.defaults.headers['Authorization'];
    return new Promise(resolve => {
        axios.get(manVehicleSysApiUrl+'createstable?stable='+stablename+'&fieldstr='+fieldstr)
            .then(res=>{
                resolve(res);
            })
    })
}