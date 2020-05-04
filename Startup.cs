using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using log4net;
using System.Reflection;
using Core_all.Controllers.Logging;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Core_all.Controllers.RateLimitter;

namespace Core_all
{
    public class Startup
    {
       
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            //services.AddHttpContextAccessor();
            //services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IRequestLogger, Log4netLogger>();
            // services.AddMvc().AddMvcOptions(options =>
            //options.Filters.Add(new ErrorHandlingFilter()));
            services.AddScoped<ErrorHandlingFilter>();
            services.AddMemoryCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,ILogger<Startup> logger, ILoggerFactory LogFactory,IRequestLogger requestLogger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error/500");
            }

           // requestLogger.setrequestid(accessor.HttpContext);

            LogFactory.AddLog4Net();

            app.UseMiddleware<RateLimitHandler>(requestLogger);

            app.UseMiddleware<RequestResponseLoggingMiddleware>(requestLogger);

            //app.Use(async (con,next) =>
            //{
            //    // var utility = new Log4netLogger(con);
            //    requestLogger.setrequestid(con);
            //      //System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            //      //watch.Start();

            //      //await con.Response.WriteAsync("done");
            //      // logger.LogInformation("logg before");

            //  // Task<HttpResponseMessage> response=  await next.Invoke();


            //    requestLogger.logrequestresponse(con);

            //    //watch.Stop();
               
            //   // logger.LogInformation("logg after Timetaken:" + watch.ElapsedMilliseconds );
            //});


           

           // var alogger = LogManager.GetLogger(Assembly.GetAssembly(typeof(Startup)), "AccessLog");

            //ILog logger1 = LogManager.GetLogger("AccessLog");
            //var repository = logger1?.Logger.Repository;

          //  alogger.Info("from A-accesslog appender");

            // app.UseDefaultFiles();
            //app.UseStaticFiles();
            app.UseDefaultFiles();

            app.UseMvc();

            //app.Run(async context => {
            //    throw new Exception("custom error");
            //    await context.Response.WriteAsync("test");

            //});

            


        }
    }
}
