using GeoNorgeAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.MetadataEditor.Models.ProductSheet
{
    public class ProductSheet
    {
        public MetadataViewModel Metadata { get; set; }

        public string MaintenanceFrequencyFromRegister { get; set; }
        public string StatusFromRegister { get; set; }
        public string AccessConstraintFromRegister { get; set; }

        public string GetThumbnail()
        {
            string thumbnailString = "";
            foreach (var thumbnail in Metadata.Thumbnails)
            {
                thumbnailString = thumbnail.URL;
                if (!thumbnail.URL.StartsWith("http"))
                {
                    thumbnailString = "https://www.geonorge.no/geonetwork/srv/nor/resources.get?uuid=" + Metadata.Uuid + "&access=public&fname=" + thumbnail.URL;
                }
                if (thumbnail.Type == "large_thumbnail")
                    break;
            }

            return thumbnailString;
        }


        public void SetMaintenanceFrequency(Dictionary<string, string> reg)
        {
            if (Metadata.MaintenanceFrequency != null)
            {
                MaintenanceFrequencyFromRegister = Metadata.MaintenanceFrequency;
                if (reg.ContainsKey(Metadata.MaintenanceFrequency))
                    MaintenanceFrequencyFromRegister = reg[Metadata.MaintenanceFrequency];
            }
        }

        public void SetStatus(Dictionary<string, string> reg)
        {
            if(Metadata.Status != null)
            { 
                StatusFromRegister = Metadata.Status;
                if (reg.ContainsKey(Metadata.Status))
                    StatusFromRegister = reg[Metadata.Status];
            }
        }

        public void SetAccessConstraint(Dictionary<string,string> reg)
        {
            if (Metadata.AccessConstraints != null)
            {
                AccessConstraintFromRegister = Metadata.AccessConstraints;
                if (reg.ContainsKey(Metadata.AccessConstraints))
                    AccessConstraintFromRegister = reg[Metadata.AccessConstraints];
            }
        }

    }
}