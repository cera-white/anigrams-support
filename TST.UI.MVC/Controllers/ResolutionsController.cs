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
    public class ResolutionsController : Controller
    {
        private TSTEntities db = new TSTEntities();

        // GET: Resolutions/Create
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TSTTicket tSTTicket = db.TSTTickets.Find(id);
            if (tSTTicket == null)
            {
                return HttpNotFound();
            }

            ViewBag.Ticket = tSTTicket;

            ViewBag.PostedBy = new SelectList(db.TSTUsers, "TSTUserID", "FullName");
            return View();
        }

        // POST: Resolutions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ResolutionID,Problem,Resolution,PostedBy,DatePosted,Attachment")] int? id, TSTResolution tSTResolution, HttpPostedFileBase attachment)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TSTTicket tSTTicket = db.TSTTickets.Find(id);
            if (tSTTicket == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                //get current user
                string userID = User.Identity.GetUserId();
                var user = db.TSTUsers.Where(u => u.UserID == userID).SingleOrDefault();

                //programatically-generated fields
                tSTResolution.PostedBy = user.TSTUserID;
                tSTResolution.DatePosted = DateTime.Now;

                #region FileUpload
                if (attachment != null)
                {
                    string fileName = attachment.FileName;

                    //get file extension
                    string ext = fileName.Substring(fileName.LastIndexOf('.'));

                    if (ext == ".exe" || ext == ".dll")
                    {
                        ModelState.AddModelError("Attachment", ".exe and .dll files not allowed");

                        return View(tSTTicket);
                    }

                    fileName = Guid.NewGuid() + ext;

                    attachment.SaveAs(Server.MapPath("~/Content/attachments/" + fileName));

                    tSTResolution.Attachment = fileName;
                }
                #endregion

                #region UpdateAssociatedTicket
                //update fields to reflect resolved status
                tSTTicket.ResolutionID = tSTResolution.ResolutionID;
                tSTTicket.DateResolved = tSTResolution.DatePosted;
                tSTTicket.TicketStatusID = 3; //Closed status
                tSTTicket.TechAssigned = tSTResolution.PostedBy;

                db.Entry(tSTTicket).State = EntityState.Modified;
                #endregion

                #region NotifyUser
                TSTUserNotification newNotification = new TSTUserNotification()
                {
                    PostedBy = user.TSTUserID,
                    TSTUserID = tSTTicket.SubmittedBy,
                    Message = string.Format("{0} resolved \"{1}\"", user.FirstName, tSTTicket.Subject),
                    DateEntered = DateTime.Now,
                    IsRead = false,
                    TicketID = tSTTicket.TicketID
                };
                db.TSTUserNotifications.Add(newNotification);

                if (tSTTicket.TSTUser.TSTUserPreference.EmailNotifications == true)
                {
                    MailMessage msg = new MailMessage(new MailAddress("noreply@anigrams.org", "Anigrams Support"), new MailAddress(tSTTicket.TSTUser.Email));
                    msg.Subject = "Your Ticket Has Been Closed";
                    msg.Body = string.Format("<p><strong>Your ticket, \"{0}\", has been {1}.</strong></p><br/><p><a href='http://support.anigrams.org/Tickets/Details/{2}'>Click here to review the changes.</a> Thank you for using Anigrams Support!</p><br/><br/><p style='font-size:.8em'>You received this email because you opted in to receive notification emails from Anigrams Support. If you no longer wish to receive these emails, <a href='http://support.anigrams.org/UserPreferences/Edit/{3}'>please change your settings.</a></p>",
                            tSTTicket.Subject, tSTTicket.TSTTicketStatus.Name, tSTTicket.TicketID, tSTTicket.TSTUser.UserPreferenceID);
                    msg.IsBodyHtml = true;

                    SmtpClient client = new SmtpClient("mail.anigrams.org", 26)
                    {
                        Credentials = new NetworkCredential("noreply@anigrams.org", "rIc41j6@")
                    };

                    client.Send(msg);
                }
                #endregion

                db.TSTResolutions.Add(tSTResolution);
                db.SaveChanges();
                return RedirectToAction("Details", "Tickets", new { @id = tSTTicket.TicketID });
            }

            ViewBag.Ticket = tSTTicket;
            ViewBag.PostedBy = new SelectList(db.TSTUsers, "TSTUserID", "FirstName", tSTResolution.PostedBy);
            return View(tSTResolution);
        }

        // GET: Resolutions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TSTResolution tSTResolution = db.TSTResolutions.Find(id);
            if (tSTResolution == null)
            {
                return HttpNotFound();
            }

            ViewBag.Ticket = db.TSTTickets.Where(t => t.ResolutionID == tSTResolution.ResolutionID).FirstOrDefault();
            ViewBag.PostedBy = new SelectList(db.TSTUsers, "TSTUserID", "FirstName", tSTResolution.PostedBy);
            return View(tSTResolution);
        }

        // POST: Resolutions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ResolutionID,Problem,Resolution,PostedBy,DatePosted,Attachment")] TSTResolution tSTResolution)
        {
            TSTTicket tSTTicket = db.TSTTickets.Where(t => t.ResolutionID == tSTResolution.ResolutionID).FirstOrDefault();

            if (ModelState.IsValid)
            {
                db.Entry(tSTResolution).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", "Tickets", new { @id = tSTTicket.TicketID });
            }

            ViewBag.Ticket = tSTTicket;
            ViewBag.PostedBy = new SelectList(db.TSTUsers, "TSTUserID", "FirstName", tSTResolution.PostedBy);
            return View(tSTResolution);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
