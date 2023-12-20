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
            manifests.Add(new PackageManifest
            {
                PackageName = Constants.PackageName,
                Version = assembly.GetName()?.Version?.ToString(3) ?? "1.0.0",
                AllowPackageTelemetry = true,
                GridEditors = new GridEditor[] {
                    new(){
                        Name = "Image Crop",
                        Alias = "gridImageCrop",
                        View = "/App_Plugins/GridImageCrop/imageCropGridEditor.html",
                        Render = "ImageCrop",
                        Icon = "icon-crop",
                        Config = new Dictionary<string, object>
                        {
                            { "selectedCrop", "Portrait" },

                            { "size", new Dictionary<string, object>
                                {
                                    { "width", "800" },
                                    { "height", "1200" }
                                } 
                            }
                        }
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