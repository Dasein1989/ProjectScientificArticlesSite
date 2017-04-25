using Blog.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Blog.Controllers
{
    public class NewsArticleController : Controller
    {
        // GET: NewsArticle
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        // GET: NewsArticle/List
        public ActionResult List()
        {
            using (var database = new BlogDBContext())
            {
                // get news articles from the database
                if (database.NewsArticles == null)
                {
                    return View();
                }

                var newsArticles = database.NewsArticles
                    .ToList();

                return View(newsArticles);
            }
        }

        // GET: NewsArticle/Details
        public ActionResult NewsDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDBContext())
            {
                // get the article form the database
                var newsArticle = database.NewsArticles
                    .Where(n => n.Id == id)
                    .First();

                if (newsArticle == null)
                {
                    return HttpNotFound();
                }

                return View(newsArticle);
            }
        }

        // GET: NewsArticle/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: NewsArticle/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(NewsArticle newsArticle)
        {
            if (ModelState.IsValid)
            {
                // insert newsArticle in the db
                if (newsArticle.Author == null)
                {
                    newsArticle.Author = "not available";
                }

                using (var database = new BlogDBContext())
                {
                    database.NewsArticles.Add(newsArticle);
                    database.SaveChanges();
                }

                return RedirectToAction("List");
            }

            return View(newsArticle);
        }

        // GET: NewsArticle/Delete
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDBContext())
            {
                var newsArticle = database.NewsArticles
                    .Where(n => n.Id == id)
                    .First();

                if (newsArticle == null)
                {
                    return HttpNotFound();
                }

                return View(newsArticle);
            }
        }

        // POST: NewsArticle/Delete
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDBContext())
            {
                var newsArticle = database.NewsArticles
                    .Where(n => n.Id == id)
                    .First();

                if (newsArticle == null)
                {
                    return HttpNotFound();
                }

                database.NewsArticles.Remove(newsArticle);
                database.SaveChanges();

                return RedirectToAction("List");
            }
        }

        // GET: NewsArticle/Edit
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDBContext())
            {
                var newsArticle = database.NewsArticles
                    .Where(n => n.Id == id)
                    .First();

                if (newsArticle == null)
                {
                    return HttpNotFound();
                }

                var model = new NewsArticleViewModel();
                model.Id = newsArticle.Id;
                model.Title = newsArticle.Title;
                model.Content = newsArticle.Content;
                model.Author = newsArticle.Author;
                model.OriginalSource = newsArticle.OriginalSource;

                return View(model);
            }
        }

        // POST: NewsArticle/Edit
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(NewsArticleViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var database = new BlogDBContext())
                {
                    var newsArticle = database.NewsArticles
                        .FirstOrDefault(n => n.Id == model.Id);

                    newsArticle.Title = model.Title;
                    newsArticle.Content = model.Content;
                    newsArticle.Author = model.Author;
                    newsArticle.OriginalSource = model.OriginalSource;

                    database.Entry(newsArticle).State = EntityState.Modified;
                    database.SaveChanges();

                    return RedirectToAction("List");
                }
            }

            return View(model);
        }
    }
}