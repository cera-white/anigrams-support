using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using TST.DATA.EF;

namespace TST.UI.MVC.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private TSTEntities db = new TSTEntities();

        // GET: Messages
        public ActionResult Index()
        {
            var tSTMessages = db.TSTMessages.Include(t => t.TSTUser).Include(t => t.TSTUser1);

            ViewBag.Contacts = FindContacts();

            //get current user
            string userID = User.Identity.GetUserId();
            TSTUser user = db.TSTUsers.Where(t => t.UserID == userID).SingleOrDefault();

            #region FilterByTab
            if (Request.QueryString["tab"] == "sent")
            {
                tSTMessages = tSTMessages.Where(t => t.Sender == user.TSTUserID && t.IsActive == true);
            }
            else if (Request.QueryString["tab"] == "trash")
            {
                tSTMessages = tSTMessages.Where(t => t.Recipient == user.TSTUserID && t.IsActive == false);
            }
            else
            {
                tSTMessages = tSTMessages.Where(t => t.Recipient == user.TSTUserID && t.IsActive == true);
            }
            #endregion

            return View(tSTMessages.ToList());
        }

        //POST: Messages
        [HttpPost]
        public ActionResult Index(int[] selectedObjects)
        {
            var tSTMessages = db.TSTMessages.Include(t => t.TSTUser).Include(t => t.TSTUser1);

            //soft delete selected messages
            foreach (int id in selectedObjects)
            {
                //find message associated with ID
                TSTMessage tSTMessage = db.TSTMessages.Find(id);

                tSTMessage.IsActive = !tSTMessage.IsActive;
                db.Entry(tSTMessage).State = EntityState.Modified;
            }
            db.SaveChanges();

            ViewBag.Contacts = FindContacts();

            //get current user
            string userID = User.Identity.GetUserId();
            TSTUser user = db.TSTUsers.Where(t => t.UserID == userID).SingleOrDefault();

            #region FilterByTab
            if (Request.QueryString["tab"] == "sent")
            {
                tSTMessages = tSTMessages.Where(t => t.Sender == user.TSTUserID && t.IsActive == true);
            }
            else if (Request.QueryString["tab"] == "trash")
            {
                tSTMessages = tSTMessages.Where(t => t.Recipient == user.TSTUserID && t.IsActive == false);
            }
            else
            {
                tSTMessages = tSTMessages.Where(t => t.Recipient == user.TSTUserID && t.IsActive == true);
            }
            #endregion

            return View(tSTMessages.ToList());
        }

        // GET: Messages/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TSTMessage tSTMessage = db.TSTMessages.Find(id);
            if (tSTMessage == null)
            {
                return HttpNotFound();
            }

            //if they got this far, mark message as read (since they are now looking at it)
            tSTMessage.IsRead = true;
            db.Entry(tSTMessage).State = EntityState.Modified;
            db.SaveChanges();

            ViewBag.Contacts = FindContacts();

            return View(tSTMessage);
        }

        [HttpPost]
        public ActionResult Details(int id)
        {
            TSTMessage tSTMessage = db.TSTMessages.Find(id);
            //db.TSTMessages.Remove(tSTMessage);
            tSTMessage.IsActive = !tSTMessage.IsActive; //soft delete - true becomes false, false becomes true
            db.Entry(tSTMessage).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: Messages/Create
        public ActionResult Create()
        {
            TSTMessage tSTMessage = new TSTMessage();

            //find pre-selected contact
            if (Request.QueryString["user"] != null)
            {
                int userID = int.Parse(Request.QueryString["user"]);
                ViewBag.Recipient = new SelectList(db.TSTUsers.Select(t => new { t.TSTUserID, t.TSTUserPreference.DisplayName }), "TSTUserID", "DisplayName", userID);
            }
            else
            {
                ViewBag.Recipient = new SelectList(db.TSTUsers.Select(t => new { t.TSTUserID, t.TSTUserPreference.DisplayName }), "TSTUserID", "DisplayName");
            }

            if (Request.QueryString["subject"] != null)
            {
                tSTMessage.Subject = Request.QueryString["subject"];
            }

            ViewBag.Sender = new SelectList(db.TSTUsers, "TSTUserID", "FirstName");

            ViewBag.Contacts = FindContacts();

            return View(tSTMessage);
        }

        // POST: Messages/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MessageID,Sender,Recipient,Subject,Message,DateSent,IsRead,IsActive")] TSTMessage tSTMessage)
        {
            if (ModelState.IsValid)
            {
                //set values not determined by user
                string userID = User.Identity.GetUserId();
                TSTUser user = db.TSTUsers.Where(t => t.UserID == userID).SingleOrDefault();

                tSTMessage.Sender = user.TSTUserID;
                tSTMessage.DateSent = DateTime.Now;
                tSTMessage.IsRead = false;
                tSTMessage.IsActive = true;

                db.TSTMessages.Add(tSTMessage);
                db.SaveChanges();

                TSTUser recipient = db.TSTUsers.Find(tSTMessage.Recipient);

                //email notification
                if (recipient.TSTUserPreference.EmailNotifications == true)
                {
                    MailMessage msg = new MailMessage(new MailAddress("noreply@anigrams.org", "Anigrams Support"), new MailAddress(recipient.Email));
                    msg.Subject = "You've Received a New Message";
                    msg.Body = string.Format("<p><strong>{0} has sent you a new message:</strong></p><blockquote>{1}</blockquote><p>Please <a href='http://support.anigrams.org/Messages/Details/{2}'>click here to review and respond to this message.</a> Thank you for using Anigrams Support!</p><br/><br/><p style='font-size:.8em'>You received this email because you opted in to receive notification emails from Anigrams Support. If you no longer wish to receive these emails, <a href='http://support.anigrams.org/UserPreferences/Edit/{3}'>please change your settings.</a></p>",
                        user.TSTUserPreference.DisplayName, tSTMessage.Message, tSTMessage.MessageID, recipient.UserPreferenceID);
                    msg.IsBodyHtml = true;

                    SmtpClient client = new SmtpClient("mail.anigrams.org", 26)
                    {
                        Credentials = new NetworkCredential("noreply@anigrams.org", "rIc41j6@")
                    };

                    client.Send(msg);
                }

                return RedirectToAction("Index");
            }

            ViewBag.Contacts = FindContacts();

            ViewBag.Sender = new SelectList(db.TSTUsers, "TSTUserID", "FirstName", tSTMessage.Sender);
            ViewBag.Recipient = new SelectList(db.TSTUsers.Select(t => new { t.TSTUserID, t.TSTUserPreference.DisplayName }), "TSTUserID", "DisplayName", tSTMessage.Recipient);
            return View(tSTMessage);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public IQueryable<TSTUser> FindContacts()
        {
            //get current user
            string userID = User.Identity.GetUserId();
            TSTUser user = db.TSTUsers.Where(t => t.UserID == userID).SingleOrDefault();

            //find user's contacts
            var contacts = db.TSTUserContacts.Where(t => t.TSTUserID == user.TSTUserID).Select(t => t.TSTUser1);

            return contacts;
        }
    }
}
