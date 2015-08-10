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

        public void ValidateAllMetadata() 
        {
            //DeleteFiles(MvcApplication.Store);
            //GetAllMetadata();
            SendEmail();

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
            while (next < numberOfRecordsMatched)
            {
                model = _metadataService.SearchMetadata("", "", next, limit);

                foreach (var item in model.MetadataItems)
                {
                    MetaDataEntry md = ValidateMetadata(item.Uuid);
                    SaveValidationResult(md);
                }

                next = model.OffsetNext();
                if (next == 0) break;
            }
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


        private void SendEmail()
        {
            try 
            { 
                var session = MvcApplication.Store.OpenSession();
                var results = session.Query<MetaDataEntry>().Take(4).OrderBy(o => o.ContactEmail);

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
                        //message.To.Add(new MailAddress("metadata@geonorge.no"));
                    message.To.Add(new MailAddress("dagolav@arkitektum.no"));
                    //}

                    message.From = new MailAddress(System.Web.Configuration.WebConfigurationManager.AppSettings["WebmasterEmail"]);
                    message.Subject = "Feil i metadata (" + report.Emails[0] + ")";
                    StringBuilder b = new StringBuilder();
                    b.Append("Vennligst rett opp følgende metatata som har feil:<br/>\r\n");
                    foreach (var meta in report.MetaData)
                    {
                        b.Append("<a href=\"http://editor.geonorge.no/Metadata/Edit?uuid=" + meta.Uuid + "\">" + meta.Title + "<br/>\r\n");
                        b.Append("Feil:<br/>\r\n");
                        foreach (var err in meta.Errors)
                        {
                            b.Append(err.Message + "<br/>\r\n");
                        }
                        b.Append("<hr/>\r\n");
                    }
                    message.Body = b.ToString();
                    message.IsBodyHtml = true;


                    using (var smtp = new SmtpClient())
                    {
                        //var credential = new NetworkCredential
                        //{
                        //    UserName = "?",  
                        //    Password = "?"  
                        //};
                        //smtp.Credentials = credential;
                        smtp.UseDefaultCredentials = true;
                        smtp.Host = "mail.statkart.no";
                        //smtp.Port = 587;
                        //smtp.EnableSsl = true;
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