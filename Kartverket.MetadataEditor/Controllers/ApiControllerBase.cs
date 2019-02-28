using System.Web.Http;
using Geonorge.AuthLib.Common;
using Kartverket.MetadataEditor.Util;

namespace Kartverket.MetadataEditor.Controllers
{
    public class ApiControllerBase : ApiController
    {
        protected string GetCurrentUserOrganizationName()
        {
            return ClaimsPrincipalUtility.GetCurrentUserOrganizationName(User);
        }

        protected string GetUsername()
        {
            return ClaimsPrincipalUtility.GetUsername(User);
        }

        protected bool UserHasMetadataAdminRole()
        {
            return ClaimsPrincipalUtility.UserHasMetadataAdminRole(User);
        }
    }
}