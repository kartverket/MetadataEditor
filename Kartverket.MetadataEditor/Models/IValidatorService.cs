using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kartverket.MetadataEditor.Models
{
    public interface IValidatorService
    {
        List<string> ValidateAllMetadata();
        void SendEmail(List<string> emails);
    }
}
