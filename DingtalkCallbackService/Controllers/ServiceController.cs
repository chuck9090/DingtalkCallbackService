using DingtalkCallbackService.Service.Handles;
using DingtalkCallbackService.Service.Process;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace DingtalkCallbackService.Controllers
{
    public class ServiceController : ApiController
    {
        private HttpRequestBase GetRequest()
        {
            HttpContextBase context = (HttpContextBase)this.Request.Properties["MS_HttpContext"];//获取传统context
            HttpRequestBase request = context.Request;//定义传统request对象
            return request;
        }

        /// <summary>
        /// 请求Url：/Service/GetSuccessCallbackNameList
        /// </summary>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public HttpResponseMessage GetSuccessCallbackNameList()
        {
            List<string> callbackNameList = SettingCallback.callbackNameList;
            string responseText = string.Empty;
            int waitingSecond = DingtalkCallbackService.Service.LaterExecute.LaterThread.ThreadSleepSecond;
            if (callbackNameList == null)
            {
                responseText = "注册/更新还未开始，或注册/更新异常。请等待" + waitingSecond + "秒后重新访问我查询，或查看日志！";
            }
            else if (callbackNameList.Count == 0)
            {
                responseText = "目前没有注册任何回调事件！";
            }
            else
            {
                responseText = JsonConvert.SerializeObject(callbackNameList);
            }
            return new HttpResponseMessage { Content = new StringContent(responseText, System.Text.Encoding.UTF8, "text/plain") };
        }

        /// <summary>
        /// 请求Url：/Service/Service
        /// </summary>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public HttpResponseMessage Service()
        {
            //将当前请求对象转换为传统HttpRequestBase对象
            HttpRequestBase requ = this.GetRequest();

            //初始化数据传递对象
            TransmitData transmitData = new TransmitData();
            //将请求附带参数赋值给TransmitData对象
            transmitData.SetRequestPara(requ);

            //获取当前回调的事件名
            string eventType = transmitData.requestPara["EventType"].ToString();

            string responseJSON = new Handles(transmitData).Handle(eventType);

            //当没有找到EventType对应Handle方法时
            if (string.IsNullOrEmpty(responseJSON))
            {
                throw new Exception("未找到“" + eventType + "”方法！");
            }

            return new HttpResponseMessage { Content = new StringContent(responseJSON, System.Text.Encoding.UTF8, "application/json") };
        }
    }
}
