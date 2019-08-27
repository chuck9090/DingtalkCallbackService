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
                executeMethod(td);
                Thread.CurrentThread.Abort();
            });
            thread.Start(transmitData);
        }


    }
}