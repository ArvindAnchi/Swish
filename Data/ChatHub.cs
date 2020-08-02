using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Swish.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Swish.Models
{
    public class ChatHub : Hub
    {
        private readonly IServiceProvider _sp;
        public readonly IWebHostEnvironment _hostEnvironment;
        public ChatHub(IServiceProvider sp, IWebHostEnvironment hostEnvironment)
        {
            _sp = sp;
            _hostEnvironment = hostEnvironment;
        }
        public async Task SendMessage(string Reciever, string message = null, string Chtimage = null)
        {
            DateTime datetime = DateTime.Now;

            using (IServiceScope scope = _sp.CreateScope())
            {
                SwishDBContext dbContext = scope.ServiceProvider.GetRequiredService<SwishDBContext>();
                try
                {
                    ChatModel t = new ChatModel
                    {
                        Sender = Context.UserIdentifier,
                        Reciever = Reciever,
                        Message = message,
                        Image = Chtimage,
                        dateTime = datetime
                    };

                    dbContext.ChatModel.Add(t);
                    dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            try
            {

                await Clients.User(Reciever).SendAsync("ReceiveMessage", Context.UserIdentifier, Context.User.Identity.Name, message, Chtimage, datetime.ToString("HH:mm"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public async Task AskForMessages(string Reciever)
        {
            Console.WriteLine("AskForMessages");
            List<string[]> messages = new List<string[]>();

            using (IServiceScope scope = _sp.CreateScope())
            {
                SwishDBContext dbContext = scope.ServiceProvider.GetRequiredService<SwishDBContext>();
                try
                {

                    var Messages = from AllMessages in dbContext.ChatModel
                                   where (AllMessages.Sender == Context.UserIdentifier && AllMessages.Reciever == Reciever) || (AllMessages.Sender == Reciever && AllMessages.Reciever == Context.UserIdentifier)
                                   select new
                                   {
                                       AllMessages.dateTime,
                                       AllMessages.Message,
                                       AllMessages.Image,
                                       Sender =
                                       (
                                           AllMessages.Sender == Reciever ? "Recieved" :
                                           AllMessages.Sender == Context.UserIdentifier ? "Sent" : "Unknown"
                                       )
                                   };
                    Console.WriteLine(Messages);
                    foreach (var RecievedMessage in Messages)
                    {
                        string[] message = new string[4];
                        message[0] = (RecievedMessage.Message);
                        message[1] = (RecievedMessage.Sender);
                        message[2] = (RecievedMessage.dateTime.ToString("HH:mm"));
                        message[3] = (RecievedMessage.Image);
                        Console.WriteLine(RecievedMessage.Message + " " + RecievedMessage.Sender + " " + RecievedMessage.dateTime.ToString("HH:mm"));
                        messages.Add(message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            await Clients.User(Context.UserIdentifier).SendAsync("GetMessagesFromUser", messages);
        }
    }
}
