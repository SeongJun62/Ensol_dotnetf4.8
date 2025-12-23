using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace dotnetfsample.MiddleWare
{
    public class LoggingHandler : DelegatingHandler
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(LoggingHandler));

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // ===== REQUEST BODY =====
            string requestBody = "";
            if (request.Content != null)
            {
                requestBody = await request.Content.ReadAsStringAsync();

                // ⚠ ReadAsStringAsync() 호출 후에도 downstream(모델바인딩 등)이 다시 읽을 수 있게
                // Content를 재구성해주는 게 안전함 (특히 StreamContent/Multipart 등에서 문제 예방)
                var mediaType = request.Content.Headers.ContentType?.MediaType;
                var charset = request.Content.Headers.ContentType?.CharSet;

                var newContent = new StringContent(
                    requestBody ?? "",
                    !string.IsNullOrWhiteSpace(charset) ? Encoding.GetEncoding(charset) : Encoding.UTF8,
                    mediaType ?? "application/json"
                );

                // 기존 헤더 최대한 복사 (Content-Type은 StringContent 생성 시 세팅되므로 중복 주의)
                foreach (var h in request.Content.Headers)
                {
                    if (string.Equals(h.Key, "Content-Type", StringComparison.OrdinalIgnoreCase))
                        continue;
                    newContent.Headers.TryAddWithoutValidation(h.Key, h.Value);
                }

                request.Content = newContent;
            }

            _logger.InfoFormat(
                "[REQUEST] {0} {1} BODY={2}{3}",
                request.Method,
                request.RequestUri != null ? request.RequestUri.PathAndQuery : "(null)",
                string.IsNullOrEmpty(requestBody) ? "" : Environment.NewLine,
                Truncate(requestBody)
            );

            // ===== CALL NEXT =====
            var response = await base.SendAsync(request, cancellationToken);

            // ===== RESPONSE BODY =====
            string responseBody = "";
            if (response?.Content != null)
            {
                responseBody = await response.Content.ReadAsStringAsync();

                // 응답도 재구성해서 실제 클라이언트로 body가 그대로 나가도록 안전하게 처리
                var mediaType = response.Content.Headers.ContentType?.MediaType;
                var charset = response.Content.Headers.ContentType?.CharSet;

                var newContent = new StringContent(
                    responseBody ?? "",
                    !string.IsNullOrWhiteSpace(charset) ? Encoding.GetEncoding(charset) : Encoding.UTF8,
                    mediaType ?? "application/json"
                );

                foreach (var h in response.Content.Headers)
                {
                    if (string.Equals(h.Key, "Content-Type", StringComparison.OrdinalIgnoreCase))
                        continue;
                    newContent.Headers.TryAddWithoutValidation(h.Key, h.Value);
                }

                response.Content = newContent;
            }

            _logger.InfoFormat(
                "[RESPONSE] {0} {1} BODY={2}{3}",
                (int)(response?.StatusCode ?? 0),
                request.RequestUri != null ? request.RequestUri.AbsolutePath : "(null)",
                string.IsNullOrEmpty(responseBody) ? "" : Environment.NewLine,
                Truncate(responseBody)
            );

            return response;
        }

        private static string Truncate(string text, int maxLength = 10000)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Length <= maxLength ? text : text.Substring(0, maxLength) + "...(truncated)";
        }
    }
}