using DingtalkCallbackService.Service.Helpers;
using DingtalkCallbackService.Service.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace DingtalkCallbackService.Service.LaterExecute
{
    public delegate void ExecutebMethod(TransmitData transmitData);

    public class LaterThread
    {
        public static int ThreadSleepSecond = 10;
        public LaterThread(ExecutebMethod executeMethod, TransmitData transmitData)
        {
            Thread thread = new Thread((object o) =>
            {
                Thread.Sleep(LaterThread.ThreadSleepSecond * 1000);
                TransmitData td = (TransmitData)o;

                LoggerHelper.Info("钉钉回调事件设置Start");
                try
                {
                    executeMethod(td);

                    LoggerHelper.Info("钉钉回调事件设置正常End");
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(new Exception("钉钉回调事件设置异常：" + ex.Message));
                }

                Thread.CurrentThread.Abort();
            });
            thread.Start(transmitData);
        }


    }
}