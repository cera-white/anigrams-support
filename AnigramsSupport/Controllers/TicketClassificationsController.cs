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
    public class TicketClassificationsController : Controller
    {
        private TSTEntities db = new TSTEntities();

        // GET: TicketClassifications
        public ActionResult Index()
        {
            return View(db.TSTTicketClassifications.ToList());
        }

        // GET: TicketClassifications/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: TicketClassifications/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include = "TicketClassificationID,Name,Description,IsActive,PriorityLevel")] TSTTicketClassification tSTTicketClassification)
        {
            if (ModelState.IsValid)
            {
                tSTTicketClassification.IsActive = true;

                db.TSTTicketClassifications.Add(tSTTicketClassification);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tSTTicketClassification);
        }

        // GET: TicketClassifications/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TSTTicketClassification tSTTicketClassification = db.TSTTicketClassifications.Find(id);
            if (tSTTicketClassification == null)
            {
                return HttpNotFound();
            }
            return View(tSTTicketClassification);
        }

        // POST: TicketClassifications/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "TicketClassificationID,Name,Description,IsActive,PriorityLevel")] TSTTicketClassification tSTTicketClassification)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tSTTicketClassification).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tSTTicketClassification);
        }

        // GET: TicketClassifications/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TSTTicketClassification tSTTicketClassification = db.TSTTicketClassifications.Find(id);
            if (tSTTicketClassification == null)
            {
                return HttpNotFound();
            }
            return View(tSTTicketClassification);
        }

        // POST: TicketClassifications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            TSTTicketClassification tSTTicketClassification = db.TSTTicketClassifications.Find(id);
            //db.TSTTicketClassifications.Remove(tSTTicketClassification);
            tSTTicketClassification.IsActive = !tSTTicketClassification.IsActive; //soft delete - true becomes false, false becomes true
            db.Entry(tSTTicketClassification).State = EntityState.Modified;
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
