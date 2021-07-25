using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using TST.DATA.EF;
using PagedList;
using System.Net.Mail;

namespace TST.UI.MVC.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private TSTEntities db = new TSTEntities();

        //GET: Index()
        public ActionResult Index(int page = 1)
        {
            int pageSize = 10;

            var tSTTickets = db.TSTTickets.Include(t => t.TSTProject).Include(t => t.TSTResolution).Include(t => t.TSTTicketClassification).Include(t => t.TSTTicketStatus).Include(t => t.TSTUser).Include(t => t.TSTUser1);
            string heading = "All Tickets";
            int statusID = 0;
            int classID = 0;

            if (User.IsInRole("User"))
            {
                string userID = User.Identity.GetUserId();
                var user = db.TSTUsers.Where(u => u.UserID == userID).SingleOrDefault();

                heading = "Tickets Submitted By Me";
                tSTTickets = tSTTickets.Where(t => t.SubmittedBy == user.TSTUserID);
            }

            if (Request.QueryString["status"] != null)
            {
                switch (Request.QueryString["status"])
                {
                    case "pending":
                        heading = "Pending Tickets";
                        tSTTickets = tSTTickets.Where(t => t.TicketStatusID == 1);
                        statusID = 1;
                        break;
                    case "open":
                        heading = "Open & Approved Tickets";
                        tSTTickets = tSTTickets.Where(t => t.TicketStatusID == 2 || t.TicketStatusID == 4);
                        statusID = 2;
                        break;
                    case "closed":
                        heading = "Closed & Denied Tickets";
                        tSTTickets = tSTTickets.Where(t => t.TicketStatusID == 3 || t.TicketStatusID == 5);
                        statusID = 3;
                        break;
                    default:
                        break;
                }
            }
            else if (Request.QueryString["cat"] != null)
            {
                classID = int.Parse(Request.QueryString["cat"]);

                string className = db.TSTTicketClassifications.Find(classID).Name;
                heading = (classID != 4) ? className + "s" : className + "es";

                tSTTickets = tSTTickets.Where(t => t.TicketClassificationID == classID);
            }

            ViewBag.Heading = heading;

            ViewBag.TicketStatusID = (statusID != 0) ? new SelectList(db.TSTTicketStatuses, "TicketStatusID", "Name", statusID) : ViewBag.TicketStatusID = new SelectList(db.TSTTicketStatuses, "TicketStatusID", "Name");
            ViewBag.TicketClassificationID = (classID != 0) ? new SelectList(db.TSTTicketClassifications.OrderBy(t => t.PriorityLevel), "TicketClassificationID", "Name", classID) : new SelectList(db.TSTTicketClassifications, "TicketClassificationID", "Name");
            ViewBag.ProjectID = new SelectList(db.TSTProjects, "ProjectID", "Name");
            return View(tSTTickets.OrderByDescending(t => t.DateSubmitted).ToPagedList(page, pageSize));
        }

        // POST: Tickets
        [HttpPost]
        public ActionResult Index(FormCollection formCollection, int page = 1)
        {
            int pageSize = 10;

            var tSTTickets = db.TSTTickets.Include(t => t.TSTProject).Include(t => t.TSTResolution).Include(t => t.TSTTicketClassification).Include(t => t.TSTTicketStatus).Include(t => t.TSTUser).Include(t => t.TSTUser1);

            if (User.IsInRole("User"))
            {
                string userID = User.Identity.GetUserId();
                var user = db.TSTUsers.Where(u => u.UserID == userID).SingleOrDefault();

                ViewBag.Heading = "Tickets Submitted By Me";
                tSTTickets = tSTTickets.Where(t => t.SubmittedBy == user.TSTUserID);
            }
            else
            {
                ViewBag.Heading = "All Tickets";
            }

            int temp = 0;

            if (!string.IsNullOrWhiteSpace(formCollection["searchCriteria"]))
            {
                string criteria = formCollection["searchCriteria"].ToLower();

                tSTTickets = tSTTickets.Where(x => (x.Subject.ToLower().Contains(criteria)) || (x.Description.ToLower().Contains(criteria)) || (x.Tags.ToLower().Contains(criteria))
                || (x.TSTResolution.Problem.ToLower().Contains(criteria)) || (x.TSTResolution.Resolution.ToLower().Contains(criteria)));

                ViewBag.Heading = "Results for \"" + criteria + "\"";
            }

            #region FilterByProject
            if (int.TryParse(formCollection["ProjectID"], out temp))
            {
                int projectID = int.Parse(formCollection["ProjectID"]);
                tSTTickets = tSTTickets.Where(t => t.ProjectID == projectID);
                string projectName = db.TSTProjects.Find(projectID).Name;
                ViewBag.Heading = "Tickets for " + projectName;
                ViewBag.ProjectID = new SelectList(db.TSTProjects, "ProjectID", "Name", projectID);
            }
            else
            {
                ViewBag.ProjectID = new SelectList(db.TSTProjects, "ProjectID", "Name");
            }
            #endregion

            #region FilterByClassification
            if (int.TryParse(formCollection["TicketClassificationID"], out temp))
            {
                int classID = int.Parse(formCollection["TicketClassificationID"]);
                tSTTickets = tSTTickets.Where(t => t.TicketClassificationID == classID);
                string className = db.TSTTicketClassifications.Find(classID).Name;
                ViewBag.Heading = (classID != 4) ? className + "s" : className + "es";
                ViewBag.TicketClassificationID = new SelectList(db.TSTTicketClassifications.OrderBy(t => t.PriorityLevel), "TicketClassificationID", "Name", classID);
            }
            else
            {
                ViewBag.TicketClassificationID = new SelectList(db.TSTTicketClassifications.OrderBy(t => t.PriorityLevel), "TicketClassificationID", "Name");
            }
            #endregion

            #region FilterByStatus
            if (int.TryParse(formCollection["TicketStatusID"], out temp))
            {
                int statusID = int.Parse(formCollection["TicketStatusID"]);
                tSTTickets = tSTTickets.Where(t => t.TicketStatusID == statusID);
                string statusName = db.TSTTicketStatuses.Find(statusID).Name;
                ViewBag.Heading = statusName + " Tickets";
                ViewBag.TicketStatusID = new SelectList(db.TSTTicketStatuses, "TicketStatusID", "Name", statusID);
            }
            else
            {
                ViewBag.TicketStatusID = new SelectList(db.TSTTicketStatuses, "TicketStatusID", "Name");
            }
            #endregion

            return View(tSTTickets.OrderByDescending(t => t.DateSubmitted).ToPagedList(page, pageSize));
        }

        // GET: Tickets/Details/5
        public ActionResult Details(int? id)
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
            return View(tSTTicket);
        }

        // GET: Tickets/Create
        [Authorize]
        public ActionResult Create()
        {
            int projectID = 0;

            if (Request.QueryString["proj"] != null)
            {
                projectID = int.Parse(Request.QueryString["proj"]);
            }

            ViewBag.ProjectID = (projectID != 0) ? new SelectList(db.TSTProjects, "ProjectID", "Name", projectID) : new SelectList(db.TSTProjects, "ProjectID", "Name");

            ViewBag.ResolutionID = new SelectList(db.TSTResolutions, "ResolutionID", "Problem");
            ViewBag.TicketClassificationID = new SelectList(db.TSTTicketClassifications.OrderBy(t => t.PriorityLevel), "TicketClassificationID", "Name");
            ViewBag.TicketStatusID = new SelectList(db.TSTTicketStatuses, "TicketStatusID", "Name");
            ViewBag.SubmittedBy = new SelectList(db.TSTUsers, "TSTUserID", "FirstName");
            ViewBag.TechAssigned = new SelectList(db.TSTUsers.Where(t => t.UserTitleID == 3 || t.UserTitleID == 4), "TSTUserID", "FullName");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create([Bind(Include = "TicketID,Subject,Description,TicketClassificationID,SubmittedBy,TechAssigned,TicketStatusID,DateSubmitted,DateResolved,ProjectID,Attachment1,Attachment2,Tags,ResolutionID")]
        TSTTicket tSTTicket, HttpPostedFileBase attachment1, HttpPostedFileBase attachment2)
        {
            if (ModelState.IsValid)
            {
                //get user
                string userID = User.Identity.GetUserId();
                var user = db.TSTUsers.Where(u => u.UserID == userID).SingleOrDefault();

                tSTTicket.SubmittedBy = user.TSTUserID;
                tSTTicket.DateSubmitted = DateTime.Now;
                tSTTicket.TicketStatusID = 1; //Pending status

                //add user to project if they aren't already
                bool inProject = db.TSTProjects_Users.Where(t => t.TSTUserID == user.TSTUserID && t.ProjectID == tSTTicket.ProjectID).Count() > 0;
                if (!inProject)
                {
                    TSTProjects_Users userProject = new TSTProjects_Users()
                    {
                        TSTUserID = user.TSTUserID,
                        ProjectID = tSTTicket.ProjectID
                    };
                    db.TSTProjects_Users.Add(userProject);
                }

                #region FileUpload
                if (attachment1 != null)
                {
                    string fileName = attachment1.FileName;

                    string ext = fileName.Substring(fileName.LastIndexOf('.'));

                    if (ext == ".exe" || ext == ".dll")
                    {
                        ModelState.AddModelError("Attachment1", ".exe and .dll files not allowed");

                        return View(tSTTicket);
                    }

                    fileName = Guid.NewGuid() + ext;

                    attachment1.SaveAs(Server.MapPath("~/Content/attachments/" + fileName));

                    tSTTicket.Attachment1 = fileName;
                }
                if (attachment2 != null)
                {
                    string fileName = attachment2.FileName;

                    string ext = fileName.Substring(fileName.LastIndexOf('.'));

                    if (ext == ".exe" || ext == ".dll")
                    {
                        ModelState.AddModelError("Attachment2", ".exe and .dll files not allowed");

                        return View(tSTTicket);
                    }

                    fileName = Guid.NewGuid() + ext;

                    attachment2.SaveAs(Server.MapPath("~/Content/attachments/" + fileName));

                    tSTTicket.Attachment2 = fileName;
                }
                #endregion

                db.TSTTickets.Add(tSTTicket);
                db.SaveChanges();

                //notify Lead Developer
                TSTUser leadDeveloper = db.TSTUsers.Find(1); //hard-coded ID since I know who the lead developer is (me)
                TSTProject project = db.TSTProjects.Find(tSTTicket.ProjectID);

                CreateNotification(user.TSTUserID, leadDeveloper.TSTUserID,
                    string.Format("{0} submitted a ticket for {1}", user.FirstName, project.Name),
                    tSTTicket.TicketID);

                if (user.TSTUserPreference.EmailNotifications == true)
                {
                    MailMessage msg = new MailMessage(new MailAddress("noreply@anigrams.org", "Anigrams Support"), new MailAddress(leadDeveloper.Email));
                    msg.Subject = "New Ticket Submitted";
                    msg.Body = string.Format("<p><strong>{0} submitted a new ticket for {1}:</strong></p><blockquote>{2}</blockquote><p>Please <a href='http://support.anigrams.org/Tickets/Details/{3}'>click here to review the details.</a> Thank you for using Anigrams Support!</p><br/><br/><p style='font-size:.8em'>You received this email because you opted in to receive notification emails from Anigrams Support. If you no longer wish to receive these emails, <a href='http://support.anigrams.org/UserPreferences/Edit/{4}'>please change your settings.</a></p>",
                    user.TSTUserPreference.DisplayName, project.Name, tSTTicket.Description, tSTTicket.TicketID, user.UserPreferenceID);
                    msg.IsBodyHtml = true;

                    SmtpClient client = new SmtpClient("mail.anigrams.org", 26)
                    {
                        Credentials = new NetworkCredential("noreply@anigrams.org", "rIc41j6@")
                    };

                    client.Send(msg);
                }

                return RedirectToAction("Index");
            }

            ViewBag.ProjectID = new SelectList(db.TSTProjects, "ProjectID", "Name", tSTTicket.ProjectID);
            ViewBag.ResolutionID = new SelectList(db.TSTResolutions, "ResolutionID", "Problem", tSTTicket.ResolutionID);
            ViewBag.TicketClassificationID = new SelectList(db.TSTTicketClassifications.OrderBy(t => t.PriorityLevel), "TicketClassificationID", "Name", tSTTicket.TicketClassificationID);
            ViewBag.TicketStatusID = new SelectList(db.TSTTicketStatuses, "TicketStatusID", "Name", tSTTicket.TicketStatusID);
            ViewBag.SubmittedBy = new SelectList(db.TSTUsers, "TSTUserID", "FirstName", tSTTicket.SubmittedBy);
            ViewBag.TechAssigned = new SelectList(db.TSTUsers.Where(t => t.UserTitleID == 3 || t.UserTitleID == 4), "TSTUserID", "FullName");

            return View(tSTTicket);
        }

        // GET: Tickets/Edit/5
        [Authorize(Roles = "Tech, Admin")]
        public ActionResult Edit(int? id)
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
            ViewBag.ProjectID = new SelectList(db.TSTProjects, "ProjectID", "Name", tSTTicket.ProjectID);
            ViewBag.ResolutionID = new SelectList(db.TSTResolutions, "ResolutionID", "Problem", tSTTicket.ResolutionID);
            ViewBag.TicketClassificationID = new SelectList(db.TSTTicketClassifications.OrderBy(t => t.PriorityLevel), "TicketClassificationID", "Name", tSTTicket.TicketClassificationID);
            ViewBag.TicketStatusID = new SelectList(db.TSTTicketStatuses, "TicketStatusID", "Name", tSTTicket.TicketStatusID);
            ViewBag.SubmittedBy = new SelectList(db.TSTUsers, "TSTUserID", "FirstName", tSTTicket.SubmittedBy);
            ViewBag.TechAssigned = new SelectList(db.TSTUsers.Where(t => t.UserTitleID == 3 || t.UserTitleID == 4), "TSTUserID", "FullName", tSTTicket.TechAssigned);
            return View(tSTTicket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "TicketID,Subject,Description,TicketClassificationID,SubmittedBy,TechAssigned,TicketStatusID,DateSubmitted,DateResolved,ProjectID,Attachment1,Attachment2,Tags,ResolutionID")]
        TSTTicket tSTTicket)
        {
            if (ModelState.IsValid)
            {
                //get tech who edited the ticket
                string techID = User.Identity.GetUserId();
                var tech = db.TSTUsers.Where(u => u.UserID == techID).SingleOrDefault();

                var submitter = db.TSTUsers.Where(u => u.TSTUserID == tSTTicket.SubmittedBy).SingleOrDefault();

                db.Entry(tSTTicket).State = EntityState.Modified;
                db.SaveChanges();

                //if status is changed to some sort of resolved status (closed, denied, canceled), set DateResolved
                if ((tSTTicket.TicketStatusID == 3 || tSTTicket.TicketStatusID == 5 || tSTTicket.TicketStatusID == 8) && tSTTicket.DateResolved == null)
                {
                    tSTTicket.DateResolved = DateTime.Now;

                    db.Entry(tSTTicket).State = EntityState.Modified;
                    db.SaveChanges();

                    //notify user who submitted ticket of status change
                    CreateNotification(tech.TSTUserID, tSTTicket.SubmittedBy,
                        string.Format("{0} closed \"{1}\"", tech.FirstName, tSTTicket.Subject),
                        tSTTicket.TicketID);

                    if (submitter.TSTUserPreference.EmailNotifications == true)
                    {
                        MailMessage msg = new MailMessage(new MailAddress("noreply@anigrams.org", "Anigrams Support"), new MailAddress(submitter.Email));
                        msg.Subject = "Your Ticket Has Been Closed";
                        msg.Body = string.Format("<p><strong>Your ticket, \"{0}\", has been {1}.</strong></p><br/><p><a href='http://support.anigrams.org/Tickets/Details/{2}'>Click here to review the changes.</a> Thank you for using Anigrams Support!</p><br/><br/><p style='font-size:.8em'>You received this email because you opted in to receive notification emails from Anigrams Support. If you no longer wish to receive these emails, <a href='http://support.anigrams.org/UserPreferences/Edit/{3}'>please change your settings.</a></p>",
                                tSTTicket.Subject, "closed", tSTTicket.TicketID, tSTTicket.TSTUser.UserPreferenceID);
                        msg.IsBodyHtml = true;

                        SmtpClient client = new SmtpClient("mail.anigrams.org", 26)
                        {
                            Credentials = new NetworkCredential("noreply@anigrams.org", "rIc41j6@")
                        };

                        client.Send(msg);
                    }
                }
                else if (tSTTicket.TicketStatusID != 1)
                {
                    //if status has been changed from pending (but not resolved), notify user
                    CreateNotification(tech.TSTUserID, tSTTicket.SubmittedBy,
                        string.Format("{0} updated \"{1}\"", tech.FirstName, tSTTicket.Subject),
                        tSTTicket.TicketID);

                    if (submitter.TSTUserPreference.EmailNotifications == true)
                    {
                        MailMessage msg = new MailMessage(new MailAddress("noreply@anigrams.org", "Anigrams Support"), new MailAddress(submitter.Email));
                        msg.Subject = "Your Ticket Has Been Opened";
                        msg.Body = string.Format("<p><strong>Your ticket, \"{0}\", has been {1}.</strong></p><p><a href='http://support.anigrams.org/Tickets/Details/{2}'>Click here to review the changes.</a> Thank you for using Anigrams Support!</p><br/><br/><p style='font-size:.8em'>You received this email because you opted in to receive notification emails from Anigrams Support. If you no longer wish to receive these emails, <a href='http://support.anigrams.org/UserPreferences/Edit/{3}'>please change your settings.</a></p>",
                            tSTTicket.Subject, "opened", tSTTicket.TicketID, submitter.UserPreferenceID);
                        msg.IsBodyHtml = true;

                        SmtpClient client = new SmtpClient("mail.anigrams.org", 26)
                        {
                            Credentials = new NetworkCredential("noreply@anigrams.org", "rIc41j6@")
                        };

                        client.Send(msg);
                    }
                }

                //notify tech assigned if they weren't the one who issued the update
                if (tSTTicket.TechAssigned != null && tSTTicket.TechAssigned != tech.TSTUserID)
                {
                    if (tSTTicket.TicketStatusID == 2 || tSTTicket.TicketStatusID == 4)
                    {
                        //if ticket has been changed to open or approved, let tech know they've been assigned
                        CreateNotification(tech.TSTUserID, (int)tSTTicket.TechAssigned,
                        string.Format("You've been assigned to \"{1}\"", tSTTicket.Subject),
                        tSTTicket.TicketID);
                    }
                    else
                    {
                        CreateNotification(tech.TSTUserID, (int)tSTTicket.TechAssigned,
                        string.Format("{0} updated \"{1}\"", tech.FirstName, tSTTicket.Subject),
                        tSTTicket.TicketID);
                    }
                }

                
                return RedirectToAction("Index");
            }

            ViewBag.ProjectID = new SelectList(db.TSTProjects, "ProjectID", "Name", tSTTicket.ProjectID);
            ViewBag.ResolutionID = new SelectList(db.TSTResolutions, "ResolutionID", "Problem", tSTTicket.ResolutionID);
            ViewBag.TicketClassificationID = new SelectList(db.TSTTicketClassifications.OrderBy(t => t.PriorityLevel), "TicketClassificationID", "Name", tSTTicket.TicketClassificationID);
            ViewBag.TicketStatusID = new SelectList(db.TSTTicketStatuses, "TicketStatusID", "Name", tSTTicket.TicketStatusID);
            ViewBag.SubmittedBy = new SelectList(db.TSTUsers, "TSTUserID", "FirstName", tSTTicket.SubmittedBy);
            ViewBag.TechAssigned = new SelectList(db.TSTUsers.Where(t => t.UserTitleID == 3 || t.UserTitleID == 4), "TSTUserID", "FullName", tSTTicket.TechAssigned);
            return View(tSTTicket);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public JsonResult AddComment(int ticketID, string comment)
        {
            //get the ticket
            TSTTicket ticket = db.TSTTickets.Find(ticketID);
            //get the user
            string userID = User.Identity.GetUserId();
            TSTUser user = db.TSTUsers.Where(t => t.UserID == userID).FirstOrDefault();

            if (user != null)
            {
                //create the comment
                TSTComment newComment = new TSTComment()
                {
                    TSTTicket = ticket,
                    TicketID = ticket.TicketID,
                    Comment = comment,
                    PostedBy = user.TSTUserID,
                    TSTUser = user,
                    DatePosted = DateTime.Now
                };
                //add to database
                db.TSTComments.Add(newComment);
                db.SaveChanges();

                //create notification to user who submitted the ticket
                CreateNotification(newComment.PostedBy, ticket.SubmittedBy,
                    string.Format("{0} commented on \"{1}\"", user.FirstName, ticket.Subject),
                    ticket.TicketID);

                if (ticket.TechAssigned != null && ticket.TechAssigned != ticket.SubmittedBy)
                {
                    //create notification to tech assigned to the ticket
                    CreateNotification(newComment.PostedBy, (int)ticket.TechAssigned,
                        string.Format("{0} commented on \"{1}\"", user.FirstName, ticket.Subject),
                        ticket.TicketID);
                }

                //send notification email to submitter of the ticket (if they've opted in to emails)
                TSTUser submitter = db.TSTUsers.Where(t => t.TSTUserID == ticket.SubmittedBy).SingleOrDefault();
                if (submitter.TSTUserPreference.EmailNotifications == true)
                {
                    MailMessage msg = new MailMessage(new MailAddress("noreply@anigrams.org", "Anigrams Support"), new MailAddress(submitter.Email));
                    msg.Subject = "New Comment for " + ticket.Subject;
                    msg.Body = string.Format("<p><strong>{0} has posted a new comment for your ticket, \"{1}\":</strong></p><blockquote>{2}</blockquote><p>Please <a href='http://support.anigrams.org/Tickets/Details/{3}'>click here to review and respond to this comment.</a> Thank you for using Anigrams Support!</p><br/><br/><p style='font-size:.8em'>You received this email because you opted in to receive notification emails from Anigrams Support. If you no longer wish to receive these emails, <a href='http://support.anigrams.org/UserPreferences/Edit/{4}'>please change your settings.</a></p>",
                        user.TSTUserPreference.DisplayName, ticket.Subject, comment, ticket.TicketID, submitter.UserPreferenceID);
                    msg.IsBodyHtml = true;

                    SmtpClient client = new SmtpClient("mail.anigrams.org", 26)
                    {
                        Credentials = new NetworkCredential("noreply@anigrams.org", "rIc41j6@")
                    };

                    client.Send(msg);
                }

                //return the data to the view to be displayed
                var data = new
                {
                    Comment = newComment.Comment,
                    User = newComment.TSTUser.TSTUserPreference.DisplayName,
                    UserImage = newComment.TSTUser.Image,
                    Date = string.Format("{0:d}", newComment.DatePosted)
                };

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        public void CreateNotification(int senderID, int recipientID, string message, int ticketID)
        {
            TSTUserNotification newNotification = new TSTUserNotification()
            {
                PostedBy = senderID,
                TSTUserID = recipientID,
                Message = message,
                DateEntered = DateTime.Now,
                IsRead = false,
                TicketID = ticketID
            };

            db.TSTUserNotifications.Add(newNotification);
            db.SaveChanges();
        }
    }//end class
}//end namespace
