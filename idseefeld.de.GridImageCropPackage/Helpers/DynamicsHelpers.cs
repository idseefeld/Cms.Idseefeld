using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace idseefeld.de.GridImageCropPackage.Helpers
{
    public class DynamicsHelpers
    {
        private readonly ILogger<DynamicsHelpers> _logger;
        public DynamicsHelpers(ILogger<DynamicsHelpers> logger) { 
            _logger = logger;
        }
        public bool HasProperty(dynamic obj, string name)
        {
            var rVal = false;
            string? json = null;
            try
            {
                json = JsonConvert.SerializeObject(obj);
                if(!json.StartsWith("[") && !json.StartsWith("{"))
                {
                    return rVal;
                }

                var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (dict == null) { return false; }

                rVal = dict.ContainsKey(name);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogError(ex, "Error in HasProperty(dynamic obj, string name) for {json} {Message}", json, ex.Message);
            }

            return rVal;
        }
    }
}
