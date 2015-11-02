using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kartverket.MetadataEditor.Controllers;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using System.Threading;
using Raven.Abstractions.Data;
using Kartverket.MetadataEditor;
using log4net;
using System.Net.Mail;
using System.Text;

namespace Kartverket.MetadataEditor.Models
{
    public class ValidatorService
    {
        private MetadataService _metadataService;
        private static readonly ILog Log = LogManager.GetLogger(typeof(MvcApplication));

        public ValidatorService() 
        {
            _metadataService = new MetadataService();
        }

        public List<string> ValidateAllMetadata() 
        {
            List<string> emails = new List<string>();

            try 
            { 
                DeleteFiles(MvcApplication.Store);
                GetAllMetadata();
                //SendEmail();
                emails = GetEmails();

            }
            catch(Exception ex)
            {
                Log.Error(ex.Message);
            }

            return emails;

        }

        private List<string> GetEmails()
        {
            var session = MvcApplication.Store.OpenSession();
            var results = session.Query<MetaDataEntry>().Take(1024).OrderBy(o => o.ContactEmail);

            var resultsList = results.ToList();

            var emailsTo = resultsList.Select(o => o.ContactEmail).Distinct().ToList();

            return emailsTo;
        }

        public void GetAllMetadata() 
        {
            MetadataIndexViewModel model = new MetadataIndexViewModel();
            int offset = 1;
            int limit = 50;
            model = _metadataService.SearchMetadata("", "", offset, limit);

            foreach(var item in model.MetadataItems)
            {
                MetaDataEntry md = ValidateMetadata(item.Uuid);
                SaveValidationResult(md);
            }

            int numberOfRecordsMatched = model.TotalNumberOfRecords;
            int next = model.OffsetNext();

            try
            {
            //while (next < numberOfRecordsMatched)
            //{
            //    model = _metadataService.SearchMetadata("", "", next, limit);

            //    foreach (var item in model.MetadataItems)
            //    {
            //        MetaDataEntry md = ValidateMetadata(item.Uuid);
            //        SaveValidationResult(md);
            //    }

            //    next = model.OffsetNext();
            //    if (next == 0) break;
            //}
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
        }

        private MetaDataEntry ValidateMetadata(string Uuid)
        {

            string url = System.Web.Configuration.WebConfigurationManager.AppSettings["ValideringUrl"] + "api/validatemetadata/" + Uuid;
            System.Net.WebClient c = new System.Net.WebClient();
            c.Encoding = System.Text.Encoding.UTF8;
            var data = c.DownloadString(url);
            var response = Newtonsoft.Json.Linq.JObject.Parse(data);
            var md = Newtonsoft.Json.JsonConvert.DeserializeObject<MetaDataEntry>(response.ToString());

            return md;
        }

        private void SaveValidationResult(MetaDataEntry result)
        {
            try 
            {
            if (result.Status == "ERRORS" && result.ContactEmail != null && result.ContactEmail != "" && result.ContactEmail.Contains('@'))
                {
                    using (var session = MvcApplication.Store.OpenSession())
                    {
                        session.Store(result);
                        session.SaveChanges();
                        
                    }
                }
            }
            catch(Exception ex){
                Log.Error(ex.Message);
            }

        }

        private static void DeleteFiles(IDocumentStore documentStore)
        {
            var indexDefinitions = documentStore.DatabaseCommands.GetIndexes(0, 100);
            foreach (var indexDefinition in indexDefinitions)
            {
                documentStore.DatabaseCommands.DeleteByIndex(indexDefinition.Name, new IndexQuery());
                Log.Info("Delete index: " + indexDefinition.Name);
            }
        }


        public void SendEmail(List<string> emailsTo)
        {
            try 
            { 
                //var cfg=(System.Web.Configuration.CompilationSection) System.Configuration.ConfigurationManager.GetSection("system.web/compilation");

                var session = MvcApplication.Store.OpenSession();
                var results = session.Query<MetaDataEntry>().Take(1024).OrderBy(o => o.ContactEmail);
                //var results = session.Query<MetaDataEntry>().Where(e => emailsTo.Contains(e.ContactEmail)).Take(1024).OrderBy(o => o.ContactEmail);

                var resultsList = results.ToList();
                List<ErrorReport> reports = new List<ErrorReport>();

                var Organisations = resultsList.Select(o => o.ContactEmail).Distinct().ToList();

                foreach (var contactEmail in Organisations) 
                {
                    var resultOrgs = resultsList.Where(r => r.ContactEmail == contactEmail).ToList();
                    ErrorReport errReport = new ErrorReport();

                    foreach(var resultOrg in resultOrgs)
                    {
                        errReport.OrganizationName = resultOrg.OrganizationName;
                        if (!errReport.Emails.Contains(resultOrg.ContactEmail))
                            errReport.Emails.Add(resultOrg.ContactEmail);

                        if (!errReport.MetaData.Contains(resultOrg))
                            errReport.MetaData.Add(resultOrg);

                    }
                    reports.Add(errReport);
                }

                foreach (var report in reports) { 

                    var message = new MailMessage();

                    //foreach (var emailTo in report.Emails) {
                        //message.To.Add(new MailAddress(emailTo));
                        message.To.Add(new MailAddress("metadata@geonorge.no"));
                    //}

                    message.From = new MailAddress(System.Web.Configuration.WebConfigurationManager.AppSettings["WebmasterEmail"]);
                    message.Subject = "Feil i metadata (" + report.Emails[0] + ")";
                    StringBuilder b = new StringBuilder();
                    b.Append("Hei!<br/><br/>\r\n");
                    b.Append("<p>Kartverket arbeider med å forbedre metadataene slik at brukere enklere kan finne fram til geografiske data</p>");
                    b.Append("Vennligst rett opp følgende metadata som har feil/mangler:<br/><br/>\r\n");
                    foreach (var meta in report.MetaData)
                    {
                        b.Append("<a href=\"http://editor.geonorge.no/Metadata/Edit?uuid=" + meta.Uuid + "\">" + meta.Title + "</a><br/>\r\n");
                        b.Append("<b>Feil</b>:<br/>\r\n");
                        foreach (var err in meta.Errors)
                        {
                            b.Append(err.Message + "<br/>\r\n");
                        }
                        b.Append("<hr/>\r\n");
                    }

                    b.Append("Mvh.<br/> Kartverket");

                    message.Body = b.ToString();
                    message.IsBodyHtml = true;


                    using (var smtp = new SmtpClient())
                    {

                        //if (cfg.Debug)
                        //{ 
                        //    smtp.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                        //    smtp.PickupDirectoryLocation = "C:\\temp\\Mails\\";
                        //    smtp.Host = "localhost";
                        //}
                        //else 
                        //{
                            smtp.Host = System.Web.Configuration.WebConfigurationManager.AppSettings["SmtpHost"];
                        //}

                        try
                        {
                            smtp.Send(message);
                        }
                        catch (Exception excep)
                        {
                            Log.Error(excep.Message);
                            Log.Error(excep.InnerException);
                        }
                        Log.Info("Send email to:" + message.To.ToString());
                        Log.Info("Subject:" + message.Subject);
                        Log.Info("Body:" + message.Body);
                    }

                }

            }
            catch (Exception exc) 
            {
                Log.Error(exc.Message);
            }
        }
        
    }

    class ErrorReport 
    {
        public ErrorReport() 
        {
            Emails = new List<string>();
            MetaData = new List<MetaDataEntry>();
        }
        public string OrganizationName { get; set; }
        public List<string> Emails { get; set; }
        public List<MetaDataEntry> MetaData { get; set; }
    }
}