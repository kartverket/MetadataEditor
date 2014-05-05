using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Kartverket.MetadataEditor.Models
{
    public class MetadataIndexViewModel
    {
        public MetadataIndexViewModel()
        {
            MetadataItems = new List<MetadataItemViewModel>();
        }

        public List<MetadataItemViewModel> MetadataItems { get; set; }
        public int Offset { get; set; }
        public int Limit { get; set; }
        public int TotalNumberOfRecords { get; set; }
        public int NumberOfRecordsReturned { get; set; }
        public string SearchString { get; set; }
        public string SearchOrganization { get; set; }

        public bool UserIsAdmin { get; set; }
        public string UserOrganization { get; set; }
        
        public int ShowTo()
        {
            return Offset + NumberOfRecordsReturned - 1;
        }

        public string PaginationLinkParameters()
        {
            StringBuilder parameters = new StringBuilder();
            if (UserIsAdmin) {
                if (!string.IsNullOrWhiteSpace(SearchOrganization))
                {
                    parameters.Append("organization=").Append(SearchOrganization).Append("&");
                }
                
                if (!string.IsNullOrWhiteSpace(SearchString))
                {
                    parameters.Append("searchString=").Append(SearchString).Append("&");
                }
            }
            return parameters.ToString();
        }

        public int OffsetPrevious()
        {
            int calculated = Offset - Limit;
            if (calculated > 0)
                return calculated;
            else
                return 0;
        }

        public int OffsetNext()
        {
            int calculated = Offset + Limit;
            if (calculated < TotalNumberOfRecords)
                return calculated;
            else
                return 0;
        }

        public String OffsetPreviousState()
        {
            if (OffsetPrevious() == 0)
                return "disabled";
            else
                return "";
        }

        public String OffsetNextState()
        {
            if (OffsetNext() == 0)
                return "disabled";
            else
                return "";
        }





        
    }
}