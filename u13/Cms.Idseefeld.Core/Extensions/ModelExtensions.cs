using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Extensions;

namespace Cms.Idseefeld.Core.Extensions
{
    public static class ModelExtensions
    {
        public static string GetPropertyDisplayName(this object model, string propertyName) {
            var modelType = model.GetType();
            var props = modelType.GetProperties();
            var prop = props.FirstOrDefault(p => p.Name.Equals(propertyName));
            if (prop == null)
            {
                prop = props.FirstOrDefault(p => p.Name.InvariantEquals(propertyName));
            }
            propertyName = prop?.Name ?? propertyName;
            var attr = prop?.GetCustomAttributes(typeof(DisplayAttribute), false).Cast<DisplayAttribute>().SingleOrDefault();
            var displayName = attr?.Name;
            var rVal = displayName ?? propertyName;
            return rVal;
        }
    }
}
