#region Using statements
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Swish.Areas.Identity.Data;
using Swish.Data;
using Swish.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
#endregion

namespace Swish.Controllers
{
    public class HomeController : Controller
    {
        #region Dependency injection
        private readonly ILogger<HomeController> _logger;
        private readonly SwishDBContext _context;
        private readonly UserManager<SwishUser> _UserManager;
        private readonly IWebHostEnvironment _hostingEnvironment;

        SwishUser applicationUser;

        public HomeController(ILogger<HomeController> logger, SwishDBContext context, UserManager<SwishUser> UserManager, IWebHostEnvironment hostingEnvironment)
        {
            _logger = logger;
            _context = context;
            _UserManager = UserManager;
            _hostingEnvironment = hostingEnvironment;
        }
        #endregion 

        public async Task<SwishUser> GetUser()
        {
            try
            {
                applicationUser = await _UserManager.GetUserAsync(User);
                return applicationUser;
            }
            catch
            {
                return null;
            }
        }

        [Authorize]
        public IActionResult Index()
        {
            SwishUser Cureuser = GetUser().Result;
            if (Cureuser.UserName != null)
            {
                DbOperations DBop = new DbOperations(_context, Cureuser);

                System.Collections.Generic.List<SwishUser> friends = DBop.GetFriends();
                string picpath = DBop.GetPicPath();

                ViewBag.FriendCount = friends.Count() > 0 ? friends.Count() : 0;
                ViewBag.Friends = friends;
                ViewBag.picpath = picpath;
                ViewBag.Posts = DBop.GetPosts();
                ViewBag.Users = _context.Users.ToList();
                ViewBag.Cureuser = Cureuser;
            }
            return View();
        }

        [Authorize]
        public IActionResult Profile(string User)
        {
            if (User != null)
            {

                SwishUser CurUser = GetUser().Result;
                DbOperations DBop = new DbOperations(_context, CurUser);

                string picpath = DBop.GetPicPath(User);
                if (DBop.IsBlocked(User))
                {
                    ViewBag.IBlocked = false;
                    ViewBag.IsBlocked = true;
                    ViewBag.Posts = null;
                }
                else
                {
                    ViewBag.IBlocked = DBop.IBlocked(User);
                    ViewBag.IsBlocked = false;
                    ViewBag.Posts = DBop.GetPosts(User).ToList();
                }
                ViewBag.FriendCount = DBop.GetFriends(User).Count();
                ViewBag.picpath = picpath;
                ViewBag.IsMyFriend = DBop.CheckIfFriend(CurUser.UserName, User);
                ViewBag.user = DBop.GetUser(User);
                ViewBag.me = CurUser;
            }
            else
            {
                return RedirectToAction("Index");
            }

            return View(new PostModel());
        }

        [Authorize]
        public IActionResult Search(string Query)
        {

            if (Query != null)
            {
                DbOperations DBop = new DbOperations(_context);

                System.Collections.Generic.List<SwishUser> ResUsers = DBop.GetSrcUsers(Query);

                ViewBag.ResUsers = ResUsers;
            }
            else
            {
                return RedirectToAction("Index");
            }

            return View();
        }

        [Authorize]
        public IActionResult ChatHub()
        {
            SwishUser Cureuser = GetUser().Result;
            ViewBag.user = Cureuser;

            if (Cureuser != null)
            {
                DbOperations DBop = new DbOperations(_context, Cureuser);

                System.Collections.Generic.List<SwishUser> friends = DBop.GetFriends();
                string picpath = DBop.GetPicPath();

                ViewBag.FriendCount = friends.Count() > 0 ? friends.Count() : 0;
                ViewBag.picpath = picpath;
                ViewBag.Friends = friends;
            }
            else
            {
                return RedirectToAction("Index");
            }

            return View();
        }

        [Authorize]
        public IActionResult Post(int postID)
        {
            SwishUser Cureuser = GetUser().Result;
            ViewBag.user = Cureuser;

            if (Cureuser != null)
            {
                DbOperations DBop = new DbOperations(_context, Cureuser);

                ViewBag.picpath = DBop.GetPicPath();
                ViewBag.Cureuser = Cureuser;
                ViewBag.Post = DBop.GetPosts(Postid: postID).FirstOrDefault();
            }
            else
            {
                return RedirectToAction("Index");
            }

            return View();
        }

        [HttpGet]
        public ActionResult BlockUser(string User)
        {
            DbOperations DBop = new DbOperations(_context, GetUser().Result, _hostingEnvironment);

            DBop.BlockUser(User);

            return RedirectToAction("Profile", "Home", new { User = User });
        }

        [HttpGet]
        public ActionResult UnblockUser(string User, string returnUrl)
        {
            DbOperations DBop = new DbOperations(_context, GetUser().Result, _hostingEnvironment);

            DBop.UnblockUser(User);

            returnUrl ??= Url.Content("~/");

            return LocalRedirect(returnUrl);
            //return RedirectToAction("Profile", "Home", new { User = User, returnurl = "ReturnUrl" });
        }

        [HttpPost]
        public ActionResult Index(PostModel userPost, IFormFile[] uploadphoto)
        {
            DbOperations DBop = new DbOperations(_context, GetUser().Result, _hostingEnvironment);

            DBop.SavePost(userPost, uploadphoto);

            ViewBag.Posts = DBop.GetPosts();

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Deletecomment(int ComID)
        {
            DbOperations DBop = new DbOperations(_context, GetUser().Result, _hostingEnvironment);

            DBop.RemoveComment(ComID);

            ViewBag.Posts = DBop.GetPosts();

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult DeletePost(int PostID)
        {
            DbOperations DBop = new DbOperations(_context, GetUser().Result, _hostingEnvironment);

            DBop.RemovePost(PostID);

            ViewBag.Posts = DBop.GetPosts();

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Profile(PostModel userPost, IFormFile[] Files)
        {
            SwishUser CurUser = GetUser().Result;

            DbOperations DBop = new DbOperations(_context, CurUser, _hostingEnvironment);

            DBop.SavePost(userPost, Files);

            ViewBag.Posts = DBop.GetPosts(CurUser.UserName);

            return RedirectToAction("Profile", "Home");
        }

        [HttpPost]
        public IActionResult AddFriend(string User, string ForConfirm, string Confirmed, bool forremove = false)
        {
            var curuser = GetUser().Result;
            DbOperations DBop = new DbOperations(_context, curuser);

            if (forremove)
            {
                DBop.RemoveFrend(User);
                return RedirectToAction("Index");
            }

            DBop.SaveFrend(User, ForConfirm, Confirmed);
            return RedirectToAction("Index");

        }

        [HttpPost]
        public async void ChatHub(IFormFile file, string FileName)
        {
            if (!(file == null || file.Length == 0))
            {
                string[] ImageExt = { ".jpg", ".png", ".gif", ".jpeg" };
                string[] VideoExt = { ".mp4", ".mov" };

                Console.WriteLine(file.FileName);
                if (ImageExt.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);
                        using (Image image = Image.FromStream(memoryStream))
                        {
                            // TODO: ResizeImage(img, 100, 100);
                            int width;
                            int height;
                            const int size = 500;
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


                            Bitmap res = new Bitmap(width, height);
                            using (Graphics graphic = Graphics.FromImage(res))
                            {
                                graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                graphic.SmoothingMode = SmoothingMode.HighQuality;
                                graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                graphic.CompositingQuality = CompositingQuality.HighQuality;
                                graphic.DrawImage(image, 0, 0, width, height);
                            }
                            res.Save(_hostingEnvironment.WebRootPath + "/Images/" + FileName, ImageFormat.Png);
                        }
                    }
                }
                if (VideoExt.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
                {
                    using (var stream = new FileStream(_hostingEnvironment.WebRootPath + "/Images/" + FileName, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, string remove = "false")
        {
            SwishUser User = GetUser().Result;
            DbOperations DBop = new DbOperations(_context, User);

            if (remove == "true")
            {
                DBop.RemovePPic();

                if (System.IO.File.Exists(_hostingEnvironment.WebRootPath + "/Images/" + User.Id + "-Thumb.png"))
                {
                    System.IO.File.Delete(_hostingEnvironment.WebRootPath + "/Images/" + User.Id + "-Thumb.png");
                }
            }

            if (file == null || file.Length == 0)
            {
                return RedirectToAction("Index");
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                using (Image image = Image.FromStream(memoryStream))
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


                    Bitmap res = new Bitmap(width, height);
                    using (Graphics graphic = Graphics.FromImage(res))
                    {
                        graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphic.SmoothingMode = SmoothingMode.HighQuality;
                        graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        graphic.CompositingQuality = CompositingQuality.HighQuality;
                        graphic.DrawImage(image, 0, 0, width, height);
                    }
                    res.Save(_hostingEnvironment.WebRootPath + "/Images/" + User.Id + "-Thumb.png", ImageFormat.Png);

                    DBop.SavePPic();
                }
            }

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
