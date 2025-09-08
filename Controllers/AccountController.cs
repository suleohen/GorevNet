using GorevNet.Context;
using GorevNet.Models;
using GorevNet.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GorevNet.Identitiy;
using GorevNet.ViewModels;

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
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            // Her login sayfasına girildiğinde eski oturumu kapat
            if (User.Identity.IsAuthenticated)
            {
                await _signInManager.SignOutAsync();
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

                            // Kullanıcı rolüne göre yönlendirme - DÜZELTME BURADA
                            var roles = await _userManager.GetRolesAsync(user);

                            if (roles.Contains("Admin") || roles.Contains("Manager"))
                            {
                                // Admin veya Manager ise Dashboard'a yönlendir
                                return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                                    ? Redirect(returnUrl)
                                    : RedirectToAction("Index", "Admin");
                            }
                            else if (roles.Contains("Employee"))
                            {
                                // Employee ise Employee Index'e yönlendir
                                return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                                    ? Redirect(returnUrl)
                                    : RedirectToAction("Index", "Employee");
                            }
                            else
                            {
                                // Hiçbir rol yoksa varsayılan
                                ModelState.AddModelError(string.Empty, "Hesabınız için uygun rol tanımlanmamış. Lütfen yöneticinizle iletişime geçin.");
                                await _signInManager.SignOutAsync();
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
                        await _signInManager.SignOutAsync();
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

        #region Profile Management

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var employee = _context.Employees.FirstOrDefault(e => e.Email == user.Email);
            if (employee == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgileri bulunamadı.";
                return RedirectToAction("Login");
            }

            // Görev istatistikleri
            var totalTasks = _context.UserTasks.Count(t => t.AssignedUserId == employee.Id);
            var completedTasks = _context.UserTasks.Count(t => t.AssignedUserId == employee.Id && t.Status == GorevNet.Models.TaskStatus.Tamamlandı);
            var pendingTasks = _context.UserTasks.Count(t => t.AssignedUserId == employee.Id && t.Status == GorevNet.Models.TaskStatus.Beklemede);
            var ongoingTasks = _context.UserTasks.Count(t => t.AssignedUserId == employee.Id && t.Status == GorevNet.Models.TaskStatus.DevamEdiyor);

            // Son 5 görevi getir
            var recentTasks = _context.UserTasks
                .Where(t => t.AssignedUserId == employee.Id)
                .OrderByDescending(t => t.CreatedDate)
                .Take(5)
                .ToList();

            var model = new UserProfileViewModel
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                Department = employee.Department,
                Position = employee.Position,
                HireDate = employee.HireDate,
                IsActive = employee.IsActive,
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                PendingTasks = pendingTasks,
                OngoingTasks = ongoingTasks,
                RecentTasks = recentTasks
            };

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var employee = _context.Employees.FirstOrDefault(e => e.Email == user.Email);
            if (employee == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgileri bulunamadı.";
                return RedirectToAction("Login");
            }

            var model = new UserProfileViewModel
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                Department = employee.Department,
                Position = employee.Position,
                HireDate = employee.HireDate,
                IsActive = employee.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(UserProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var employee = _context.Employees.FirstOrDefault(e => e.Id == model.Id);
            if (employee == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgileri bulunamadı.";
                return RedirectToAction("Profile");
            }

            // Sadece kullanıcının değiştirebileceği alanları güncelle
            employee.FirstName = model.FirstName;
            employee.LastName = model.LastName;

            // Email değişikliği kontrolü (genelde admin işlemi)
            if (employee.Email != model.Email)
            {
                // Email değişikliği için ek kontroller yapılabilir
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    ModelState.AddModelError("Email", "Bu email adresi zaten kullanımda.");
                    return View(model);
                }

                employee.Email = model.Email;
                user.Email = model.Email;
                user.UserName = model.Email;
                await _userManager.UpdateAsync(user);
            }

            employee.ModifiedBy = user.UserName;
            employee.ModifiedDate = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Profil bilgileriniz başarıyla güncellendi.";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Güncelleme sırasında bir hata oluştu: " + ex.Message);
                return View(model);
            }
        }

        #endregion

        #region Helper Methods

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return null; // Null döndür ki ?? operator sonraki seçeneği kullansın
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