using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Mioto.Models;

namespace Mioto.Controllers
{
    public class XesController : Controller
    {
        private DB_MiotoEntities1 db = new DB_MiotoEntities1();

        // GET: Xes
        public ActionResult Index()
        {
            var xe = db.Xe.Include(x => x.ChuXe);
            return View(xe.ToList());
        }

        // GET: Xes/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Xe xe = db.Xe.Find(id);
            if (xe == null)
            {
                return HttpNotFound();
            }
            return View(xe);
        }

        // GET: Xes/Create
        public ActionResult Create()
        {
            ViewBag.IDCX = new SelectList(db.ChuXe, "IDCX", "Ten");
            return View();
        }

        // POST: Xes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BienSoXe,HangXe,MauXe,NamSanXuat,SoGhe,TinhNang,GiaThue,TrangThai,IDCX,HinhAnh,KhuVuc")] Xe xe, HttpPostedFileBase HinhAnh, byte[] fileName)
        {
            if (ModelState.IsValid)
            {
                if(HinhAnh != null)
                {
                    var fileName1 = Path.GetFileName(HinhAnh.FileName);

                    var path = Path.Combine(Server.MapPath("~/Images"), fileName1);

                    xe.HinhAnh = fileName1;

                    HinhAnh.SaveAs(path);
                }
                db.Xe.Add(xe);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            
            ViewBag.IDCX = new SelectList(db.ChuXe, "IDCX", "Ten", xe.IDCX);
            return View(xe);
        }

        // GET: Xes/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Xe xe = db.Xe.Find(id);
            if (xe == null)
            {
                return HttpNotFound();
            }
            ViewBag.IDCX = new SelectList(db.ChuXe, "IDCX", "Ten", xe.IDCX);
            return View(xe);
        }

        // POST: Xes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BienSoXe,HangXe,MauXe,NamSanXuat,SoGhe,TinhNang,GiaThue,TrangThai,IDCX,HinhAnh,KhuVuc")] Xe xe)
        {
            if (ModelState.IsValid)
            {
                db.Entry(xe).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IDCX = new SelectList(db.ChuXe, "IDCX", "Ten", xe.IDCX);
            return View(xe);
        }

        // GET: Xes/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Xe xe = db.Xe.Find(id);
            if (xe == null)
            {
                return HttpNotFound();
            }
            return View(xe);
        }

        // POST: Xes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Xe xe = db.Xe.Find(id);
            db.Xe.Remove(xe);
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
