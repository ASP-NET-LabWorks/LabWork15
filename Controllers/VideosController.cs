using LabWork15.Models;
using LabWork15.ViewModels;
using System;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace LabWork15.Controllers
{
    public class VideosController : Controller
    {
        private VideoDbContext db = new VideoDbContext();

        public FileStreamResult Video(VideoWatchViewModel videoWatch)
        {
            var stream = new FileStream(videoWatch.FileName, FileMode.Open, FileAccess.Read);

            return new FileStreamResult(stream, MimeMapping.GetMimeMapping(videoWatch.FileName));
        }

        // GET: Videos
        public ActionResult Index()
        {
            return View(db.Videos.ToList());
        }

        // GET: Videos/Watch/5
        public ActionResult Watch(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var video = db.Videos.Find(id);

            if (video == null)
                return HttpNotFound();

            var videoWatch = new VideoWatchViewModel
            {
                Title = video.Title,
                FileName = video.FileName
            };

            return View(videoWatch);
        }

        // GET: Videos/Upload
        public ActionResult Upload()
        {
            return View();
        }

        // POST: Videos/Upload
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upload([Bind(Include = "Title,File")] VideoUploadViewModel videoUpload)
        {
            if (ModelState.IsValid)
            {
                var videoStoragePath = Server.MapPath(ConfigurationManager.AppSettings.Get("VideoStoragePath"));
                var guid = Guid.NewGuid().ToString();
                var fileName = Path.GetFileName(videoUpload.File.FileName);
                var uniqueFileName = $"{guid}-{fileName}";
                var uniqueFilePath = $"{videoStoragePath}{uniqueFileName}";

                videoUpload.File.SaveAs(uniqueFilePath);

                var video = new Video
                {
                    Title = videoUpload.Title,
                    FileName = uniqueFileName,
                    UploadDate = DateTime.Now
                };

                db.Videos.Add(video);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(videoUpload);
        }

        // GET: Videos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Video video = db.Videos.Find(id);

            if (video == null)
                return HttpNotFound();

            var videoEdit = new VideoEditViewModel
            {
                Title = video.Title
            };

            return View(videoEdit);
        }

        // POST: Videos/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Title")] VideoEditViewModel videoEdit, int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (ModelState.IsValid)
            {
                var video = db.Videos.Find(id);
                video.Title = videoEdit.Title;

                db.Entry(video).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(videoEdit);
        }

        // GET: Videos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var video = db.Videos.Find(id);

            if (video == null)
                return HttpNotFound();

            return View(video);
        }

        // POST: Videos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var video = db.Videos.Find(id);
            db.Videos.Remove(video);

            db.SaveChanges();

            if (System.IO.File.Exists(video.FileName))
                System.IO.File.Delete(video.FileName);

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
