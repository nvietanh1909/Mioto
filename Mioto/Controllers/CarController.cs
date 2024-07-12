using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mioto.Models;
namespace Mioto.Controllers
{
    public class CarController : Controller
    {

        // GET: Car
        public ActionResult InfoCar()
        {
            return View();
        }
    }
}