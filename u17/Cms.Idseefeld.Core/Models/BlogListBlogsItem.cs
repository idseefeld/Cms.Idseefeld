using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cms.Idseefeld.Core.Models
{
    public class BlogListBlogsItem
    {
        public string Label { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public bool IsCurrent { get; set; } = false;
    }
}
