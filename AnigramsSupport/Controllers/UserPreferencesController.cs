using IdentitySample.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TST.DATA.EF;

namespace TST.UI.MVC.Controllers
{
    [Authorize]
    public class UserPreferencesController : Controller
    {
        private TSTEntities db = new TSTEntities();

        // GET: UserPreferences/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TSTUserPreference tSTUserPreference = db.TSTUserPreferences.Find(id);
            if (tSTUserPreference == null)
            {
                return HttpNotFound();
            }

            //figure out whether current user has a confirmed email
            string userID = User.Identity.GetUserId();
            var userManager = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            ApplicationUser user = userManager.FindById(userID);

            ViewBag.EmailConfirmed = user.EmailConfirmed;

            ViewBag.DisplayNameOptions = new SelectList(DisplayNameOptions());
            ViewBag.DisplayAddressOptions = new SelectList(DisplayAddressOptions());
            return View(tSTUserPreference);
        }

        // POST: UserPreferences/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserPreferenceID,DisplayName,ShowStreetAddress,ShowCityState,ShowZip,ShowCountry,ShowEmail,ShowBirthday,EmailNotifications")]
        TSTUserPreference tSTUserPreference, FormCollection formCollection)
        {
            if (ModelState.IsValid)
            {
                switch (formCollection["DisplayNameOptions"])
                {
                    case "First & Last Name":
                        tSTUserPreference.DisplayName = db.TSTUsers.Where(t => t.UserPreferenceID == tSTUserPreference.UserPreferenceID).FirstOrDefault().FullName;
                        break;
                    case "First Name, Last Initial":
                        string firstName = db.TSTUsers.Where(t => t.UserPreferenceID == tSTUserPreference.UserPreferenceID).FirstOrDefault().FirstName;
                        string lastInitial = db.TSTUsers.Where(t => t.UserPreferenceID == tSTUserPreference.UserPreferenceID).FirstOrDefault().LastName.Substring(0,1);

                        tSTUserPreference.DisplayName = firstName + " " + lastInitial + ".";
                        break;
                    case "First Initial, Last Name":
                        string lastName = db.TSTUsers.Where(t => t.UserPreferenceID == tSTUserPreference.UserPreferenceID).FirstOrDefault().LastName;
                        string firstInitial = db.TSTUsers.Where(t => t.UserPreferenceID == tSTUserPreference.UserPreferenceID).FirstOrDefault().FirstName.Substring(0, 1);

                        tSTUserPreference.DisplayName = firstInitial + ". " + lastName;
                        break;
                    default:
                        tSTUserPreference.DisplayName = db.TSTUsers.Where(t => t.UserPreferenceID == tSTUserPreference.UserPreferenceID).FirstOrDefault().FullName;
                        break;
                }

                switch (formCollection["DisplayAddressOptions"])
                {
                    case "Show Full Address":
                        tSTUserPreference.ShowStreetAddress = true;
                        tSTUserPreference.ShowCityState = true;
                        tSTUserPreference.ShowZip = true;
                        tSTUserPreference.ShowCountry = true;
                        break;
                    case "Show City and State":
                        tSTUserPreference.ShowStreetAddress = false;
                        tSTUserPreference.ShowCityState = true;
                        tSTUserPreference.ShowZip = false;
                        tSTUserPreference.ShowCountry = false;
                        break;
                    case "Show City, State, Country":
                        tSTUserPreference.ShowStreetAddress = false;
                        tSTUserPreference.ShowCityState = true;
                        tSTUserPreference.ShowZip = false;
                        tSTUserPreference.ShowCountry = true;
                        break;
                    case "Show Country Only":
                        tSTUserPreference.ShowStreetAddress = false;
                        tSTUserPreference.ShowCityState = false;
                        tSTUserPreference.ShowZip = false;
                        tSTUserPreference.ShowCountry = true;
                        break;
                    case "Do Not Show":
                        tSTUserPreference.ShowStreetAddress = false;
                        tSTUserPreference.ShowCityState = false;
                        tSTUserPreference.ShowZip = false;
                        tSTUserPreference.ShowCountry = false;
                        break;
                    default:
                        break;
                }

                db.Entry(tSTUserPreference).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", "Users", new { @id = db.TSTUsers.Where(t => t.UserPreferenceID == tSTUserPreference.UserPreferenceID).FirstOrDefault().TSTUserID });
            }

            //figure out whether current user has a confirmed email
            string userID = User.Identity.GetUserId();
            var userManager = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            ApplicationUser user = userManager.FindById(userID);

            ViewBag.EmailConfirmed = user.EmailConfirmed;

            ViewBag.DisplayNameOptions = new SelectList(DisplayNameOptions());
            ViewBag.DisplayAddressOptions = new SelectList(DisplayAddressOptions());
            return View(tSTUserPreference);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private IEnumerable<string> DisplayNameOptions()
        {
            return new List<string>
            {
                "First & Last Name",
                "First Name, Last Initial",
                "First Initial, Last Name"
            };
        }

        private IEnumerable<string> DisplayAddressOptions()
        {
            return new List<string>
            {
                "Show Full Address",
                "Show City and State",
                "Show City, State, Country",
                "Show Country Only",
                "Do Not Show"
            };
        }
    }//end class
}//end namespace
