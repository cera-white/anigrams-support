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
    public class TicketStatusesController : Controller
    {
        private TSTEntities db = new TSTEntities();

        // GET: TicketStatus
        public ActionResult Index()
        {
            return View(db.TSTTicketStatuses.ToList());
        }

        // GET: TicketStatus/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: TicketStatus/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include = "TicketStatusID,Name,Description,IsActive")] TSTTicketStatus tSTTicketStatus)
        {
            if (ModelState.IsValid)
            {
                tSTTicketStatus.IsActive = true;

                db.TSTTicketStatuses.Add(tSTTicketStatus);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tSTTicketStatus);
        }

        // GET: TicketStatus/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TSTTicketStatus tSTTicketStatus = db.TSTTicketStatuses.Find(id);
            if (tSTTicketStatus == null)
            {
                return HttpNotFound();
            }
            return View(tSTTicketStatus);
        }

        // POST: TicketStatus/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "TicketStatusID,Name,Description,IsActive")] TSTTicketStatus tSTTicketStatus)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tSTTicketStatus).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tSTTicketStatus);
        }

        // GET: TicketStatus/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TSTTicketStatus tSTTicketStatus = db.TSTTicketStatuses.Find(id);
            if (tSTTicketStatus == null)
            {
                return HttpNotFound();
            }
            return View(tSTTicketStatus);
        }

        // POST: TicketStatus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            TSTTicketStatus tSTTicketStatus = db.TSTTicketStatuses.Find(id);
            //db.TSTTicketStatuses.Remove(tSTTicketStatus);
            tSTTicketStatus.IsActive = !tSTTicketStatus.IsActive; //soft delete - true becomes false, false becomes true
            db.Entry(tSTTicketStatus).State = EntityState.Modified;
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
    }
}
