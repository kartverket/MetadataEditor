using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Kartverket.MetadataEditor.Controllers
{
    [Authorize]
    public class ApiMetaController : ApiController
    {
        [Route("api/uploadthumbnail/{uuid}")]
        [HttpPost]
        public HttpResponseMessage UploadThumbnail(string uuid, bool scaleImage = false)
        {
            string filename = null;
            string fullPath = null;
            string url = null;

            try
            {

                if (HttpContext.Current.Request.Files.Count > 0)
                {
                    HttpPostedFile file = HttpContext.Current.Request.Files[0];

                    if (file.ContentType == "image/jpeg" || file.ContentType == "image/gif" || file.ContentType == "image/png")
                    {

                        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                        filename = uuid + "_" + timestamp + "_" + file.FileName;
                        fullPath = HttpContext.Current.Server.MapPath("~/thumbnails/" + filename);

                        url = "https://" + HttpContext.Current.Request.Url.Host + (HttpContext.Current.Request.Url.IsDefaultPort ? "" : ":" + HttpContext.Current.Request.Url.Port) + "/thumbnails/" + filename;

                        if (scaleImage)
                        {
                            var image = Image.FromStream(file.InputStream);
                            var newImage = ScaleImage(image, 180, 1000);
                            newImage.Save(fullPath);
                        }
                        else
                        {
                            file.SaveAs(fullPath);
                        }
                    }
                    else
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                }
            }
            catch (HttpResponseException e)
            {
                var error = new { errorMessage = "Bare filtype jpeg, gif eller png er tillatt" };
                return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, error);
            }
            catch (System.Exception e) 
            {
                var error = new { errorMessage = e.Message };
                return Request.CreateResponse(HttpStatusCode.InternalServerError, error); 
            }
            var uploaded = new { uploadedToURL = url };
            return Request.CreateResponse(HttpStatusCode.OK, uploaded);
        }

        public static Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);
            Graphics.FromImage(newImage).DrawImage(image, 0, 0, newWidth, newHeight);
            return newImage;
        }
    }
}
