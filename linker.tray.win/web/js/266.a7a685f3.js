"use strict";(self["webpackChunklinker_web"]=self["webpackChunklinker_web"]||[]).push([[266],{8835:function(e,t,n){n.d(t,{P$:function(){return s},ai:function(){return l},zj:function(){return o}});var a=n(4);const o=()=>(0,a.zG)("configclient/get"),l=e=>(0,a.zG)("configclient/install",e),s=()=>(0,a.zG)("configclient/export")},4:function(e,t,n){n.d(t,{a1:function(){return m},e3:function(){return k},jH:function(){return S},zG:function(){return C}});n(4114),n(6573),n(8100),n(7936);var a=n(1219);let o=0,l=null,s="",i=1,r="";const u={},c={connected:!1,connecting:!1},d=()=>{const e=Date.now();for(let t in u){const n=u[t];e-n.time>n.timeout&&(n.reject("超时~"),delete u[t])}setTimeout(d,1e3)};d();const p={subs:{},add:function(e,t){"function"==typeof t&&(this.subs[e]||(this.subs[e]=[]),this.subs[e].push(t))},remove(e,t){let n=this.subs[e]||[];for(let a=n.length-1;a>=0;a--)n[a]==t&&n.splice(a,1)},push(e,t){let n=this.subs[e]||[];for(let a=n.length-1;a>=0;a--)n[a](t)}},g=()=>{c.connected=!0,c.connecting=!1,p.push(w,c.connected)},f=e=>{c.connected=!1,c.connecting=!1,p.push(w,c.connected),setTimeout((()=>{m()}),1e3)},v=e=>{if("string"!=typeof e.data)return void e.data.arrayBuffer().then((t=>{const n=new DataView(t).getInt8(),a=new FileReader;a.readAsText(e.data.slice(4,4+n),"utf8"),a.onload=()=>{let o=JSON.parse(a.result);o.Content={Name:o.Content,Img:e.data.slice(4+n,e.data.length),ArrayBuffer:t},h(o)}}));let t=JSON.parse(e.data);h(t)},h=e=>{let t=u[e.RequestId];t?(0==e.Code?t.resolve(e.Content):1==e.Code?t.reject(e.Content):255==e.Code?(t.reject(e.Content),t.errHandle||a.nk.error(`${t.path}:${e.Content}`)):p.push(e.Path,e.Content),delete u[e.RequestId]):p.push(e.Path,e.Content)},m=(e=s,t=r)=>{if(r=t,s=e,c.connecting||c.connected)return;null!=l&&l.close(),c.connecting=!0;const n=t||"snltty";l=new WebSocket(s,[n]),l.iddd=++i,l.onopen=g,l.onclose=f,l.onmessage=v},k=()=>{l&&l.close()},C=(e,t={},n=!1,a=15e3)=>new Promise(((s,i)=>{let r=++o;try{u[r]={resolve:s,reject:i,errHandle:n,path:e,time:Date.now(),timeout:a};let o=JSON.stringify({Path:e,RequestId:r,Content:"string"==typeof t?t:JSON.stringify(t)});c.connected&&1==l.readyState?l.send(o):i("网络错误~")}catch(d){console.log(d),i("网络错误~"),delete u[r]}})),w=Symbol(),S=e=>{p.add(w,e)}},9299:function(e,t,n){n.d(t,{BS:function(){return u},SZ:function(){return s},Se:function(){return c},VN:function(){return i},gC:function(){return d},jU:function(){return l},nD:function(){return r},rd:function(){return o}});var a=n(4);const o=e=>(0,a.zG)("signInclient/set",e),l=e=>(0,a.zG)("signInclient/setservers",e),s=()=>(0,a.zG)("signInclient/info"),i=e=>(0,a.zG)("signInclient/setorder",e),r=e=>(0,a.zG)("signInclient/List",e),u=e=>(0,a.zG)("signInclient/ids",e),c=e=>(0,a.zG)("signInclient/del",e),d=e=>(0,a.zG)("signInclient/setname",e)},920:function(e,t,n){n.d(t,{BJ:function(){return c},NS:function(){return s},Vk:function(){return u},aP:function(){return p},ir:function(){return r},lJ:function(){return l},mK:function(){return o},y2:function(){return d},yN:function(){return i}});var a=n(4);const o=(e="0")=>(0,a.zG)("updaterclient/get",e),l=e=>(0,a.zG)("updaterclient/confirm",e),s=e=>(0,a.zG)("updaterclient/exit",e),i=()=>(0,a.zG)("updaterclient/GetSecretKey"),r=e=>(0,a.zG)("updaterclient/SetSecretKey",e),u=()=>(0,a.zG)("updaterclient/getcurrent"),c=()=>(0,a.zG)("updaterclient/getserver"),d=e=>(0,a.zG)("updaterclient/confirmserver",e),p=()=>(0,a.zG)("updaterclient/exitserver")},3830:function(e,t,n){n.d(t,{B:function(){return r},v:function(){return i}});var a=n(4),o=n(144),l=n(6768);const s=Symbol(),i=()=>{const e=(0,o.KR)({api:{connected:!1},height:0,config:{Common:{},Client:{Servers:[]},Server:{},Running:{Relay:{Servers:[]},Tuntap:{IP:"",PrefixLength:24},Client:{Servers:[]},AutoSyncs:{}},configed:!1},signin:{Connected:!1,Connecting:!1,Version:"v1.0.0.0"},bufferSize:["1KB","2KB","4KB","8KB","16KB","32KB","64KB","128KB","256KB","512KB","1024KB"],updater:{},self:{}});return(0,a.jH)((t=>{e.value.api.connected=t})),(0,l.Gt)(s,e),e},r=()=>(0,l.WQ)(s)},8018:function(e,t,n){n.d(t,{A:function(){return se}});var a=n(6768),o=n(4232),l=n.p+"img/memory.a28433e5.svg",s=n.p+"img/wechat.5c371c5d.jpg",i=n.p+"img/alipay.ff9b1e7c.jpg";const r=e=>((0,a.Qi)("data-v-7391329a"),e=e(),(0,a.jt)(),e),u={class:"status-wrap flex"},c={class:"copy"},d=r((()=>(0,a.Lk)("img",{src:l,alt:"memory"},null,-1))),p=r((()=>(0,a.Lk)("span",null,"赞助",-1))),g=[d,p],f=r((()=>(0,a.Lk)("a",{href:"https://github.com/snltty/linker",target:"_blank"},"snltty©linker",-1))),v=["title"],h={key:0,class:"progress"},m={key:1,class:"progress"},k=r((()=>(0,a.Lk)("div",{class:"flex-1"},null,-1))),C={class:"share"},w={class:"api"},S={class:"server"},y=r((()=>(0,a.Lk)("div",{class:"pay"},[(0,a.Lk)("img",{src:s,alt:""}),(0,a.Lk)("img",{src:i,alt:""})],-1)));function b(e,t,n,l,s,i){const r=(0,a.g2)("Loading"),d=(0,a.g2)("el-icon"),p=(0,a.g2)("Download"),b=(0,a.g2)("CircleCheck"),V=(0,a.g2)("Share"),F=(0,a.g2)("Api"),L=(0,a.g2)("Server"),_=(0,a.g2)("el-dialog");return(0,a.uX)(),(0,a.CE)("div",u,[(0,a.Lk)("div",c,[(0,a.Lk)("a",{href:"javascript:;",class:"memory",title:"赞助一笔，让作者饱餐一顿",onClick:t[0]||(t[0]=e=>l.state.showPay=!0)},g),f,(0,a.Lk)("a",{href:"javascript:;",class:(0,o.C4)(["download",l.updateColor]),onClick:t[1]||(t[1]=e=>l.handleUpdate()),title:l.updateText},[(0,a.Lk)("span",null,[(0,a.Lk)("span",null,(0,o.v_)(l.self.Version),1),l.updater.Version?((0,a.uX)(),(0,a.CE)(a.FK,{key:0},[1==l.updater.Status?((0,a.uX)(),(0,a.Wv)(d,{key:0,size:"14",class:"loading"},{default:(0,a.k6)((()=>[(0,a.bF)(r)])),_:1})):2==l.updater.Status?((0,a.uX)(),(0,a.Wv)(d,{key:1,size:"14"},{default:(0,a.k6)((()=>[(0,a.bF)(p)])),_:1})):3==l.updater.Status||5==l.updater.Status?((0,a.uX)(),(0,a.CE)(a.FK,{key:2},[(0,a.bF)(d,{size:"14",class:"loading"},{default:(0,a.k6)((()=>[(0,a.bF)(r)])),_:1}),0==l.updater.Length?((0,a.uX)(),(0,a.CE)("span",h,"0%")):((0,a.uX)(),(0,a.CE)("span",m,(0,o.v_)(parseInt(l.updater.Current/l.updater.Length*100))+"%",1))],64)):6==l.updater.Status?((0,a.uX)(),(0,a.Wv)(d,{key:3,size:"14",class:"yellow"},{default:(0,a.k6)((()=>[(0,a.bF)(b)])),_:1})):(0,a.Q3)("",!0)],64)):((0,a.uX)(),(0,a.Wv)(d,{key:1,size:"14"},{default:(0,a.k6)((()=>[(0,a.bF)(p)])),_:1}))])],10,v)]),k,(0,a.Lk)("div",C,[(0,a.bF)(V,{config:l.config},null,8,["config"])]),(0,a.Lk)("div",w,[(0,a.bF)(F,{config:l.config},null,8,["config"])]),(0,a.Lk)("div",S,[(0,a.bF)(L,{config:l.config},null,8,["config"])]),(0,a.bF)(_,{modelValue:l.state.showPay,"onUpdate:modelValue":t[2]||(t[2]=e=>l.state.showPay=e),title:"赞助linker",width:"300",top:"1vh"},{default:(0,a.k6)((()=>[y])),_:1},8,["modelValue"])])}n(4114);var V=n(144);const F={href:"javascript:;",title:"此设备的管理接口"},L={class:"port-wrap t-c"},_={class:"pdt-10"};function z(e,t,n,l,s,i){const r=(0,a.g2)("Tools"),u=(0,a.g2)("el-icon"),c=(0,a.g2)("el-popconfirm"),d=(0,a.g2)("el-input"),p=(0,a.g2)("el-button"),g=(0,a.g2)("el-dialog");return l.config?((0,a.uX)(),(0,a.CE)("div",{key:0,class:(0,o.C4)(["status-api-wrap",{connected:l.connected}])},[(0,a.bF)(c,{"confirm-button-text":"清除","cancel-button-text":"更改",title:"确定你的操作？",onCancel:l.handleShow,onConfirm:l.handleResetConnect},{reference:(0,a.k6)((()=>[(0,a.Lk)("a",F,[(0,a.bF)(u,{size:"16"},{default:(0,a.k6)((()=>[(0,a.bF)(r)])),_:1}),(0,a.eW)(" 管理接口 ")])])),_:1},8,["onCancel","onConfirm"]),(0,a.bF)(g,{class:"options-center",title:"管理接口","destroy-on-close":"",modelValue:l.showPort,"onUpdate:modelValue":t[2]||(t[2]=e=>l.showPort=e),center:"","show-close":!1,"close-on-click-modal":!1,"align-center":"",width:"200"},{footer:(0,a.k6)((()=>[(0,a.bF)(p,{type:"success",onClick:l.handleConnect1,plain:""},{default:(0,a.k6)((()=>[(0,a.eW)("确 定")])),_:1},8,["onClick"])])),default:(0,a.k6)((()=>[(0,a.Lk)("div",L,[(0,a.Lk)("div",null,[(0,a.eW)(" 接口 : "),(0,a.bF)(d,{modelValue:l.state.api,"onUpdate:modelValue":t[0]||(t[0]=e=>l.state.api=e),style:{width:"70%"}},null,8,["modelValue"])]),(0,a.Lk)("div",_,[(0,a.eW)(" 秘钥 : "),(0,a.bF)(d,{type:"password",modelValue:l.state.psd,"onUpdate:modelValue":t[1]||(t[1]=e=>l.state.psd=e),style:{width:"70%"}},null,8,["modelValue"])])])])),_:1},8,["modelValue"])],2)):(0,a.Q3)("",!0)}var W=n(1387),$=n(3830),T=n(4),x=n(9299),I=n(8835),B=n(7477),E={components:{Tools:B.S0q},props:["config"],setup(e){const t=(0,$.B)(),n=(0,a.EW)((()=>t.value.api.connected)),o=(0,W.rd)(),l=(0,W.lq)(),s={api:`${window.location.hostname}:1803`,psd:"snltty"},i=JSON.parse(localStorage.getItem("api-cache")||JSON.stringify(s)),r=(0,V.Kh)({api:i.api,psd:i.psd,showPort:!1}),u=(0,a.EW)((()=>0==t.value.api.connected&&r.showPort)),c=()=>{localStorage.setItem("api-cache",""),o.push({name:l.name}),window.location.reload()},d=()=>{i.api=r.api,i.psd=r.psd,localStorage.setItem("api-cache",JSON.stringify(i)),(0,T.e3)(),(0,T.a1)(`ws://${r.api}`,r.psd)},p=()=>{d(),window.location.reload()},g=()=>{(0,T.e3)(),(0,T.a1)(`ws://${window.location.hostname}:12345`,r.psd)},f=()=>{(0,I.zj)().then((e=>{t.value.config.Common=e.Common,t.value.config.Client=e.Client,t.value.config.Server=e.Server,t.value.config.Running=e.Running,t.value.config.configed=!0,setTimeout((()=>{f()}),1e3)})).catch((e=>{setTimeout((()=>{f()}),1e3)}))},v=()=>{(0,x.SZ)().then((e=>{t.value.signin.Connected=e.Connected,t.value.signin.Connecting=e.Connecting,t.value.signin.Version=e.Version,setTimeout((()=>{v()}),1e3)})).catch((e=>{setTimeout((()=>{v()}),1e3)}))};return(0,a.sV)((()=>{setTimeout((()=>{r.showPort=!0}),500),(0,T.jH)((e=>{e&&(f(),v())})),o.isReady().then((()=>{r.api=l.query.api?`${window.location.hostname}:${l.query.api}`:r.api,r.psd=l.query.psd||r.psd,d()}))})),{config:e.config,state:r,showPort:u,handleConnect1:p,connected:n,handleShow:g,handleResetConnect:c}}},K=n(1241);const j=(0,K.A)(E,[["render",z],["__scopeId","data-v-0444aa84"]]);var P=j;const G=["title"],X={key:0,class:"progress"},R={key:1,class:"progress"},N={class:"dialog-footer t-c"};function U(e,t,n,l,s,i){const r=(0,a.g2)("Promotion"),u=(0,a.g2)("el-icon"),c=(0,a.g2)("Loading"),d=(0,a.g2)("Download"),p=(0,a.g2)("CircleCheck"),g=(0,a.g2)("el-input"),f=(0,a.g2)("el-form-item"),v=(0,a.g2)("el-form"),h=(0,a.g2)("el-button"),m=(0,a.g2)("el-dialog");return(0,a.uX)(),(0,a.CE)(a.FK,null,[(0,a.Lk)("div",{class:(0,o.C4)(["status-server-wrap",{connected:l.state.connected}])},[(0,a.Lk)("a",{href:"javascript:;",title:"更改你的连接设置",onClick:t[0]||(t[0]=(...e)=>l.handleConfig&&l.handleConfig(...e))},[(0,a.bF)(u,{size:"16"},{default:(0,a.k6)((()=>[(0,a.bF)(r)])),_:1}),(0,a.eW)(" 信标服务器")]),(0,a.Lk)("a",{href:"javascript:;",title:"服务端的程序版本",onClick:t[1]||(t[1]=(...e)=>l.handleUpdate&&l.handleUpdate(...e)),class:(0,o.C4)(["download",l.updateColor()])},[(0,a.Lk)("span",null,(0,o.v_)(l.state.version),1),l.updaterCurrent.Version?((0,a.uX)(),(0,a.CE)(a.FK,{key:0},[1==l.updaterCurrent.Status?((0,a.uX)(),(0,a.Wv)(u,{key:0,size:"14",class:"loading"},{default:(0,a.k6)((()=>[(0,a.bF)(c)])),_:1})):2==l.updaterServer.Status?((0,a.uX)(),(0,a.Wv)(u,{key:1,size:"14"},{default:(0,a.k6)((()=>[(0,a.bF)(d)])),_:1})):3==l.updaterServer.Status||5==l.updaterServer.Status?((0,a.uX)(),(0,a.CE)(a.FK,{key:2},[(0,a.bF)(u,{size:"14",class:"loading"},{default:(0,a.k6)((()=>[(0,a.bF)(c)])),_:1}),0==l.updaterServer.Length?((0,a.uX)(),(0,a.CE)("span",X,"0%")):((0,a.uX)(),(0,a.CE)("span",R,(0,o.v_)(parseInt(l.updaterServer.Current/l.updaterServer.Length*100))+"%",1))],64)):6==l.updaterServer.Status?((0,a.uX)(),(0,a.Wv)(u,{key:3,size:"14",class:"yellow"},{default:(0,a.k6)((()=>[(0,a.bF)(p)])),_:1})):(0,a.Q3)("",!0)],64)):((0,a.uX)(),(0,a.Wv)(u,{key:1,size:"14"},{default:(0,a.k6)((()=>[(0,a.bF)(d)])),_:1}))],10,G)],2),(0,a.bF)(m,{modelValue:l.state.show,"onUpdate:modelValue":t[5]||(t[5]=e=>l.state.show=e),title:"连接设置",width:"300"},{footer:(0,a.k6)((()=>[(0,a.Lk)("div",N,[(0,a.bF)(h,{onClick:t[4]||(t[4]=e=>l.state.show=!1),loading:l.state.loading},{default:(0,a.k6)((()=>[(0,a.eW)("取消")])),_:1},8,["loading"]),(0,a.bF)(h,{type:"primary",onClick:l.handleSave,loading:l.state.loading},{default:(0,a.k6)((()=>[(0,a.eW)("确定保存")])),_:1},8,["onClick","loading"])])])),default:(0,a.k6)((()=>[(0,a.Lk)("div",null,[(0,a.bF)(v,{model:l.state.form,rules:l.state.rules,"label-width":"6rem"},{default:(0,a.k6)((()=>[(0,a.bF)(f,{label:"机器名",prop:"name"},{default:(0,a.k6)((()=>[(0,a.bF)(g,{modelValue:l.state.form.name,"onUpdate:modelValue":t[2]||(t[2]=e=>l.state.form.name=e),maxlength:"12","show-word-limit":""},null,8,["modelValue"])])),_:1}),(0,a.bF)(f,{label:"分组名",prop:"groupid"},{default:(0,a.k6)((()=>[(0,a.bF)(g,{modelValue:l.state.form.groupid,"onUpdate:modelValue":t[3]||(t[3]=e=>l.state.form.groupid=e),type:"password","show-password":"",maxlength:"36","show-word-limit":""},null,8,["modelValue"])])),_:1})])),_:1},8,["model","rules"])])])),_:1},8,["modelValue"])],64)}var A=n(1219),D=n(2933),J=n(920),M={components:{Promotion:B.Yk4,Download:B.f5X,Loading:B.Rhj,CircleCheck:B.rW7},props:["config"],setup(e){const t=(0,$.B)(),n=(0,V.KR)({Version:"",Msg:[],DateTime:"",Status:0,Length:0,Current:0}),o=(0,V.KR)({Version:"",Status:0,Length:0,Current:0}),l=(0,a.EW)((()=>`${n.value.Version}->${n.value.DateTime}\n${n.value.Msg.map(((e,t)=>`${t+1}、${e}`)).join("\n")}`)),s=(0,V.Kh)({show:!1,loading:!1,connected:(0,a.EW)((()=>t.value.signin.Connected)),version:(0,a.EW)((()=>t.value.signin.Version)),form:{name:t.value.config.Client.Name,groupid:t.value.config.Client.GroupId},rules:{}}),i=()=>{(0,J.Vk)().then((e=>{n.value.DateTime=e.DateTime,n.value.Version=e.Version,n.value.Status=e.Status,n.value.Length=e.Length,n.value.Current=e.Current,n.value.Msg=e.Msg,setTimeout((()=>{i()}),1e3)})).catch((()=>{setTimeout((()=>{i()}),1e3)}))},r=()=>{(0,J.BJ)().then((e=>{o.value.Version=e.Version,o.value.Status=e.Status,o.value.Length=e.Length,o.value.Current=e.Current,o.value.Status>2&&o.value.Status<6&&setTimeout((()=>{r()}),1e3)})).catch((()=>{setTimeout((()=>{r()}),1e3)}))},u=()=>n.value.Version?o.value.Status<=2?s.version!=n.value.Version?`不是最新版本(${n.value.Version})，建议更新\n${l.value}`:`是最新版本，但我无法阻止你喜欢更新\n${l.value}`:{3:"正在下载",4:"已下载",5:"正在解压",6:"已解压，请重启"}[o.value.Status]:"未检测到更新",c=()=>s.version!=n.value.Version?"yellow":"green",d=()=>{e.config&&(n.value.Version?[0,1,3,5].indexOf(o.value.Status)>=0?A.nk.error("操作中，请稍后!"):6!=o.value.Status?2==n.value.Status&&D.s.confirm("确定更新服务端吗？","提示",{confirmButtonText:"确定",cancelButtonText:"取消",type:"warning"}).then((()=>{(0,J.y2)(n.value.Version).then((()=>{setTimeout((()=>{r()}),1e3)}))})).catch((()=>{})):D.s.confirm("确定关闭服务端吗？","提示",{confirmButtonText:"确定",cancelButtonText:"取消",type:"warning"}).then((()=>{(0,J.aP)()})).catch((()=>{})):A.nk.error("未检测到更新"))},p=()=>{e.config&&(s.form.name=t.value.config.Client.Name,s.form.groupid=t.value.config.Client.GroupId,s.show=!0)},g=()=>{s.loading=!0,(0,x.rd)(s.form).then((()=>{s.loading=!1,s.show=!1,A.nk.success("已操作")})).catch((e=>{s.loading=!1,A.nk.success("操作失败!")}))};return(0,a.sV)((()=>{(0,T.jH)((e=>{e&&(i(),r())}))})),{config:e.config,state:s,handleConfig:p,handleSave:g,updaterCurrent:n,updaterServer:o,handleUpdate:d,updateText:u,updateColor:c}}};const O=(0,K.A)(M,[["render",U],["__scopeId","data-v-d4c02b68"]]);var q=O;const Q=e=>((0,a.Qi)("data-v-aeeb44f6"),e=e(),(0,a.jt)(),e),H={key:0,class:"status-share-wrap"},Y=Q((()=>(0,a.Lk)("div",{class:"port-wrap t-c"}," 导出配置，作为节点客户端运行，其仅有查看基本信息的能力，无法修改任何配置，如果使用docker，可以仅复制configs文件夹过去，docker映射配置文件夹即可 ",-1)));function Z(e,t,n,o,l,s){const i=(0,a.g2)("Share",!0),r=(0,a.g2)("el-icon"),u=(0,a.g2)("el-button"),c=(0,a.g2)("el-dialog");return o.config?((0,a.uX)(),(0,a.CE)("div",H,[(0,a.Lk)("a",{href:"javascript:;",title:"此设备的管理接口",onClick:t[0]||(t[0]=e=>o.state.show=!0)},[(0,a.bF)(r,{size:"16"},{default:(0,a.k6)((()=>[(0,a.bF)(i)])),_:1}),(0,a.eW)(" 导出配置 ")]),(0,a.bF)(c,{class:"options-center",title:"导出配置","destroy-on-close":"",modelValue:o.state.show,"onUpdate:modelValue":t[2]||(t[2]=e=>o.state.show=e),center:"",width:"300",top:"1vh"},{footer:(0,a.k6)((()=>[(0,a.bF)(u,{plain:"",onClick:t[1]||(t[1]=e=>o.state.show=!1),loading:o.state.loading},{default:(0,a.k6)((()=>[(0,a.eW)("取消")])),_:1},8,["loading"]),(0,a.bF)(u,{type:"success",plain:"",onClick:o.handleExport,loading:o.state.loading},{default:(0,a.k6)((()=>[(0,a.eW)("确定导出")])),_:1},8,["onClick","loading"])])),default:(0,a.k6)((()=>[Y])),_:1},8,["modelValue"])])):(0,a.Q3)("",!0)}var ee={components:{Share:B.SYj},props:["config"],setup(e){const t=(0,V.Kh)({show:!1,loading:!1}),n=()=>{t.loading=!0,(0,I.P$)().then((()=>{t.loading=!1,t.show=!1,A.nk.success("导出成功");const e=document.createElement("a");e.download="client-node-export.zip",e.href="/client-node-export.zip",document.body.appendChild(e),e.click(),document.body.removeChild(e)})).catch((()=>{t.loading=!1}))};return{config:e.config,state:t,handleExport:n}}};const te=(0,K.A)(ee,[["render",Z],["__scopeId","data-v-aeeb44f6"]]);var ne=te,ae=n(2248),oe={components:{Api:P,Server:q,Share:ne,Download:B.f5X,Loading:B.Rhj,CircleCheck:B.rW7},props:["config"],setup(e){const t=(0,$.B)(),n=(0,a.EW)((()=>t.value.updater)),o=(0,a.EW)((()=>n.value.Version)),l=(0,a.EW)((()=>t.value.self)),s=(0,a.EW)((()=>t.value.signin.Version)),i=(0,a.EW)((()=>`${n.value.Version}->${n.value.DateTime}\n${n.value.Msg.map(((e,t)=>`${t+1}、${e}`)).join("\n")}`)),r=(0,a.EW)((()=>n.value.Version&&l.value.Version?n.value<=2?n.value.Version!=s.value?`与服务器版本(${s.value})不一致，建议更新`:l.value.Version!=n.value.Version?`不是最新版本(${l.value.Version})，建议更新\n${i.value}`:`是最新版本，但我无法阻止你喜欢更新\n${i.value}`:{3:"正在下载",4:"已下载",5:"正在解压",6:"已解压，请重启"}[n.value.Status]:"未检测到更新")),u=(0,a.EW)((()=>n.value.Version!=s.value?"red":l.value.Version!=n.value.Version?"yellow":"green")),c=()=>{if(e.config)if(n.value.Version)if([0,1,3,5].indexOf(n.value.Status)>=0)A.nk.error("操作中，请稍后!");else if(6!=n.value.Status){if(2==n.value.Status){const t=(0,V.KR)(o.value),n=[(0,a.h)(ae.P9,{label:`仅[${l.value.MachineName}] -> ${o.value}(最新版本)`,value:o.value})];e.config&&n.push((0,a.h)(ae.P9,{label:`[所有] -> ${o.value}(最新版本)`,value:`all->${o.value}`})),l.value.Version!=s.value&&o.value!=s.value&&(n.push((0,a.h)(ae.P9,{label:`仅[${l.value.MachineName}] -> ${s.value}(服务器版本)`,value:s.value})),e.config&&n.push((0,a.h)(ae.P9,{label:`[所有] -> ${s.value}(服务器版本)`,value:`all->${s.value}`}))),(0,D.s)({title:"选择版本",message:()=>(0,a.h)(ae.AV,{modelValue:t.value,placeholder:"请选择",style:"width:20rem;","onUpdate:modelValue":e=>{t.value=e}},n),confirmButtonText:"确定",cancelButtonText:"取消"}).then((()=>{const e={MachineId:l.value.MachineId,Version:t.value.replace("all->",""),All:t.value.indexOf("all->")>=0};e.All&&(e.MachineId=""),(0,J.lJ)(e)})).catch((()=>{}))}}else D.s.confirm("确定关闭程序吗？","提示",{confirmButtonText:"确定",cancelButtonText:"取消",type:"warning"}).then((()=>{(0,J.NS)(l.value.MachineId)})).catch((()=>{}));else A.nk.error("未检测到更新")},d=(0,V.Kh)({showPay:!1});return{state:d,config:e.config,self:l,updater:n,updateText:r,updateColor:u,handleUpdate:c}}};const le=(0,K.A)(oe,[["render",b],["__scopeId","data-v-7391329a"]]);var se=le}}]);