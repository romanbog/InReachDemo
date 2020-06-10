using InReachDemo.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace InReachDemo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public string FileName;
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(FileModel UserInput, HttpPostedFileBase uploadFile)
        {
            if(uploadFile != null)
            {
                //janky, but works
                UserInput.UserFile = uploadFile;

                ViewBag.Message = UserInput.SendOut();
                return View();
            }
            else
            {
                ViewBag.Message = "No File found";
                return View();
            }

        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

    }
}