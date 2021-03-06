using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using SampleAspNetCoreAuth.Authorization;
using SampleAspNetCoreAuth.Constant;

namespace SampleAspNetCoreAuth
{
    //项目的配置类，项目的主要配置信息都在这个类里，它被Program.cs载入
    //本配置类的作用是实现基于Cookie的认证
    // 参考: 
    //ASP.NET Core 认证与授权[2]:Cookie认证 https://www.cnblogs.com/RainingNight/p/cookie-authentication-in-asp-net-core.html
    //https://github.com/RainingNight/AspNetCoreSample/tree/master/src/Functional/Authentication/CookieSample
    //使用 cookie 而无需 ASP.NET Core 标识的身份验证 https://docs.microsoft.com/zh-cn/aspnet/core/security/authentication/cookie
    public class StartupCookie
    {
        public StartupCookie(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Events = new CookieAuthenticationEvents()
                {
                    OnRedirectToLogin = context =>      //在这里重新定义认证失败时的行为，默认系统会自动跳转到LoginPath指定的url
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        // added for .NET Core 1.0.1 and above (thanks to @Sean for the update)
                        context.Response.WriteAsync("{\"error\": " + context.Response.StatusCode + "; Unauthorized" + "}");

                        //context.Response.Redirect(context.RedirectUri);       跳转的写法
                        return Task.CompletedTask;
                    },

                    OnRedirectToAccessDenied = context =>      //在这里重新定义授权失败时的行为，默认系统会自动跳转到AccessDeniedPath指定的url
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        // added for .NET Core 1.0.1 and above (thanks to @Sean for the update)
                        context.Response.WriteAsync("{\"error\": " + context.Response.StatusCode + ",AccessDenied" + "}");

                        //context.Response.Redirect(context.RedirectUri);       跳转的写法
                        return Task.CompletedTask;
                    }
                };

                // 在这里可以根据需要添加一些Cookie认证相关的配置，在本次示例中使用默认值就可以了。
                options.Cookie.HttpOnly = true;         //cookie是否httpOnly
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);       //超时时间
                options.Cookie.Name = "NCOOKIE";        //cookie名称
                options.LoginPath = "/cookie/user/login";        //登入url页
                options.LogoutPath = "/user/dologout";      //登出url
                options.AccessDeniedPath = "/user/accessdeny";        //拒绝地址
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();     //HttpContext中间件
            
            //基于策略的授权配置方法
            services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();     //权限处理Handler注册
            
            services.AddAuthorization(options =>            //把权限和Requirement关联
            {   
                
                options.AddPolicy(Permissions.UserCreate, policy => policy.AddRequirements(new PermissionAuthorizationRequirement(Permissions.UserCreate)));
                options.AddPolicy(Permissions.UserRead, policy => policy.AddRequirements(new PermissionAuthorizationRequirement(Permissions.UserRead)));
                options.AddPolicy(Permissions.UserUpdate, policy => policy.AddRequirements(new PermissionAuthorizationRequirement(Permissions.UserUpdate)));
                options.AddPolicy(Permissions.UserDelete, policy => policy.AddRequirements(new PermissionAuthorizationRequirement(Permissions.UserDelete)));
                 
                
            });
            
            


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();      //强制https跳转
            app.UseStatusCodePages();       //使用HTTP错误代码页中间件(该选项检查状态代码介于400和599之间且没有正文的响应,给他们加上状态码内容，注意中间有顺序问题，不能加在ExceptionHandler前面)
            app.UseCookiePolicy();          //Cookie 策略中间件

            app.UseAuthentication();        //添加了身份验证中间件到请求管道
            app.UseMvc();
        }
    }
}