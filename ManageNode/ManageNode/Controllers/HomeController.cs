using ManageNode.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ManageNode.ViewModel;

namespace ManagmentServer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private List<FileViewModel> fileViews = new List<FileViewModel>();
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Privacy()
        {
            return View();
        }
        
        
        [HttpPost]
        public ActionResult Show()
        {
            return View(fileViews);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
