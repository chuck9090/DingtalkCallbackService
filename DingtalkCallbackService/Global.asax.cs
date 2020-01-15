using DingtalkCallbackService.Service;
using DingtalkCallbackService.Service.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace DingtalkCallbackService
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            //自动注册回调事件
            SettingCallback.GetInstance(new TransmitData());
        }
    }
}
