using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;

namespace DingtalkCallbackService.Service.WebApiFilters
{
    public class WebApiErrorHandleAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            Helpers.LoggerHelper.Error(actionExecutedContext.Exception);
            actionExecutedContext.Response = new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(new
                {
                    errcode = 500,
                    errmsg = "ServiceError"
                }), System.Text.Encoding.UTF8, "application/json"),

                StatusCode = System.Net.HttpStatusCode.OK
            };
        }
    }
}