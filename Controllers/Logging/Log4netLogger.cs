using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;

namespace Core_all.Controllers.Logging
{
    public  class Log4netLogger : IRequestLogger
    {
       //private  HttpContext _context;
       private Stopwatch _stopwatch;

        public Log4netLogger()
        {           
           // _context = context.HttpContext;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();

        }

        public void logrequestresponse(HttpContext _context,string Request,string Response)
        {
            var AccessLogger = LogManager.GetLogger(Assembly.GetAssembly(typeof(Startup)), "AccessLog");
            var ForensicLogger = LogManager.GetLogger(Assembly.GetAssembly(typeof(Startup)), "ForensicLog");
            try
            {
                _stopwatch.Stop();

                string mid = string.Empty;
                //string Response = string.Empty;
                //string Request = string.Empty;
                string timetaken = _stopwatch.ElapsedMilliseconds.ToString();
                string responseCode = string.Empty;

                mid = Guid.NewGuid().ToString();
               // Response =await FormatResponse(_context.Response);
               // Request =await _context.Request.GetRawBodyStringAsync();
                responseCode = "1004";

                log4net.LogicalThreadContext.Properties["requestId"] = _context.Request.Headers["requestId"].ToString();
                log4net.LogicalThreadContext.Properties["forwardedFor"] = _context.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                log4net.LogicalThreadContext.Properties["merchantId"] = mid;
                log4net.LogicalThreadContext.Properties["currentRequestUrl"] = _context.Request.HttpContext.Request.Path + _context.Request.HttpContext.Request.QueryString.Value;
                log4net.LogicalThreadContext.Properties["timeTaken"] = timetaken;
                log4net.LogicalThreadContext.Properties["status"] = _context.Response.StatusCode;
                log4net.LogicalThreadContext.Properties["contentLength"] = _context.Request.ContentLength;
                log4net.LogicalThreadContext.Properties["userAgent"] = _context.Request.Headers["User-Agent"].FirstOrDefault();
                log4net.LogicalThreadContext.Properties["request"] = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Request));
                log4net.LogicalThreadContext.Properties["response"] = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Response));
                log4net.LogicalThreadContext.Properties["accessToken"] = _context.Request.Headers["AccessToken"];
                log4net.LogicalThreadContext.Properties["responseCode"] = responseCode;
                log4net.LogicalThreadContext.Properties["method"] = _context.Request.Method;
                log4net.LogicalThreadContext.Properties["fullurl"] = string.Concat(_context.Request.Scheme, "://", _context.Request.Host, _context.Request.Path, _context.Request.QueryString);
               
            }
            catch (Exception ex)
            {
                AccessLogger.Error("accesslogerror-",ex);
               
                //throw;
            }
            finally{
               
                AccessLogger.Info(" ");
                ForensicLogger.Info(" ");
            }

        }

        public void setrequestid(HttpContext _context)
        {
            if(! _context.Request.Headers.ContainsKey("requestId"))
            {
                _context.Request.Headers["requestId"] = Guid.NewGuid().ToString();
                log4net.LogicalThreadContext.Properties["requestId"] = _context.Request.Headers["requestId"].ToString();
            }
        }

    }

    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private IRequestLogger _requestlogger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, IRequestLogger requestLogger)
        {
            _next = next;
            _requestlogger = requestLogger;
        }

        public async Task Invoke(HttpContext context)
        {
            _requestlogger.setrequestid(context);
            //First, get the incoming request
            var request = await FormatRequest(context.Request);

            //Copy a pointer to the original response body stream
            var originalBodyStream = context.Response.Body;

            //Create a new memory stream...
            using (var responseBody = new MemoryStream())
            {
                //...and use that for the temporary response body
                context.Response.Body = responseBody;

                //Continue down the Middleware pipeline, eventually returning to this class
                await _next(context);

                //Format the response from the server
                var response = await FormatResponse(context.Response);

                //TODO: Save log to chosen datastore
               _requestlogger. logrequestresponse(context, request,response);

                //Copy the contents of the new memory stream (which contains the response) to the original stream, which is then returned to the client.
                await responseBody.CopyToAsync(originalBodyStream);
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

        private async Task<string> FormatResponse(HttpResponse response)
        {
            //We need to read the response stream from the beginning...
            response.Body.Seek(0, SeekOrigin.Begin);

            //...and copy it into a string
            string text = await new StreamReader(response.Body).ReadToEndAsync();

            //We need to reset the reader for the response so that the client can read it.
            response.Body.Seek(0, SeekOrigin.Begin);

            //Return the string for the response, including the status code (e.g. 200, 404, 401, etc.)
            //return $"{response.StatusCode}: {text}";
            return $"{text}";
        }
    }
}




