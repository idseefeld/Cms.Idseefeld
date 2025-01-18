using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cms.Idseefeld.Core.Models
{
    public class CultureUrl
    {
        public string? Culture { get; set; }

        public string? Url { get; set; }

        public string? Name => Culture?.Substring(3, 2).ToUpper();

        public bool Current { get; set; }
    }
}
