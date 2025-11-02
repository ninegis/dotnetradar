export const getValueByKey=(key)=>{
    switch (key){
        case 'ipv4':
            return 'ipv4地址';
        case 'port':
            return '默认端口';
        case 'slaveId':
            return '出厂ID';
        case 'protocol':
            return '协议';
        case 'radarOri':
            return '雷达零点朝向';
        case 'AnteBeam_half':
            return '天线波';
        case 'ArmLen':
            return '';
        case 'FreqBand':
            return '';
        case 'ImgAngleEnd':
            return '';
        case 'ImgAngleRes':
            return '';
        case 'ImgAngleStart':
            return '';
        case 'ImgRngRes':
            return '';
        case 'IniSARImgNum':
            return '';
        case 'RngMax':
            return '';
        case 'RngMin':
            return '';
        case 'rpm':
            return '';
        case 'freqBand':
            return '';
    }
}
export function getAlarmLevelIndex(value){
    let result = '';
    switch (value){
        case '正常运行':
        case 'Normal Running':
            result = '0';
            break;
        case '蓝色预警':
        case 'Blue Warning':
            result = '1';
            break;
        case '黄色预警':
        case 'Yellow Warning':
            result = '2';
            break;
        case '橙色预警':
        case 'Orange Warning':
            result = '3';
            break;
        case '红色预警':
        case 'Red Warning':
            result = '4';
            break;
    }
    return result;
}