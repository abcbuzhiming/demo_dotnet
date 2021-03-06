using IdentityModel;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace SampleAspNetCoreAuth.Authorization
{
    /// <summary>
    /// 基于策略的授权处理程序，配合PermissionAuthorizationRequirement使用
    /// </summary>
    /// <remark>这个处理程序需要设定特定的策略，才能运行，每个策略只能针对一条规则，使用很繁琐</remark>
    /// <typeparam name="PermissionAuthorizationRequirement"></typeparam>
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionAuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionAuthorizationRequirement requirement)
        {
            Console.WriteLine("PermissionAuthorizationHandler.HandleRequirementAsync:" + requirement.Name);
            if(context.User == null){
                Console.WriteLine("context.User is null");
                return Task.CompletedTask;
            }
            
            //var userIdClaim = context.User.FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier);       //cookie的方式
            var userIdClaim = context.User.FindFirst(claim => claim.Type == JwtClaimTypes.Id);         //jwt方式
            if (userIdClaim == null)
            {
                Console.WriteLine("userIdClaim is null");
                return Task.CompletedTask;
            }
            var role = context.User.FindFirst(claim => claim.Type == ClaimTypes.Role); 
            Console.WriteLine("role:" + role);
            //从这里，引入自定义存储，检测用户是否有足够的权限

            //if (_userStore.CheckPermission(int.Parse(userIdClaim.Value), requirement.Name))
            //{
                context.Succeed(requirement);
            //}
            return Task.CompletedTask;
        }
    }
}