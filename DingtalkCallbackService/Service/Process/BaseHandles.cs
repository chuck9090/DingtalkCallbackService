using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace DingtalkCallbackService.Service.Process
{
    public class BaseHandles
    {
        /// <summary>
        /// 数据传输对象
        /// </summary>
        protected TransmitData transmitData = null;

        /// <summary>
        /// Handles类路径
        /// </summary>
        public string HandlesClassPath = "DingtalkCallbackService.Service.Handles.Handles";

        /// <summary>
        /// 响应的内容
        /// </summary>
        protected string responseContent = "success";

        protected BaseHandles(TransmitData transmitData)
        {
            this.transmitData = transmitData;
        }

        #region 在测试、注册、更新回调事件时，钉钉会对本服务进行验证
        public void check_create_suite_url() { }

        public void check_update_suite_url() { }

        public void suite_ticket() { }

        /// <summary>
        /// 注册/更新回调事件时，钉钉会向本服务发送回调Url验证请求，本方法会处理该请求
        /// </summary>
        /// <returns></returns>
        public void check_url() { }
        #endregion

        #region 
        /// <summary>
        /// 通过反射技术，根据EventType自动调用响应处理，并返回加密后的响应JSON
        /// </summary>
        /// <param name="eventType">回调请求中附带的EventType值</param>
        /// <returns>加密后的响应JSON</returns>
        public string Handle(string eventType)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type type = assembly.GetType(this.HandlesClassPath);
            if (type == null)
            {
                throw new Exception("根据“" + this.HandlesClassPath + "”未找到Handles类！");
            }
            MethodInfo methodInfo = type.GetMethod(eventType);
            if (methodInfo == null)
            {
                return null;
            }
            methodInfo.Invoke(this, null);
            string result = this.ContentToResponseJSON("success");

            //log记录
            Helpers.LoggerHelper.Info(JsonConvert.SerializeObject(new
            {
                EventType = eventType,
                ResponseContent = this.responseContent
            }));

            return result;
        }

        /// <summary>
        /// 构造钉钉所需的响应JSON
        /// </summary>
        /// <returns></returns>
        private string ContentToResponseJSON(string content)
        {
            string encryptContent = string.Empty, encryptSignature = string.Empty;
            this.transmitData.Encrypt(content, ref encryptContent, ref encryptSignature);

            string responseJsonResult = JsonConvert.SerializeObject(new
            {
                msg_signature = encryptSignature,
                encrypt = encryptContent,
                timeStamp = this.transmitData.timeStamp,
                nonce = this.transmitData.nonce
            });
            return responseJsonResult;
        }
        #endregion
    }
}