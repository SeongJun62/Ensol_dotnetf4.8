using dotnetfsample.Dtos;
using dotnetfsample.Dtos.TestDto;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace dotnetfsample.Controllers
{
    [RoutePrefix("api/test")]
    public class TestController : ApiController
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string _SAPPOURL;
        private static readonly ILog _logger =
        LogManager.GetLogger(typeof(TestController));

        public TestController()
        {
            _SAPPOURL = ConfigurationManager.AppSettings["SAPPOURL"] ?? throw new InvalidOperationException("설정이 없습니다: SAPPOURL");
        }

        /// <summary>
        /// 샘플 - 수신 (SAPPO -> Legacy)
        /// </summary>
        [HttpPost]
        [Route("Sample_Receive")]
        public IHttpActionResult SampleReceive([FromBody] LegacyDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            ErpDto response;
            try
            {
                response = BizLogic(request);
            }
            catch (Exception ex)
            {
                return ResponseHelper.Fail(this, 100, ex.Message); // ResponseHelper 대신 예시
            }

            return ResponseHelper.Success(this, response, "Success");
        }

        /// <summary>
        /// 샘플 - 송신 (Legacy -> SAPPO)
        /// </summary>
        [HttpPost]
        [Route("Sample_Send")]
        public async Task<IHttpActionResult> SampleSend_Trigger()
        {
            // 요청객체 생성
            var request = new ErpDto
            {
                IV_BUKRS = "testdata01",
                IT_HEAD = new IT_HEAD
                {
                    ZDEDN = "testdata02",
                    APPDAT = "20251201"
                }
            };

            try
            {
                // 요청 로깅(간단 버전)
                _logger.InfoFormat(
                "{0}{1}{2}",
                _SAPPOURL,
                Environment.NewLine,
                JsonConvert.SerializeObject(request, Formatting.Indented)
                );

                // Newtonsoft로 직접 직렬화해서 전송
                var jsonBody = JsonConvert.SerializeObject(request);
                using (var content = new StringContent(jsonBody, Encoding.UTF8, "application/json"))
                using (var resp = await _httpClient.PostAsync(_SAPPOURL, content))
                {
                    var respBody = await resp.Content.ReadAsStringAsync();

                    _logger.InfoFormat(
                "[RESPONSE] {0} {1}{2}{3}",
                (int)resp.StatusCode,
                _SAPPOURL,
                Environment.NewLine,
                respBody
                );


                    resp.EnsureSuccessStatusCode();

                    // 수신 JSON -> 객체 변환
                    var resDto = JsonConvert.DeserializeObject<ResponseDto<LegacyDto>>(respBody);
                    if (resDto == null) return InternalServerError(new Exception("응답 파싱 실패"));

                    if (resDto.ReturnCode > 0) return Ok(resDto); // 또는 BadRequest/Content 등 정책에 맞게

                    var result = BizLogic(resDto.Data);
                    return ResponseHelper.Success(this, result, "OK");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"[RESPONSE] Sync Request Failed to {_SAPPOURL}", ex);
                return InternalServerError(ex);
            }
        }

        private ErpDto BizLogic(LegacyDto receiveData)
        {
            // 자동 변환된 객체 확인 로깅
            _logger.InfoFormat(
            "{0}{1}{2}",
            "BODY=",
            Environment.NewLine,
            JsonConvert.SerializeObject(receiveData, Formatting.Indented)
            );

            // 케이스별 자동변환 체크
            var bukrs = receiveData.BUKRS;
            var ext_Field = receiveData.EXT_FIELD;
            var rowCount = receiveData.IT_HEAD == null ? 0 : receiveData.IT_HEAD.Length;
            var zinteid = (receiveData.IT_HEAD != null && receiveData.IT_HEAD.Length > 0) ? receiveData.IT_HEAD[0].ZINTEID : null;
            var zdedn = receiveData.IT_ITEM == null ? null : receiveData.IT_ITEM.ToString();

            // 테스트용 송신 데이터 생성
            var ed = new ErpDto
            {
                IV_BUKRS = "NJ00",
                IT_HEAD = new IT_HEAD
                {
                    ZDEDN = "5000010090",
                    APPDAT = "20251129"
                }
            };

            return ed;
        }
    }
}
