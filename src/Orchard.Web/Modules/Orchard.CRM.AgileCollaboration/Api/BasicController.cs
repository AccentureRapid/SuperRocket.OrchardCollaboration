using Newtonsoft.Json;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.CRM.AgileCollaboration.Common;
using Orchard.CRM.AgileCollaboration.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Orchard.Users.Events;
using Orchard.CRM.Core.Services;
using Orchard.CRM.Core.Models;
using Orchard.Core.Common.Models;
using Orchard.CRM.Core.ViewModels;
using Orchard.UI.Navigation;
using Orchard.Settings;
using Orchard.CRM.Core;
using System.Globalization;
using Orchard.Core.Title.Models;
using Orchard.Users.Models;
using System.Dynamic;

namespace Orchard.CRM.AgileCollaboration.Api
{

    public class BasicController : ApiController
    {
        
        private static readonly char[] separator = new[] { '{', '}', ',' };
        private readonly IContentManager _contentManager;
        private readonly IContentTypesService _contentTypesService;
        private readonly IHtmlModuleService _moduleService;
        private readonly IUserEventHandler _userEventHandler;
        private readonly IMembershipService _membershipService;
        private readonly IProjectService _projectService;
        private readonly IAgileCollaborationService _agileCollaborationService;
        private readonly ICRMContentOwnershipService _crmContentOwnershipService;
        private readonly IBasicDataService _basicDataService;
        private readonly ISiteService _siteService;
        private readonly ISearchTicketService _searchTicketService;
        private readonly IBusinessUnitService _businessUnitService;

        public BasicController(
            IContentManager contentManager,
            IContentTypesService contentTypesService,
            IHtmlModuleService moduleService,
            IUserEventHandler userEventHandler,
            IMembershipService membershipService,
            IProjectService projectService,
            IOrchardServices _services,
            IAgileCollaborationService agileCollaborationService,
            ICRMContentOwnershipService crmContentOwnershipService,
            IBasicDataService basicDataService,
            ISiteService siteService,
            ISearchTicketService searchTicketService,
            IBusinessUnitService businessUnitService
            )
        {
            _contentManager = contentManager;
            _contentTypesService = contentTypesService;
            _moduleService = moduleService;
            _userEventHandler = userEventHandler;
            _membershipService = membershipService;
            _projectService = projectService;
            Services = _services;
            _agileCollaborationService = agileCollaborationService;
            _crmContentOwnershipService = crmContentOwnershipService;
            _basicDataService = basicDataService;
            _siteService = siteService;
            _searchTicketService = searchTicketService;
            _businessUnitService = businessUnitService;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public IOrchardServices Services { get; set; }

        /// <summary>
        /// GET api/Basic/GetBusinessUnitMembers
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetBusinessUnitMembers()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                var result = _basicDataService.GetBusinessUnitMembers();
                response.Content = Serialize(result, response);
            }
            catch (Exception ex)
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                Logger.Error("Error occurs when GetBusinessUnitMembers :" + ex.StackTrace);
            }
            return response;
        }

        /// <summary>
        /// GET api/Basic/GetBusinessUnits
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetBusinessUnits()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                var result = _basicDataService.GetBusinessUnits().Select(
                    x => new {
                        x.Id,
                        x.As<BusinessUnitPart>().Name,
                        x.As<CommonPart>().CreatedUtc,
                        x.As<CommonPart>().PublishedUtc,
                        x.As<CommonPart>().ModifiedUtc,
                        x.As<CommonPart>().VersionCreatedUtc,
                        x.As<CommonPart>().VersionModifiedUtc,
                        x.As<CommonPart>().VersionPublishedUtc,
                        x.As<CommonPart>().Owner.UserName
                    }
                    ); 
                response.Content = Serialize(result, response);
            }
            catch (Exception ex)
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                Logger.Error("Error occurs when GetBusinessUnits :" + ex.StackTrace);
            }
            return response;
        }

        /// <summary>
        /// GET api/Basic/GetStatusRecords
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetStatusRecords()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                var result = _basicDataService.GetStatusRecords().Select(
                    x => new {
                        x.Id,
                        x.Name,
                        x.StatusTypeId,
                        x.IsHardCode,
                        x.OrderId,
                        x.Deleted
                    }
                    );
                response.Content = Serialize(result, response);
            }
            catch (Exception ex)
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                Logger.Error("Error occurs when GetStatusRecords :" + ex.StackTrace);
            }
            return response;
        }
        /// <summary>
        /// GET api/Basic/GetTicketTypes
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetTicketTypes()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                var result = _basicDataService.GetTicketTypes().Select(
                    x => new {
                        x.Id,
                        x.Name,
                        x.IsHardCode,
                        x.OrderId,
                        x.Deleted
                    }
                    );
                response.Content = Serialize(result, response);
            }
            catch (Exception ex)
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                Logger.Error("Error occurs when GetTicketTypes :" + ex.StackTrace);
            }
            return response;
        }

        /// <summary>
        /// GET api/Basic/GetPriorities
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetPriorities()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                var result = _basicDataService.GetPriorities().Select(
                    x => new {
                        x.Id,
                        x.Name,
                        x.IsHardCode,
                        x.OrderId
                    }
                    );
                response.Content = Serialize(result, response);
            }
            catch (Exception ex)
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                Logger.Error("Error occurs when GetPriorities :" + ex.StackTrace);
            }
            return response;
        }

        /// <summary>
        /// GET api/Basic/GetServices
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetServices()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                var result = _basicDataService.GetServices().Select(
                    x => new {
                        x.Id,
                        x.Name,
                        x.Description,
                        x.Deleted
                    }
                    );
                response.Content = Serialize(result, response);
            }
            catch (Exception ex)
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                Logger.Error("Error occurs when GetServices :" + ex.StackTrace);
            }
            return response;
        }

        private StringContent Serialize(dynamic source, HttpResponseMessage response)
        {
            if (source == null)
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
            }
            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new NullToEmptyStringResolver(),
                PreserveReferencesHandling = PreserveReferencesHandling.None
            };

            var stringcontent = JsonConvert.SerializeObject(source, Newtonsoft.Json.Formatting.Indented, settings);
            return new StringContent(stringcontent, Encoding.GetEncoding("UTF-8"), "application/json");
        }
    }
}
