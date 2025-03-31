using Microsoft.AspNetCore.Mvc;

namespace Sahafa.Controllers
{
    public class BooksController : Controller
    {
        public IActionResult Allbooks()
        {
            return View();
        }
        public IActionResult Textbooks()
        {
            return View();
        }

        public IActionResult Novels()
        {
            return View();
        }

        public IActionResult Comics()
        {
            return View();
        }

        public IActionResult BookForKid ()
        {
            return View();
        }

        public IActionResult Econmics()
        {
            return View();
        }
    }
}
