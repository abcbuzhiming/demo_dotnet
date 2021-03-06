using System;
using System.Text;
using IdentityModel;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.IdentityModel.Tokens;
using SampleAspNetCoreAuth.Data;
using Microsoft.AspNetCore.Authorization;
using SampleAspNetCoreAuth.Authorization;
using SampleAspNetCoreAuth.Constant;


namespace SampleAspNetCoreAuth
{
    //项目的配置类，项目的主要配置信息都在这个类里，它被Program.cs载入
    //本配置类的作用是实现基于json web token的认证，jwt附加在http header中
    //此配置需要第三方依赖IdentityModel。dotnet add package IdentityModel
    // 参考: 
    //ASP.NET Core 认证与授权[4]:JwtBearer认证 https://www.cnblogs.com/RainingNight/p/jwtbearer-authentication-in-asp-net-core.html
    //参考示例代码 https://github.com/RainingNight/AspNetCoreSample/tree/master/src/Functional/Authentication/JwtBearerSample
    
    public class StartupJwt
    {
        public StartupJwt(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                //JwtBearer认证中，默认是通过Http的Authorization头来获取的.如果要自定义，就需要自定义事件
                //自定义处理事件，当获取msg的时候，从query中得到token。用这个方法
                /*
                options.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Query["access_token"];
                        return Task.CompletedTask;
                    }
                };
                 */
                
                                 

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = JwtClaimTypes.Name,     //定义哪些属性的key
                    RoleClaimType = ClaimTypes.Role,   //这里似乎有bug，和实际的JwtClaimTypes不符合的时候可以通过授权，符合了反而通不过了

                    ValidIssuer = "http://localhost:5200",      //Token颁发机构
                    ValidAudience = "api",                      //颁发给谁
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Consts.Secret)),     //签名秘钥
                    
                    /***********************************TokenValidationParameters的参数默认值***********************************/
                    // RequireSignedTokens = true,
                    // SaveSigninToken = false,
                    // ValidateActor = false,
                    // 将下面两个参数设置为false，可以不验证Issuer和Audience，但是不建议这样做。
                    // ValidateAudience = true,
                    // ValidateIssuer = true, 
                    // ValidateIssuerSigningKey = false,
                    // 是否要求Token的Claims中必须包含Expires
                    // RequireExpirationTime = true,
                    // 允许的服务器时间偏移量
                    // ClockSkew = TimeSpan.FromSeconds(300),
                    // 是否验证Token有效期，使用当前时间与Token的Claims中的NotBefore和Expires对比
                    ValidateLifetime = true
                };
            });

            //services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();     //HttpContext中间件

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
            app.UseAuthentication();        //添加了身份验证中间件到请求管道
            app.UseMvc();
        }
    }
}