using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.PropertyEditors.ValueConverters.ImageCropperValue;

namespace idseefeld.de.GridImageCropPackage.Models
{
    public class GridImageCropperValue
    {
        [DataMember(Name = "src")] public string? Src { get; set; } = string.Empty;
        [DataMember(Name = "udi")] public string? Udi { get; set; } = string.Empty;
        [DataMember(Name = "id")] public int Id { get; set; }
        [DataMember(Name = "width")] public int Width { get; set; }
        [DataMember(Name = "height")] public int Height { get; set; }
        [DataMember(Name = "focalPoint")] public ImageCropperFocalPoint? FocalPoint { get; set; }
        [DataMember(Name = "caption")] public string Caption { get; set; } = string.Empty;
        [DataMember(Name = "altText")] public string AltText { get; set; } = string.Empty;
        [DataMember(Name = "selectedCrop")] public ImageCropperCrop? SelectedCrop { get; set; }
        [DataMember(Name = "coordinates")] public ImageCropperCropCoordinates? Coordinates { get; set; }

    }
}
