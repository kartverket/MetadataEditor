using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.MetadataEditor.Models
{
    public class ReportService
    {
        private MetadataService _metadataService;
        private static readonly ILog Log = LogManager.GetLogger(typeof(MvcApplication));

        public ReportService() 
        {
            _metadataService = new MetadataService();
        }

        public List<MetadataViewModel> Report1()
        {

            List<MetadataViewModel> report = new List<MetadataViewModel>();

            MetadataIndexViewModel model = new MetadataIndexViewModel();
            int offset = 1;
            int limit = 50;
            model = _metadataService.SearchMetadata("", "", offset, limit);

            foreach (var item in model.MetadataItems)
            {
                //if (item.Uuid != "b7dd7cbb-5cd2-4124-bf9c-fc83f9579982") 
                //{ 
                MetadataViewModel md = _metadataService.GetMetadataModel(item.Uuid);
                if(md.IsDataset())
                report.Add(md);
                //}
            }

            int numberOfRecordsMatched = model.TotalNumberOfRecords;
            int next = model.OffsetNext();

            
                while (next < numberOfRecordsMatched)
                {
                    
                    model = _metadataService.SearchMetadata("", "", next, limit);

                    foreach (var item in model.MetadataItems)
                    {
                        try
                        {
                        MetadataViewModel md = _metadataService.GetMetadataModel(item.Uuid);
                        if (md.IsDataset())
                            report.Add(md);
                        }
                        catch (Exception e)
                        {
                            Log.Error(e.Message);
                        }
                    }

                    next = model.OffsetNext();
                    if (next == 0) break;
                }

            return report;
        }

    }
}