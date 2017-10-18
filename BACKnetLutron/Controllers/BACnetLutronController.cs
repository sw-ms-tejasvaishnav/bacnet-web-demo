using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BACKnetLutron.Controllers
{
    public class BACnetLutronController : Controller
    {
        // GET: BACnetLutron
        public ActionResult LutronLightFloor()
        {
            ViewBag.Title = "Home Page";
            return View();
        }


        public ActionResult JACEBacnet()
        {
            ViewBag.Title = "Home Page";
            return View();
        }
    }
}