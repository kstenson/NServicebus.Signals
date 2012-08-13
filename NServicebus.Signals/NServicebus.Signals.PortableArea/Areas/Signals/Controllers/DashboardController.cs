using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NServicebus.Signals.PortableArea.Areas.Signals.Controllers
{
    public class DashboardController : Controller
    {
        //
        // GET: /Signals/Dashboard/

        public ActionResult Index()
        {
            return View();
        }

    }
}
