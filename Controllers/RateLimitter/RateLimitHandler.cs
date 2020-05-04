using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Core_all.Controllers.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Caching.Memory;

namespace Core_all.Controllers.RateLimitter
{
    public class RateLimitHandler 
    {
        private readonly RequestDelegate _next;
        private IRequestLogger _requestlogger;
        private readonly IMemoryCache _memoryCache;

        public RateLimitHandler(RequestDelegate next, IRequestLogger requestLogger, IMemoryCache memoryCache)
        {
            _next = next;
            _requestlogger = requestLogger;
            _memoryCache = memoryCache;

        }

        public async Task Invoke(HttpContext context)
        {
            _requestlogger.setrequestid(context);

            if (!isValidRequest())
            {
                string Throttlemessage = "Exceeded request quota.please after 30 sec.";
                // var response = new HttpRequestMessage().CreateResponse((System.Net.HttpStatusCode)429);

                //response.Headers.Add("Retry-After", new string[] { "Exceeded request quota.please after 30 sec." });

                var request = await FormatRequest(context.Request);
                context.Response.StatusCode = 429;
                _requestlogger.logrequestresponse(context, request, Throttlemessage);
                await context.Response.WriteAsync(Throttlemessage);

              // await Task.FromResult(response);
            }
            else {
                await _next(context);
            }
        }

        private async Task<string> FormatRequest(HttpRequest request)
        {
            var body = request.Body;

            //This line allows us to set the reader for the request back at the beginning of its stream.
            request.EnableRewind();

            //We now need to read the request stream.  First, we create a new byte[] with the same length as the request stream...
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];

            //...Then we copy the entire request stream into the new buffer.
            await request.Body.ReadAsync(buffer, 0, buffer.Length);

            //We convert the byte[] into a string using UTF8 encoding...
            var bodyAsText = Encoding.UTF8.GetString(buffer);

            //..and finally, assign the read body back to the request body, which is allowed because of EnableRewind()
            request.Body = body;

            // return $"{request.Scheme} {request.Host}{request.Path} {request.QueryString} {bodyAsText}";
            return $"{bodyAsText}";
        }


        public bool isValidRequest() {
            bool isvalid = true;

            string publickey = "abc123";
            int allowedrequestcountpermint = 2;

            int  requestcount =0;
            bool isExist = _memoryCache.TryGetValue(publickey, out requestcount);

            if (!isExist)
            {
                requestcount =1;

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(60));

                _memoryCache.Set(publickey, requestcount, cacheEntryOptions);
            }
            else
            {
               
                requestcount = requestcount + 1;
                if (allowedrequestcountpermint < requestcount)
                {
                    isvalid = false;
                }
                _memoryCache.Set(publickey, requestcount);

            }

            return isvalid;
        }

    }
}
