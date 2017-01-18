using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard;
using System.Collections;

namespace Orchard.CRM.AgileCollaboration.Services
{
    public interface IContentTypesService : IDependency
    {
        IEnumerable<string> GetContentTypes();
        dynamic Publish(string name);
    }
}