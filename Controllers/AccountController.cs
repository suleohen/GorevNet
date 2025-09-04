using GorevNet.Context;
using GorevNet.Models;
using GorevNet.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GorevNet.Identitiy;

namespace GorevNet.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly TasksDBContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            TasksDBContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        #region Login

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Kullanıcıyı email ile bul
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    // Şifre ile giriş yap
                    var result = await _signInManager.PasswordSignInAsync(
                        user.UserName,
                        model.Password,
                        model.RememberMe,
                        lockoutOnFailure: true);

                    if (result.Succeeded)
                    {
                        // Employee tablosundan kullanıcı bilgilerini al
                        var employee = _context.Employees
                            .FirstOrDefault(e => e.Email == user.Email && e.IsActive);

                        if (employee != null)
                        {
                            // Şifre değişikliği kontrolü
                            if (employee.MustChangePassword)
                            {
                                TempData["InfoMessage"] = "İlk girişiniz. Şifrenizi değiştirmeniz gerekmektedir.";
                                return RedirectToAction("ChangePassword");
                            }

                            // Kullanıcı rolüne göre yönlendirme
                            var roles = await _userManager.GetRolesAsync(user);

                            if (roles.Contains("Admin") || roles.Contains("Manager"))
                            {
                                return RedirectToLocal(returnUrl) ?? RedirectToAction("Dashboard", "Admin");
                            }
                            else
                            {
                                return RedirectToLocal(returnUrl) ?? RedirectToAction("Index", "Employee");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Hesabınız deaktif durumda. Lütfen yöneticinizle iletişime geçin.");
                            await _signInManager.SignOutAsync();
                        }
                    }
                    else if (result.IsLockedOut)
                    {
                        ModelState.AddModelError(string.Empty, "Hesabınız geçici olarak kilitlenmiştir. Lütfen daha sonra tekrar deneyin.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Email veya şifre hatalı.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email veya şifre hatalı.");
                }
            }

            return View(model);
        }

        #endregion

        #region Logout

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["SuccessMessage"] = "Başarıyla çıkış yaptınız.";
            return RedirectToAction("Login");
        }

        #endregion

        #region Change Password

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                if (result.Succeeded)
                {
                    // Employee tablosunda MustChangePassword'u false yap
                    var employee = _context.Employees.FirstOrDefault(e => e.Email == user.Email);
                    if (employee != null)
                    {
                        employee.MustChangePassword = false;
                        await _context.SaveChangesAsync();
                    }

                    await _signInManager.RefreshSignInAsync(user);
                    TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirildi.";

                    // Kullanıcı rolüne göre yönlendirme
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains("Admin") || roles.Contains("Manager"))
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Employee");
                    }
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return View(model);
        }

        #endregion

        #region Forgot Password

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Güvenlik için kullanıcı bulunamasa bile başarılı mesajı göster
                    TempData["InfoMessage"] = "Eğer email adresiniz sistemde kayıtlıysa, şifre sıfırlama linki gönderilecektir.";
                    return View("ForgotPasswordConfirmation");
                }

                // Şifre sıfırlama token'ı oluştur
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Email gönderme işlemi (implement edilmeli)
                // await SendPasswordResetEmailAsync(user.Email, token);

                TempData["InfoMessage"] = "Şifre sıfırlama talimatları email adresinize gönderildi.";
                return View("ForgotPasswordConfirmation");
            }

            return View(model);
        }

        #endregion

        #region Access Denied

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #endregion

        #region Helper Methods

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private async Task SendPasswordResetEmailAsync(string email, string token)
        {
            // Email gönderme işlemi - SMTP ayarlarınıza göre implement edin
            // Örnek:
            /*
            var resetLink = Url.Action("ResetPassword", "Account", 
                new { token = token, email = email }, Request.Scheme);
            
            var emailContent = $@"
                Şifre sıfırlama isteğiniz alınmıştır.
                
                Şifrenizi sıfırlamak için aşağıdaki linke tıklayınız:
                {resetLink}
                
                Bu link 24 saat geçerlidir.
            ";
            
            // Email gönder
            */
        }

        #endregion
    }
}