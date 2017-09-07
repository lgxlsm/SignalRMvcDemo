using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SignalRMvcDemo
{
    [HubName("MessageHub")]
    public class MessageHub : Hub
    {
        //当前用户
        public static List<UserInfo> OnlineUsers = new List<UserInfo>(); // 在线用户列表
        RedisClient client = new RedisClient("192.168.10.134", 6379, "111111", 3);

        /// <summary>
        /// 登录连线
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="userName">用户名</param>
        public void Register(string userName)
        {
            OnlineUsers = client.Get<List<UserInfo>>("list") ?? new List<UserInfo>();
            var connnectId = Context.ConnectionId;
            if (!OnlineUsers.Any(x => x.ConnectionId == connnectId))
            {
                //添加在线人员
                OnlineUsers.Add(new UserInfo
                {
                    ConnectionId = connnectId,
                    UserName = userName,
                    LastLoginTime = DateTime.Now
                });
            }
            // 所有客户端同步在线用户
            Clients.All.onConnected(connnectId, userName, OnlineUsers);
            client.Set<List<UserInfo>>("list", OnlineUsers);
        }

        /// <summary>
        /// 发送私聊
        /// </summary>
        /// <param name="toUserId">接收方用户ID</param>
        /// <param name="message">内容</param>
        public void SendPrivateMessage(string toConnectionId, string message)
        {
            OnlineUsers = client.Get<List<UserInfo>>("list") ?? new List<UserInfo>();
            var fromConnectionId = Context.ConnectionId;

            var toUser = OnlineUsers.FirstOrDefault(x => x.ConnectionId == toConnectionId);
            var fromUser = OnlineUsers.FirstOrDefault(x => x.ConnectionId == fromConnectionId);

            if (toUser != null)
            {
                Clients.Client(toUser.ConnectionId).receivePrivateMessage(fromUser.ConnectionId, fromUser.UserName, message);
            }
            else
            {
                //表示对方不在线
                Clients.Caller.absentSubscriber();
            }
        }

        /// <summary>
        /// 全部发送
        /// </summary>
        /// <param name="message"></param>
        public void AllSend(string name, string message)
        {
            Clients.All.AllSend(name, message);
        }

        /// <summary>
        /// 连线时调用
        /// </summary>
        /// <returns></returns>
        public override Task OnConnected()
        {
            //Console.WriteLine("客户端连接，连接ID是:{0},当前在线人数为{1}", Context.ConnectionId, OnlineUsers.Count + 1);
            return base.OnConnected();
        }

        /// <summary>
        /// 断线时调用
        /// </summary>
        /// <param name="stopCalled"></param>
        /// <returns></returns>
        public override Task OnDisconnected(bool stopCalled)
        {
            OnlineUsers = client.Get<List<UserInfo>>("list")??new List<UserInfo>();
            var user = OnlineUsers.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);

            // 判断用户是否存在,存在则删除
            if (user == null)
            {
                return base.OnDisconnected(stopCalled);
            }
            // 删除用户
            OnlineUsers.Remove(user);
            Clients.All.onUserDisconnected(OnlineUsers);   //调用客户端用户离线通知
            client.Set<List<UserInfo>>("list", OnlineUsers);

            return base.OnDisconnected(stopCalled);
        }

        /// <summary>
        /// 重新连接时调用
        /// </summary>
        /// <returns></returns>
        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }
    }


    public class UserInfo
    {
        public string ConnectionId { get; set; }
        public string UserName { get; set; }
        public DateTime LastLoginTime { get; set; }
    }
}