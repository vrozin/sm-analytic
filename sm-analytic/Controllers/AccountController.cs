using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using sm_analytic.Models;
using sm_analytic.Controllers;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Text.Encodings.Web;

namespace sm_analytic.Controllers
{
    [ApiController]
    [EnableCors("AllowMyOrigin")]
    public class AccountController : ControllerBase
    {
        private readonly DataDbContext                    _dataDbContext;
        private readonly Manager                          _manager;
        private readonly UserManager<IdentityCustomModel> _userManager;
        private readonly IJwtManager                      _jwtManager;
        private readonly JwtIssuerProps                   _jwtProps;
        private readonly string                           _adminEmail;
        private readonly string                           _emailPassword;

        public AccountController(UserManager<IdentityCustomModel> userManager, DataDbContext context, IJwtManager jwtManager, IOptions<JwtIssuerProps> jwtProps, IHttpContextAccessor httpContextAccessor)
        {
            _userManager   = userManager;
            _dataDbContext = context;
            _manager       = new Manager(_dataDbContext);
            _jwtManager    = jwtManager;
            _jwtProps      = jwtProps.Value;
            _adminEmail    = "smanalyticjmv@gmail.com";//Environment.GetEnvironmentVariable("AdminEmail");
            _emailPassword = "Qwerty112358";//Environment.GetEnvironmentVariable("EmailPassword");
        }

        /// <summary>
        /// Registration endpoint
        /// </summary>
        /// <param name="newAccount">Accepts all necessary data to register user</param>
        /// <returns>If successful, returns notification of successful registration</returns>
        [Route("~/api/Account/Register")]
        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> RegistryEndPoint([FromBody]AccountAdd newAccount)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);

            }

            var newUserIdentity = _manager.IdentityGetMapped(newAccount);
            if (newAccount.Email == _adminEmail)
            {
                newUserIdentity.IsApplicationAdmin = true;
                newUserIdentity.EmailConfirmed = true;
                newUserIdentity.EmailConfirmedByAdmin = true;
            }

            var result = await _userManager.CreateAsync(newUserIdentity, newAccount.Password);

            if (!result.Succeeded)
            {
                return new BadRequestObjectResult(new { message = "The User Cannot Be Registered" });
            }
            
            await _dataDbContext.Accounts.AddAsync(new Account { IdentityCustomModelId = newUserIdentity.Id,
                                                                 SearchBasicLimit      = 50,
                                                                 SearchBasicNum        = 0,
                                                                 SearchRegularLimit    = 250,
                                                                 SearchRegularNum      = 0,
                                                                 LastRequest           = DateTime.Now,
                                                                 });


            await _dataDbContext.SaveChangesAsync();

            // Sending email confirmation request
            var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(newUserIdentity);
            
            var callbackUrl = Environment.GetEnvironmentVariable("baseURL") +
                "auth?userConfirmed=true&userId=" + newUserIdentity.Id +
                "&confirmationToken=" + WebUtility.UrlEncode(emailConfirmationToken);

            Console.WriteLine("___!!! Confirmation Token: " + emailConfirmationToken);

            SmtpClient smtp = new SmtpClient("smtp.gmail.com")
            {
                EnableSsl = true,
                Port = 587,
                Credentials = new NetworkCredential(_adminEmail, _emailPassword)
            };

            MailMessage message = new MailMessage
            {
                Sender = new MailAddress(_adminEmail, "Admin"),
                From = new MailAddress(_adminEmail, "SM Analytic")
            };

            // List of recipients
            message.To.Add(new MailAddress(newUserIdentity.Email, newUserIdentity.FirstName + " " + newUserIdentity.LastName));

            message.Subject = "Please Confirm Your Account";
            message.Body = $"Please open the following link in your browser to confirm your new account: <a href='" +
                callbackUrl + "'>link</a>";
            message.IsBodyHtml = true;

            await smtp.SendMailAsync(message);

            return new OkObjectResult(new { message = "Account Has Been Created" });
        }

        /// <summary>
        /// Confirms user's account after registration
        /// </summary>
        /// <param name="userId">ID of the newly registered user</param>
        /// <param name="confirmationToken">Token generated upon registration</param>
        /// <returns>Success if confirmed</returns>
        [Route("~/api/Account/Confirm")]
        [HttpGet, AllowAnonymous]
        public async Task<IActionResult> EmailUserConfirmationEndPoint(string userId, string confirmationToken)
        {
            if (userId == null || confirmationToken == null)
            {
                return new BadRequestObjectResult( new { message = "Invalid Confirmation Url" } );
            }

            var userToConfirm = await _dataDbContext.Users.SingleOrDefaultAsync(i => i.Id == userId);

            Console.WriteLine("___!!! UserToConfirm: " + userToConfirm.Email);
            Console.WriteLine("___!!! UserID: " + userId);
            Console.WriteLine("___!!! UserToken: " + confirmationToken);

            if (userToConfirm == null)
            {
                return new BadRequestObjectResult(new { message = "Invalid Confirmation Url" });
            }

            var result = await _userManager.ConfirmEmailAsync(userToConfirm, confirmationToken);
            if (result.Succeeded)
            {
                return new OkObjectResult(new { message = "Email has been confirmed! Please wait when admin approves your account." });
            }

            return new BadRequestObjectResult(new { message = "Unknown problem has appeared while confirming your email" });
        }

        /// <summary>
        /// Login endpoint
        /// </summary>
        /// <param name="account">Accepts login credentials (login(user's email) and password)</param>
        /// <returns>If successful, return JSON Web Token attached to current session</returns>
        [Route("~/api/Account/Login")]
        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> LoginEndPoint([FromBody]AccountLogin account)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState); 
            }

            var identity = await GetClaimsIdentity(account.Email, account.Password);
            
            if (identity == null)
            {
                return new BadRequestObjectResult(new { message = "Login Failed. Invalid Username or Password." });
            }

            var userToValidate = await _dataDbContext.Users.SingleOrDefaultAsync(i => i.Email == account.Email);

            if (!userToValidate.EmailConfirmed)
            {
                return new BadRequestObjectResult(new { message = "Login Failed. Please confirm your email." });
            }

            if (!userToValidate.EmailConfirmedByAdmin)
            {
                return new BadRequestObjectResult(new { message = "Login Failed. Please wait until your account is confirmed by admin." });
            }

            var jwt = await Manager.TokenGenerator.GenerateJwt(identity, _jwtManager, account.Email, _jwtProps);
            var toReturn = JsonConvert.SerializeObject(jwt);

            return new OkObjectResult(toReturn);
        }

        private async Task<ClaimsIdentity> GetClaimsIdentity(string email /*aka userName*/, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return await Task.FromResult<ClaimsIdentity>(null);

            //Get the user, which is to be verified
            var userToVerify = await _userManager.FindByNameAsync(email);

            if (userToVerify == null)
                return await Task.FromResult<ClaimsIdentity>(null);

            //Check the credentials
            if (await _userManager.CheckPasswordAsync(userToVerify, password))
            {
                return await Task.FromResult(_jwtManager.GenerateClaimsIdentity(email, userToVerify.Id));
            }

            //Credentials are invalid, or account doesn't exist
            return await Task.FromResult<ClaimsIdentity>(null);
        }


    }
}
