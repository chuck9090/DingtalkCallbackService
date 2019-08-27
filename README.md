# DingtalkCallbackService
钉钉回调事件监听WebAPI（企业内部应用开发）。
.net webapi 

# 使用说明
1. 在 钉钉开发者平台-企业内部开发 内添加一个小程序；
2. 点开新建的小程序，点击 应用信息-查看详情 ，在 服务器公网出口IP名单 设置本服务的域名/IP（PS：本服务挂载的服务器上在百度搜索“ip”查询服务器外网IP）；
3. 打开本项目的 Web.config 文件，配置appSettings；
4. 打开本项目 Service/Handles/Handles.cs 文件，书写监听事件回调时需要执行的代码，方法名即钉钉回调事件名，方法需是public且非static（PS：因为Handles.cs继承了BaseHandles.cs，回调请求的参数已经封装在继承到的transmitData字段里，直接用this.transmitData.requestPara获取）。
