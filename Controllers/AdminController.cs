using GorevNet.Context;
using GorevNet.Models;
using GorevNet.ViewModels;
using GorevNet.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GorevNet.Identitiy;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace GorevNet.Controllers
{
    [Authorize(Roles = "Manager,Admin")]
    public class AdminController : Controller
    {
        private readonly TasksDBContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            TasksDBContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }


        #region Dashboard & Index

        public IActionResult Index()
        {
            // Admin controller'dan gelen Index metodunu Dashboard'a yönlendiriyoruz
            return RedirectToAction("Dashboard");
        }

        public IActionResult Dashboard()
        {
            try
            {
                var dashboardData = new ManagerDashboardViewModel
                {
                    TotalEmployees = _context.Employees.Count(e => e.IsActive),
                    ActiveTasks = _context.UserTasks.Count(t => t.Status != GorevNet.Models.TaskStatus.Tamamlandı),
                    PendingTasks = _context.UserTasks.Count(t => t.Status == GorevNet.Models.TaskStatus.Beklemede),
                    OverdueTasks = _context.UserTasks.Count(t => t.DueDate < DateTime.Now && t.Status != GorevNet.Models.TaskStatus.Tamamlandı),
                    CompletedTasks = _context.UserTasks.Count(t => t.Status == GorevNet.Models.TaskStatus.Tamamlandı),
                    OngoingTasks = _context.UserTasks.Count(t => t.Status == GorevNet.Models.TaskStatus.DevamEdiyor),
                    RecentEmployees = _context.Employees
                        .Where(e => e.IsActive)
                        .OrderByDescending(e => e.HireDate)
                        .Take(5)
                        .ToList()
                };

                return View(dashboardData);
            }
            catch (Exception ex)
            {
                // Hata durumunda debug için
                TempData["ErrorMessage"] = $"Dashboard yüklenirken hata: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        #endregion


        #region Görev Yönetimi
        public IActionResult ActiveTasks()
        {
            var tasksWithEmployeeNames = (from task in _context.UserTasks
                                          join employee in _context.Employees
                                          on task.AssignedUserId equals employee.Id
                                          select new TaskDisplayViewModel
                                          {
                                              Id = task.Id,
                                              Title = task.Title,
                                              Description = task.Description,
                                              Status = task.Status,
                                              Priority = task.Priority,
                                              CreatedDate = task.CreatedDate,
                                              DueDate = task.DueDate,
                                              Comment = task.Comment,
                                              AssignedUserId = task.AssignedUserId,
                                              AssignedUserName = employee.FirstName + " " + employee.LastName,
                                              CreatedBy = task.CreatedBy,
                                              ModifiedBy = task.ModifiedBy,
                                              ModifiedDate = task.ModifiedDate
                                          }).ToList();

            return View(tasksWithEmployeeNames);
        }

        [HttpGet]
        public IActionResult CreateTask()
        {
            var model = new CreateTaskViewModel();
            PopulateTaskEmployees(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTask(CreateTaskViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userTask = new UserTask
                    {
                        Title = model.Title,
                        Description = model.Description,
                        DueDate = model.DueDate,
                        Priority = model.Priority,
                        Status = Models.TaskStatus.Beklemede,
                        AssignedUserId = model.AssignedUserId,
                        CreatedBy = User.Identity.Name,
                        CreatedDate = DateTime.Now
                    };

                    _context.UserTasks.Add(userTask);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Görev başarıyla oluşturuldu.";
                    return RedirectToAction("ActiveTasks");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Görev oluşturulurken bir hata oluştu: " + ex.Message);
                }
            }

            PopulateTaskEmployees(model);
            return View(model);
        }

        [HttpGet]
        public IActionResult EditTask(int id)
        {
            var task = _context.UserTasks.FirstOrDefault(x => x.Id == id);
            if (task == null)
            {
                return NotFound();
            }

            var model = new EditTaskViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                Status = task.Status,
                Priority = task.Priority,
                Comment = task.Comment,
                AssignedUserId = task.AssignedUserId
            };

            PopulateTaskEmployees(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTask(EditTaskViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingTask = _context.UserTasks.FirstOrDefault(x => x.Id == model.Id);
                if (existingTask == null)
                {
                    return NotFound();
                }

                try
                {
                    existingTask.Title = model.Title;
                    existingTask.Description = model.Description;
                    existingTask.DueDate = model.DueDate;
                    existingTask.Status = model.Status;
                    existingTask.Priority = model.Priority;
                    existingTask.Comment = model.Comment;
                    existingTask.AssignedUserId = model.AssignedUserId;
                    existingTask.ModifiedBy = User.Identity.Name;
                    existingTask.ModifiedDate = DateTime.Now;

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Görev başarıyla güncellendi.";
                    return RedirectToAction("ActiveTasks");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Güncelleme sırasında bir hata oluştu: " + ex.Message);
                }
            }

            PopulateTaskEmployees(model);
            return View(model);
        }


        [HttpGet]
        public IActionResult TaskDetails(int id)
        {
            var task = _context.UserTasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
            {
                TempData["ErrorMessage"] = "Görev bulunamadı.";
                return RedirectToAction("ActiveTasks");
            }

            var employee = _context.Employees.FirstOrDefault(e => e.Id == task.AssignedUserId);

            var taskDetail = new TaskDisplayViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                CreatedDate = task.CreatedDate,
                DueDate = task.DueDate,
                Comment = task.Comment,
                AssignedUserId = task.AssignedUserId,
                AssignedUserName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Bilinmiyor",
                CreatedBy = task.CreatedBy,
                ModifiedBy = task.ModifiedBy,
                ModifiedDate = task.ModifiedDate
            };

            return View(taskDetail);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                var task = _context.UserTasks.FirstOrDefault(x => x.Id == id);
                if (task == null)
                {
                    return Json(new { success = false, message = "Görev bulunamadı." });
                }

                _context.UserTasks.Remove(task);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Görev başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Silme işlemi sırasında bir hata oluştu: " + ex.Message });
            }
        }

        #endregion

        #region Personel Yönetimi
        public IActionResult EmployeeManagement()
        {
            var employees = _context.Employees
                .OrderByDescending(e => e.HireDate)
                .ToList();

            return View(employees);
        }

        // Admin controller'dan gelen ListEmployee metodunu yönlendiriyoruz
        public IActionResult ListEmployee()
        {
            var employees = _context.Employees.ToList();
            return View(employees);
        }

        [HttpGet]
        public IActionResult CreateEmployee()
        {
            var model = new CreateEmployeeViewModel();
            PopulateDepartments(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmployee(CreateEmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Email kontrolü
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Bu email adresi zaten kullanımda.");
                    PopulateDepartments(model);
                    return View(model);
                }

                try
                {
                    // 1. Application User oluştur
                    var user = new ApplicationUser
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        EmailConfirmed = true // Kurumsal ortamda email onayı gerekmiyor
                    };

                    // Geçici şifre oluştur (personel ilk girişte değiştirmek zorunda)
                    string temporaryPassword = GenerateTemporaryPassword();

                    var result = await _userManager.CreateAsync(user, temporaryPassword);

                    if (result.Succeeded)
                    {
                        // 2. Role ata
                        await _userManager.AddToRoleAsync(user, model.Role);

                        // 3. Employee tablosuna kaydet
                        var employee = new Employee
                        {
                            UserId = user.Id,
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            Email = model.Email,
                            Department = model.Department,
                            Position = model.Position,
                            HireDate = model.HireDate,
                            IsActive = true,
                            MustChangePassword = true, // İlk girişte şifre değiştirme zorunlu
                            CreatedBy = User.Identity.Name,
                            CreatedDate = DateTime.Now
                        };

                        _context.Employees.Add(employee);
                        await _context.SaveChangesAsync();

                        // 4. Email ile bilgilendirme (opsiyonel)
                        await SendWelcomeEmailAsync(employee, temporaryPassword);

                        TempData["SuccessMessage"] = $"Personel başarıyla oluşturuldu. Geçici şifre: {temporaryPassword}";
                        return RedirectToAction("CreateEmployee");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Inner exception mesajını al
                    var inner = ex.InnerException?.Message;

                    // Hata mesajını ModelState’e ekle
                    ModelState.AddModelError(
                        string.Empty,
                        "Personel oluşturulurken bir hata oluştu: "
                        + ex.Message
                        + (inner != null ? " | Inner: " + inner : "")
                    );

                    // İstersen debug için konsola da yazdırabilirsin
                    Console.WriteLine(ex);
                    if (inner != null) Console.WriteLine(inner);
                }
            }

            PopulateDepartments(model);
            return View(model);
        }

        // Admin controller'dan gelen basit CreateEmployee metodunu da destekliyoruz (geriye uyumluluk için)
        [HttpPost]
        public async Task<IActionResult> CreateEmployeeSimple(Employee model)
        {
            try
            {
                model.IsActive = true;
                model.CreatedBy = User.Identity.Name;
                model.CreatedDate = DateTime.Now;

                _context.Employees.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Personel başarıyla oluşturuldu.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Bir hata oluştu: " + ex.Message;
                return RedirectToAction("CreateEmployee");
            }
        }



        [HttpGet]
        public async Task<IActionResult> EditEmployee(int id)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.Id == id);
            if (employee == null)
            {
                TempData["ErrorMessage"] = "Personel bulunamadı.";
                return RedirectToAction("ListEmployee");
            }

            // Identity kullanıcısını bul ve rolünü al
            string currentRole = "Employee"; // Varsayılan rol
            if (!string.IsNullOrEmpty(employee.UserId))
            {
                var user = await _userManager.FindByIdAsync(employee.UserId);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    currentRole = roles.FirstOrDefault() ?? "Employee";
                }
            }

            var model = new EditEmployeeViewModel
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                Department = employee.Department,
                Position = employee.Position,
                HireDate = employee.HireDate,
                IsActive = employee.IsActive,
                CurrentRole = currentRole,
                NewRole = currentRole
            };

            PopulateDepartments(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEmployee(EditEmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var employee = _context.Employees.FirstOrDefault(e => e.Id == model.Id);
                if (employee == null)
                {
                    TempData["ErrorMessage"] = "Personel bulunamadı.";
                    return RedirectToAction("ListEmployee");
                }

                try
                {
                    // Email değişimi kontrolü
                    bool emailChanged = employee.Email != model.Email;

                    if (emailChanged)
                    {
                        // Yeni email'in kullanımda olup olmadığını kontrol et
                        var existingUser = await _userManager.FindByEmailAsync(model.Email);
                        var currentUser = string.IsNullOrEmpty(employee.UserId) ? null : await _userManager.FindByIdAsync(employee.UserId);

                        if (existingUser != null && (currentUser == null || existingUser.Id != currentUser.Id))
                        {
                            ModelState.AddModelError("Email", "Bu email adresi zaten kullanımda.");
                            PopulateDepartments(model);
                            return View(model);
                        }
                    }

                    // Employee bilgilerini güncelle
                    employee.FirstName = model.FirstName;
                    employee.LastName = model.LastName;
                    employee.Email = model.Email;
                    employee.Department = model.Department;
                    employee.Position = model.Position;
                    employee.IsActive = model.IsActive;
                    employee.ModifiedBy = User.Identity.Name;
                    employee.ModifiedDate = DateTime.Now;

                    // Identity kullanıcısını güncelle
                    if (!string.IsNullOrEmpty(employee.UserId))
                    {
                        var user = await _userManager.FindByIdAsync(employee.UserId);
                        if (user != null)
                        {
                            // Email güncelleme
                            if (emailChanged)
                            {
                                user.Email = model.Email;
                                user.UserName = model.Email;
                                await _userManager.UpdateAsync(user);
                            }

                            // Rol güncelleme
                            if (!string.IsNullOrEmpty(model.NewRole) && model.CurrentRole != model.NewRole)
                            {
                                // Eski rolü kaldır
                                if (!string.IsNullOrEmpty(model.CurrentRole))
                                {
                                    await _userManager.RemoveFromRoleAsync(user, model.CurrentRole);
                                }

                                // Yeni rolü ekle
                                await _userManager.AddToRoleAsync(user, model.NewRole);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Personel bilgileri başarıyla güncellendi.";
                    return RedirectToAction("ListEmployee");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Güncelleme sırasında bir hata oluştu: " + ex.Message);
                }
            }

            PopulateDepartments(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateEmployee(int id)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.Id == id);
            if (employee == null)
            {
                return Json(new { success = false, message = "Personel bulunamadı." });
            }

            try
            {
                // Personeli deaktif et (silme yerine)
                employee.IsActive = false;
                await _context.SaveChangesAsync();

                // Aktif görevlerini askıya al
                var activeTasks = _context.UserTasks.Where(t => t.AssignedUserId == id && t.Status != GorevNet.Models.TaskStatus.Tamamlandı);
                foreach (var task in activeTasks)
                {
                    task.Status = GorevNet.Models.TaskStatus.Beklemede;
                    task.Comment = $"Personel deaktif edildiği için görev askıya alındı - {DateTime.Now:dd.MM.yyyy}";
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Personel başarıyla deaktif edildi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetEmployeePassword(int id)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.Id == id);
            if (employee == null)
            {
                return Json(new { success = false, message = "Personel bulunamadı." });
            }

            try
            {
                var user = await _userManager.FindByIdAsync(employee.UserId.ToString());
                if (user == null)
                {
                    return Json(new { success = false, message = "Kullanıcı bulunamadı." });
                }

                // Yeni geçici şifre oluştur
                string newPassword = GenerateTemporaryPassword();

                // Şifreyi sıfırla
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

                if (result.Succeeded)
                {
                    // Şifre değiştirme zorunluluğu ekle
                    employee.MustChangePassword = true;
                    await _context.SaveChangesAsync();

                    return Json(new
                    {
                        success = true,
                        message = "Şifre başarıyla sıfırlandı.",
                        newPassword = newPassword
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Şifre sıfırlama işlemi başarısız." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            try
            {
                var employee = _context.Employees.FirstOrDefault(x => x.Id == id);
                if (employee == null)
                {
                    return Json(new { success = false, message = "Personel bulunamadı." });
                }

                // Identity User'ı da bul ve sil
                if (!string.IsNullOrEmpty(employee.UserId))
                {
                    var user = await _userManager.FindByIdAsync(employee.UserId);
                    if (user != null)
                    {
                        // Önce Identity User'ı sil
                        var deleteUserResult = await _userManager.DeleteAsync(user);
                        if (!deleteUserResult.Succeeded)
                        {
                            return Json(new
                            {
                                success = false,
                                message = "Kullanıcı hesabı silinirken hata oluştu: " +
                                          string.Join(", ", deleteUserResult.Errors.Select(e => e.Description))
                            });
                        }
                    }
                }

                // Personelin aktif görevlerini kontrol et
                var activeTasks = _context.UserTasks
                    .Where(t => t.AssignedUserId == id && t.Status != Models.TaskStatus.Tamamlandı)
                    .ToList();

                if (activeTasks.Any())
                {
                    // Aktif görevleri askıya al veya başka birine ata
                    foreach (var task in activeTasks)
                    {
                        task.Status = Models.TaskStatus.Beklemede;
                        task.Comment = $"Personel silindiği için görev askıya alındı - {DateTime.Now:dd.MM.yyyy HH:mm}";
                        task.ModifiedBy = User.Identity.Name;
                        task.ModifiedDate = DateTime.Now;
                    }
                }

                // Employee'yi sil
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();

                string message = activeTasks.Any()
                    ? $"Personel başarıyla silindi. {activeTasks.Count} aktif görev askıya alındı."
                    : "Personel başarıyla silindi.";

                return Json(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                // Hata loglaması ekleyin
                // _logger.LogError(ex, "Personel silme hatası - ID: {EmployeeId}", id);

                return Json(new
                {
                    success = false,
                    message = "Silme işlemi sırasında bir hata oluştu: " + ex.Message
                });
            }
        }

        [HttpGet]
        public IActionResult DetailEmployee(int id)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.Id == id);
            if (employee == null)
            {
                TempData["ErrorMessage"] = "Personel bulunamadı.";
                return RedirectToAction("ListEmployee");
            }

            // Bu personele ait görevleri al
            var employeeTasks = _context.UserTasks
                .Where(t => t.AssignedUserId == employee.Id)
                .OrderByDescending(t => t.CreatedDate)
                .ToList();

            // Görev istatistikleri
            var taskStats = new
            {
                TotalTasks = employeeTasks.Count,
                CompletedTasks = employeeTasks.Count(t => t.Status == GorevNet.Models.TaskStatus.Tamamlandı),
                PendingTasks = employeeTasks.Count(t => t.Status == GorevNet.Models.TaskStatus.Beklemede),
                OngoingTasks = employeeTasks.Count(t => t.Status == GorevNet.Models.TaskStatus.DevamEdiyor)
            };

            ViewBag.TaskStats = taskStats;
            ViewBag.EmployeeTasks = employeeTasks;

            return View(employee);
        }


        #endregion


        #region Bulk Operations

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkCreateEmployees(List<BulkCreateEmployeeModel> employees)
        {
            var results = new List<BulkOperationResult>();

            foreach (var emp in employees)
            {
                try
                {
                    // Email kontrolü
                    var existingUser = await _userManager.FindByEmailAsync(emp.Email);
                    if (existingUser != null)
                    {
                        results.Add(new BulkOperationResult
                        {
                            Email = emp.Email,
                            Success = false,
                            Message = "Email zaten kullanımda"
                        });
                        continue;
                    }

                    // User oluştur
                    var user = new ApplicationUser
                    {
                        UserName = emp.Email,
                        Email = emp.Email,
                        EmailConfirmed = true
                    };

                    string tempPassword = GenerateTemporaryPassword();
                    var result = await _userManager.CreateAsync(user, tempPassword);

                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "Employee");

                        var employee = new Employee
                        {
                            UserId = user.Id,
                            FirstName = emp.FirstName,
                            LastName = emp.LastName,
                            Email = emp.Email,
                            Department = emp.Department,
                            Position = emp.Position,
                            HireDate = DateTime.Now,
                            IsActive = true,
                            MustChangePassword = true,
                            CreatedBy = User.Identity.Name,
                            CreatedDate = DateTime.Now
                        };

                        _context.Employees.Add(employee);
                        await _context.SaveChangesAsync();

                        results.Add(new BulkOperationResult
                        {
                            Email = emp.Email,
                            Success = true,
                            Message = $"Başarıyla oluşturuldu. Şifre: {tempPassword}"
                        });
                    }
                    else
                    {
                        results.Add(new BulkOperationResult
                        {
                            Email = emp.Email,
                            Success = false,
                            Message = string.Join(", ", result.Errors.Select(e => e.Description))
                        });
                    }
                }
                catch (Exception ex)
                {
                    results.Add(new BulkOperationResult
                    {
                        Email = emp.Email,
                        Success = false,
                        Message = ex.Message
                    });
                }
            }

            return Json(results);
        }

        #endregion



        #region Helper Methods

        private string GenerateTemporaryPassword(int length = 10)
        {
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string specials = "!@#$%^&*";

            var allChars = upper + lower + digits + specials;
            var random = new Random();

            // Minimum bir karakterden her kategoriden ekle
            var password = new char[length];
            password[0] = upper[random.Next(upper.Length)];
            password[1] = lower[random.Next(lower.Length)];
            password[2] = digits[random.Next(digits.Length)];
            password[3] = specials[random.Next(specials.Length)];

            for (int i = 4; i < length; i++)
            {
                password[i] = allChars[random.Next(allChars.Length)];
            }

            return new string(password.OrderBy(x => random.Next()).ToArray());
        }

        private void PopulateDepartments(dynamic model)
        {
            model.Departments = new List<string>
            {
                "Bilgi İşlem",
                "Yazılım",
                "İnsan Kaynakları",
                "Muhasebe",
                "Pazarlama",
                "Satış",
                "Üretim",
                "Kalite Kontrol",
                "Lojistik"
            };

            model.Roles = new List<string>
            {
                "Employee",
                "Manager",
                "Admin"
            };
        }

        //Model’in Employees listesine, dropdown’da seçilecek tüm aktif çalışanları ekliyor.
        private void PopulateTaskEmployees(dynamic model)
        {
            var employees = _context.Employees
                .Where(e => e.IsActive)
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.FirstName} {e.LastName} ({e.Department})"
                })
                .ToList();

            model.Employees = employees;
        }

        public static class SeedData
        {
            public static async Task SeedAdminAndManager(
                UserManager<ApplicationUser> userManager,
                RoleManager<ApplicationRole> roleManager,
                TasksDBContext context)
            {
                // 1️⃣ Roller
                if (!await roleManager.RoleExistsAsync("Admin"))
                    await roleManager.CreateAsync(new ApplicationRole("Admin"));

                if (!await roleManager.RoleExistsAsync("Manager"))
                    await roleManager.CreateAsync(new ApplicationRole("Manager"));

                // 2️⃣ Admin kullanıcı
                var admin = await userManager.FindByEmailAsync("admin@domain.com");
                if (admin == null)
                {
                    admin = new ApplicationUser
                    {
                        UserName = "admin@domain.com",
                        Email = "admin@domain.com",
                        EmailConfirmed = true
                       
                    };

                    await userManager.CreateAsync(admin, "Admin123!");
                    await userManager.AddToRoleAsync(admin, "Admin");
                }

                // 3️⃣ Manager kullanıcı
                var manager = await userManager.FindByEmailAsync("manager@domain.com");
                if (manager == null)
                {
                    manager = new ApplicationUser
                    {
                        UserName = "manager@domain.com",
                        Email = "manager@domain.com",
                        EmailConfirmed = true
                    };

                    await userManager.CreateAsync(manager, "Manager123!");
                    await userManager.AddToRoleAsync(manager, "Manager");
                }

                // 4️⃣ Employee tablosuna kayıt (UserId yok)
                if (!context.Employees.Any(e => e.Email == admin.Email))
                {
                    var employeeAdmin = new Employee
                    {
                        FirstName = "admin",
                        LastName = "admin",
                        Email = admin.Email,
                        Department = "IT",
                        Position = "Administrator",
                        HireDate = DateTime.Now,
                        IsActive = true,
                        MustChangePassword = false,
                        CreatedBy = "System",
                        CreatedDate = DateTime.Now,
                        ModifiedBy = "System",       // burayı ekle
                        ModifiedDate = DateTime.Now
                    };
                    context.Employees.Add(employeeAdmin);
                }

                if (!context.Employees.Any(e => e.Email == manager.Email))
                {
                    var employeeManager = new Employee
                    {
                        FirstName = "manager",
                        LastName = "manager",
                        Email = manager.Email,
                        Department = "IT",
                        Position = "Manager",
                        HireDate = DateTime.Now,
                        IsActive = true,
                        MustChangePassword = false,
                        CreatedBy = "System",
                        CreatedDate = DateTime.Now,
                        ModifiedBy = "System",       // burayı ekle
                        ModifiedDate = DateTime.Now
                    };
                    context.Employees.Add(employeeManager);
                }

                await context.SaveChangesAsync();
            }
        }


        private async Task SendWelcomeEmailAsync(Employee employee, string temporaryPassword)
        {
            // Email gönderme işlemi (SMTP ayarlarınıza göre implement edin)Microsoft.CSharp.RuntimeBinder.RuntimeBinderException: 'Cannot implicitly convert type 'System.Collections.Generic.List<<>f__AnonymousType2<int,string>>' to 'System.Collections.Generic.List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>''

            // Bu kısım opsiyoneldir

            // Örnek email içeriği:
            /*
            Merhaba {employee.FirstName},
            
            GörevNET sistemine hoş geldiniz!
            
            Giriş bilgileriniz:
            Email: {employee.Email}
            Geçici Şifre: {temporaryPassword}
            
            İlk girişinizde şifrenizi değiştirmeniz gerekmektedir.
            
            Sistem linki: {Request.Scheme}://{Request.Host}/Account/Login
            */
        }

        #endregion

    }

}
