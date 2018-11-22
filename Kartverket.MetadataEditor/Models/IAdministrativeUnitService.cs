using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kartverket.MetadataEditor.Models.Rdf
{
    public interface IAdministrativeUnitService
    {
        void UpdateKeywordsAdministrativeUnitsWithUri(ref List<string> keywordsAdministrativeUnits, ref List<string> keywordsPlace);
    }
}
