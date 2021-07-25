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
    public class ProjectCategoriesController : Controller
    {
        private TSTEntities db = new TSTEntities();

        // GET: ProjectCategories
        public ActionResult Index()
        {
            return View(db.TSTProjectCategories.ToList());
        }

        // GET: ProjectCategories/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: ProjectCategories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include = "ProjectCategoryID,Name,Description,IsActive")] TSTProjectCategory tSTProjectCategory)
        {
            if (ModelState.IsValid)
            {
                tSTProjectCategory.IsActive = true;

                db.TSTProjectCategories.Add(tSTProjectCategory);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tSTProjectCategory);
        }

        // GET: ProjectCategories/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TSTProjectCategory tSTProjectCategory = db.TSTProjectCategories.Find(id);
            if (tSTProjectCategory == null)
            {
                return HttpNotFound();
            }
            return View(tSTProjectCategory);
        }

        // POST: ProjectCategories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "ProjectCategoryID,Name,Description,IsActive")] TSTProjectCategory tSTProjectCategory)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tSTProjectCategory).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tSTProjectCategory);
        }

        // GET: ProjectCategories/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TSTProjectCategory tSTProjectCategory = db.TSTProjectCategories.Find(id);
            if (tSTProjectCategory == null)
            {
                return HttpNotFound();
            }
            return View(tSTProjectCategory);
        }

        // POST: ProjectCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            TSTProjectCategory tSTProjectCategory = db.TSTProjectCategories.Find(id);
            //db.TSTProjectCategories.Remove(tSTProjectCategory);
            tSTProjectCategory.IsActive = !tSTProjectCategory.IsActive; //soft delete - true becomes false, false becomes true
            db.Entry(tSTProjectCategory).State = EntityState.Modified;
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
