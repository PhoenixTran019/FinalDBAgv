using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace Sahafa.Controllers
{
    public class EmployeeController : Controller
    {
        public IActionResult ManagerDashboard()
        {
            return View();
        }

        public IActionResult StaffDashboard()
        {
            return View();
        }
    }
}
