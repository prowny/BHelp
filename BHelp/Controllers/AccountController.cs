﻿using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using BHelp.DataAccessLayer;
using BHelp.Models;
using System.Collections.Generic;
using System;
using System.Data.SqlClient;
using System.Net.Mail;
using BHelp.ViewModels;
using System.Net;

namespace BHelp.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get { return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>(); }
            private set { _signInManager = value; }
        }

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl, string errorMessage)
        {
            ViewBag.ReturnUrl = returnUrl;
            LoginViewModel model = new LoginViewModel();
            if (errorMessage != null)
            {
                model.ErrorMessage = errorMessage;
            }

            var builder = new SqlConnectionStringBuilder(db.Database.Connection.ConnectionString);
            model.ServerName = builder.DataSource;
            model.DatabaseName = builder.InitialCatalog;

            return View(model);
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe,
                shouldLockout: false);

            Session["BagWeightList"] = db.BagWeights.OrderBy(d => d.EffectiveDate).ToList();
            WriteToLoginTable(model.UserName, result.ToString());
            switch (result)
            {
                case SignInStatus.Success:
                    var usr = db.Users.FirstOrDefault(n => n.UserName == model.UserName);
                    if (usr != null) // Block inactive user account.
                    {
                        if (!usr.Active)
                        {
                            return RedirectToAction("InactiveMessage");
                        }
                    }

                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, model.RememberMe });
                //case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", @"Invalid login attempt.");
                    return View(model);
            }
        }

        public ActionResult InactiveMessage()
        {
            return View();
        }

        private void WriteToLoginTable(string userName, string status)
        {
            var login = new Login { UserName = userName };
            if (db.Users.Any(u => u.UserName == userName))
            {
                var user = (from u in db.Users where u.UserName == userName select u).Single();
                login.FirstName = user.FirstName;
                login.LastName = user.LastName;
            }

            var localZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            var dt1 = DateTime.Now;
            var localStandard = TimeZoneInfo.ConvertTime(dt1, TimeZoneInfo.Local, localZone);
            login.DateTime = localStandard;
            login.Status = status;
            //if (status == "Success") { user.LastDate = dt1; }   //Added 07/07/2021 REMOVED 11/28/2021

            db.Logins.Add(login);
            db.SaveChanges();
        }
        
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }

            return View(new VerifyCodeViewModel
                { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code,
                isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                //case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", @"Invalid code.");
                    return View(model);
            }
        }

        // GET: /Account/Register
        [Authorize(Roles = "Administrator")]
        public ActionResult Register()
        {
            // Add block to non-Administrators 08/13/2021:
            //if (!User.IsInRole("Administrator"))
            //{
            //    return RedirectToAction("Login", "Account");
            //}
            var view = new RegisterViewModel
                {
                 VolunteerCategories = HoursRoutines.GetHoursCategoriesSelectList(false,false),
                 VolunteerSubcategories = HoursRoutines.GetHoursSubcategoriesSelectList(null),
                 States = AppRoutines.GetStatesSelectList()
                };

            return View(view);
        }

        // POST: /Account/Register
        [HttpPost, Authorize(Roles = "Administrator, Developer")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Title=model.Title,  
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    PhoneNumberConfirmed = true,
                    EmailConfirmed = true,
                    BeginDate = DateTime.Today,
                    LastDate = new DateTime(1900, 01, 01),
                    Active = true,
                    Address = model.Address,
                    City = model.City,
                    State = model.State,
                    Zip = model.Zip,
                    VolunteerCategory = model.VolunteerCategory,
                    VolunteerSubcategory = model.VolunteerSubcategory, 
                    States =AppRoutines.GetStatesSelectList(),
                    Notes =model.Notes 
                };

                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    //var receiverRole = "ReceiveRegistrationEmail";
                    //UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

                    //foreach (ApplicationUser dbUser in db.Users)
                    //{
                    //    bool isReceiver = await userManager.IsInRoleAsync(dbUser.Id, receiverRole).ConfigureAwait(false);
                    //    if (isReceiver)
                    //    {
                    //        var plainTextContent = "New Registration: " + user.FullName + " " + user.Email;

                    //using (MailMessage msg = new MailMessage())
                    //{
                    //    msg.From = new MailAddress("Admin@SenecaHeights.org", "Administrator, SHEP");
                    //    msg.To.Add(new MailAddress(dbUser.Email, user.FirstName));
                    //    msg.Subject = "SHEP / HFED Seneca Heights Programs";
                    //    msg.Body = plainTextContent;
                    //    msg.IsBodyHtml = false;
                    //    msg.Priority = MailPriority.Normal;
                    //    using (SmtpClient mailClient = new SmtpClient("SenecaHeights.org", 587))
                    //    {
                    //        mailClient.Credentials = new NetworkCredential("Admin@SenecaHeights.org", "@fEfY8-9vFFDwD!YTo!A");
                    //        await mailClient.SendMailAsync(msg);
                    //    }
                    //}
                    //    }
                    //}
                    return RedirectToAction("Index", "Users");
                    //return User.Identity.Name.IsNullOrEmpty() ? RedirectToAction("Login", "Account") : RedirectToAction("Index", "Home");
                }

                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            var errors = ModelState.Select(x => x.Value.Errors)
                .Where(y => y.Count > 0)
                .ToList();
            var errmsg = errors[0];
            model.ErrorMessage = errmsg[0].ErrorMessage;
            model.VolunteerCategories = HoursRoutines.GetHoursCategoriesSelectList(false, false);
            model.VolunteerSubcategories = HoursRoutines.GetHoursSubcategoriesSelectList(null);
            model.States = AppRoutines.GetStatesSelectList();
            //return RedirectToAction("Register", "Account");
            return View(model);
        }

        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }

            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            var user = System.Web.HttpContext.Current.GetOwinContext()
                .GetUserManager<ApplicationUserManager>()
                .FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
            if (user == null) return RedirectToAction("Logout", "Home");
           
            // this is Reset MY password
            ResetPasswordViewModel viewModel = new ResetPasswordViewModel
            {
                UserName = User.Identity.Name
            };
            return View(viewModel);
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.UserName = User.Identity.Name;
            var user = await UserManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }

            var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
            code = code.Replace(" ", "+"); // Stackoverflow sometime fix
            var result = await UserManager.ResetPasswordAsync(user.Id, code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }

            AddErrors(result);
            return View();
        }

        [Authorize(Roles = "Administrator,Developer")]
        public ActionResult ResetAnyPassword(string code)
        {
            // this is Reset ANY password
            var viewModel = new ResetAnyPasswordViewModel
            {
                UserNames = GetUserNamesList()
            };
            return View(viewModel);
        }

        private List<SelectListItem> GetUserNamesList()
        {
            var userList = db.Users.OrderBy(l => l.LastName).ToList();
            List<SelectListItem> userNames = new List<SelectListItem>();
            foreach (var user in userList)
            {
                SelectListItem userName = new SelectListItem
                {
                    Text = user.UserName,
                    Value = user.UserName
                };
                userNames.Add(userName);
            }

            return userNames;
        }

        [HttpPost,Authorize(Roles = "Administrator, Developer")]
        public async Task<ActionResult> ResetAnyPassword(ResetAnyPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.UserNames = GetUserNamesList();
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.UserName);
            //if (user == null)
            //{
            //    // Don't reveal that the user does not exist
            //    return RedirectToAction("ResetAnyPasswordConfirmation", "Account");
            //}
            var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
            code = code.Replace(" ", "+");    // Stackoverflow sometime fix
            var result = await UserManager.ResetPasswordAsync(user.Id, code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetAnyPasswordConfirmation", "Account", model);
            }
            AddErrors(result);
            return View();
        }

        [Authorize(Roles = "Administrator, Developer")]
        public ActionResult ResetAnyPasswordConfirmation(ResetAnyPasswordViewModel model)
        {
            return View(model);
        }
        public async Task<ActionResult> EmailPasswordReset(string userName, string newPwd)
        {
            var user = (from u in db.Users where u.UserName == userName select u).Single();
            var plainTextContent = "Temporary New Credential: " + newPwd + " "
                + Environment.NewLine + "  Use this to log into the website, and immediately change it to your desired value."
                + Environment.NewLine + "  Please do not reply to this email.";
            using (var msg = new MailMessage())
            {
                msg.From = new MailAddress("Admin@BethesdaHelpFd.org", "Administrator, BHelp");
                msg.To.Add(new MailAddress(user.Email, user.FullName));
                msg.Subject = "BHELP - Credential";
                msg.Body = plainTextContent;
                msg.IsBodyHtml = false;
                msg.Priority = MailPriority.Normal;
                using (var mailClient = new SmtpClient("BethesdaHelpFd.org", 587))
                {
                    mailClient.Credentials = new NetworkCredential("Admin@BethesdaHelpFd.org", "Qo3YQoyYAghL*U.W-KEU");
                    await mailClient.SendMailAsync(msg);
                }
            }

            return RedirectToAction("Index", "Home");
        }
        
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return View();
        }

        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, model.ReturnUrl, model.RememberMe });
        }

        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                //case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                // ReSharper disable once Mvc.ActionNotResolved
                // ReSharper disable once Mvc.ControllerNotResolved
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }

        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }
            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}