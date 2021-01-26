using Dassanie.Data;
using Dassanie.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dassanie.Controllers
{
    public class SMSController : Controller
    {
        private ApplicationDbContext _ctx;

        public SMSController(ApplicationDbContext context)
        {
            _ctx = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(ClickSendInboundSMS clickSendMessage)
        {

            if (_ctx == null)
            {
                throw new Exception("Null context found");
            }

            if(clickSendMessage == null)
            {
                var json = JsonSerializer.Serialize(clickSendMessage);
                throw new Exception($"Invalid input {json}");
            }

            if (clickSendMessage.from == null)
            {
                var json = JsonSerializer.Serialize(clickSendMessage);
                throw new Exception($"Missing from {json}");
            }



            var user = _ctx.Users.FirstOrDefault(c => c.PhoneNumberConfirmed == false && clickSendMessage.from.ToString().EndsWith(c.PhoneNumber));

            if(user != null)
            {
                user.PhoneNumberConfirmed = true;
                _ctx.Users.Update(user);
                _ctx.SaveChanges();
            }
            else
            {
                var json = JsonSerializer.Serialize(clickSendMessage);
                throw new Exception($"Unable to find user input {json}");
            }
            
            return View();
        }
    }
}
