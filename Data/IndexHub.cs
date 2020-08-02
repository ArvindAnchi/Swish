using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Swish.Areas.Identity.Data;
using Swish.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Swish.Data
{
    public class IndexHub : Hub
    {
        private readonly IServiceProvider _sp;
        public IndexHub(IServiceProvider sp)
        {
            _sp = sp;
        }
        public async Task SaveComment(int PostId, string Comment)
        {
            try
            {
                using (IServiceScope scope = _sp.CreateScope())
                {

                    SwishDBContext dbContext = scope.ServiceProvider.GetRequiredService<SwishDBContext>();
                    CommentModel t = new CommentModel
                    {
                        UserID = Context.User.Identity.Name,
                        PostId = PostId,
                        Comment = Comment,
                        CLikes = 0,
                        Deleted = false
                    };

                    Console.WriteLine(PostId + " : " + Comment);

                    dbContext.CommentsModels.Add(t);
                    dbContext.SaveChanges();
                    
                    await Clients.Caller.SendAsync("LikedCom", t.CommentID);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public async Task LikePost(int PostId, int likes)
        {
            Console.WriteLine(PostId);
            try
            {
                string mylike;
                using (IServiceScope scope = _sp.CreateScope())
                {
                    SwishDBContext dbContext = scope.ServiceProvider.GetRequiredService<SwishDBContext>();
                    likes = dbContext.UserPost.FromSqlRaw("EXECUTE dbo.LikePost @UName={0}, @PostID={1}",
                        Context.User.Identity.Name,
                        PostId).ToList().FirstOrDefault().PLikes;

                    mylike = (from AllMessages in dbContext.LikedPosts
                              where (AllMessages.UName == Context.User.Identity.Name && AllMessages.PostId == PostId)
                              select new
                              {
                                  AllMessages.UName
                              }).FirstOrDefault().UName;

                }
                Console.WriteLine("PostID: " + PostId + "\nLikes: " + likes);
                await Clients.All.SendAsync("GetPostLikes", PostId, likes, "1" + mylike);
            }
            catch (Exception ex)
            {
                await Clients.All.SendAsync("GetPostLikes", PostId, likes, "0" + Context.User.Identity.Name);
                Console.WriteLine(ex.ToString());
            }
        }
        public async Task LikeComment(int CommentId, int likes)
        {
            try
            {
                using (IServiceScope scope = _sp.CreateScope())
                {
                    SwishDBContext dbContext = scope.ServiceProvider.GetRequiredService<SwishDBContext>();
                    likes = dbContext.CommentsModels.FromSqlRaw("EXECUTE dbo.LikeComment @UName={0}, @CommentID={1}",
                        Context.User.Identity.Name,
                        CommentId).FirstOrDefault().CLikes;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            await Clients.All.SendAsync("GetCommentLikes", CommentId, likes);
        }
        public async Task RetrievePosts(int skip, string User = "All")
        {
            try
            {
                List<PostViewModel> Posts;
                List<SwishUser> MyFriends;

                using (IServiceScope scope = _sp.CreateScope())
                {
                    SwishDBContext dbContext = scope.ServiceProvider.GetRequiredService<SwishDBContext>();

                    MyFriends = dbContext.Users.FromSqlRaw("EXECUTE dbo.GetFriends @MyUserName={0}", Context.User.Identity.Name).ToList();

                    Posts = (from allposts in dbContext.UserPost
                             orderby allposts.PDate descending
                             where (User == "All" ? true : allposts.UserID == User)
                             select new PostViewModel
                             {
                                 PostID = allposts.PostID,
                                 PLikes = allposts.PLikes,
                                 PostTxt = allposts.PText,
                                 PostDt = allposts.PDate,
                                 PostImages = (from PImages in dbContext.PostImages
                                               where PImages.postModel.PostID == allposts.PostID
                                               select new pImage
                                               {
                                                   Image = PImages.ImageFileName,
                                                   IsVideo = PImages.IsVideo
                                               }).ToList(),
                                 Comments = (from allcomments in dbContext.CommentsModels
                                             join Commenter in dbContext.Users on allcomments.UserID equals Commenter.UserName
                                             orderby allcomments.CommentID descending
                                             where allcomments.PostId == allposts.PostID
                                             select new CommentViewModel
                                             {
                                                 ComID = allcomments.CommentID,
                                                 UName = Commenter.UserName,
                                                 Fname = Commenter.FName,
                                                 Lname = Commenter.LName,
                                                 PPicPath = Commenter.PPicPath,
                                                 CommentTxt = allcomments.Comment,
                                                 CommentLikes = allcomments.CLikes,
                                                 Deleted = allcomments.Deleted
                                             }).Take(5).ToList(),
                                 PosterUser = (from Poster in dbContext.Users
                                               where (allposts.UserID == Poster.UserName)
                                               select Poster
                                               ).FirstOrDefault(),
                                 mylike = (from AllMessages in dbContext.LikedPosts
                                           where (AllMessages.UName == Context.User.Identity.Name && AllMessages.PostId == allposts.PostID)
                                           select new
                                           {
                                               AllMessages.UName
                                           }).Count()
                             }).Where(Poster => MyFriends.Contains(Poster.PosterUser) || Poster.PosterUser.UserName == Context.User.Identity.Name).Skip(skip).Take(5).ToList();
                }

                Console.WriteLine("<---------------------Posts------------------->");
                Console.WriteLine(JsonConvert.SerializeObject(Posts));
                Console.WriteLine("<---------------------Friends------------------->");
                Console.WriteLine(JsonConvert.SerializeObject(MyFriends));
                Console.WriteLine("<---------------------End------------------->");
                await Clients.Caller.SendAsync("GetPosts", JsonConvert.SerializeObject(Posts));
                //await Clients.Caller.SendAsync("GetPosts", Posts);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public async Task AddFriend(string User, string ForConfirm, string Confirmed, bool forremove = false)
        {
            try
            {
                using (IServiceScope scope = _sp.CreateScope())
                {
                    SwishDBContext dbContext = scope.ServiceProvider.GetRequiredService<SwishDBContext>();

                    if (forremove)
                    {
                        dbContext.Database.ExecuteSqlRaw("EXECUTE dbo.RemoveFriend @MyUserName={0}, @UserName={1}", Context.User.Identity.Name, User);
                        return;
                    }

                    if (ForConfirm == "true")
                    {
                        if (Confirmed == "true")
                        {
                            FriendsModel entity = dbContext.Friends.FirstOrDefault(item => item.FriendKey == (Context.User.Identity.Name + User));
                            if (entity != null)
                            {
                                entity.Confirmed = true;
                                dbContext.Friends.Update(entity);
                                dbContext.SaveChanges();
                            }
                        }
                        else
                        {
                            FriendsModel entity = dbContext.Friends.FirstOrDefault(item => item.FriendKey == (Context.User.Identity.Name + User));
                            if (entity != null)
                            {
                                entity.Confirmed = true;
                                dbContext.Friends.Remove(entity);
                                dbContext.SaveChanges();
                            }
                        }
                    }
                    else
                    {
                        FriendsModel t = new FriendsModel
                        {
                            FriendKey = User + Context.User.Identity.Name,
                            User1 = Context.User.Identity.Name,
                            User2 = User
                        };

                        dbContext.Friends.Add(t);
                        dbContext.SaveChanges();
                    }

                    await Clients.User(User).SendAsync("ReceiveNotif", ForConfirm, Context.User.Identity.Name);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}