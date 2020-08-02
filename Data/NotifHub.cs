using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Swish.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Swish.Data
{
    public class NotifHub : Hub
    {
        private readonly IServiceProvider _sp;
        public NotifHub(IServiceProvider sp)
        {
            _sp = sp;
        }

        public async Task GetNotifs()
        {
            try
            {
                List<SwishUser> MyNotifs;

                using (IServiceScope scope = _sp.CreateScope())
                {
                    SwishDBContext dbContext = scope.ServiceProvider.GetRequiredService<SwishDBContext>();

                    MyNotifs = dbContext.Users.FromSqlRaw("EXECUTE dbo.GetNotifications @UserName={0}", Context.User.Identity.Name).ToList();
                }

                await Clients.Caller.SendAsync("RecieveNotifs", JsonConvert.SerializeObject(MyNotifs), Context.User.Identity.Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task SendNotifs(string User, string Message, int MsgType)
        {
            try
            {
                switch (MsgType)
                {
                    case 0:
                        await Clients.User(User).SendAsync("PopupNotifRFR");
                        break;
                    case 1:
                        SwishUser UserDet;
                        using (IServiceScope scope = _sp.CreateScope())
                        {
                            SwishDBContext dbContext = scope.ServiceProvider.GetRequiredService<SwishDBContext>();
                            UserDet = (from Userdet in dbContext.Users
                                       where Userdet.UserName == Context.User.Identity.Name
                                       select Userdet).FirstOrDefault();
                        }
                        await Clients.User(User).SendAsync("PopupNotifSFR", Message, JsonConvert.SerializeObject(UserDet), Guid.NewGuid());
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
