using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace DingtalkCallbackService.Service.Helpers
{
    public class LoggerHelper
    {
        private static readonly ILog logger = LogManager.GetLogger("LogHelper");

        public static void Info(string message)
        {
            LoggerHelper.logger.Info(message);
        }

        public static void Error(Exception ex)
        {
            LoggerHelper.logger.Error(JsonConvert.SerializeObject(new
            {
                Message = ex.Message,
                StackTrace = ex.StackTrace
            }));
        }
    }
}