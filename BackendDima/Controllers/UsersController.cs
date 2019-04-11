using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendDima.DAL.Entities;
using BackendDima.Helpers;
using BackendDima.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BackendDima.Controllers
{
    [Produces("application/json")]
    [Route("api/user-portal")]
    //[ApiController]
    public class UsersController : ControllerBase
    {
        private readonly EFContext _context;
        private readonly UserManager<DbUser> _userManager;
        private readonly IEmailSender _emailSender;
        public UsersController(EFContext context,
            UserManager<DbUser> userManager,
            IEmailSender emailSender)//ctor
        {
            _userManager = userManager;
            _context = context;
            _emailSender = emailSender;
        }
        [HttpGet("users")]
        public List<UserViewModel> Get()
        {
            var model = _context.Users
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.UserProfile.FirstName,
                    LastName = u.UserProfile.LastName,
                    Age = u.UserProfile.Age,
                    Salary = u.UserProfile.Salary
                }).ToList();
            return model;
        }

        // POST api/user-portal/users
        [HttpPost("users")]
        public async Task<IActionResult> Post([FromBody]UserAddViewModel model)
        {
            string id = null;
            if (!ModelState.IsValid)
            {
                var errors = CustomValidator.GetErrorsByModel(ModelState);
                return BadRequest(errors);
            }
            var user = new DbUser() { UserName = model.Email, Email = model.Email };
            IdentityResult result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = CustomValidator.GetErrorsByIdentityResult(result);
                return BadRequest(errors);
            }
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var callbackUrl = Url.Action(
               "",
               "resetpassword",
               //pageHandler: null,
               values: new { userId = user.Id, code = code },
               protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(model.Email, "Reset Password",
               $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");

            return Ok(new { answer = "Check your email" });

            //return Ok("SEMEN");
        }
    }
}