using Newtonsoft.Json;
using Orchard.Environment;
using Orchard.SuiteCRM.Connector.Providers.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.SuiteCRM.Connector
{
    public class Initiliaziation : IOrchardShellEvents
    {
        public void Activated()
        {
            // register JSON Converters
            JsonSerializerSettings defaultSetting = JsonConvert.DefaultSettings != null ? JsonConvert.DefaultSettings() : new JsonSerializerSettings();

            defaultSetting.Converters = defaultSetting.Converters != null ? defaultSetting.Converters : new List<JsonConverter>();

            defaultSetting.Converters.Add(new SuiteCRMProjectPartConverter());

            JsonConvert.DefaultSettings = () => defaultSetting;
        }

        public void Terminating()
        {

        }
    }
}