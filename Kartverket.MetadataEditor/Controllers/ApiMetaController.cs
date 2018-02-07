using Kartverket.MetadataEditor.Models;
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
    
    public class ApiMetaController : ApiController
    {
        /// <summary>
        /// Upload thumbnail
        /// </summary>
        /// <param name="uuid">The identifier of the metadata</param>
        /// <param name="scaleImage">Scale to maxWidth=180 and maxHeight=1000 if set to true </param>
        [Authorize]
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
                            OptimizeImage(file, 180, 1000, fullPath);
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

        public static void OptimizeImage(HttpPostedFile file, int maxWidth, int maxHeight, string outputPath, int quality = 70)
        {
            ImageResizer.ImageJob newImage =
                new ImageResizer.ImageJob(file, outputPath,
                new ImageResizer.Instructions("maxwidth=" + maxWidth + ";maxheight=" + maxHeight + ";quality=" + quality));

            newImage.Build();
        }


        /// <summary>
        /// Validate metadata
        /// </summary>
        /// <param name="uuid">The identifier of the metadata</param>
        [Route("api/validatemetadata/{uuid}")]
        [HttpGet]
        public MetaDataEntry ValidateMetadata(string uuid)
        {
            MetaDataEntry metadata = new MetaDataEntry();
            metadata.Errors = new List<Error>();

            try
            {
                Kartverket.MetadataEditor.Models.MetadataService _metadataService = new Kartverket.MetadataEditor.Models.MetadataService();
                Kartverket.MetadataEditor.Models.MetadataViewModel model = _metadataService.GetMetadataModel(uuid);


                if (model != null) 
                {
                    string role = GetSecurityClaim("role");
                    if (!string.IsNullOrWhiteSpace(role) && role.Equals("nd.metadata_admin"))
                        model.ValidateAllRequirements = true;

                    metadata.Uuid = model.Uuid;
                    metadata.Title = model.Title;
                    metadata.OrganizationName = model.ContactMetadata.Organization != null ? model.ContactMetadata.Organization : "";
                    metadata.ContactEmail = model.ContactMetadata.Email != null ? model.ContactMetadata.Email : "";

                    if (model.MetadataStandard == "ISO19115:Norsk versjon")
                    {
                        Kartverket.MetadataEditor.Models.SimpleMetadataService _simpleMetadataService = new Kartverket.MetadataEditor.Models.SimpleMetadataService();
                        Kartverket.MetadataEditor.Models.SimpleMetadataViewModel modelSimple = _simpleMetadataService.GetMetadataModel(uuid);

                        Validate(modelSimple);

                        var errors = ModelState.Where(n => n.Value.Errors.Count > 0).ToList();

                        foreach (var error in errors)
                        {
                            metadata.Errors.Add(new Error { Key = error.Key.ToString(), Message = error.Value.Errors[0].ErrorMessage });
                        }

                    }

                    else
                    {                

                    var thumb = model.Thumbnails.Where(t => t.Type == "thumbnail" || t.Type == "miniatyrbilde");
                    if (thumb.Count() == 0)
                        ModelState.AddModelError("ThumbnailMissing", "Det er påkrevd å fylle ut illustrasjonsbilde");
                    else if (thumb.Count() > 0)
                    {
                        try
                        {
                            //Disable SSL sertificate errors
                            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                            delegate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                                                    System.Security.Cryptography.X509Certificates.X509Chain chain,
                                                    System.Net.Security.SslPolicyErrors sslPolicyErrors)
                            {
                                return true; // **** Always accept
                            };
                            using (var client = new HttpClient())
                            {
                                client.DefaultRequestHeaders.Accept.Clear();
                                string Url = thumb.Select(t => t.URL).FirstOrDefault().ToString();
                                HttpResponseMessage response = client.GetAsync(new Uri(Url)).Result;
                                if (response.StatusCode != HttpStatusCode.OK)
                                {
                                    metadata.Errors.Add(new Error { Key = "ThumbnailNotFound", Message = "Feil ressurslenke til illustrasjonsbilde" });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            metadata.Errors.Add(new Error { Key = "Error", Message = ex.Message });
                        }
                    }

                    Validate(model);

                    var errors = ModelState.Where(n => n.Value.Errors.Count > 0).ToList();

                    foreach (var error in errors) {
                        metadata.Errors.Add(new Error{ Key = error.Key.ToString(), Message = error.Value.Errors[0].ErrorMessage } );
                    }

                    }
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

        private string GetSecurityClaim(string type)
        {
            string result = null;
            foreach (var claim in System.Security.Claims.ClaimsPrincipal.Current.Claims)
            {
                if (claim.Type == type && !string.IsNullOrWhiteSpace(claim.Value))
                {
                    result = claim.Value;
                    break;
                }
            }

            // bad hack, must fix BAAT
            if (!string.IsNullOrWhiteSpace(result) && type.Equals("organization") && result.Equals("Statens kartverk"))
            {
                result = "Kartverket";
            }

            return result;
        }

        /// <summary>
        /// Get list of places for coordinates
        /// </summary>
        /// <param name="nordmin">Minimum nord verdi for omskreven boks.</param>
        /// <param name="austmin"> Minimum aust verdi for omskreven boks.</param>
        ///  <param name="nordmax">Maximum nord verdi for omskreven boks.</param>
        ///  <param name="austmax">Maximum nord verdi for omskreven boks.</param>
        ///  <param name="koordsysut">SOSI koordinat system for returnerte data. Må alltid være ulik 0.</param>
        ///  <param name="koordsysinn">SOSI koordinat system for søke data. Må være ulik 0 hvis austMin, austMax, nordMin eller nordMax er ulik 0.</param>
        [Route("api/places/{nordmin}/{austmin}/{nordmax}/{austmax}/{koordsysut}/{koordsysinn}")]
        [HttpGet]
        public List<string> GetPlaces(double nordmin, double austmin, double nordmax, double austmax, int koordsysut, int koordsysinn)
        {
            KomDataService test = new KomDataService();
            List<string> result = test.GetPlaces(nordmin, austmin, nordmax, austmax, koordsysut, koordsysinn);

            return result;
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
