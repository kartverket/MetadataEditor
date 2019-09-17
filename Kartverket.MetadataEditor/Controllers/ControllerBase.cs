using System.Web.Mvc;
using Geonorge.AuthLib.Common;
using Kartverket.MetadataEditor.Util;

namespace Kartverket.MetadataEditor.Controllers
{
    public class ControllerBase : Controller
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

        protected bool UserHasEditorRole()
        {
            return ClaimsPrincipalUtility.UserHasMetadataEditorRole(User);
        }
    }
}