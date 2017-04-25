using Blog.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Blog.Controllers
{
    public class ArticleController : Controller
    {
        // GET: Article
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        // GET: Article/List
        public ActionResult List()
        {
            using (var database = new BlogDBContext())
            {
                var articles = database.Articles
                    .Include(a => a.Author)
                    .Include(a => a.Tags)
                    .ToList();

                return View(articles);
            }
        }

        // Get: Article/Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDBContext())
            {
                var article = database.Articles
                    .Where(a => a.ArticleId == id)
                    .Include(a => a.Author)
                    .Include(a => a.Tags)
                    .FirstOrDefault();

                if (article == null)
                {
                    return HttpNotFound();
                }

                List<Comment> currentArticleComments = new List<Comment>();

                currentArticleComments = database.Comments
                    .Where(c => c.ArticleId == id)
                    .ToList();

                article.Comments = new List<Comment>();

                article.Comments.AddRange(currentArticleComments);

                return View(article);
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult ListByAuthor(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDBContext())
            {
                var currentArticle = database.Articles.Where(a => a.ArticleId == id).First();

                if (currentArticle == null)
                {
                    return HttpNotFound();
                }

                string writer = currentArticle.WrittenBy;

                List<Article> articleByWriter = new List<Article>();

                var articles = database.Articles
                    .Include(a => a.Author)
                    .Include(a => a.Tags)
                    .ToList();

                foreach (var article in articles)
                {
                    if (article.WrittenBy.Equals(writer))
                    {
                        articleByWriter.Add(article);
                    }
                }
                
                return View(articleByWriter);
            }
        }

        // get
        [Authorize]
        public ActionResult AddComment(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDBContext())
            {
                // get the article from the database
                var article = database.Articles
                    .Where(a => a.ArticleId == id)
                    .First();

                // check if article exists
                if (article == null)
                {
                    return HttpNotFound();
                }

                // EXTREMELY IMPORTANT SHIT !!!
                // create the view model
                var model = new Comment();
                model.ArticleId = article.ArticleId;
                string email = User.Identity.Name;
                var author = database.Users.Where(u => u.Email.Equals(email)).First();
                model.Author = author.FullName;
                model.AuthorEmail = email;
                model.Content = "";

                // pass the view model to the view
                return View(model);
            }
        }

        // post
        [HttpPost]
        [Authorize]
        public ActionResult AddComment(Comment model)
        {
            // check if model state is valid
            if (ModelState.IsValid)
            {
                using (var database = new BlogDBContext())
                {
                    // get the article from db
                    var article = database.Articles.FirstOrDefault(a => a.ArticleId == model.Id);

                    DateTime currentDate = DateTime.Now;
                    model.PostTime = currentDate;
                    // set article properties
                    article.Comments = new List<Comment>();
                    article.Comments.Add(model);

                    // set article state
                    database.Entry(article).State = EntityState.Modified;
                    database.SaveChanges();

                    // redirect to index page
                    return RedirectToAction("Details", new { id = model.ArticleId });
                }
            }

            // if not valid return same view
            return View(model);
        }

        // get
        [Authorize]
        public ActionResult EditComment(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDBContext())
            {
                var currentComment = database.Comments.Where(c => c.Id == id).First();

                if (currentComment == null)
                {
                    return HttpNotFound();
                }

                // create the view model
                var model = new Comment();
                model.Content = currentComment.Content;
                model.Author = currentComment.Author;
                model.AuthorEmail = currentComment.AuthorEmail;
                model.ArticleId = currentComment.ArticleId;

                if (this.User.Identity.Name != model.AuthorEmail && !this.User.IsInRole("Admin"))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                // pass the view model to the view
                return View(model);
            }
        }

        // post
        [HttpPost]
        [Authorize]
        public ActionResult EditComment(Comment model)
        {
            // check if model state is valid
            if (ModelState.IsValid)
            {
                using (var database = new BlogDBContext())
                {
                    // get the article from db
                    var currentComment = database.Comments.FirstOrDefault(c => c.Id == model.Id);

                    var articleWithComment = database.Articles.Where(a => a.ArticleId == currentComment.ArticleId).First();

                    int position = -1;

                    for (int i = 0; i < articleWithComment.Comments.Count; i++)
                    {
                        if (articleWithComment.Comments[i].Id == model.Id)
                        {
                            position = i;
                        }
                    }

                    // set article properties
                    currentComment.Content = model.Content;
                    currentComment.AuthorEmail = model.AuthorEmail;
                    // add the date it was edite and by who
                    string editorEmail = this.User.Identity.Name;
                    ApplicationUser editor = database.Users.Where(u => u.Email.Equals(editorEmail)).First();
                    string editTime = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                    string editMessage = string.Format("Last edit on {0} by {1}", editTime, editor.FullName);
                    currentComment.EditMessage = editMessage;

                    articleWithComment.Comments.RemoveAt(position);
                    articleWithComment.Comments.Insert(position, currentComment);

                    // set article state
                    database.Entry(articleWithComment).State = EntityState.Modified;
                    database.SaveChanges();

                    // redirect to index page
                    return RedirectToAction("Details", new { id = model.ArticleId });
                }
            }

            return View(model);
        }

        // get
        [Authorize]
        public ActionResult DeleteComment(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (!this.User.IsInRole("Admin"))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            using (var database = new BlogDBContext())
            {
                // get article fron the database
                var currentComment = database.Comments.Where(c => c.Id == id).First();

                // check if article exists
                if (currentComment == null)
                {
                    return HttpNotFound();
                }

                // pass article to view
                return View(currentComment);
            }
        }

        // post
        [HttpPost]
        [Authorize]
        [ActionName("DeleteComment")]
        public ActionResult DeleteCommentConfirmed(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDBContext())
            {
                // get article from the db
                var currentComment = database.Comments.Where(c => c.Id == id).First();
                int articleId = currentComment.ArticleId;

                // check if article exists
                if (currentComment == null)
                {
                    return HttpNotFound();
                }

                database.Comments.Remove(currentComment);
                database.SaveChanges();

                // redirect to Index page
                return RedirectToAction("Details", new { id = articleId });
            }
        }

        // Get
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            using (var db = new BlogDBContext())
            {
                var model = new ArticleViewModel();
                model.Categories = db.Categories
                    .OrderBy(c => c.Name)
                    .ToList();

                return View(model);
            }
        }

        // GET
        [Authorize]
        public ActionResult Error()
        {
            return View();
        }

        // Post
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(ArticleViewModel model)
        {
            bool modelNotValid = string.IsNullOrEmpty(model.Title) || string.IsNullOrEmpty(model.Content) || string.IsNullOrEmpty(model.WrittenBy) || string.IsNullOrEmpty(model.Tags) || model.CategoryId == null;

            if (modelNotValid)
            {
                return RedirectToAction("Error");
            }

            if (ModelState.IsValid)
            {
                // Insert article in DB
                using (var database = new BlogDBContext())
                {
                    var authorId = database.Users
                        .Where(u => u.UserName == this.User.Identity.Name)
                        .First()
                        .Id;

                    var article = new Article(authorId, model.Title, model.Content, model.CategoryId);

                    article.WrittenBy = model.WrittenBy;

                    if (!string.IsNullOrEmpty(model.Tags))
                    {
                        this.SetArticleTags(article, model, database);
                    }

                    database.Articles.Add(article);
                    database.SaveChanges();

                    return RedirectToAction("ListCategories", "Home");
                }
            }

            return View(model);
        }

        private void SetArticleTags(Article article, ArticleViewModel model, BlogDBContext database)
        {
            // split tags
            var tagsStrings = model.Tags
                .Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.ToLower())
                .Distinct();

            // clear current article tags
            article.Tags.Clear();

            // set new article tags
            foreach (var tagString in tagsStrings)
            {
                // get tag from db by name
                Tag tag = database.Tags.FirstOrDefault(t => t.Name.Equals(tagString));

                // if the tag is null create new tag
                if (tag == null)
                {
                    tag = new Tag() { Name = tagString };
                    database.Tags.Add(tag);
                }

                // add tag to article tags
                article.Tags.Add(tag);
            }
        }

        // Get
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDBContext())
            {
                // get article fron the database
                var article = database.Articles
                    .Where(a => a.ArticleId == id)
                    .Include(a => a.Author)
                    .Include(a => a.Category)
                    .First();

                ViewBag.TagsString = string.Join(", ", article.Tags.Select(t => t.Name));

                // check if article exists
                if (article == null)
                {
                    return HttpNotFound();
                }

                // pass article to view
                return View(article);
            }
        }

        // POST/Delete
        [HttpPost]
        [ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDBContext())
            {
                // get article from the db
                var article = database.Articles
                    .Where(a => a.ArticleId == id)
                    .Include(a => a.Author)
                    .First();

                // check if article exists
                if (article == null)
                {
                    return HttpNotFound();
                }

                // remove article form DB
                database.Articles.Remove(article);
                database.SaveChanges();

                // redirect to Index page
                return RedirectToAction("ListCategories", "Home");
            }
        }

        // get
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDBContext())
            {
                // get the article from the database
                var article = database.Articles
                    .Where(a => a.ArticleId == id)
                    .First();

                // check if article exists
                if (article == null)
                {
                    return HttpNotFound();
                }

                // create the view model
                var model = new ArticleViewModel();
                model.Id = article.ArticleId;
                model.Title = article.Title;
                model.Content = article.Content;
                model.WrittenBy = article.WrittenBy;
                model.CategoryId = article.CategoryId;
                model.Categories = database.Categories
                    .OrderBy(c => c.Name)
                    .ToList();

                model.Tags = string.Join(", ", article.Tags.Select(t => t.Name));

                // pass the view model to the view
                return View(model);
            }
        }

        [Authorize]
        public ActionResult ErrorEdit(ArticleViewModel model)
        {
            return View(model);
        }

        // post
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(ArticleViewModel model)
        {
            bool modelNotValid = string.IsNullOrEmpty(model.Title) || string.IsNullOrEmpty(model.Content) || string.IsNullOrEmpty(model.WrittenBy) || string.IsNullOrEmpty(model.Tags) || model.CategoryId == null;

            if (modelNotValid)
            {
                return RedirectToAction("ErrorEdit", model);
            }
            // check if model state is valid
            if (ModelState.IsValid)
            {
                using (var database = new BlogDBContext())
                {
                    // get the article from db
                    var article = database.Articles.FirstOrDefault(a => a.ArticleId == model.Id);

                    // set article properties
                    article.Title = model.Title;
                    article.Content = model.Content;
                    article.CategoryId = model.CategoryId;
                    article.WrittenBy = model.WrittenBy;
                    this.SetArticleTags(article, model, database);

                    // set article state
                    database.Entry(article).State = EntityState.Modified;
                    database.SaveChanges();

                    // redirect to index page
                    return RedirectToAction("Details", new { id = model.Id });
                }
            }

            // if not valid return same view
            return View(model);
        }
    }
}