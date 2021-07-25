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
using TST.UI.MVC.Models;

namespace IdentitySample.Controllers
{
    public class HomeController : Controller
    {
        private TSTEntities db = new TSTEntities();

        public ActionResult Index()
        {
            var tSTTickets = db.TSTTickets.Include(t => t.TSTProject).Include(t => t.TSTResolution).Include(t => t.TSTTicketClassification).Include(t => t.TSTTicketStatus).Include(t => t.TSTUser).Include(t => t.TSTUser1);

            if (Request.IsAuthenticated)
            {
                string userID = User.Identity.GetUserId();
                var user = db.TSTUsers.Where(u => u.UserID == userID).SingleOrDefault();

                if (!User.IsInRole("User"))
                {
                    bool contains = db.TSTProjects_Users.Where(p => p.TSTUserID == user.TSTUserID).Count() > 0;

                    if (contains)
                    {
                        var projects = from proj in db.TSTProjects
                                       join userProj in db.TSTProjects_Users on proj.ProjectID equals userProj.ProjectID
                                       where userProj.TSTUserID == user.TSTUserID
                                       select proj;

                        ViewBag.Projects = projects.ToList();
                    }
                    else
                    {
                        ViewBag.Projects = null;
                    }
                }
                else
                {
                    ViewBag.Projects = null;
                }

                ViewBag.User = user.TSTUserID;
            }
            else
            {
                ViewBag.Projects = db.TSTProjects.ToList();
            }
            
            return View(tSTTickets.ToList());
        }

        public ActionResult Help()
        {
            return View();
        }

        //GET: Contact()
        public ActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Contact(string name, string email, string subject, string message)
        {
            if (ModelState.IsValid)
            {
                //Instantiate the ViewModel and populate it with data
                ContactViewModel contact = new ContactViewModel()
                {
                    Name = name,
                    Email = email,
                    Subject = subject,
                    Message = message,
                    DateSent = DateTime.Now
                };

                //Add a using statement for System.Net.Mail

                //Create the body for the confirmation email
                string body = string.Format("{0} ({1}) sent you a message from Anigrams Support. This is their message: <br /><br />{2}",
                    contact.Name,
                    contact.Email,
                    contact.Message);

                //Create the MailMessage object
                //MailMessage msg = new MailMessage(
                //    "noreply@anigrams.org", //from website email (set up at centriqhosting.com)
                //    "white.ca12@gmail.com", //to personal email
                //    contact.Subject, //subject
                //    body);

                //msg.IsBodyHtml = true;
                //msg.ReplyToList.Add(contact.Email);

                //SmtpClient client = new SmtpClient("mail.anigrams.org", 26)
                //{
                //    Credentials = new NetworkCredential("noreply@anigrams.org", "rIc41j6@")
                //};

                //client.Send(msg);

                return RedirectToAction("Confirmation", contact);
            }
            else
            {
                return View();
            }
        }

        public ActionResult Confirmation(ContactViewModel contact)
        {
            return View(contact);
        }

    }//end class
}//end namespace
