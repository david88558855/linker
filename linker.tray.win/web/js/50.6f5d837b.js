"use strict";(self["webpackChunklinker_web"]=self["webpackChunklinker_web"]||[]).push([[50],{83:function(e,t,n){n.d(t,{$M:function(){return s},KW:function(){return r},S3:function(){return d},Vn:function(){return o},b0:function(){return u},gM:function(){return c},ix:function(){return l},r7:function(){return i},zp:function(){return h}});var a=n(4);const l=()=>(0,a.zG)("tunnel/gettypes"),u=e=>(0,a.zG)("tunnel/SetServers",e),i=(e="0")=>(0,a.zG)("tunnel/get",e),c=()=>(0,a.zG)("tunnel/refresh"),r=e=>(0,a.zG)("tunnel/SetRouteLevel",e),s=e=>(0,a.zG)("tunnel/SetTransports",e),o=()=>(0,a.zG)("tunnel/GeInterfaces"),d=e=>(0,a.zG)("tunnel/SetInterface",e),h=()=>(0,a.zG)("tunnel/Records")},920:function(e,t,n){n.d(t,{BJ:function(){return o},NS:function(){return i},Vk:function(){return s},aP:function(){return h},ir:function(){return r},lJ:function(){return u},mK:function(){return l},y2:function(){return d},yN:function(){return c}});var a=n(4);const l=(e="0")=>(0,a.zG)("updaterclient/get",e),u=e=>(0,a.zG)("updaterclient/confirm",e),i=e=>(0,a.zG)("updaterclient/exit",e),c=()=>(0,a.zG)("updaterclient/GetSecretKey"),r=e=>(0,a.zG)("updaterclient/SetSecretKey",e),s=()=>(0,a.zG)("updaterclient/getcurrent"),o=()=>(0,a.zG)("updaterclient/getserver"),d=e=>(0,a.zG)("updaterclient/confirmserver",e),h=()=>(0,a.zG)("updaterclient/exitserver")},5962:function(e,t,n){n.d(t,{Y:function(){return c},q:function(){return r}});var a=n(8835),l=n(144),u=n(6768);const i=Symbol(),c=()=>{const e=(0,l.KR)({list:{},timer:0,hashcode:0});(0,u.Gt)(i,e);const t=()=>{(0,a.QI)(e.value.hashcode.toString()).then((n=>{e.value.hashcode=n.HashCode,n.List&&(e.value.list=n.List),e.value.timer=setTimeout(t,1e3)})).catch((()=>{e.value.timer=setTimeout(t,1e3)}))},n=()=>{clearTimeout(e.value.timer)};return{access:e,_getAccessInfo:t,clearAccessTimeout:n}},r=()=>(0,u.WQ)(i)},9383:function(e,t,n){n.d(t,{T:function(){return s},d:function(){return r}});var a=n(920),l=n(3830),u=n(144),i=n(6768);const c=Symbol(),r=()=>{const e=(0,l.B)(),t=(0,u.KR)({timer:0,list:{},hashcode:0,current:{Version:"",Msg:[],DateTime:"",Status:0,Length:0,Current:0}});(0,i.Gt)(c,t);const n=()=>{(0,a.mK)(t.value.hashcode.toString()).then((a=>{if(t.value.hashcode=a.HashCode,a.List){const n=Object.values(a.List).filter((e=>!!e.Version))[0];n&&(Object.assign(t.value.current,{DateTime:n.DateTime,Version:n.Version,Status:n.Status,Length:n.Length,Current:n.Current,Msg:n.Msg}),e.value.updater=t.value.current),t.value.list=a.List}t.value.timer=setTimeout(n,800)})).catch((()=>{t.value.timer=setTimeout(n,800)}))},r=()=>{clearTimeout(t.value.timer)};return{updater:t,_getUpdater:n,clearUpdaterTimeout:r}},s=()=>(0,i.WQ)(c)},6611:function(e,t,n){n.d(t,{A:function(){return d}});var a=n(6768);function l(e,t,n,l,u,i){const c=(0,a.g2)("el-checkbox"),r=(0,a.g2)("el-col"),s=(0,a.g2)("el-row"),o=(0,a.g2)("el-checkbox-group");return(0,a.uX)(),(0,a.CE)(a.FK,null,[(0,a.bF)(s,null,{default:(0,a.k6)((()=>[(0,a.bF)(r,{span:8},{default:(0,a.k6)((()=>[(0,a.bF)(c,{modelValue:l.state.checkAll,"onUpdate:modelValue":t[0]||(t[0]=e=>l.state.checkAll=e),onChange:l.handleCheckAllChange,label:"全选",indeterminate:l.state.isIndeterminate},null,8,["modelValue","onChange","indeterminate"])])),_:1})])),_:1}),(0,a.bF)(o,{modelValue:l.state.checkList,"onUpdate:modelValue":t[1]||(t[1]=e=>l.state.checkList=e),onChange:l.handleCheckedChange},{default:(0,a.k6)((()=>[(0,a.bF)(s,null,{default:(0,a.k6)((()=>[((0,a.uX)(!0),(0,a.CE)(a.FK,null,(0,a.pI)(l.access,((e,t)=>((0,a.uX)(),(0,a.Wv)(r,{key:t,span:8},{default:(0,a.k6)((()=>[(0,a.bF)(c,{value:e.Value,label:e.Text},null,8,["value","label"])])),_:2},1024)))),128))])),_:1})])),_:1},8,["modelValue","onChange"])],64)}n(4114);var u=n(144),i=n(3830),c=n(5962),r={props:["machineid"],setup(e){const t=(0,i.B)(),n=(0,c.q)(),l=(0,a.EW)((()=>{const e=t.value.config.Client.Accesss;return Object.keys(e).reduce(((n,a,l)=>{if(t.value.hasAccess(a)){const t=e[a];t.Key=a,n.push(t)}return n}),[])})),r=(0,u.Kh)({checkList:[t.value.config.Client.Accesss.Api.Value,t.value.config.Client.Accesss.Web.Value,t.value.config.Client.Accesss.NetManager.Value,t.value.config.Client.Accesss.FullManager.Value,t.value.config.Client.Accesss.Transport.Value,t.value.config.Client.Accesss.Action.Value],checkAll:!1,isIndeterminate:!1}),s=()=>r.checkList.reduce(((e,t)=>(e|t)>>>0),0),o=e=>{const t=e.length;r.checkAll=t===l.value.length,r.isIndeterminate=t>0&&t<l.value.length},d=e=>{r.checkAll=e,r.checkList=e?l.value.map((e=>e.Value)):[],r.isIndeterminate=!1};return(0,a.sV)((()=>{if(n&&n.value.list[e.machineid]){const t=n.value.list[e.machineid];r.checkList=l.value.reduce(((e,n)=>((t&n.Value)>>>0==n.Value&&e.push(n.Value),e)),[])}o(r.checkList)})),{state:r,access:l,getValue:s,handleCheckAllChange:d,handleCheckedChange:o}}},s=n(1241);const o=(0,s.A)(r,[["render",l],["__scopeId","data-v-bdd023b0"]]);var d=o},2126:function(e,t,n){n.d(t,{A:function(){return V}});var a=n(6768),l=n(4232);const u=["title"],i={key:0,class:"progress"},c={key:1,class:"progress"};function r(e,t,n,r,s,o){const d=(0,a.g2)("Loading"),h=(0,a.g2)("el-icon"),v=(0,a.g2)("Download"),m=(0,a.g2)("CircleCheck");return(0,a.uX)(),(0,a.CE)("a",{href:"javascript:;",class:(0,l.C4)(["download",r.updaterColor]),onClick:t[0]||(t[0]=e=>r.handleUpdate()),title:r.updaterText},[(0,a.Lk)("span",null,[(0,a.Lk)("span",null,(0,l.v_)(r.item.Version),1),r.updater.list[r.item.MachineId]?((0,a.uX)(),(0,a.CE)(a.FK,{key:0},[1==r.updater.list[r.item.MachineId].Status?((0,a.uX)(),(0,a.Wv)(h,{key:0,size:"14",class:"loading"},{default:(0,a.k6)((()=>[(0,a.bF)(d)])),_:1})):2==r.updater.list[r.item.MachineId].Status?((0,a.uX)(),(0,a.Wv)(h,{key:1,size:"14"},{default:(0,a.k6)((()=>[(0,a.bF)(v)])),_:1})):3==r.updater.list[r.item.MachineId].Status||5==r.updater.list[r.item.MachineId].Status?((0,a.uX)(),(0,a.CE)(a.FK,{key:2},[(0,a.bF)(h,{size:"14",class:"loading"},{default:(0,a.k6)((()=>[(0,a.bF)(d)])),_:1}),0==r.updater.list[r.item.MachineId].Length?((0,a.uX)(),(0,a.CE)("span",i,"0%")):((0,a.uX)(),(0,a.CE)("span",c,(0,l.v_)(parseInt(r.updater.list[r.item.MachineId].Current/r.updater.list[r.item.MachineId].Length*100))+"%",1))],64)):6==r.updater.list[r.item.MachineId].Status?((0,a.uX)(),(0,a.Wv)(h,{key:3,size:"14",class:"yellow"},{default:(0,a.k6)((()=>[(0,a.bF)(m)])),_:1})):(0,a.Q3)("",!0)],64)):((0,a.uX)(),(0,a.Wv)(h,{key:1,size:"14"},{default:(0,a.k6)((()=>[(0,a.bF)(v)])),_:1}))])],10,u)}n(4114);var s=n(3830),o=n(144),d=n(1219),h=n(2933),v=n(2248),m=n(920),f=n(7477),p=n(9383),g={props:["item","config"],components:{Download:f.f5X,Loading:f.Rhj,CircleCheck:f.rW7},setup(e){const t=(0,s.B)(),n=(0,a.EW)((()=>t.value.hasAccess("UpdateSelf"))),l=(0,a.EW)((()=>t.value.hasAccess("UpdateOther"))),u=(0,p.T)(),i=(0,a.EW)((()=>t.value.signin.Version)),c=(0,a.EW)((()=>u.value.current.Version)),r=(0,a.EW)((()=>`${c.value}->${u.value.current.DateTime}\n${u.value.current.Msg.map(((e,t)=>`${t+1}、${e}`)).join("\n")}`)),f=(0,a.EW)((()=>u.value.list[e.item.MachineId]?u.value.list[e.item.MachineId].Status<=2?e.item.Version!=i.value?`与服务器版本(${i.value})不一致，建议更新`:c.value!=e.item.Version?`不是最新版本(${c.value})，建议更新\n${r.value}`:`是最新版本，但我无法阻止你喜欢更新\n${r.value}`:{3:"正在下载",4:"已下载",5:"正在解压",6:"已解压，请重启"}[u.value.list[e.item.MachineId].Status]:"未检测到更新")),g=(0,a.EW)((()=>e.item.Version!=i.value?"red":u.value.list[e.item.MachineId]&&c.value!=e.item.Version?"yellow":"green")),k=()=>{if(!e.config)return;if(!n.value)return;const t=u.value.list[e.item.MachineId];if(t)if([0,1,3,5].indexOf(t.Status)>=0)d.nk.error("操作中，请稍后!");else if(6!=t.Status){if(2==t.Status){const t=(0,o.KR)(c.value),n=[(0,a.h)(v.P9,{label:`仅[${e.item.MachineName}] -> ${c.value}(最新版本)`,value:c.value})];e.config&&l.value&&n.push((0,a.h)(v.P9,{label:`[所有] -> ${c.value}(最新版本)`,value:`all->${c.value}`})),e.item.Version!=i.value&&c.value!=i.value&&(n.push((0,a.h)(v.P9,{label:`仅[${e.item.MachineName}] -> ${i.value}(服务器版本)`,value:i.value})),e.config&&l.value&&n.push((0,a.h)(v.P9,{label:`[所有] -> ${i.value}(服务器版本)`,value:`all->${i.value}`}))),(0,h.s)({title:"选择版本",message:()=>(0,a.h)(v.AV,{modelValue:t.value,placeholder:"请选择",style:"width:20rem;","onUpdate:modelValue":e=>{t.value=e}},n),confirmButtonText:"确定",cancelButtonText:"取消"}).then((()=>{const n={MachineId:e.item.MachineId,Version:t.value.replace("all->",""),All:t.value.indexOf("all->")>=0};n.All&&(n.MachineId=""),(0,m.lJ)(n)})).catch((()=>{}))}}else h.s.confirm("确定关闭程序吗？","提示",{confirmButtonText:"确定",cancelButtonText:"取消",type:"warning"}).then((()=>{exit(e.item.MachineId)})).catch((()=>{}));else d.nk.error("未检测到更新")};return{item:(0,a.EW)((()=>e.item)),updater:u,updaterText:f,updaterColor:g,handleUpdate:k}}},k=n(1241);const C=(0,k.A)(g,[["render",r],["__scopeId","data-v-56d38c60"]]);var V=C}}]);