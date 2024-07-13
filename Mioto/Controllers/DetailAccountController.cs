using Mioto.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Mioto.Controllers
{
    public class DetailAccountController : Controller
    {
        DB_MiotoEntities db = new DB_MiotoEntities();
        public bool IsLoggedIn { get => Session["KhachHang"] != null; }
        // GET: DetailAccount
        public ActionResult InfoAccount()
        {
            return View();
        }
        public ActionResult FavoriteCar()
        {
            return View();
        }
        public ActionResult Gift()
        {
            return View();
        }

        // GET: Detailt/MyCar
        public ActionResult MyCar()
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            var guest = Session["KhachHang"] as KhachHang;
            if (guest == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Khách hàng không tồn tại");
            var cars = db.Xe.Where(x => x.IDCX == guest.IDKH).ToList();
            return View(cars);
        }
        public ActionResult MyTrip()
        {
            return View();
        }
        public ActionResult LongTermOrder()
        {
            return View();
        }
        public ActionResult RequestDeleteAccount()
        {
            return View();
        }
        public ActionResult MyAddress()
        {
            return View();
        }
        public ActionResult ChangePassword()
        {
            return View();
        }
    }
}