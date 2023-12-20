using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Packaging;

namespace idseefeld.de.GridImageCropPackage.Migrations
{
    public class PackageMigrationPlan : AutomaticPackageMigrationPlan
    {
        /// <summary>
        /// This package can simple use an inlined grid editor view file in package.xml.
        /// IMPORTANT: package.xml file property "Build Action" must be set to "Embedded Resource".
        /// The most simple way to get the package.xml is ot create a package in the backoffice.
        /// </summary>
        public PackageMigrationPlan() : base(Constants.PackageName) { }
    }
}
