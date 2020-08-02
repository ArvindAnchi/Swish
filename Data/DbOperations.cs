#region Using statements
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;
using Swish.Areas.Identity.Data;
using Swish.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
#endregion

namespace Swish.Models
{
    public class DbOperations
    {
        #region Dependency injection
        private readonly SwishDBContext _context;
        private readonly SwishUser Cureuser;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public DbOperations(SwishDBContext context, SwishUser _Cureuser = null, IWebHostEnvironment hostingEnvironment = null)
        {
            _context = context;
            Cureuser = _Cureuser;
            _hostingEnvironment = hostingEnvironment;
        }
        #endregion

        #region Read operations
        public bool IsBlocked(string User)
        {
            if ((from fc in _context.BlockedModel where (fc.UserID == User && fc.OtherUserID == Cureuser.UserName) select fc.UserID).ToList().Count() > 0)
                return true;
            return false;
        }
        public bool IBlocked(string User)
        {
            if ((from fc in _context.BlockedModel where (fc.OtherUserID == User && fc.UserID == Cureuser.UserName) select fc.UserID).ToList().Count() > 0)
                return true;
            return false;
        }
        public List<string> GetBlockedUsers()
        {
            return (from fc in _context.BlockedModel where fc.UserID == Cureuser.UserName select fc.OtherUserID).ToList();
        }
        public List<SwishUser> GetFriends(string UserName = null)
        {
            return UserName != null
                ? _context.Users.FromSqlRaw("EXECUTE dbo.GetFriends @MyUserName={0}", UserName).ToList()
                : _context.Users.FromSqlRaw("EXECUTE dbo.GetFriends @MyUserName={0}", Cureuser.UserName).ToList();
        }
        public string GetPicPath(string UserName = null)
        {
            return UserName != null
                ? (from fc in _context.Users
                   where (fc.UserName == UserName)
                   select fc.PPicPath).FirstOrDefault()
                : (from fc in _context.Users
                   where (fc.UserName == Cureuser.UserName)
                   select fc.PPicPath).FirstOrDefault();
        }
        public IQueryable<PostViewModel> GetPosts(string User = "All", int Postid = -1)
        {
            List<SwishUser> Friends = GetFriends();
            return (from allposts in _context.UserPost
                    orderby allposts.PDate descending
                    where ((User == "All" ? true : allposts.UserID == User) && (Postid == -1 ? true : allposts.PostID == Postid))
                    select new PostViewModel
                    {
                        PostID = allposts.PostID,
                        PLikes = allposts.PLikes,
                        PostTxt = allposts.PText,
                        PostDt = allposts.PDate,
                        PostImages = (from PImages in _context.PostImages
                                      where PImages.postModel.PostID == allposts.PostID
                                      select new pImage
                                      {
                                          Image = PImages.ImageFileName,
                                          IsVideo = PImages.IsVideo
                                      }).ToList(),
                        Comments = (from allcomments in _context.CommentsModels
                                    join Commenter in _context.Users on allcomments.UserID equals Commenter.UserName
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
                                    }).Take(Postid == -1 ? 5 : (from allcomments in _context.CommentsModels
                                                                join Commenter in _context.Users on allcomments.UserID equals Commenter.UserName
                                                                where allcomments.PostId == allposts.PostID
                                                                select new
                                                                {
                                                                    ComID = allcomments.CommentID
                                                                }).Count()).ToList(),
                        PosterUser = (from Poster in _context.Users
                                      where (allposts.UserID == Poster.UserName)
                                      select Poster
                                      ).FirstOrDefault(),
                        mylike = (from AllMessages in _context.LikedPosts
                                  where (AllMessages.UName == Cureuser.UserName && AllMessages.PostId == allposts.PostID)
                                  select new
                                  {
                                      AllMessages.UName
                                  }).Count()
                    }).Where(Poster => Friends.Contains(Poster.PosterUser) || Poster.PosterUser == Cureuser).Take(5);
        }
        public int CheckIfFriend(string Me, string User)
        {
            return _context.IsFriendViewModel.FromSqlRaw("EXECUTE dbo.CheckIfFriend @MyUserName={0}, @ChkUserName={1}", Me, User).AsEnumerable().FirstOrDefault().FCode;
        }
        public SwishUser GetUser(string UserName = null)
        {
            return (from fc in _context.Users
                    where (fc.UserName == UserName)
                    select fc).FirstOrDefault();
        }
        public List<SwishUser> GetSrcUsers(string Query)
        {
            if (Query[0] == '@')
            {
                return (from fc in _context.Users
                        where EF.Functions.Like(fc.UserName, '%' + Query.Substring(1) + '%')
                        select fc).ToList();
            }
            else
            {
                return (from fc in _context.Users
                        where (EF.Functions.Like(fc.FName, '%' + Query + '%') || EF.Functions.Like(fc.LName, '%' + Query + '%'))
                        select fc).ToList();
            }
        }
        #endregion

        #region Write operations
        public void BlockUser(string user)
        {
            BlockedModel t = new BlockedModel
            {
                UserID = Cureuser.UserName,
                OtherUserID = user
            };

            _context.BlockedModel.Add(t);
            _context.SaveChanges();
        }
        public void SavePost(PostModel userPost, IFormFile[] Files)
        {
            List<PostImages> postImages = new List<PostImages>();

            if (Files != null || Files.Length > 0)
            {
                string[] ImageExt = { ".jpg", ".png", ".gif", ".jpeg" };
                string[] VideoExt = { ".mp4", ".mov" };

                foreach (IFormFile File in Files)
                {
                    Debug.WriteLine(Path.GetExtension(File.FileName));

                    if (ImageExt.Contains(Path.GetExtension(File.FileName).ToLowerInvariant()))
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            File.CopyToAsync(memoryStream);
                            using (System.Drawing.Image image = System.Drawing.Image.FromStream(memoryStream))
                            {
                                // TODO: ResizeImage(img, 100, 100);
                                int width;
                                int height;
                                const int size = 100;

                                if (image.Width > image.Height)
                                {
                                    width = size;
                                    height = Convert.ToInt32(image.Height * size / (double)image.Width);
                                }
                                else
                                {
                                    width = Convert.ToInt32(image.Width * size / (double)image.Height);
                                    height = size;
                                }

                                var imagepath = Path.GetFileNameWithoutExtension(File.FileName) + Guid.NewGuid();

                                using (System.Drawing.Bitmap res = new System.Drawing.Bitmap(image.Width, image.Height))
                                {
                                    using (System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(res))
                                    {
                                        graphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                        graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                                        graphic.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                                        graphic.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                                        graphic.DrawImage(image, 0, 0, image.Width, image.Height);
                                    }

                                    using (MemoryStream memory = new MemoryStream())
                                    {
                                        using (FileStream fs = new FileStream(_hostingEnvironment.WebRootPath + "/Images/" + imagepath + ".png", FileMode.Create, FileAccess.ReadWrite))
                                        {
                                            res.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                                            byte[] bytes = memory.ToArray();
                                            fs.Write(bytes, 0, bytes.Length);
                                        }
                                    }
                                }

                                using (System.Drawing.Bitmap res = new System.Drawing.Bitmap(width, height))
                                {
                                    using (System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(res))
                                    {
                                        graphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                        graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                                        graphic.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                                        graphic.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                                        graphic.DrawImage(image, 0, 0, width, height);
                                    }

                                    using (MemoryStream memory = new MemoryStream())
                                    {
                                        using (FileStream fs = new FileStream(_hostingEnvironment.WebRootPath + "/Images/" + imagepath + "-Thumb.png", FileMode.Create, FileAccess.ReadWrite))
                                        {
                                            res.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                                            byte[] bytes = memory.ToArray();
                                            fs.Write(bytes, 0, bytes.Length);
                                        }
                                    }

                                    PostImages postImage = new PostImages()
                                    {
                                        ImageFileName = imagepath,
                                        IsVideo = false
                                    };
                                    postImages.Add(postImage);
                                }
                            }
                        }
                    }
                    if (VideoExt.Contains(Path.GetExtension(File.FileName).ToLowerInvariant()))
                    {
                        var imagepath = Path.GetFileNameWithoutExtension(File.FileName) + Guid.NewGuid();

                        using (var stream = new FileStream(_hostingEnvironment.WebRootPath + "/Images/" + imagepath + Path.GetExtension(File.FileName), FileMode.Create))
                        {
                            File.CopyTo(stream);
                        }

                        PostImages postImage = new PostImages()
                        {
                            ImageFileName = imagepath + Path.GetExtension(File.FileName),
                            IsVideo = true
                        };
                        postImages.Add(postImage);
                    }
                }
                _context.PostImages.AddRange(postImages);
                _context.SaveChanges();
            }

            if (userPost.PText != null)
            {
                PostModel t = new PostModel
                {
                    UserID = Cureuser.UserName,
                    PText = userPost.PText,
                    PDate = DateTime.Now,
                    postImages = postImages
                };

                _context.UserPost.Add(t);
                _context.SaveChanges();
            }
        }
        public void SavePPic()
        {
            SwishUser entity = _context.Users.FirstOrDefault(Me => Me.UserName == (Cureuser.UserName));
            if (entity != null)
            {
                entity.PPicPath = Cureuser.Id + "-Thumb.png";
                _context.Users.Update(entity);
                _context.SaveChanges();
            }
        }
        public void RemovePPic()
        {
            Console.WriteLine("<------------Remove profile pic------------>");
            SwishUser entity = _context.Users.FirstOrDefault(Me => Me.UserName == (Cureuser.UserName));
            if (entity != null)
            {
                entity.PPicPath = "ProfilePlaceholder.png";
                _context.Users.Update(entity);
                _context.SaveChanges();
            }
        }
        public void SaveFrend(string User, string ForConfirm, string Confirmed)
        {
            if (ForConfirm == "true")
            {
                if (Confirmed == "true")
                {
                    FriendsModel entity = _context.Friends.FirstOrDefault(item => item.FriendKey == (Cureuser.UserName + User));
                    if (entity != null)
                    {
                        entity.Confirmed = true;
                        _context.Friends.Update(entity);
                        _context.SaveChanges();
                    }
                }
                else
                {
                    FriendsModel entity = _context.Friends.FirstOrDefault(item => item.FriendKey == (Cureuser.UserName + User));
                    if (entity != null)
                    {
                        entity.Confirmed = true;
                        _context.Friends.Remove(entity);
                        _context.SaveChanges();
                    }
                }
            }
            else
            {
                FriendsModel t = new FriendsModel
                {
                    FriendKey = User + Cureuser.UserName,
                    User1 = Cureuser.UserName,
                    User2 = User
                };

                _context.Friends.Add(t);
                _context.SaveChanges();
            }
        }
        #endregion

        #region Remove operations
        public void RemoveFrend(string user)
        {
            _context.Database.ExecuteSqlRaw("EXECUTE dbo.RemoveFriend @MyUserName={0}, @UserName={1}", Cureuser.UserName, user);
        }
        public void RemovePost(int PostID)
        {
            _context.UserPost.RemoveRange(_context.UserPost.Where(x => x.PostID == PostID));
            _context.SaveChanges();
        }
        public void RemoveComment(int CommentID)
        {
            var entity = _context.CommentsModels.FirstOrDefault(x => x.CommentID == CommentID);
            entity.Deleted = true;
            _context.Update(entity);
            _context.SaveChanges();
        }
        public void UnblockUser(string user)
        {
            _context.BlockedModel.RemoveRange(_context.BlockedModel.Where(x => x.UserID == Cureuser.UserName && x.OtherUserID == user));
            _context.SaveChanges();
        }
        #endregion  
    }
}