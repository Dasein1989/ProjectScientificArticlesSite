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
    public class EventController : Controller
    {
        // GET: Event
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            using (var database = new BlogDBContext())
            {
                var events = database.Events
                    .ToList();

                return View(events);
            }
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDBContext())
            {
                var currEvent = database.Events
                    .Where(e => e.Id == id)
                    .First();

                if (currEvent == null)
                {
                    return HttpNotFound();
                }

                return View(currEvent);
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
        public ActionResult Create(Event conference)
        {
            if (ModelState.IsValid)
            {
                using (var database = new BlogDBContext())
                {
                    database.Events.Add(conference);
                    database.SaveChanges();
                }

                return RedirectToAction("List");
            }

            return View(conference);
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
                var conference = database.Events
                    .Where(n => n.Id == id)
                    .First();

                if (conference == null)
                {
                    return HttpNotFound();
                }

                return View(conference);
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
                var conference = database.Events
                    .Where(n => n.Id == id)
                    .First();

                if (conference == null)
                {
                    return HttpNotFound();
                }

                database.Events.Remove(conference);
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
                var conference = database.Events
                    .Where(n => n.Id == id)
                    .First();

                if (conference == null)
                {
                    return HttpNotFound();
                }

                var model = new EventViewModel();
                model.Id = conference.Id;
                model.Title = conference.Title;
                model.Content = conference.Content;
                model.Date = conference.Date;

                return View(model);
            }
        }

        // POST: NewsArticle/Edit
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(Event model)
        {
            if (ModelState.IsValid)
            {
                using (var database = new BlogDBContext())
                {
                    var conference = database.Events
                        .FirstOrDefault(n => n.Id == model.Id);

                    conference.Title = model.Title;
                    conference.Content = model.Content;
                    conference.Date = model.Date;

                    database.Entry(conference).State = EntityState.Modified;
                    database.SaveChanges();

                    return RedirectToAction("List");
                }
            }

            return View(model);
        }
    }
}