using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.PropertyEditors;

namespace idseefeld.de.GridImageCropPackage
{
    internal class GridImageCropPackageManifestFilter : IManifestFilter

    {
        public void Filter(List<PackageManifest> manifests)
        {
            var assembly = typeof(GridImageCropPackageManifestFilter).Assembly;
            var version = assembly.GetName().Version?.ToString(3) ?? "2.0.0";

            manifests.Add(new PackageManifest
            {
                PackageName = Constants.PackageName,
                Version = version,
                AllowPackageTelemetry = true,
                GridEditors = new GridEditor[] {
                    new(){
                        Name = "Image Crop",
                        Alias = "gridImageCrop",
                        View = "/App_Plugins/GridImageCrop/imageCropGridEditor.html",
                        Render = "ImageCrop",
                        Icon = "icon-crop"
                    }
            },
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