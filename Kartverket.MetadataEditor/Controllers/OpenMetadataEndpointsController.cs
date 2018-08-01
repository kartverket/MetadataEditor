using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Kartverket.MetadataEditor.Models;
using Kartverket.MetadataEditor.Models.OpenData;

namespace Kartverket.MetadataEditor.Controllers
{
    [Authorize]
    public class OpenMetadataEndpointsController : Controller
    {
        private readonly MetadataContext _db;

        public OpenMetadataEndpointsController(MetadataContext dbContext)
        {
            _db = dbContext;
        }

        // GET: OpenMetadataEndpoints
        public ActionResult Index()
        {
            return View(_db.OpenMetadataEndpoints.ToList());
        }

        // GET: OpenMetadataEndpoints/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OpenMetadataEndpoint openMetadataEndpoint = _db.OpenMetadataEndpoints.Find(id);
            if (openMetadataEndpoint == null)
            {
                return HttpNotFound();
            }
            return View(openMetadataEndpoint);
        }

        // GET: OpenMetadataEndpoints/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: OpenMetadataEndpoints/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Url,OrganizationName")] OpenMetadataEndpoint openMetadataEndpoint)
        {
            if (ModelState.IsValid)
            {
                _db.OpenMetadataEndpoints.Add(openMetadataEndpoint);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(openMetadataEndpoint);
        }

        // GET: OpenMetadataEndpoints/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OpenMetadataEndpoint openMetadataEndpoint = _db.OpenMetadataEndpoints.Find(id);
            if (openMetadataEndpoint == null)
            {
                return HttpNotFound();
            }
            return View(openMetadataEndpoint);
        }

        // POST: OpenMetadataEndpoints/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Url,OrganizationName")] OpenMetadataEndpoint openMetadataEndpoint)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(openMetadataEndpoint).State = EntityState.Modified;
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(openMetadataEndpoint);
        }

        // GET: OpenMetadataEndpoints/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OpenMetadataEndpoint openMetadataEndpoint = _db.OpenMetadataEndpoints.Find(id);
            if (openMetadataEndpoint == null)
            {
                return HttpNotFound();
            }
            return View(openMetadataEndpoint);
        }

        // POST: OpenMetadataEndpoints/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            OpenMetadataEndpoint openMetadataEndpoint = _db.OpenMetadataEndpoints.Find(id);
            _db.OpenMetadataEndpoints.Remove(openMetadataEndpoint);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
