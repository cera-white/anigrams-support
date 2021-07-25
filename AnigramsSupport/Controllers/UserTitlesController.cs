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
    public class UserTitlesController : Controller
    {
        private TSTEntities db = new TSTEntities();

        // GET: UserTitles
        public ActionResult Index()
        {
            return View(db.TSTUserTitles.ToList());
        }

        // GET: UserTitles/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: UserTitles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include = "UserTitleID,Name,Description,IsActive")] TSTUserTitle tSTUserTitle)
        {
            if (ModelState.IsValid)
            {
                tSTUserTitle.IsActive = true;

                db.TSTUserTitles.Add(tSTUserTitle);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tSTUserTitle);
        }

        // GET: UserTitles/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TSTUserTitle tSTUserTitle = db.TSTUserTitles.Find(id);
            if (tSTUserTitle == null)
            {
                return HttpNotFound();
            }
            return View(tSTUserTitle);
        }

        // POST: UserTitles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "UserTitleID,Name,Description,IsActive")] TSTUserTitle tSTUserTitle)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tSTUserTitle).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tSTUserTitle);
        }

        // GET: UserTitles/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TSTUserTitle tSTUserTitle = db.TSTUserTitles.Find(id);
            if (tSTUserTitle == null)
            {
                return HttpNotFound();
            }
            return View(tSTUserTitle);
        }

        // POST: UserTitles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            TSTUserTitle tSTUserTitle = db.TSTUserTitles.Find(id);
            //db.TSTUserTitles.Remove(tSTUserTitle);
            tSTUserTitle.IsActive = !tSTUserTitle.IsActive; //soft delete - true becomes false, false becomes true
            db.Entry(tSTUserTitle).State = EntityState.Modified;
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
