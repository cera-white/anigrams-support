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
    public class ProjectsController : Controller
    {
        private TSTEntities db = new TSTEntities();

        // GET: Projects
        public ActionResult Index()
        {
            var tSTProjects = db.TSTProjects.Include(t => t.TSTProjectCategory);
            tSTProjects = tSTProjects.OrderBy(t => t.Name);

            if (Request.QueryString["cat"] != null)
            {
                int catID = int.Parse(Request.QueryString["cat"]);
                tSTProjects = tSTProjects.Where(t => t.ProjectCategoryID == catID);
            }

            return View(tSTProjects.ToList().Where(t => t.IsActive == true));
        }

        // GET: Projects/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TSTProject tSTProject = db.TSTProjects.Find(id);
            if (tSTProject == null)
            {
                return HttpNotFound();
            }

            return View(tSTProject);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.ProjectCategoryID = new SelectList(db.TSTProjectCategories, "ProjectCategoryID", "Name");
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include = "ProjectID,Name,Description,ProjectCategoryID,Image,IsActive,Website")] TSTProject tSTProject, HttpPostedFileBase projectImage)
        {
            if (ModelState.IsValid)
            {
                #region FileUpload
                //default image
                string imageName = "noimage.jpg";

                if (projectImage != null)
                {
                    imageName = projectImage.FileName;

                    //get file extension
                    string ext = imageName.Substring(imageName.LastIndexOf('.'));

                    //only accept image files
                    if (ext == ".jpg" || ext == ".png" || ext == ".gif" || ext == ".jpeg")
                    {
                        imageName = Guid.NewGuid() + ext;

                        projectImage.SaveAs(Server.MapPath("~/images/projects/" + imageName));
                    }
                    else
                    {
                        ModelState.AddModelError("Image", "Only image files (.jpg, .png, .gif) are allowed");

                        return View(tSTProject);
                    }
                }

                //no matter what, add the image value to the project
                tSTProject.Image = imageName;
                #endregion

                tSTProject.IsActive = true;

                db.TSTProjects.Add(tSTProject);
                db.SaveChanges();
                return RedirectToAction("Details", new { @id = tSTProject.ProjectID });
            }

            ViewBag.ProjectCategoryID = new SelectList(db.TSTProjectCategories, "ProjectCategoryID", "Name", tSTProject.ProjectCategoryID);
            return View(tSTProject);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TSTProject tSTProject = db.TSTProjects.Find(id);
            if (tSTProject == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProjectCategoryID = new SelectList(db.TSTProjectCategories, "ProjectCategoryID", "Name", tSTProject.ProjectCategoryID);
            return View(tSTProject);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "ProjectID,Name,Description,ProjectCategoryID,Image,IsActive,Website")] TSTProject tSTProject, HttpPostedFileBase projectImage)
        {
            if (ModelState.IsValid)
            {
                #region FileUpload
                if (projectImage != null)
                {
                    string imageName = projectImage.FileName;

                    //get file extension
                    string ext = imageName.Substring(imageName.LastIndexOf('.'));

                    //only accept image files
                    if (ext == ".jpg" || ext == ".png" || ext == ".gif" || ext == ".jpeg")
                    {
                        imageName = Guid.NewGuid() + ext;

                        projectImage.SaveAs(Server.MapPath("~/images/projects/" + imageName));

                        tSTProject.Image = imageName;
                    }
                    else
                    {
                        ModelState.AddModelError("Image", "Only image files (.jpg, .png, .gif) are allowed");

                        return View(tSTProject);
                    }
                }
                #endregion

                db.Entry(tSTProject).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", new { @id = tSTProject.ProjectID });
            }
            ViewBag.ProjectCategoryID = new SelectList(db.TSTProjectCategories, "ProjectCategoryID", "Name", tSTProject.ProjectCategoryID);
            return View(tSTProject);
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TSTProject tSTProject = db.TSTProjects.Find(id);
            if (tSTProject == null)
            {
                return HttpNotFound();
            }
            return View(tSTProject);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            TSTProject tSTProject = db.TSTProjects.Find(id);
            //db.TSTProjects.Remove(tSTProject);
            tSTProject.IsActive = !tSTProject.IsActive; //soft delete - true becomes false, false becomes true
            db.Entry(tSTProject).State = EntityState.Modified;
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
