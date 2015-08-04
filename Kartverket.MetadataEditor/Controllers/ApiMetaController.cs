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
        /// <summary>
        /// Upload thumbnail
        /// </summary>
        /// <param name="uuid">The identifier of the metadata</param>
        /// <param name="scaleImage">Scale to maxWidth=180 and maxHeight=1000 if set to true </param>
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


        /// <summary>
        /// Validate metadata
        /// </summary>
        /// <param name="uuid">The identifier of the metadata</param>
        [Route("api/validatemetadata/{uuid}")]
        [HttpGet]
        public MetaDataEntry Validate(string uuid)
        {
            MetaDataEntry metadata = new MetaDataEntry();
            metadata.Errors = new List<Error>();

            try
            {
                Kartverket.MetadataEditor.Models.MetadataService _metadataService = new Kartverket.MetadataEditor.Models.MetadataService();
                Kartverket.MetadataEditor.Models.MetadataViewModel model = _metadataService.GetMetadataModel(uuid);


                if (model != null) 
                {
                    metadata.Uuid = model.Uuid;
                    metadata.Title = model.Title;
                    metadata.OrganizationName = model.ContactMetadata.Organization != null ? model.ContactMetadata.Organization : "";
                    metadata.ContactEmail = model.ContactMetadata.Email != null ? model.ContactMetadata.Email : "";
                }

                var thumb = model.Thumbnails.Where(t => t.Type == "thumbnail" || t.Type == "miniatyrbilde");
                if (thumb.Count() == 0)
                    ModelState.AddModelError("ThumbnailMissing", "Det er påkrevd å fylle ut miniatyrbilde under grafisk bilde");

                
                Validate<Kartverket.MetadataEditor.Models.MetadataViewModel>(model);
                var errors = ModelState.Where(n => n.Value.Errors.Count > 0).ToList();

                foreach (var error in errors) {
                    metadata.Errors.Add(new Error{ Key = error.Key.ToString(), Message = error.Value.Errors[0].ErrorMessage } );
                }                

            }
            catch (Exception ex) {
                metadata.Errors.Add(new Error { Key = "Error", Message = ex.Message });
            }

            if (metadata.Errors.Count > 0)
                metadata.Status = "ERRORS";
            else
                metadata.Status = "OK";

            return metadata;
        
        }

    }

    public class MetaDataEntry 
    {
        public string Uuid { get; set; }
        public string Title { get; set; }
        public string OrganizationName { get; set; }
        public string ContactEmail { get; set; }
        public string Status { get; set; }
        public List<Error> Errors { get; set; }
    }

    public class Error
    {
        public string Key { get; set; }
        public string Message { get; set; }
    }

}
