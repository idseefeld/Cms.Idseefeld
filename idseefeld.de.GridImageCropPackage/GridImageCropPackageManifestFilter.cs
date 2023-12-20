using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Manifest;

namespace idseefeld.de.GridImageCropPackage
{
    internal class GridImageCropPackageManifestFilter : IManifestFilter

    {
        public void Filter(List<PackageManifest> manifests)
        {
            var assembly = typeof(GridImageCropPackageManifestFilter).Assembly;
            manifests.Add(new PackageManifest
            {
                PackageName = assembly.GetName().Name,
                Version = assembly.GetName()?.Version?.ToString(3) ?? "1.0.0",
                AllowPackageTelemetry = true,
                Scripts = new string[] {
                    "/App_Plugins/GridImageCrop/imageCropGridEditor.controller.js",
                    "/App_Plugins/GridImageCrop/mediapicker.controller.js",
                    "/App_Plugins/GridImageCrop/overlays/mediacropdetails.controller.js"
                },
                Stylesheets = new string[] {
                    "/App_Plugins/GridImageCrop/imageCrop.css"
                }
            });
        }
    }
}
