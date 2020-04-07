using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cw5.Middlewars
{
    public class LogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string Path = @"d:\Users\User\Desktop\APBD\Cw5\Logs.txt";
        public LogMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            httpContext.Request.EnableBuffering();

            string ToWrite = string.Empty;
            ToWrite += "Method: " + httpContext.Request.Method + "\n";
            ToWrite += "Path: " + httpContext.Request.Path + "\n";
          
            var bodyStream = string.Empty;

            using (var reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8, true, 1024, true))
            {
                bodyStream = await reader.ReadToEndAsync();
            }

            ToWrite += "Body: " + bodyStream + "\n";
            ToWrite += "Query: " + httpContext.Request.Query.ToString() + "\n\n\n";

            File.WriteAllText(Path, ToWrite);


            httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
            await _next(httpContext);
        }
    }
}
