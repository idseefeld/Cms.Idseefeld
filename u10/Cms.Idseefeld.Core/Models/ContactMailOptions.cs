using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cms.Idseefeld.Core.Models
{
    public class ContactMailOptions
    {
        public const string ContactMail = "ContactMail";

        public string? To { get; set; }
        public string? ToName { get; set; }
        public bool? UseMailKit { get; set; }

        public int? MinFormEditingInSeconds { get; set; }
    }
}
