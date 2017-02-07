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
using Orchard.CRM.Project.Services;
using Orchard.Core.Settings.Models;
using Orchard.CRM.Project.Models;

namespace Orchard.CRM.AgileCollaboration.Api
{

    public class WikiController : ApiController
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
        private readonly IExtendedProjectService _extendedProjectService;
        private readonly IFolderService _folderService;

        public WikiController(
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
            IBusinessUnitService businessUnitService,
            IExtendedProjectService extendedProjectService,
            IFolderService folderService
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
            _extendedProjectService = extendedProjectService;
            _folderService = folderService;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public IOrchardServices Services { get; set; }

        /// <summary>
        /// GET api/Wiki/GetFolders?projectId=110
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetFolders(int projectId)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                var result = _folderService.GetFolders(projectId).Select(
                    x => new
                    {
                        x.Id,
                        x.As<FolderPart>().Record.Title,
                        x.As<FolderPart>().Record.Parent_Id,
                        Project_Id = x.As<FolderPart>().Record.Project.Id,
                        x.As<CommonPart>().CreatedUtc,
                        x.As<CommonPart>().PublishedUtc,
                        x.As<CommonPart>().ModifiedUtc,
                        x.As<CommonPart>().VersionCreatedUtc,
                        x.As<CommonPart>().VersionModifiedUtc,
                        x.As<CommonPart>().VersionPublishedUtc,
                        x.As<CommonPart>().Owner.UserName
                    });

                response.Content = Serialize(result, response);
            }
            catch (Exception ex)
            {
                response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                Logger.Error("Error occurs when GetFolders :" + ex.StackTrace);
            }
            return response;
        }

        /// <summary>
        /// GET api/Wiki/GetAttachedItemsInRootFolder?projectId=110&page=1
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetAttachedItemsInRootFolder(int projectId, int? page)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                var currentPage = page == null ? 1 : page;
                var pager = new Pager(this.Services.WorkContext.CurrentSite, currentPage, this.Services.WorkContext.CurrentSite.PageSize);

                var result = _folderService.GetAttachedItemsInRootFolder(projectId, pager).Select(
                      x => new {
                          x.Id,
                          x.As<TitlePart>().Title,
                          x.As<BodyPart>().Text,
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
                response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                Logger.Error("Error occurs when GetAttachedItemsInRootFolder :" + ex.StackTrace);
            }
            return response;
        }

        /// <summary>
        /// GET api/Wiki/GetAttachedItemsToFolder?projectId=110&page=1
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetAttachedItemsToFolder(int folderId, int projectId, int? page)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                var currentPage = page == null ? 1 : page;
                var pager = new Pager(this.Services.WorkContext.CurrentSite, currentPage, this.Services.WorkContext.CurrentSite.PageSize);

                var result = _folderService.GetAttachedItemsToFolder(folderId , projectId, pager).Select(
                      x => new {
                          x.Id,
                          x.As<TitlePart>().Title,
                          x.As<BodyPart>().Text,
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
                response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                Logger.Error("Error occurs when GetAttachedItemsToFolder :" + ex.StackTrace);
            }
            return response;
        }

        private StringContent Serialize(dynamic source, HttpResponseMessage response)
        {
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
