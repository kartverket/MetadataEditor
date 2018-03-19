using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Kartverket.MetadataEditor.Models
{
    public interface IBatchService
    {
        void UpdateRegisterTranslations(string username, string uuid);
        void UpdateAll(BatchData data, string v1, string v2);
        void Update(BatchData data, string v);
        void GenerateMediumThumbnails(string v1, string v2, string v3);
        void Update(HttpPostedFileBase file, string v, string metadatafield, bool deleteData, string metadatafieldEnglish);
        void UpdateFormatOrganization(string v);
        void UpdateKeywordServiceType(string v);
    }
}
