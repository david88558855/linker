<template>
   <a href="javascript:;"  @click="handleUpdate" class="download" :title="updateText()" :class="updateColor()">
        <span>{{state.version}}</span>
        <template v-if="updaterCurrent.Version">
            <template v-if="updaterCurrent.Status == 1">
                <el-icon size="14" class="loading"><Loading /></el-icon>
            </template>
            <template v-else-if="updaterServer.Status == 2">
                <el-icon size="14"><Download /></el-icon>
            </template>
            <template v-else-if="updaterServer.Status == 3 || updaterServer.Status == 5">
                <el-icon size="14" class="loading"><Loading /></el-icon>
                <span class="progress" v-if="updaterServer.Length ==0">0%</span>
                <span class="progress" v-else>{{parseInt(updaterServer.Current/updaterServer.Length*100)}}%</span>
            </template>
            <template v-else-if="updaterServer.Status == 6">
                <el-icon size="14" class="yellow"><CircleCheck /></el-icon>
            </template>
        </template>
        <template v-else>
            <el-icon size="14"><Download /></el-icon>
        </template>
    </a>
</template>
<script>
import { injectGlobalData } from '@/provide';
import { ElMessage, ElMessageBox } from 'element-plus';
import { computed, onMounted, reactive, ref } from 'vue';
import {Promotion,Download,Loading,CircleCheck} from '@element-plus/icons-vue'
import { confirmServer, exitServer, getUpdaterCurrent, getUpdaterServer } from '@/apis/updater';
import ServerFlow from './ServerFlow.vue';
import { useI18n } from 'vue-i18n';
export default {
    components:{Promotion,Download,Loading,CircleCheck,ServerFlow},
    props:['config'],
    setup(props) {

        const { t } = useI18n();
        const globalData = injectGlobalData();
        const hasUpdateServer = computed(()=>globalData.value.hasAccess('UpdateServer')); 
        const updaterCurrent = ref({Version: '', Msg: [], DateTime: '', Status: 0, Length: 0, Current: 0});
        const updaterServer = ref({Version: '', Status: 0, Length: 0, Current: 0});
        const updaterMsg = computed(()=>{
            return `${updaterCurrent.value.Version}->${updaterCurrent.value.DateTime}\n${updaterCurrent.value.Msg.map((value,index)=>`${index+1}、${value}`).join('\n')}`;
        });

        const state = reactive({
            show: false,
            loading: false,

            connected: computed(() => globalData.value.signin.Connected),
            version: computed(() => globalData.value.signin.Version),
        });

        const _getUpdaterCurrent = ()=>{
            getUpdaterCurrent().then((res)=>{
                updaterCurrent.value.DateTime = res.DateTime;
                updaterCurrent.value.Version = res.Version;
                updaterCurrent.value.Status = res.Status;
                updaterCurrent.value.Length = res.Length;
                updaterCurrent.value.Current = res.Current;
                updaterCurrent.value.Msg = res.Msg;
                setTimeout(()=>{
                    _getUpdaterCurrent();
                },1000);
            }).catch(()=>{
                setTimeout(()=>{
                    _getUpdaterCurrent();
                },1000);
            })
        }
        const _getUpdaterServer = ()=>{
            getUpdaterServer().then((res)=>{
                updaterServer.value.Version = res.Version;
                updaterServer.value.Status = res.Status;
                updaterServer.value.Length = res.Length;
                updaterServer.value.Current = res.Current;
                if(updaterServer.value.Status > 2 && updaterServer.value.Status < 6){
                    setTimeout(()=>{
                        _getUpdaterServer();
                    },1000);
                }
            }).catch(()=>{
                setTimeout(()=>{
                    _getUpdaterServer();
                },1000);
            });
        }
        const updateText = ()=>{
            if(!updaterCurrent.value.Version){
                return t('status.serverNoUpdate');
            }
            if(updaterServer.value.Status <= 2) {
                return state.version != updaterCurrent.value.Version  
                ? `${t('status.serverNotNew')}(${updaterCurrent.value.Version})\n${updaterMsg.value}` 
                : `${t('status.serverNew')}\n${updaterMsg.value}`
            }
            return {
                3:t('status.serverDownloading'),
                4:t('status.serverDownloaded'),
                5:t('status.serverUnZipIng'),
                6:t('status.serverUnZip'),
            }[updaterServer.value.Status];
        }
        const updateColor = ()=>{
            return state.version != updaterCurrent.value.Version  ? 'yellow' :'green'
        }
        const handleUpdate = ()=>{
            if(!props.config || !hasUpdateServer.value){
                return;
            }
            if(!updaterCurrent.value.Version){
                ElMessage.error(t('status.serverNoUpdate'));
                return;
            }
            //未检测，检测中，下载中，解压中
            if([0,1,3,5].indexOf(updaterServer.value.Status)>=0){
                ElMessage.error(t('common.operating'));
                return;
            }
            //已解压
            if(updaterServer.value.Status == 6){
                ElMessageBox.confirm(t('status.serverCloseConfirm'), t('common.tips'), {
                    confirmButtonText: t('common.confirm'),
                    cancelButtonText:t('common.cancel'),
                    type: 'warning'
                }).then(() => {
                    exitServer();
                }).catch(() => {});
                return;
            }

            //已检测
            if(updaterCurrent.value.Status == 2){
                ElMessageBox.confirm(t('status.serverConfirm'), t('common.tips'), {
                    confirmButtonText:  t('common.confirm'),
                    cancelButtonText: t('common.cancel'),
                    type: 'warning'
                }).then(() => {
                    confirmServer(updaterCurrent.value.Version).then(()=>{
                        setTimeout(()=>{
                            _getUpdaterServer();
                        },1000);
                    });
                }).catch(() => {});
            }
        }

        onMounted(() => {
            _getUpdaterCurrent();
            _getUpdaterServer();
        });

        return {
         config:props.config,  state, updaterCurrent,updaterServer,handleUpdate,updateText,updateColor
        }
    }
}
</script>
<style lang="stylus" scoped>

@keyframes loading {
    from{transform:rotate(0deg)}
    to{transform:rotate(360deg)}
}

a{
    font-weight:bold;
    .el-icon{
        vertical-align:text-bottom;
    }

    &.download{
        .el-icon{
            font-weight:bold;
            &.loading{
                animation:loading 1s linear infinite;
            }
            margin-left:.3rem
        }
    }
}

</style>