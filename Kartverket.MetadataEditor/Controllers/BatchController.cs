﻿using Kartverket.MetadataEditor.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Kartverket.MetadataEditor.Controllers
{
    public class BatchController : Controller
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MvcApplication));

        private MetadataService _metadataService;

        public BatchController()
        {
            _metadataService = new MetadataService();
        }

        public ActionResult BatchUpdate(FormCollection batch) 
        {
            Log.Info("Starting batch update metadata.");

            BatchData data = GetFormData(batch);

            new Thread(() => new BatchService().Update(data, GetUsername())).Start();

            TempData["message"] = "Batch-oppdatering er startet!";

            return RedirectToAction("Index");
        }

        private BatchData GetFormData(FormCollection batch)
        {
            BatchData data = new BatchData();

            data.dataField = batch["batchField"];
            string dataValue = batch["batchValue"];
            if (data.dataField == "ContactMetadata_Organization" || data.dataField == "ContactPublisher_Organization" || data.dataField == "ContactOwner_Organization")
            {
                dataValue = batch["OrganizationContactMetadata"];
            }
            else if (data.dataField == "MaintenanceFrequency")
            {
                dataValue = batch["MaintenanceFrequency"];
            }

            data.dataValue = dataValue;

            var uuids = batch["uuids"].Split(',');

            List<MetaDataEntry> mdList = new List<MetaDataEntry>();

            foreach (var uuid in uuids)
            {
                MetaDataEntry md = new MetaDataEntry();
                md.Uuid = uuid;
                mdList.Add(md);
            }

            data.MetaData = mdList;

            return data;
        }

        public ActionResult Index(MetadataMessages? message, string organization = "", string searchString = "", int offset = 1, int limit = 50)
        {
            ViewBag.StatusMessage =
                message == MetadataMessages.InvalidUuid ? Resources.UI.Error_InvalidUuid
                : "";

            MetadataIndexViewModel model = new MetadataIndexViewModel();

            if (User.Identity.IsAuthenticated)
            {
                string userOrganization = GetSecurityClaim("organization");
                string role = GetSecurityClaim("role");
                if (!string.IsNullOrWhiteSpace(role) && role.Equals("nd.metadata_admin"))
                {
                    model = _metadataService.SearchMetadata(organization, searchString, offset, limit);
                    model.UserIsAdmin = true;
                }
                else
                {
                    model = _metadataService.SearchMetadata(userOrganization, searchString, offset, limit);
                }

                model.UserOrganization = userOrganization;
            }

            Dictionary<string, string> OrganizationList = GetListOfOrganizations();
            ViewBag.OrganizationContactMetadata = new SelectList(OrganizationList, "Key", "Value");
            ViewBag.MaintenanceFrequency = new SelectList(GetListOfMaintenanceFrequencyValues(), "Key", "Value");


            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }

            return View(model);
        }

        private string GetUsername()
        {
            return GetSecurityClaim("username");
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

        public Dictionary<string, string> GetListOfOrganizations()
        {
            Dictionary<string, string> Organizations = new Dictionary<string, string>();

            System.Net.WebClient c = new System.Net.WebClient();
            c.Encoding = System.Text.Encoding.UTF8;
            var data = c.DownloadString(System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/register/organisasjoner");
            var response = Newtonsoft.Json.Linq.JObject.Parse(data);

            var orgs = response["containeditems"];

            foreach (var org in orgs)
            {
                if (!Organizations.ContainsKey(org["label"].ToString()))
                {
                    Organizations.Add(org["label"].ToString(), org["label"].ToString());
                }
            }

            Organizations = Organizations.OrderBy(o => o.Value).ToDictionary(o => o.Key, o => o.Value);

            return Organizations;
        }

        public Dictionary<string, string> GetCodeList(string systemid)
        {
            Dictionary<string, string> CodeValues = new Dictionary<string, string>();
            string url = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/kodelister/" + systemid;
            System.Net.WebClient c = new System.Net.WebClient();
            c.Encoding = System.Text.Encoding.UTF8;
            var data = c.DownloadString(url);
            var response = Newtonsoft.Json.Linq.JObject.Parse(data);

            var codeList = response["containeditems"];

            foreach (var code in codeList)
            {
                var codevalue = code["codevalue"].ToString();
                if (string.IsNullOrWhiteSpace(codevalue))
                    codevalue = code["label"].ToString();

                if (!CodeValues.ContainsKey(codevalue))
                {
                    CodeValues.Add(codevalue, code["label"].ToString());
                }
            }

            CodeValues = CodeValues.OrderBy(o => o.Value).ToDictionary(o => o.Key, o => o.Value);

            return CodeValues;
        }

        public Dictionary<string, string> GetListOfMaintenanceFrequencyValues()
        {

            return GetCodeList("9A46038D-16EE-4562-96D2-8F6304AAB124");
        }

    }
}