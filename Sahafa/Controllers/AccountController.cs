using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sahafa.Data;
using Sahafa.Models;
using Sahafa.Services;
using System.Security.Claims;

namespace Sahafa.Controllers
{
    public class AccountController : Controller
    {
        

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly AuthService _authService;

        public AccountController(ApplicationDbContext context, UserManager<IdentityUser> userManager,
                                 SignInManager<IdentityUser> signInManager, AuthService authService)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _authService = authService;
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        

        [HttpPost]
        public IActionResult Register(RegisterCustomer model)
        {
            if (ModelState.IsValid)
            {
                // Chuyển username thành chữ thường để tránh phân biệt hoa/thường
                string normalizedUsername = model.Username.ToLower();

                // Kiểm tra xem username hoặc email đã tồn tại chưa
                bool userExists = _context.AccountCustomer.Any(a => a.Username == normalizedUsername);
                bool emailExists = _context.Customers.Any(c => c.Email == model.Email);

                if (userExists)
                {
                    ModelState.AddModelError("", "Username đã tồn tại.");
                    return View(model);
                }

                if (emailExists)
                {
                    ModelState.AddModelError("", "Email đã được sử dụng.");
                    return View(model);
                }

                // Tạo một Customer mới
                var customer = new Customers
                {
                    FullName = "", // User có thể cập nhật sau
                    Email = model.Email,
                    Phone = "",
                    Address = "",
                    DOB = null
                };

                _context.Customers.Add(customer);
                _context.SaveChanges(); // Lưu xuống SQL Server để có CustomerID

                // Tạo một AccountCustomer mới
                var accountCustomer = new AccountCustomers
                {
                    Username = normalizedUsername,
                    Password = model.Password, // Không mã hóa theo yêu cầu bài tập
                    CustomerID = customer.CustomerID,
                    RegistrationDate = DateTime.Now
                };

                _context.AccountCustomer.Add(accountCustomer);
                _context.SaveChanges(); // Lưu xuống SQL Server

                return RedirectToAction("Login", "Account");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.ErrorMessage = "❌ Vui lòng nhập đầy đủ tài khoản và mật khẩu!";
                return View();
            }

            string normalizedUsername = username.Trim().ToLower();
            var (user, role) = await _authService.Authenticate(normalizedUsername, password);

            if (user == null || string.IsNullOrEmpty(role))
            {
                ViewBag.ErrorMessage = "❌ Sai tài khoản hoặc mật khẩu!";
                return View();
            }

            // 🔹 Lưu thông tin vào Session
            HttpContext.Session.SetString("Username", normalizedUsername);
            HttpContext.Session.SetString("UserRole", role);

            // 🔹 Điều hướng theo role
            return role switch
            {
                "Customer" => RedirectToAction("Index", "Home"),
                "Manager" => RedirectToAction("ManagerDashboard", "Employee"),
                "Staff" => RedirectToAction("StaffDashboard", "Employee"),
                _ => RedirectToAction("Login", "Account") // Nếu role sai, quay lại Login
            };
        }


        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Xóa toàn bộ Session
            return RedirectToAction("Index", "Home");
        }
    }
}
