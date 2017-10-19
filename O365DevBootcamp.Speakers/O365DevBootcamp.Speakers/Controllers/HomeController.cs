using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using O365DevBootcamp.Speakers.Data;
using O365DevBootcamp.Speakers.Models;

namespace O365DevBootcamp.Speakers.Controllers
{
    public class HomeController : Controller
    {
        private IDbContext _db = null;

        public HomeController(IDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Checkin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Checkin(string email, string fullname, string twitter, string blog, string picture)
        {
            Attendee attendee = new Attendee
            {
                Email = email,
                Fullname = fullname,
                Twitter = twitter,
                Blog = blog,
                Picture = picture
            };

            //defaultConnection
            await _db.InsertAsync(attendee);

            return RedirectToAction("Welcome");
        }

        public IActionResult Welcome()
        {
            return View();
        }
    }
    [Table("Attendee")]
    public class Attendee
    {
        public string Email { get; set; }
        public string Fullname { get; set; }
        public string Twitter { get; set; }
        public string Blog { get; set; }
        public string Picture { get; set; }
        public bool Added { get; set; }
    }
}
