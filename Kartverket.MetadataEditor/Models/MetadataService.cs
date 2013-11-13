using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using www.opengis.net;

namespace Kartverket.MetadataEditor.Models
{
    public class MetadataService
    {
        public List<MetadataItemViewModel> GetMyMetadata(string organizationName)
        {
            var geonorge = new GeoNorgeAPI.GeoNorge();
            SearchResultsType results = geonorge.SearchWithOrganisationName(organizationName);

            var metadata = new List<MetadataItemViewModel>();

            foreach (var item in results.Items)
            {
                RecordType record = (RecordType)item;

                string title = null;
                string uuid = null;
                string organization = null;

                for (int i = 0; i < record.ItemsElementName.Length; i++)
                {
                    var name = record.ItemsElementName[i];
                    var value = record.Items[i].Text != null ? record.Items[i].Text[0] : null;

                    if (name == ItemsChoiceType24.title)
                        title = value;
                    else if (name == ItemsChoiceType24.identifier)
                        uuid = value;
                    else if (name == ItemsChoiceType24.creator)
                        organization = value;
                }
                metadata.Add(new MetadataItemViewModel { Title = title, Uuid = uuid, Organization = organization });
            }

            return metadata;
        }

    }
}