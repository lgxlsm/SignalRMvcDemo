using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;

[assembly: OwinStartup(typeof(SignalRMvcDemo.StartupSignalR))]

namespace SignalRMvcDemo
{
    public class StartupSignalR
    {
        public void Configuration(IAppBuilder app)
        {
            //允许CORS跨域
            app.UseCors(CorsOptions.AllowAll);
            #region Redis配置
            //添加redis
            RedisScaleoutConfiguration redisScaleoutConfiguration = new RedisScaleoutConfiguration("192.168.10.134", 6379, "111111", "redis_signalr");
            //连接DB，默认为0
            redisScaleoutConfiguration.Database = 3;
            //SignalR用Redis
            GlobalHost.DependencyResolver.UseRedis(redisScaleoutConfiguration);
            #endregion
            // 有关如何配置应用程序的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkID=316888
            app.MapSignalR();//启动SignalR
        }
    }
}
