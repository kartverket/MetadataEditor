using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Kartverket.MetadataEditor.Util
{
    public class MyRequiredAttributeAdapter : RequiredAttributeAdapter
    {
        public MyRequiredAttributeAdapter(
            ModelMetadata metadata,
            ControllerContext context,
            RequiredAttribute attribute
        )
            : base(metadata, context, attribute)
        {
            attribute.ErrorMessageResourceType = typeof(Resources.UI);
            attribute.ErrorMessageResourceName = "PropertyValueRequired";
        }
    }
}