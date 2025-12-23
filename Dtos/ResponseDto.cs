using dotnetfsample.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

namespace dotnetfsample.Dtos
{
    public class ResponseDto<T>
    {
        public int ReturnCode { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        [JsonConverter(typeof(SapDatsConverter))]
        [JsonIgnore]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public static class ResponseHelper
    {
        /// <summary>
        /// 성공 응답 (HTTP 200)
        /// </summary>
        public static IHttpActionResult Success<T>(ApiController controller, T data, string message = "Success")
        {
            var response = new ResponseDto<T>
            {
                ReturnCode = 0,
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            };

            HttpResponseMessage resp = controller.Request.CreateResponse(HttpStatusCode.OK, response);
            return new ResponseMessageResult(resp);
        }

        /// <summary>
        /// 실패 응답 (HTTP 200 유지 - 기존 정책 유지)
        /// </summary>
        public static IHttpActionResult Fail(ApiController controller, int returnCode, string message = "Fail")
        {
            var response = new ResponseDto<object>
            {
                ReturnCode = returnCode,
                Message = message,
                Data = null,
                Timestamp = DateTime.UtcNow
            };

            HttpResponseMessage resp = controller.Request.CreateResponse(HttpStatusCode.OK, response);
            return new ResponseMessageResult(resp);
        }
    }
}