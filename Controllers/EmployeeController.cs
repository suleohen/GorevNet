using GorevNet.Context;
using GorevNet.Models;
using GorevNet.Models.ViewModels;
using GorevNet.Identitiy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskStatus = GorevNet.Models.TaskStatus;
using GorevNet.ViewModels;


namespace GorevNet.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeeController : Controller
    {
        private readonly TasksDBContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public EmployeeController(
            TasksDBContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        #region Dashboard
        public async Task<IActionResult> Index()
        {
            Console.WriteLine("=== EMPLOYEE INDEX BAŞLADI ===");

            var user = await _userManager.GetUserAsync(User);
            Console.WriteLine($"User: {user?.Email ?? "NULL"}");

            if (user == null)
            {
                Console.WriteLine("User null, Login'e yönlendiriliyor");
                return RedirectToAction("Login", "Account");
            }

            var employee = _context.Employees.FirstOrDefault(e => e.Email == user.Email && e.IsActive);
            if (employee == null)
            {
                Console.WriteLine("Employee bulunamadı, Login'e yönlendiriliyor");
                TempData["ErrorMessage"] = "Çalışan bilgileri bulunamadı.";
                return RedirectToAction("Login", "Account");
            }

            Console.WriteLine("Employee Index normal akış devam ediyor");

            // Dashboard için temel bilgileri al
            var myTasks = _context.UserTasks
                .Where(t => t.AssignedUserId == employee.Id)
                .OrderByDescending(t => t.CreatedDate)
                .Take(10)
                .ToList();

            ViewBag.PendingTasks = myTasks.Count(t => t.Status == TaskStatus.Beklemede);
            ViewBag.OngoingTasks = myTasks.Count(t => t.Status == TaskStatus.DevamEdiyor);
            ViewBag.CompletedTasks = myTasks.Count(t => t.Status == TaskStatus.Tamamlandı);
            ViewBag.EmployeeName = employee.FullName;

            return View(myTasks);
        }
        #endregion

        #region My Tasks
        public async Task<IActionResult> MyTasks()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var employee = _context.Employees.FirstOrDefault(e => e.Email == user.Email && e.IsActive);
            if (employee == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var myTasks = _context.UserTasks
                .Where(t => t.AssignedUserId == employee.Id)
                .OrderByDescending(t => t.CreatedDate)
                .ToList();

            return View(myTasks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTaskStatus(int taskId, TaskStatus status, string comment = "")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Oturum süreniz dolmuş." });
            }

            var employee = _context.Employees.FirstOrDefault(e => e.Email == user.Email && e.IsActive);
            if (employee == null)
            {
                return Json(new { success = false, message = "Çalışan bilgileri bulunamadı." });
            }

            var task = _context.UserTasks.FirstOrDefault(t => t.Id == taskId && t.AssignedUserId == employee.Id);
            if (task == null)
            {
                return Json(new { success = false, message = "Görev bulunamadı veya bu göreve erişim yetkiniz yok." });
            }

            try
            {
                task.Status = status;
                task.StatusChangedDate = DateTime.Now;
                task.ModifiedBy = user.UserName;
                task.ModifiedDate = DateTime.Now;

                if (!string.IsNullOrWhiteSpace(comment))
                {
                    task.Comment = comment;
                }

                await _context.SaveChangesAsync();

                string statusText = status switch
                {
                    TaskStatus.Beklemede => "Beklemede",
                    TaskStatus.DevamEdiyor => "Devam Ediyor",
                    TaskStatus.Tamamlandı => "Tamamlandı",
                    _ => "Bilinmiyor"
                };

                return Json(new { success = true, message = $"Görev durumu '{statusText}' olarak güncellendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Güncelleme sırasında bir hata oluştu: " + ex.Message });
            }
        }
        #endregion

        #region Profile
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var employee = _context.Employees.FirstOrDefault(e => e.Email == user.Email && e.IsActive);
            if (employee == null)
            {
                TempData["ErrorMessage"] = "Çalışan bilgileri bulunamadı.";
                return RedirectToAction("Login", "Account");
            }

            // Görev istatistikleri
            var totalTasks = _context.UserTasks.Count(t => t.AssignedUserId == employee.Id);
            var completedTasks = _context.UserTasks.Count(t => t.AssignedUserId == employee.Id && t.Status == TaskStatus.Tamamlandı);
            var pendingTasks = _context.UserTasks.Count(t => t.AssignedUserId == employee.Id && t.Status == TaskStatus.Beklemede);
            var ongoingTasks = _context.UserTasks.Count(t => t.AssignedUserId == employee.Id && t.Status == TaskStatus.DevamEdiyor);

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
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var employee = _context.Employees.FirstOrDefault(e => e.Email == user.Email && e.IsActive);
            if (employee == null)
            {
                TempData["ErrorMessage"] = "Çalışan bilgileri bulunamadı.";
                return RedirectToAction("Login", "Account");
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(UserProfileViewModel model)
        {
            // Sadece gerekli alanları validate et
            ModelState.Remove("Department");
            ModelState.Remove("Position");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var employee = _context.Employees.FirstOrDefault(e => e.Id == model.Id && e.IsActive);
            if (employee == null)
            {
                TempData["ErrorMessage"] = "Çalışan bilgileri bulunamadı.";
                return RedirectToAction("Profile");
            }

            // Güvenlik kontrolü: Kullanıcı sadece kendi profilini düzenleyebilir
            if (employee.Email != user.Email)
            {
                TempData["ErrorMessage"] = "Bu profili düzenleme yetkiniz yok.";
                return RedirectToAction("Profile");
            }

            bool emailChanged = false;
            string oldEmail = employee.Email;

            try
            {
                // Sadece kullanıcının değiştirebileceği alanları güncelle
                employee.FirstName = model.FirstName;
                employee.LastName = model.LastName;

                // Email değişikliği kontrolü
                if (employee.Email != model.Email)
                {
                    // Email değişikliği için ek kontroller
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null && existingUser.Id != user.Id)
                    {
                        ModelState.AddModelError("Email", "Bu email adresi zaten kullanımda.");
                        return View(model);
                    }

                    employee.Email = model.Email;
                    user.Email = model.Email;
                    user.UserName = model.Email;

                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        ModelState.AddModelError("Email", "Email güncellenirken bir hata oluştu.");
                        return View(model);
                    }

                    emailChanged = true;
                }

                employee.ModifiedBy = user.UserName;
                employee.ModifiedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Profil bilgileriniz başarıyla güncellendi.";

                // Email değiştirildi ise çıkış yap
                if (emailChanged)
                {
                    await _signInManager.RefreshSignInAsync(user);
                    TempData["InfoMessage"] = "Email adresiniz değiştirildi. Güvenlik nedeniyle yeniden giriş yapmanız gerekmektedir.";
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Login", "Account");
                }
               
                

                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Güncelleme sırasında bir hata oluştu: " + ex.Message);

                // Email değişikliği başarısızsa geri al
                if (emailChanged)
                {
                    employee.Email = oldEmail;
                    user.Email = oldEmail;
                    user.UserName = oldEmail;
                    await _userManager.UpdateAsync(user);
                }

                return View(model);
            }
        }
        #endregion
        public IActionResult TestAccess()
        {
            return Content($@"
        <h2>Employee Controller Test</h2>
        <p><strong>Authenticated:</strong> {User.Identity.IsAuthenticated}</p>
        <p><strong>User Name:</strong> {User.Identity.Name}</p>
        <p><strong>User Email:</strong> {User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "No email claim"}</p>
        <p><strong>Roles:</strong> {string.Join(", ", User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value))}</p>
        <p><strong>Current Time:</strong> {DateTime.Now}</p>
        <hr>
        <a href='/Employee/Index'>Employee Index'e git</a><br>
        <a href='/Home/Index'>Home Index'e git</a>
    ");
        }
    }
}