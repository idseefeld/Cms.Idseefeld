using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Cms.Idseefeld.Core.Models
{
    public class ImageCropViewModel
    {
        public ImageCropperValue? CropperValue { get; set; }
        public string Alt { get; set; } = string.Empty;
        public string? Caption { get; set; }
    }
}
