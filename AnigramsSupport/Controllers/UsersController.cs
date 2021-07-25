using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TST.DATA.EF;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using IdentitySample.Models;

namespace TST.UI.MVC.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private TSTEntities db = new TSTEntities();

        // GET: Users
        public ActionResult Index()
        {
            var tSTUsers = db.TSTUsers.Include(t => t.TSTUserTitle);
            string heading = "Active Users";
            bool activeStatus = true;

            if (Request.QueryString["title"] != null)
            {
                int titleID = int.Parse(Request.QueryString["title"]);
                string titleName = db.TSTUserTitles.Find(titleID).Name;
                heading = titleName + "s";

                tSTUsers = tSTUsers.Where(t => t.UserTitleID == titleID).OrderBy(t => t.FirstName);
            }
            else if (Request.QueryString["active"] != null)
            {
                activeStatus = bool.Parse(Request.QueryString["active"]);
                heading = (activeStatus) ? "Active Users" : "Inactive Users";

                tSTUsers = tSTUsers.Where(t => t.IsActive == activeStatus).OrderBy(t => t.FirstName);
            }
            else
            {
                tSTUsers = tSTUsers.Where(t => t.IsActive == true).OrderBy(t => t.FirstName);
            }

            ViewBag.Heading = heading;
            return View(tSTUsers.ToList());
        }

        // GET: Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TSTUser tSTUser = db.TSTUsers.Find(id);
            if (tSTUser == null)
            {
                return HttpNotFound();
            }

            #region ShowInvolvedProjects
            bool contains = db.TSTProjects_Users.Where(p => p.TSTUserID == tSTUser.TSTUserID).Count() > 0;

            if (contains)
            {
                var projects = from proj in db.TSTProjects
                               join userProj in db.TSTProjects_Users on proj.ProjectID equals userProj.ProjectID
                               where userProj.TSTUserID == tSTUser.TSTUserID
                               select proj;

                ViewBag.Projects = projects.ToList();
            }
            else
            {
                ViewBag.Projects = null;
            }
            #endregion

            //figure out if displayed user is in current user's contact list
            string userID = User.Identity.GetUserId();
            TSTUser currentUser = db.TSTUsers.Where(t => t.UserID == userID).FirstOrDefault();

            bool isFriend = db.TSTUserContacts.Where(t => t.TSTUserID == currentUser.TSTUserID && t.FriendID == tSTUser.TSTUserID).Count() > 0;
            ViewBag.IsFriend = isFriend;

            return View(tSTUser);
        }

        // GET: Users/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.UserTitleID = new SelectList(db.TSTUserTitles, "UserTitleID", "Name");
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include = "TSTUserID,FirstName,LastName,UserID,UserTitleID,StartDate,EndDate,Address1,Address2,City,State,Zip,Country,Email,Image,DateOfBirth,UserPreferenceID,IsActive")]
        TSTUser tSTUser, HttpPostedFileBase userImage)
        {
            if (ModelState.IsValid)
            {
                #region SynchUserToUserAccount
                //similar code can be found in the UsersAdmin Create controller (HttpPost)

                //create a userManager
                var userManager = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();

                //create a new ApplicationUser Object
                //assign the username, email properties here
                var newUser = new ApplicationUser()
                {
                    //object initilizer syntax
                    UserName = tSTUser.Email,
                    Email = tSTUser.Email
                };

                //use the UserManager object to assign username and password combination
                userManager.Create(newUser, tSTUser.FirstName + "X1234$");
                //actually setting the default password
                //you could go to the identity.config and configure to email their new password to them.
                //don't forget to set up email in the ROOT web.config - you can also set it up in the Identity.config

                //add the user to Role based on Title
                if (tSTUser.UserTitleID == 3)
                {
                    //if developer
                    userManager.AddToRole(newUser.Id, "Tech");
                }
                else if (tSTUser.UserTitleID == 4)
                {
                    //if lead developer
                    userManager.AddToRole(newUser.Id, "Admin");
                }
                else
                {
                    //anyone else
                    userManager.AddToRole(newUser.Id, "User");
                }

                //assign the Employee.UserID to represent the newly created Identity UserId
                tSTUser.UserID = newUser.Id;
                //there is NO REASON to dispaly the UserID in ANY of your views, you can remove it from all
                #endregion

                #region CreatePreferences
                TSTUserPreference preferences = new TSTUserPreference()
                {
                    DisplayName = tSTUser.FullName,
                    ShowStreetAddress = false,
                    ShowCityState = true,
                    ShowZip = false,
                    ShowCountry = true,
                    ShowEmail = true,
                    ShowBirthday = true,
                    EmailNotifications = false
                };
                db.TSTUserPreferences.Add(preferences);

                tSTUser.UserPreferenceID = preferences.UserPreferenceID;
                #endregion

                #region FileUpload
                //default image
                string imageName = "noimage.jpg";

                if (userImage != null)
                {
                    imageName = userImage.FileName;

                    //get file extension
                    string ext = imageName.Substring(imageName.LastIndexOf('.'));

                    //only accept image files
                    if (ext == ".jpg" || ext == ".png" || ext == ".gif" || ext == ".jpeg")
                    {
                        imageName = Guid.NewGuid() + ext;

                        userImage.SaveAs(Server.MapPath("~/images/users/" + imageName));
                    }
                    else
                    {
                        ModelState.AddModelError("Image", "Only image files (.jpg, .png, .gif) are allowed");

                        return View(tSTUser);
                    }
                }

                //no matter what, add the image value to the project
                tSTUser.Image = imageName;
                #endregion

                tSTUser.StartDate = DateTime.Now;
                tSTUser.IsActive = true;

                db.TSTUsers.Add(tSTUser);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserTitleID = new SelectList(db.TSTUserTitles, "UserTitleID", "Name", tSTUser.UserTitleID);
            return View(tSTUser);
        }

        // GET: Users/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TSTUser tSTUser = db.TSTUsers.Find(id);
            if (tSTUser == null)
            {
                return HttpNotFound();
            }

            bool contains = db.TSTProjects_Users.Where(p => p.TSTUserID == tSTUser.TSTUserID).Count() > 0;
            List<TSTProject> userProjects = null;

            if (contains)
            {
                var projects = from proj in db.TSTProjects
                               join userProj in db.TSTProjects_Users on proj.ProjectID equals userProj.ProjectID
                               where userProj.TSTUserID == tSTUser.TSTUserID
                               select proj;

                userProjects = projects.ToList();
            }

            if (userProjects != null)
            {
                ViewBag.Projects = new MultiSelectList(db.TSTProjects, "ProjectID", "Name", userProjects.Select(p => p.ProjectID).ToArray());
            }
            else
            {
                ViewBag.Projects = new MultiSelectList(db.TSTProjects, "ProjectID", "Name");
            }

            ViewBag.UserTitleID = new SelectList(db.TSTUserTitles, "UserTitleID", "Name", tSTUser.UserTitleID);
            return View(tSTUser);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit([Bind(Include = "TSTUserID,FirstName,LastName,UserID,UserTitleID,StartDate,EndDate,Address1,Address2,City,State,Zip,Country,Email,Image,DateOfBirth,UserPreferenceID,IsActive")]
        TSTUser tSTUser, HttpPostedFileBase userImage, int[] selectedProjects)
        {
            if (ModelState.IsValid)
            {
                #region FileUpload
                if (userImage != null)
                {
                    string imageName = userImage.FileName;

                    //get file extension
                    string ext = imageName.Substring(imageName.LastIndexOf('.')).ToLower();

                    //only accept image files
                    if (ext == ".jpg" || ext == ".png" || ext == ".gif" || ext == ".jpeg")
                    {
                        imageName = Guid.NewGuid() + ext;

                        userImage.SaveAs(Server.MapPath("~/images/users/" + imageName));

                        tSTUser.Image = imageName;
                    }
                    else
                    {
                        ModelState.AddModelError("Image", "Only image files (.jpg, .png, .gif) are allowed");

                        ViewBag.UserTitleID = new SelectList(db.TSTUserTitles, "UserTitleID", "Name", tSTUser.UserTitleID);
                        return View(tSTUser);
                    }
                }
                #endregion

                if (tSTUser.EndDate == null && tSTUser.IsActive == false)
                {
                    //if user deactivated, auto-generate EndDate
                    tSTUser.EndDate = DateTime.Now;
                }

                if (selectedProjects != null)
                {
                    foreach (var item in selectedProjects)
                    {
                        bool inProject = db.TSTProjects_Users.Where(t => t.TSTUserID == tSTUser.TSTUserID && t.ProjectID == item).Count() > 0;

                        if (!inProject)
                        {
                            TSTProjects_Users userProject = new TSTProjects_Users()
                            {
                                TSTUserID = tSTUser.TSTUserID,
                                ProjectID = item
                            };
                            db.TSTProjects_Users.Add(userProject);
                        }
                    }
                }

                db.Entry(tSTUser).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", new { @id = tSTUser.TSTUserID });
            }

            bool contains = db.TSTProjects_Users.Where(p => p.TSTUserID == tSTUser.TSTUserID).Count() > 0;
            List<TSTProject> userProjects = null;

            if (contains)
            {
                var projects = from proj in db.TSTProjects
                               join userProj in db.TSTProjects_Users on proj.ProjectID equals userProj.ProjectID
                               where userProj.TSTUserID == tSTUser.TSTUserID
                               select proj;

                userProjects = projects.ToList();
            }

            if (userProjects != null)
            {
                ViewBag.Projects = new MultiSelectList(db.TSTProjects, "ProjectID", "Name", userProjects.Select(p => p.ProjectID).ToArray());
            }
            else
            {
                ViewBag.Projects = new MultiSelectList(db.TSTProjects, "ProjectID", "Name");
            }

            ViewBag.UserTitleID = new SelectList(db.TSTUserTitles, "UserTitleID", "Name", tSTUser.UserTitleID);
            
            return View(tSTUser);
        }

        // GET: Users/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TSTUser tSTUser = db.TSTUsers.Find(id);
            if (tSTUser == null)
            {
                return HttpNotFound();
            }
            return View(tSTUser);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            TSTUser tSTUser = db.TSTUsers.Find(id);
            //db.TSTUsers.Remove(tSTUser);
            tSTUser.IsActive = !tSTUser.IsActive; //soft delete - true becomes false, false becomes true
            db.Entry(tSTUser).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public void AddContact(int id)
        {
            //get current user
            string userID = User.Identity.GetUserId();
            TSTUser user = db.TSTUsers.Where(t => t.UserID == userID).SingleOrDefault();

            //find contact to be added
            TSTUser contact = db.TSTUsers.Find(id);

            TSTUserContact newContact = new TSTUserContact()
            {
                TSTUserID = user.TSTUserID,
                FriendID = contact.TSTUserID
            };

            db.TSTUserContacts.Add(newContact);

            //create a notification
            TSTUserNotification newNotification = new TSTUserNotification()
            {
                TSTUserID = contact.TSTUserID,
                PostedBy = user.TSTUserID,
                DateEntered = DateTime.Now,
                IsRead = false,
                Message = user.FirstName + " added you as a friend",
                TicketID = null
            };

            db.TSTUserNotifications.Add(newNotification);
            db.SaveChanges();
        }
    }//end class
}//end namespace
