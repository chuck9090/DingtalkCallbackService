using DingtalkCallbackService.Service.Process;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DingtalkCallbackService.Service.Handles
{
    public partial class Handles : BaseHandles
    {
        public Handles(TransmitData transmitData) : base(transmitData)
        {

        }

        #region 此处只是示例，实际请删除
        /*
        /// <summary>
        /// 审批任务开始，结束，转交
        /// </summary>
        /// <returns></returns>
        public void bpms_task_change()
        {
            //这里写收到回调时要执行的代码，this.transmitData.requestPara即钉钉回调发过来的参数
            
        }
        */
        #endregion
    }
}