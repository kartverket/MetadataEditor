using Kartverket.MetadataEditor.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http;

namespace Kartverket.MetadataEditor.Controllers
{
    
    public class ApiMetaController : ApiControllerBase
    {
        private IMetadataService _metadataService;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ApiMetaController(IMetadataService metadataService)
        {
            _metadataService = metadataService;
        }
        /// <summary>
        /// Upload thumbnail
        /// </summary>
        /// <param name="uuid">The identifier of the metadata</param>
        /// <param name="scaleImage">Scale to maxWidth=180 and maxHeight=1000 if set to true </param>
        [ApiAuthorizeAttribute]
        [Route("api/uploadthumbnail/{uuid}")]
        [HttpPost]
        public Upload UploadThumbnail(string uuid)
        {
            string url = null;
            string urlMini = null;
            string urlMedium = null;

            try
            {
                if (HttpContext.Current.Request.Files.Count > 0)
                {
                    HttpPostedFile file = HttpContext.Current.Request.Files[0];

                    if (file.ContentType == "image/jpeg" || file.ContentType == "image/gif" || file.ContentType == "image/png")
                    {

                        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                        //var filename = uuid + "_" + timestamp + "_" + file.FileName;
                        //var fullPath = HttpContext.Current.Server.MapPath("~/thumbnails/" + filename);

                        //file.SaveAs(fullPath);
                        //url = "https://" + HttpContext.Current.Request.Url.Host + (HttpContext.Current.Request.Url.IsDefaultPort ? "" : ":" + HttpContext.Current.Request.Url.Port) + "/thumbnails/" + filename;

                        //var filenameMini = uuid + "_" + timestamp + "_mini_" + file.FileName;
                        //var fullPathMini = HttpContext.Current.Server.MapPath("~/thumbnails/" + filenameMini);

                        //OptimizeImage(file, 180, 1000, fullPathMini);
                        //urlMini = "https://" + HttpContext.Current.Request.Url.Host + (HttpContext.Current.Request.Url.IsDefaultPort ? "" : ":" + HttpContext.Current.Request.Url.Port) + "/thumbnails/" + filenameMini;

                        var filenameMedium = uuid + "_" + timestamp + "_medium_" + file.FileName;
                        var fullPathMedium = HttpContext.Current.Server.MapPath("~/thumbnails/" + filenameMedium);

                        OptimizeImage(file, 300, 1000, fullPathMedium);
                        urlMedium = "https://" + HttpContext.Current.Request.Url.Host + (HttpContext.Current.Request.Url.IsDefaultPort ? "" : ":" + HttpContext.Current.Request.Url.Port) + "/thumbnails/" + filenameMedium;


                    }
                    else
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Error(ex);
            }

            return new Upload { UrlMediumImage = urlMedium };
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
                Kartverket.MetadataEditor.Models.MetadataViewModel model = _metadataService.GetMetadataModel(uuid);


                if (model != null) 
                {
                    if (UserHasMetadataAdminRole())
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

        /// <summary>
        /// Get list of places for coordinates
        /// </summary>
        /// <param name="nord">Nord verdi for boks.</param>
        /// <param name="aust">Aust verdi for omskreven boks.</param>
        [Route("api/places")]
        [HttpGet]
        public List<string> GetPlaces(string nord, string aust)
        {
            KomDataService areas = new KomDataService();
            List<string> result = areas.GetPlaces(nord, aust);

            return result;
        }


    }

    public class Upload
    {
        //public String UrlOriginalImage { get; set; }
        //public String UrlSmallImage { get; set; }
        public String UrlMediumImage { get; set; }
    }
}
