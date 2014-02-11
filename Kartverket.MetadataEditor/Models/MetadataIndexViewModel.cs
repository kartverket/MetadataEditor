using System;
using System.Collections.Generic;
using System.Linq;
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

        public int ShowTo()
        {
            return Offset + NumberOfRecordsReturned - 1;
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