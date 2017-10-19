using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols;
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

        public async Task<IActionResult> List()
        {
            var list = await _db.QueryManyAsync<Attendee>("select * from Attendee");
            return View(list.ToList());
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

        public async Task<IActionResult> Add(string email)
        {
            var attendee = await _db.QueryAsync<Attendee>("select top 1 * from Attendee where email = @email", new { email });


            var client = new HttpClient();
            var url = "https://outlook.office.com/webhook/4227e51c-d798-4c41-8e2c-9139d6afc5f2@5697ac0d-aec6-4e7d-9437-986d6cac2590/IncomingWebhook/821d15eb43414232a1e958e61582be7f/7924b20e-426e-4276-a710-d9ed98b152d5";
            var message = $"{{\"text\": \"{attendee.Fullname} checked in.\",\"sections\": [{{\"activityTitle\": \"Check-in\",\"activityText\": \"{attendee.Email}\",\"activityImage\": \"{attendee.Picture}\"}}]}}";
            var body = new StringContent(message, System.Text.Encoding.UTF8, "application/json");
            await client.PostAsync(url, body);

            
            attendee.Added = true;
            await _db.UpdateAsync(attendee);

            return RedirectToAction("List");
        }
    }
    [Table("Attendee")]
    public class Attendee
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Email { get; set; }
        public string Fullname { get; set; }
        public string Twitter { get; set; }
        public string Blog { get; set; }
        public string Picture { get; set; }
        public bool Added { get; set; }
    }
}
