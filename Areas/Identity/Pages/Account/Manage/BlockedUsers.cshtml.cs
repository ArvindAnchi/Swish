using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Swish.Areas.Identity.Data;
using Swish.Data;
using Swish.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Swish.Areas.Identity.Pages.Account.Manage
{
    public partial class BlockedUsersModel : PageModel
    {
        private readonly UserManager<SwishUser> _userManager;
        private readonly SwishDBContext _context;
        private readonly UserManager<SwishUser> _UserManager;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public BlockedUsersModel(UserManager<SwishUser> userManager, SwishDBContext context, UserManager<SwishUser> UserManager, IWebHostEnvironment hostingEnvironment)
        {
            _userManager = userManager;
            _context = context;
            _UserManager = UserManager;
            _hostingEnvironment = hostingEnvironment;
        }

        public string UserID { get; set; }


        private void Load(SwishUser user)
        {
            List<SwishUser> swishUsers = new List<SwishUser>();
            DbOperations dbOperations = new DbOperations(_context, user, _hostingEnvironment);
            foreach (string asd in dbOperations.GetBlockedUsers())
            {
                SwishUser BlockedUser = _userManager.FindByNameAsync(asd).Result;
                ViewData["UserID"] += asd + Environment.NewLine;
                swishUsers.Add(BlockedUser);
            }
            ViewData["ResUsers"] = swishUsers;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            SwishUser user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            Load(user);
            return Page();
        }

    }
}
