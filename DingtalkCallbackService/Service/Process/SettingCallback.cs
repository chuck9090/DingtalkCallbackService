using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

namespace DingtalkCallbackService.Service.Process
{
    public static class CallbackSettingUrl
    {
        public static string getTokenUrlTemplate = "https://oapi.dingtalk.com/gettoken?appkey={0}&appsecret={1}";
        /// <summary>
        /// 设置回调事件的请求Url模板
        /// </summary>
        public static string callBackSettingTemplate = "https://oapi.dingtalk.com/call_back/{0}?access_token={1}";
        /// <summary>
        /// 注册回调事件Url
        /// </summary>
        public static string register_call_back = "register_call_back";
        /// <summary>
        /// 查询所有注册的回调事件Url
        /// </summary>
        public static string get_call_back = "get_call_back";
        /// <summary>
        /// 更新回调事件Url
        /// </summary>
        public static string update_call_back = "update_call_back";
        /// <summary>
        /// 删除回调事件Url
        /// </summary>
        public static string delete_call_back = "delete_call_back";
        /// <summary>
        /// 获取回调失败的结果
        /// </summary>
        public static string get_call_back_failed_result = "get_call_back_failed_result";
    }

    public class SettingCallback
    {
        public string access_token = string.Empty;
        /// <summary>
        /// 注册成功的回调事件集合
        /// </summary>
        public static List<string> callbackNameList = null;

        //定义一个用于保存静态变量的实例
        private static SettingCallback instance = null;
        //定义一个保证线程同步的标识
        private static readonly object locker = new object();

        private SettingCallback(TransmitData transmitData)
        {
            this.access_token = this.GetToken(transmitData.appKey, transmitData.appSecret);

            List<string> tagList = new List<string>();
            bool hasSetting = this.HasSetting(out tagList);
            if (transmitData.callbackTag == null || transmitData.callbackTag.Count == 0)
            {
                if (hasSetting)
                {
                    new LaterExecute.LaterThread(RemoveSetting, transmitData);
                }
                else
                {
                    SettingCallback.callbackNameList = transmitData.callbackTag;
                }
            }
            else
            {
                if (hasSetting)
                {
                    if (!SettingCallback.ArrayIsEqual(tagList, transmitData.callbackTag))
                    {
                        //修改
                        new LaterExecute.LaterThread(UpdateSetting, transmitData);
                    }
                    else
                    {
                        SettingCallback.callbackNameList = tagList;
                    }
                }
                else
                {
                    //新增
                    new LaterExecute.LaterThread(RegisterSetting, transmitData);
                }
            }
        }

        private static bool ArrayIsEqual(List<string> a, List<string> b)
        {
            if (a == b)
            {
                return true;
            }
            else
            {
                if (a.Count != b.Count)
                {
                    return false;
                }
                else
                {
                    a.Sort();
                    b.Sort();
                    for (int i = 0; i < a.Count; i++)
                    {
                        if (a[i] != b[i])
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
        }

        public static SettingCallback GetInstance(TransmitData transmitData)
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new SettingCallback(transmitData);
                    }
                }
                return instance;
            }
            instance.access_token = instance.GetToken(transmitData.appKey, transmitData.appSecret);
            return instance;
        }

        private static string Request(string url, string method, Dictionary<string, object> dicParams)
        {
            HttpWebRequest request = (System.Net.HttpWebRequest)WebRequest.Create(url);
            request.Method = method.ToUpper();
            request.ContentType = "application/json";
            request.Timeout = 60000;

            if (dicParams != null && dicParams.Count > 0)
            {
                string jsonData = JsonConvert.SerializeObject(dicParams);
                byte[] bytes;
                bytes = System.Text.Encoding.UTF8.GetBytes(jsonData);
                request.ContentLength = bytes.Length;
                using (Stream writer = request.GetRequestStream())
                {
                    writer.Write(bytes, 0, bytes.Length);
                }
            }

            string strValue = string.Empty;
            using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse())
            {
                using (System.IO.Stream s = response.GetResponseStream())
                {
                    string StrDate = string.Empty;
                    using (StreamReader Reader = new StreamReader(s, Encoding.UTF8))
                    {
                        while ((StrDate = Reader.ReadLine()) != null)
                        {
                            strValue += StrDate + "\r\n";
                        }
                        return strValue;
                    }
                }
            }
        }

        private string GetToken(string appkey, string appsecret)
        {
            string url = string.Format(CallbackSettingUrl.getTokenUrlTemplate, appkey, appsecret);
            string result = SettingCallback.Request(url, "GET", null);
            JObject rObj = JObject.Parse(result);
            int errcode = Convert.ToInt32(rObj["errcode"].ToString());
            if (errcode == 0)
            {
                string access_token = rObj["access_token"].ToString().Replace("\"", "");
                return access_token;
            }
            else
            {
                throw new Exception("获取应用凭证发生异常！响应内容：" + result);
            }
        }

        private bool HasSetting(out List<string> tagList)
        {
            tagList = new List<string>();

            string url = string.Format(CallbackSettingUrl.callBackSettingTemplate, CallbackSettingUrl.get_call_back, this.access_token);
            string result = SettingCallback.Request(url, "GET", null);

            JObject rObj = JObject.Parse(result);
            int errcode = Convert.ToInt32(rObj["errcode"].ToString());
            if (errcode == 0)
            {
                JArray call_back_tag = (JArray)rObj["call_back_tag"];
                if (call_back_tag == null)
                {
                    throw new Exception("查询注册成功的回调事件响应内容中无“call_back_tag”属性！响应内容：" + result);
                }
                foreach (JToken jto in call_back_tag)
                {
                    string tag = jto + string.Empty;
                    tagList.Add(tag);
                }
                return call_back_tag.Count > 0;
            }
            else if (errcode == 71007)
            {
                return false;
            }
            else
            {
                throw new Exception("查询注册成功的回调事件发生异常！响应内容：" + result);
            }
        }

        private void RegisterSetting(TransmitData transmitData)
        {
            //https://oapi.dingtalk.com/call_back/register_call_back?access_token=ACCESS_TOKEN
            string url = string.Format(CallbackSettingUrl.callBackSettingTemplate, CallbackSettingUrl.register_call_back, this.access_token);

            Dictionary<string, object> dicPara = new Dictionary<string, object>();
            dicPara.Add("call_back_tag", transmitData.callbackTag.ToArray());
            dicPara.Add("token", transmitData.token);
            dicPara.Add("aes_key", transmitData.aesKey);
            dicPara.Add("url", transmitData.callbackUrl);

            string result = SettingCallback.Request(url, "POST", dicPara);

            JObject rObj = JObject.Parse(result);
            int errcode = Convert.ToInt32(rObj["errcode"].ToString());
            if (errcode == 0)
            {
                SettingCallback.callbackNameList = transmitData.callbackTag;
            }
            else
            {
                throw new Exception("注册事件回调发生异常！响应内容：" + result);
            }
        }

        private void UpdateSetting(TransmitData transmitData)
        {
            //https://oapi.dingtalk.com/call_back/update_call_back?access_token=ACCESS_TOKEN
            string url = string.Format(CallbackSettingUrl.callBackSettingTemplate, CallbackSettingUrl.update_call_back, this.access_token);
            Dictionary<string, object> dicPara = new Dictionary<string, object>();
            dicPara.Add("call_back_tag", transmitData.callbackTag.ToArray());
            dicPara.Add("token", transmitData.token);
            dicPara.Add("aes_key", transmitData.aesKey);
            dicPara.Add("url", transmitData.callbackUrl);

            string result = SettingCallback.Request(url, "POST", dicPara);

            JObject rObj = JObject.Parse(result);
            int errcode = Convert.ToInt32(rObj["errcode"].ToString());
            if (errcode == 0)
            {
                SettingCallback.callbackNameList = transmitData.callbackTag;
            }
            else
            {
                throw new Exception("更新事件回调发生异常！响应内容：" + result);
            }
        }

        private void RemoveSetting(TransmitData transmitData)
        {
            string url = string.Format(CallbackSettingUrl.callBackSettingTemplate, CallbackSettingUrl.delete_call_back, this.access_token);

            string result = SettingCallback.Request(url, "GET", null);

            JObject rObj = JObject.Parse(result);
            int errcode = Convert.ToInt32(rObj["errcode"].ToString());
            if (errcode == 0)
            {
                SettingCallback.callbackNameList = transmitData.callbackTag;
            }
            else
            {
                throw new Exception("删除事件回调发生异常！响应内容：" + result);
            }
        }
    }
}
