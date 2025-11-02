import {ElMessage, ElMessageBox} from "element-plus";

export class CommonUtils{
    /**
     * 弹窗控件
     * @param message
     * @param type ['success','info','warning','error']
     * @param duration 持续时间
     */
    static ShowMessage(message,type='success',duration=3000){
        ElMessage({
            message,type,duration
        })
    }

    /**
     * 弹窗输入对话框
     * @param instruction 弹窗标题
     * @param content 弹窗内容
     * @param value //输入框默认值
     * @returns {Promise<unknown>}
     */
    static ShowInputDialog(instruction,content,value=''){
        return new Promise(resolve => {
            ElMessageBox.prompt(content, instruction, {
                confirmButtonText: '确认提交',
                cancelButtonText: '取消',
                inputValue:value
            })
                .then(({ value }) => {
                    resolve(value);
                })
                .catch(() => {
                    resolve('reject');
                })
        })
    }
    static FindIndexOfArray(primarykey,value,object){
        const obj = Object.keys(object);
        let result = -1;
        for (let i = 0; i < obj.length; i++) {
            const item = object[obj[i]];
            if (item[primarykey]===value){
                result = i;
                break;
            }
        }
        return result;
    }
    static FindObjectOfArray(primarykey,value,object){
        const obj = Object.keys(object);
        let result = null;
        for (let i = 0; i < obj.length; i++) {
            const item = object[obj[i]];
            if (item[primarykey]===value){
                result = item;
                break;
            }
        }
        return result;
    }
    static DateTimeToStr(date){
        return date.getFullYear()+'-'+
            (date.getMonth()>8?date.getMonth()+1:'0'+(date.getMonth()+1))+'-'+
            (date.getDate()>9?date.getDate():'0'+date.getDate())+' '+
            (date.getHours()>9?date.getHours():'0'+date.getHours())+':'+
            (date.getMinutes()>9?date.getMinutes():'0'+date.getMinutes())+':'+
            (date.getSeconds()>9?date.getSeconds():'0'+date.getSeconds());
    }
}