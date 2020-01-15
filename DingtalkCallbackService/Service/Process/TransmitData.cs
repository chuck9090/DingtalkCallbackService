using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace DingtalkCallbackService.Service.Process
{
    public class TransmitData
    {
        #region 注册回调事件相关配置
        /// <summary>
        /// 需要注册的回调事件名集合
        /// </summary>
        public List<string> callbackTag = new List<string>();
        /// <summary>
        /// 需要注册的回调URL
        /// </summary>
        public string callbackUrl = string.Empty;
        #endregion

        #region 应用参数配置
        /// <summary>
        /// 企业ID
        /// </summary>
        public string corpId = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string appKey = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string appSecret = string.Empty;

        public string token = string.Empty;
        public string aesKey = string.Empty;
        public string suiteKey = string.Empty;
        #endregion

        #region 回调请求里的参数
        /// <summary>
        /// 签名
        /// </summary>
        public string msgSignature = string.Empty;
        /// <summary>
        /// 时间戳
        /// </summary>
        public string timeStamp = string.Empty;
        /// <summary>
        /// 随机字符串
        /// </summary>
        public string nonce = string.Empty;
        #endregion

        #region 
        /// <summary>
        /// 加解密操作对象
        /// </summary>
        public DingTalkCrypt crypt = null;
        /// <summary>
        /// 本次请求的附带参数
        /// </summary>
        public JObject requestPara = null;
        #endregion

        /// <summary>
        /// 初始化 配置参数
        /// </summary>
        public TransmitData()
        {
            string tags_Str = this.GetAppSettingValue("callbackTag");
            if (!string.IsNullOrWhiteSpace(tags_Str))
            {
                tags_Str = tags_Str.Trim();
                if (tags_Str.IndexOf(',') >= 0)
                {
                    string[] tags = tags_Str.Split(',');
                    //需要注册的回调事件
                    this.callbackTag.AddRange(tags);
                }
                else
                {
                    this.callbackTag.Add(tags_Str);
                }
            }

            //回调URL
            this.callbackUrl = this.GetAppSettingValue("callbackUrl");

            //企业corpId
            this.corpId = this.GetAppSettingValue("corpId");
            this.appKey = this.GetAppSettingValue("appKey");
            this.appSecret = this.GetAppSettingValue("appSecret");

            //自定义，用于数据加密（怕随机生成麻烦，直接固定亦可）
            this.token = "DingtalkCallbackServiceToken";
            //自定义，用于数据加密（怕随机生成麻烦，直接固定亦可）
            this.aesKey = "dt2eg2b73q6jtwwxbn0tur0mxjaxru93wuedur9ckrg";
            //企业应用suiteKey为企业的corpId
            this.suiteKey = this.corpId;
        }

        /// <summary>
        /// 从请求获取回调参数
        /// </summary>
        /// <param name="Request"></param>
        public void SetRequestPara(HttpRequestBase Request)
        {
            this.msgSignature = Request["signature"];
            this.timeStamp = Request["timestamp"];
            this.nonce = Request["nonce"];

            if (string.IsNullOrWhiteSpace(this.msgSignature) || string.IsNullOrWhiteSpace(this.msgSignature) || string.IsNullOrWhiteSpace(this.msgSignature))
            {
                throw new Exception("请求参数中“signature”、“timestamp”、“nonce”其中一个或多个参数值为null！");
            }

            //加解密类的实例化
            this.crypt = new DingTalkCrypt(this.token, this.aesKey, this.suiteKey);

            //获取请求中Body部分的加密内容
            string encryptStr = this.GetPostParam(Request);
            //得到本次请求的附带参数
            this.requestPara = this.Decrypt(encryptStr);
        }

        /// <summary>
        /// 获取请求BODY部分内容
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        private string GetPostParam(HttpRequestBase Request)
        {
            if ("POST" == Request.RequestType)
            {
                Stream sm = Request.InputStream;//获取post正文
                int len = (int)sm.Length;//post数据长度
                byte[] inputByts = new byte[len];//字节数据,用于存储post数据
                sm.Read(inputByts, 0, len);//将post数据写入byte数组中
                sm.Close();//关闭IO流
                sm.Dispose();

                //**********下面是把字节数组类型转换成字符串**********

                string data = Encoding.UTF8.GetString(inputByts);//转为String
                data = data.Replace("{\"encrypt\":\"", "").Replace("\"}", "");
                return data;
            }
            return string.Empty;
        }

        /// <summary>
        /// 从配置文件中读取配置值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string GetAppSettingValue(string key)
        {
            return System.Configuration.ConfigurationManager.AppSettings[key];
        }

        /// <summary>
        /// 对内容进行解密
        /// </summary>
        /// <returns></returns>
        public JObject Decrypt(string encryptStr)
        {
            string plainText = string.Empty;
            int decryptResult = crypt.DecryptMsg(this.msgSignature, this.timeStamp, this.nonce, encryptStr, ref plainText);
            if (decryptResult != 0)
            {
                throw new Exception("解密内容异常，错误码：" + decryptResult);
            }
            return JObject.Parse(plainText);
        }

        /// <summary>
        /// 对内容进行加密
        /// </summary>
        /// <param name="responseContent">响应的内容</param>
        /// <param name="encryptContent"></param>
        /// <param name="encryptSignature"></param>
        public void Encrypt(string responseContent, ref string encryptContent, ref string encryptSignature)
        {
            int encryptResult = this.crypt.EncryptMsg(responseContent, this.timeStamp, this.nonce, ref encryptContent, ref encryptSignature);
            if (encryptResult != 0)
            {
                throw new Exception("加密内容异常，错误码：" + encryptResult);
            }
        }
    }
}