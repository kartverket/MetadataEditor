using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kartverket.MetadataEditor.Models.Rdf
{
    public interface IAdministrativeUnitService
    {
        List<string> UpdateKeywordsPlaceWithUri(List<string> keywordsPlace);
    }
}
