using Blog.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Blog.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult ListCategories()
        {
            using (var db = new BlogDBContext())
            {
                var categories = db.Categories
                    .Include(c => c.Articles)
                    .OrderBy(c => c.Name)
                    .ToList();

                return View(categories);
            }
        }

        public ActionResult ListArticles(int? categoryId)
        {
            if (categoryId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new BlogDBContext())
            {
                var articles = db.Articles
                    .Where(a => a.CategoryId == categoryId)
                    .Include(a => a.Author)
                    .Include(a => a.Tags)
                    .ToList();

                return View(articles);
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public ActionResult ContentAnalysis()
        {
            ContentAnalysisViewModel data = new ContentAnalysisViewModel();

            return View(data);
        }

        [Authorize]
        [HttpPost]
        public ActionResult ContentAnalysis(ContentAnalysisViewModel user)
        {
            string emailOfUser = this.User.Identity.Name;

            using (var database = new BlogDBContext())
            {
                var currentUser = database.Users
                    .Where(u => u.Email.Equals(emailOfUser))
                    .First();

                currentUser.TextForAnalysis = user.Text;
                database.SaveChanges();

                return RedirectToAction("Analyse", currentUser);
            }
        }

        [Authorize]
        public ActionResult Analyse(ApplicationUser user)
        {
            return View(user);
        }
    }
}