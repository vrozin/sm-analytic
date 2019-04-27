using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sm_analytic.Models;

namespace sm_analytic.Controllers
{
    [ApiController]
    [EnableCors("AllowMyOrigin")]
    public class DashboardController : ControllerBase
    {
        private readonly DataDbContext _dataDbContext;
        private readonly ClaimsPrincipal _userCaller;
        private readonly UserManager<IdentityCustomModel> _userManager;

        private readonly string _adminEmail;
        private readonly string _emailPassword;

        public DashboardController(DataDbContext dataDbContext, IHttpContextAccessor httpContextAccessor, UserManager<IdentityCustomModel> userManager)
        {
            _userCaller    = httpContextAccessor.HttpContext.User;
            _dataDbContext = dataDbContext;
            _userManager   = userManager;

            _adminEmail    = Environment.GetEnvironmentVariable("AdminEmail");
            _emailPassword = Environment.GetEnvironmentVariable("EmailPassword");
        }

        /// <summary>
        /// Getting details of already logged in user
        /// </summary>
        /// <returns>First and last names, email, date of birth</returns>
        [Route("~/api/Dashboard/GetUserDetails")]
        [Authorize(Policy = "SMAnalytic")]
        [HttpGet]
        public async Task<IActionResult> GetCallerDetails()
        {

            Console.WriteLine("!!!____ _userCaller: isSET:" + "authenticated=" + _userCaller.Identity.IsAuthenticated + "  " + _userCaller.Identity.Name + "  Claim count=" + _userCaller.Claims.Count());
            _userCaller.Claims.ToList().ForEach(i => Console.WriteLine("!!!____ _userCaller.Claims: "+ i.Type + "  " + i.Value));

            var userId = _userCaller.Claims.Single(i => i.Type == Manager.JwtClaimHelper.ClaimIdentifierId).Value;

            var user = await _dataDbContext.Accounts.Include(i => i.IdentityCustomModel).SingleAsync(i => i.IdentityCustomModelId == userId);

            var toReturn = new AccountBaseInfo
            {
                FirstName             = user.IdentityCustomModel.FirstName,
                LastName              = user.IdentityCustomModel.LastName,
                Email                 = user.IdentityCustomModel.Email,
                DOB                   = user.IdentityCustomModel.DOB,
                IsAdmin               = user.IdentityCustomModel.IsApplicationAdmin,
                EmailConfirmed        = user.IdentityCustomModel.EmailConfirmed,
                EmailConfirmedByAdmin = user.IdentityCustomModel.EmailConfirmedByAdmin,
                LastActivtyTime       = user.IdentityCustomModel.LastLoginDate
            };

            return new OkObjectResult(toReturn);

        }

        /// <summary>
        /// Sends email to SMAnalytic's email box
        /// </summary>
        /// <param name="EmailMessage">Email body message and its destination</param>
        /// <returns>Success message if sent</returns>
        [Route("~/api/Dashboard/SendEmail")]
        [Authorize(Policy = "SMAnalytic")]
        [HttpPost]
        public async Task<IActionResult> SendEmail([FromBody]EmailMessage emailMessage)
        {
            // Getting the caller's details
            var userActionResult = await GetCallerDetails();
            var userObjectResult = userActionResult as OkObjectResult;
            var user = userObjectResult.Value as AccountBaseInfo;

            // Sending the email
            SmtpClient smtp = new SmtpClient("smtp.office365.com");

            smtp.EnableSsl = true;
            smtp.Port = 587;
            smtp.Credentials = new NetworkCredential(_adminEmail, _emailPassword);

            MailMessage message = new MailMessage();

            message.Sender = new MailAddress(_adminEmail, user.Email);
            message.From = new MailAddress(_adminEmail, "SM Analytic Help");

            // List of recipients
            message.To.Add(new MailAddress(emailMessage.Destination, "Admin"));

            message.Subject = "Help Request";
            message.Body = "<b>FROM:</b> " + user.FirstName + " " + user.LastName + " (" + user.Email + ")" +
               "<p> <b>MESSAGE:</b> " +
               emailMessage.Message +
               "</p> ";

            message.IsBodyHtml = true;

            await smtp.SendMailAsync(message);

            return new OkObjectResult(new { result = "The email has been sent" });
        }

        /// <summary>
        /// Admin sends an email to all app users
        /// </summary>
        /// <param name="emailMessage">Email to be sent to all users</param>
        /// <returns>Success message if sent</returns>
        [Route("~/api/Dashboard/SendEmailBroadcast")]
        [Authorize(Policy = "SMAnalytic")]
        [HttpPost]
        public async Task<IActionResult> SendEmailBroadcast([FromBody]EmailSubjectMessage emailMessage)
        {
            // Getting the caller's details
            var userActionResult = await GetCallerDetails();
            var userObjectResult = userActionResult as OkObjectResult;
            var user = userObjectResult.Value as AccountBaseInfo;

            if (!user.IsAdmin)
            {
                return new BadRequestObjectResult(new { message = "Not authorized to send this type of email!" });
            }

            // Sending the email
            SmtpClient smtp = new SmtpClient("smtp.office365.com");

            smtp.EnableSsl = true;
            smtp.Port = 587;
            smtp.Credentials = new NetworkCredential(_adminEmail, _emailPassword);

            MailMessage message = new MailMessage
            {
                Sender = new MailAddress(_adminEmail, "Admin"),
                From = new MailAddress(_adminEmail, "SM Analytic Notifications"),
                Subject = emailMessage.Subject,
                Body = "<b>FROM:</b> " + user.FirstName + " " + user.LastName + " (" + user.Email + ")" +
                    "<p> <b>MESSAGE:</b> " +
                    emailMessage.Message +
                    "</p> ",
                IsBodyHtml = true
            };

            await _dataDbContext
                .Users
                .Where(i => i.IsApplicationAdmin == false && i.EmailConfirmedByAdmin == true && i.EmailConfirmedByAdmin == true)
                .ForEachAsync(i =>
                {
                    message.Bcc.Add(new MailAddress(i.Email, i.FirstName + " " + i.LastName));
                });

            await smtp.SendMailAsync(message);

            return new OkObjectResult(new { result = "The email has been sent" });
        }

        /// <summary>
        /// Admin can get 
        /// </summary>
        /// <returns></returns>
        [Route("~/api/Dashboard/GetAllUsers")]
        [Authorize(Policy = "SMAnalytic")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            // Getting the caller's details
            var userActionResult = await GetCallerDetails();
            var userObjectResult = userActionResult as OkObjectResult;
            var user = userObjectResult.Value as AccountBaseInfo;

            if (!user.IsAdmin)
            {
                return new BadRequestObjectResult(new { message = "Not authorized to send this type of email!" });
            }

            List<AccountBaseInfo> data = new List<AccountBaseInfo>();

            await _dataDbContext.Users.Where(i => i.Email != user.Email).ForEachAsync(i =>
            {
                data.Add( new AccountBaseInfo { FirstName = i.FirstName,
                                                LastName = i.LastName,
                                                Email = i.Email,
                                                DOB = i.DOB,
                                                IsAdmin = i.IsApplicationAdmin,
                                                EmailConfirmed = i.EmailConfirmed,
                                                EmailConfirmedByAdmin = i.EmailConfirmedByAdmin,
                                                LastActivtyTime = i.LastLoginDate});

            });

            Console.WriteLine("____!!! The dataset we got: "+data.ToList());

            return new OkObjectResult(data);
        }
    }
}
