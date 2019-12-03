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

        public void Video(VideoWatchViewModel videoWatch)
        {
            var storagePath = Server.MapPath(ConfigurationManager.AppSettings.Get("VideoStoragePath"));
            var filePath = Path.Combine(storagePath, videoWatch.FileName);

            // https://blogs.visigo.com/chriscoulson/easy-handling-of-http-range-requests-in-asp-net/

            long size, start, end, length, fp = 0;
            using (var reader = new StreamReader(filePath))
            {
                size = reader.BaseStream.Length;
                start = 0;
                end = size - 1;
                length = size;
                // Now that we've gotten so far without errors we send the accept range header
                /* At the moment we only support single ranges.
                 * Multiple ranges requires some more work to ensure it works correctly
                 * and comply with the spesifications: http://www.w3.org/Protocols/rfc2616/rfc2616-sec19.html#sec19.2
                 *
                 * Multirange support annouces itself with:
                 * header('Accept-Ranges: bytes');
                 *
                 * Multirange content must be sent with multipart/byteranges mediatype,
                 * (mediatype = mimetype)
                 * as well as a boundry header to indicate the various chunks of data.
                 */
                Response.AddHeader("Accept-Ranges", $"0-{size}");
                // header('Accept-Ranges: bytes');
                // multipart/byteranges
                // http://www.w3.org/Protocols/rfc2616/rfc2616-sec19.html#sec19.2

                if (!string.IsNullOrEmpty(Request.ServerVariables["HTTP_RANGE"]))
                {
                    long anotherStart = start;
                    long anotherEnd = end;
                    string[] arr_split = Request.ServerVariables["HTTP_RANGE"].Split(new char[] { Convert.ToChar("=") });
                    string range = arr_split[1];

                    // Make sure the client hasn't sent us a multibyte range
                    if (range.IndexOf(",") > -1)
                    {
                        // (?) Shoud this be issued here, or should the first
                        // range be used? Or should the header be ignored and
                        // we output the whole content?
                        Response.AddHeader("Content-Range", $"bytes {start}-{end}/{size}");
                        throw new HttpException(416, "Requested Range Not Satisfiable");
                    }

                    // If the range starts with an '-' we start from the beginning
                    // If not, we forward the file pointer
                    // And make sure to get the end byte if spesified
                    if (range.StartsWith("-"))
                    {
                        // The n-number of the last bytes is requested
                        anotherStart = size - Convert.ToInt64(range.Substring(1));
                    }
                    else
                    {
                        arr_split = range.Split(new char[] { Convert.ToChar("-") });
                        anotherStart = Convert.ToInt64(arr_split[0]);
                        long temp = 0;
                        anotherEnd = (arr_split.Length > 1 && long.TryParse(arr_split[1].ToString(), out temp)) ? Convert.ToInt64(arr_split[1]) : size;
                    }
                    /* Check the range and make sure it's treated according to the specs.
                     * http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html
                     */
                    // End bytes can not be larger than $end.
                    anotherEnd = (anotherEnd > end) ? end : anotherEnd;
                    // Validate the requested range and return an error if it's not correct.
                    if (anotherStart > anotherEnd || anotherStart > size - 1 || anotherEnd >= size)
                    {
                        Response.AddHeader("Content-Range", $"bytes {start}-{end}/{size}");
                        throw new HttpException(416, "Requested Range Not Satisfiable");
                    }
                    start = anotherStart;
                    end = anotherEnd;

                    length = end - start + 1; // Calculate new content length
                    fp = reader.BaseStream.Seek(start, SeekOrigin.Begin);
                    Response.StatusCode = 206;
                }
            }
            // Notify the client the byte range we'll be outputting
            Response.AddHeader("Content-Range", $"bytes {start}-{end}/{size}");
            Response.AddHeader("Content-Length", length.ToString());
            // Start buffered download
            Response.WriteFile(filePath, fp, length);
            Response.End();
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
                var storagePath = Server.MapPath(ConfigurationManager.AppSettings.Get("VideoStoragePath"));
                var guid = Guid.NewGuid().ToString();
                var fileName = Path.GetFileName(videoUpload.File.FileName);
                var uniqueFileName = $"{guid}-{fileName}";
                var uniqueFilePath = Path.Combine(storagePath, uniqueFileName);

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

            var storagePath = Server.MapPath(ConfigurationManager.AppSettings.Get("VideoStoragePath"));
            var filePath = Path.Combine(storagePath, video.FileName);

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

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