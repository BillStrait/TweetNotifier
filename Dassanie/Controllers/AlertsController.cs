using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Dassanie.Data;
using Dassanie.Models;
using LinqToTwitter;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Dassanie.Controllers
{
    [Authorize]
    public class AlertsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private IdentityUser _user;
        private TwitterContext _twtContext;
        private UserDetail _userDetails;
        public AlertsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Alerts
        public async Task<IActionResult> Index()
        {
            SetupUser();
            
            if (_user != null)
            {
                var alerts = _context.Alerts.Where(c => c.UserId == _user.Id)?.ToList();
                var alertVMs = new List<AlertVM>();
                
                foreach(var alert in alerts)
                {
                    alertVMs.Add(new AlertVM(alert));
                }


                return View(alertVMs);

            }

            return View(await _context.Alerts.ToListAsync());
        }

        // GET: Alerts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            SetupUser();

            var alert = await _context.Alerts
                .FirstOrDefaultAsync(m => m.AlertId == id && m.UserId == _user.Id);
            if (alert == null)
            {
                return NotFound();
            }

            return View(alert);
        }

        // GET: Alerts/Create
        public IActionResult Create()
        {
            var vm = new AlertVM() {  };
            return View(vm);
        }

        // POST: Alerts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string FollowerName, ulong FollowerId, string TriggerWords, bool IncludeLink, bool AlwaysAlert)
        {
            SetupUser();


            if((string.IsNullOrWhiteSpace(TriggerWords) && !AlwaysAlert) || string.IsNullOrWhiteSpace(FollowerName) || FollowerId==0)
            {
                //TODO: Abstract creating an empty viewmodel. Add it here and the base create controller. Prolly can use the same thing
                //for edit too.

                return View(new AlertVM()
                {
                    Error = "Sorry, something went wrong. Please refresh this page and try again.",
                    AlertWords = TriggerWords,
                    IncludeLink = IncludeLink,
                    AlwaysAlert = AlwaysAlert
                });
            }

            var alert = new Alert()
            {
                TriggerWords = TriggerWords,
                SMS = true, //this is always true for now. When other alert options are added this will no longer be hard coded.
                LastChecked = DateTime.UtcNow,
                IncludeLink = IncludeLink,
                TwitterFollowId = FollowerId,
                TwitterFollowName = FollowerName,
                UserId = _user.Id,
                AlwaysAlert = AlwaysAlert
            };

            _context.Alerts.Add(alert);
            var result = _context.SaveChangesAsync();

            if (await result>0)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(new AlertVM(alert));
        }

        // GET: Alerts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            SetupUser();
            var alert = await _context.Alerts.FirstAsync(c=>c.AlertId == id && c.UserId == _userDetails.UserId);
            if (alert == null)
            {
                return NotFound();
            }

            var vm = new AlertVM(alert);

            return View(vm);
        }

        // POST: Alerts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int alertId, string triggerWords, bool includeLink, bool alwaysAlert)
        {
            SetupUser();

            var alert = await _context.Alerts.FirstAsync(c => c.AlertId == alertId && c.UserId == _userDetails.UserId);

            if (alert == null) {
                return NotFound();
            }
            else if(string.IsNullOrEmpty(triggerWords) && !alwaysAlert) 
            {
                return View(new AlertVM(alert) { Error = "Something went wrong. Please make sure you have either Alert Words or Always Alert enabled. If the problem persists please return to the index and try again." });
            }


            alert.IncludeLink = includeLink;
            alert.TriggerWords = triggerWords;
            alert.AlwaysAlert = alwaysAlert;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(alert);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlertExists(alert.AlertId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(alert);
        }

        // GET: Alerts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            SetupUser();
            var alert = await _context.Alerts
                .FirstOrDefaultAsync(m => m.AlertId == id && m.UserId == _userDetails.UserId);
            if (alert == null)
            {
                return NotFound();
            }

            return View(alert);
        }

        // POST: Alerts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alert = await _context.Alerts.FindAsync(id);
            _context.Alerts.Remove(alert);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private void SetupUser()
        {
            _user = _context.Users.First(c => c.Email == HttpContext.User.Identity.Name);
            _userDetails = _context.UserDetails.First(c => c.UserId == _user.Id);
        }

        private void SetupContext()
        {
            if(_userDetails == null)
            {
                _userDetails = _context.UserDetails.First(c => c.UserId == _user.Id);
            }
            
            _twtContext = new TwitterContext(new MvcAuthorizer()
            {
                CredentialStore = new SessionStateCredentialStore(HttpContext.Session)
                {
                    OAuthToken = _userDetails.TwitterOAuthToken,
                    OAuthTokenSecret = _userDetails.TwitterOAuthSecret,
                    ConsumerKey = Environment.GetEnvironmentVariable("twitterApiKey"),
                    ConsumerSecret = Environment.GetEnvironmentVariable("twitterApiSecret")
                }
            });
        }
        private bool AlertExists(int id)
        {
            return _context.Alerts.Any(e => e.AlertId == id);
        }

        
    }
}
