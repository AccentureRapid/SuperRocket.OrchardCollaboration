using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Security;
using Orchard;
using Orchard.Logging;
using Orchard.Localization;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.Fields;

using Newtonsoft.Json.Linq;
using System.Text;
using Orchard.Settings;
using Orchard.Caching;
using Orchard.Services;
using Orchard.CRM.AgileCollaboration.Common;
using Orchard.CRM.Core;
using Orchard.CRM.Core.ViewModels;
using Orchard.CRM.Core.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using Orchard.CRM.Core.Providers.Filters;
using Orchard.CRM.Core.Models;
using System.Dynamic;

namespace Orchard.CRM.AgileCollaboration.Services
{
    public class AgileCollaborationService : IAgileCollaborationService
    {
        private readonly IContentManager _contentManager;
        private readonly ISiteService _siteService;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private IClock _clock;
        private readonly IBasicDataService _basicDataService;
        private readonly ICRMContentOwnershipService _crmContentOwnershipService;
        private readonly IProjectionManagerWithDynamicSort _projectionManagerWithDynamicSort;
        private readonly IGroupQuery _groupQuery;
        public AgileCollaborationService(
            IOrchardServices orchardServices,
            IContentManager contentManager,
            ISiteService siteService,
            ICacheManager cacheManager,
            ISignals signals,
            IClock clock,
            IBasicDataService basicDataService,
            ICRMContentOwnershipService crmContentOwnershipService,
            IProjectionManagerWithDynamicSort projectionManagerWithDynamicSort,
            IGroupQuery groupQuery) {

            Services = orchardServices;
            _contentManager = contentManager;
            _siteService = siteService;
            _cacheManager = cacheManager;
            _signals = signals;
            _clock = clock;

            _basicDataService = basicDataService;
            _crmContentOwnershipService = crmContentOwnershipService;
            _projectionManagerWithDynamicSort = projectionManagerWithDynamicSort;
            _groupQuery = groupQuery;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }
        public dynamic GetAvailableHtmlModules()
        {
            var result = _contentManager.Query("HtmlModule").List().Select(contentItem => new
            {
                Title = ((dynamic)contentItem).HtmlModulePart.Title.Value,
                Author = ((dynamic)contentItem).HtmlModulePart.Author.Value,
                Url = ((dynamic)contentItem).HtmlModulePart.Url.Value,
                Description = ((dynamic)contentItem).HtmlModulePart.Description.Value,
                File = GetBaseUrl() + ((dynamic)contentItem).HtmlModulePart.HtmlModuleFile.FirstMediaUrl ?? string.Empty,
                Ico = GetBaseUrl() +((dynamic)contentItem).HtmlModulePart.HtmlModuleIco.FirstMediaUrl ?? string.Empty
            });
            return result;
        }

        public dynamic GetDashBoardViewModel()
        {
            if (this.Services.WorkContext.CurrentUser == null)
            {
                return null;
            }

            var contentQuery = this.Services.ContentManager.HqlQuery().ForVersion(VersionOptions.Published);
            var statusRecords = this._basicDataService.GetStatusRecords().OrderBy(c => c.OrderId).ToList();

            DashboardViewModel model = new DashboardViewModel();
            model.CurrentUserId = this.Services.WorkContext.CurrentUser.Id;
            model.IsCustomer = this._crmContentOwnershipService.IsCurrentUserCustomer();
            model.IsOperator = this.Services.Authorizer.Authorize(Permissions.OperatorPermission);
            dynamic state = new JObject();

            // Query items created by customer
            if (model.IsCustomer)
            {
                // Ticket contentType
                state.ContentTypes = "Ticket";
                contentQuery = this._projectionManagerWithDynamicSort.ApplyFilter(contentQuery, "Content", "ContentTypes", state);

                state.RequestingUser_Id = model.CurrentUserId.ToString(CultureInfo.InvariantCulture);
                contentQuery = this._projectionManagerWithDynamicSort.ApplyFilter(contentQuery, TicketFieldsFilter.CategoryName, TicketFieldsFilter.RequestingUserType, state);

                var userTicketsCountByStateIds = _groupQuery.GetCount(contentQuery, "TicketPartRecord", "StatusRecord.Id");

                model.CurrentUserRequestingTickets = new Collection<dynamic>();
                CRMHelper.AddStatusGroupRecordsToModel(statusRecords, userTicketsCountByStateIds, model.CurrentUserRequestingTickets);

                // overrude items of current users
                state.MaxDueDate = DateTime.UtcNow.Date;
                contentQuery = this._projectionManagerWithDynamicSort.ApplyFilter(contentQuery, TicketFieldsFilter.CategoryName, TicketFieldsFilter.TicketDueDateType, state);
                model.CurrentUserOverrudeRequestingTicketCount = contentQuery.Count();
            }

            // Query the counts of the current user tickets group by stateId
            // *******************************************************
            if (model.IsOperator)
            {
                // Ticket contentType
                state.ContentTypes = "Ticket";
                contentQuery = this._projectionManagerWithDynamicSort.ApplyFilter(contentQuery, "Content", "ContentTypes", state);

                //dynamic temp = new
                //{
                //    Users = new int[] { model.CurrentUserId },
                //    Teams = new int[] { },
                //    BusinessUnits = new int[] { },
                //    AccessType = ContentItemPermissionAccessTypes.Assignee
                //};

                dynamic temp = new ExpandoObject();

                temp.Users = new int[] { model.CurrentUserId };
                temp.Teams = new int[] { };
                temp.BusinessUnits = new int[] { };
                temp.AccessType = ContentItemPermissionAccessTypes.Assignee;
                
                

                contentQuery = this._projectionManagerWithDynamicSort.ApplyFilter(contentQuery, ContentItemPermissionFilter.CategoryName, ContentItemPermissionFilter.AnySelectedUserTeamBusinessUnit, temp);

                var userTicketsCountByStateIds = _groupQuery.GetCount(contentQuery, "TicketPartRecord", "StatusRecord.Id");

                model.CurrentUserTickets = new Collection<dynamic>();
                CRMHelper.AddStatusGroupRecordsToModel(statusRecords, userTicketsCountByStateIds, model.CurrentUserTickets);

                // overrude items of current users
                state.MaxDueDate = DateTime.UtcNow.Date;
                contentQuery = this._projectionManagerWithDynamicSort.ApplyFilter(contentQuery, TicketFieldsFilter.CategoryName, TicketFieldsFilter.TicketDueDateType, state);
                model.CurrentUserOverrudeItemsCount = contentQuery.Count();
                //*******************************************************
            }

            bool isAdmin = this.Services.Authorizer.Authorize(Permissions.AdvancedOperatorPermission);

            if (isAdmin)
            {
                // Query the counts of the whole tickets in the system based on stateId
                state = new JObject();

                contentQuery = this.Services.ContentManager.HqlQuery().ForVersion(VersionOptions.Published);

                state.ContentTypes = "Ticket";
                contentQuery = this._projectionManagerWithDynamicSort.ApplyFilter(contentQuery, "Content", "ContentTypes", state);
                var ticketCountsByStateIds = _groupQuery.GetCount(contentQuery, "TicketPartRecord", "StatusRecord.Id");

                model.AllTickets = new Collection<dynamic>();
                CRMHelper.AddStatusGroupRecordsToModel(statusRecords, ticketCountsByStateIds, model.AllTickets);

                state.MaxDueDate = DateTime.UtcNow.Date;
                contentQuery = this._projectionManagerWithDynamicSort.ApplyFilter(contentQuery, TicketFieldsFilter.CategoryName, TicketFieldsFilter.TicketDueDateType, state);
                model.AllOverrudeItemsCount = contentQuery.Count();
            }

            // get items without any owner
            contentQuery = this.Services.ContentManager.HqlQuery().ForVersion(VersionOptions.Published);
            state.ContentTypes = "Ticket";
            contentQuery = this._projectionManagerWithDynamicSort.ApplyFilter(contentQuery, "Content", "ContentTypes", state);
            contentQuery = this._projectionManagerWithDynamicSort.ApplyFilter(contentQuery, ContentItemPermissionFilter.CategoryName, "ContentItemPermissionPartRecord.ItemsWithoutAnyOwner", state);
            model.AllItemsWithoutOwnerCount = contentQuery.Count();

            return model;
        }
        private string GetBaseUrl()
        {
            var result = _cacheManager.Get(CacheAndSignals.BaseUrlCache, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(999)));
                return GetBaseUrlToCache();
            });
            return result;
        }

        private string GetBaseUrlToCache()
        {
            return _siteService.GetSiteSettings().BaseUrl;
        }

      
    }
}