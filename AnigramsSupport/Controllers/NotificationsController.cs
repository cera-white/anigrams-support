using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TST.DATA.EF;

namespace TST.UI.MVC.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private TSTEntities db = new TSTEntities();

        // GET: Notifications
        public ActionResult Index()
        {
            string userID = User.Identity.GetUserId();
            var user = db.TSTUsers.Where(u => u.UserID == userID).SingleOrDefault();

            var tSTUserNotifications = db.TSTUserNotifications.Where(t => t.TSTUserID == user.TSTUserID).OrderByDescending(t => t.DateEntered);

            return View(tSTUserNotifications);
        }

        public ActionResult ClearNotifications(int? id)
        {
            string userID = User.Identity.GetUserId();
            var user = db.TSTUsers.Where(u => u.UserID == userID).SingleOrDefault();

            var tSTUserNotifications = db.TSTUserNotifications.Where(t => t.TSTUserID == user.TSTUserID);

            if (id != null)
            {
                //if an id is passed for an individual notification, update that notification's IsRead property
                TSTUserNotification tSTUserNotification = db.TSTUserNotifications.Find(id);
                tSTUserNotification.IsRead = true;
                db.Entry(tSTUserNotification).State = EntityState.Modified;
                db.SaveChanges();

                //redirect to details page - either for the ticket or the user
                if (Request.QueryString["ticketID"] != null)
                {
                    int ticketID = int.Parse(Request.QueryString["ticketID"]);
                    return RedirectToAction("Details", "Tickets", new { @id = ticketID });
                }
                else
                {
                    return RedirectToAction("Details", "Users", new { @id = tSTUserNotification.PostedBy });
                }
            }

            return View();
        }

        public void ClearAllNotifications()
        {
            string userID = User.Identity.GetUserId();
            var user = db.TSTUsers.Where(u => u.UserID == userID).SingleOrDefault();

            var tSTUserNotifications = db.TSTUserNotifications.Where(t => t.TSTUserID == user.TSTUserID);

            foreach (var item in tSTUserNotifications)
            {
                item.IsRead = true;
                db.Entry(item).State = EntityState.Modified;
            }
            db.SaveChanges();
        }
    }//end class
}//end namespace
