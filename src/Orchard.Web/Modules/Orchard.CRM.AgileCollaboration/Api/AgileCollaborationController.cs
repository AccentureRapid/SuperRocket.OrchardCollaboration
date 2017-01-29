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

namespace Orchard.CRM.AgileCollaboration.Api
{

    public class AgileCollaborationController : ApiController
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

        public AgileCollaborationController(
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
        /// GET api/AgileCollaboration/GetContentTypeDefinition?type=User
        /// example: http://localhost/api/AgileCollaboration/GetContentTypeDefinition?type=User
        /// </summary>
        /// <param name="type">Content Type Name</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetContentTypeDefinition(string type)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                 var definition = (from t in _contentManager.GetContentTypeDefinitions()
                 where t.Name == type
                 select t).FirstOrDefault();

                
                response.Content = Serialize(definition, response);
            }
            catch (Exception ex)
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                Logger.Error("Error occurs when GetContentTypeDefinition :" + ex.Message);
            }
            return response;
        }

        /// <summary>
        /// GET api/AgileCollaboration/ContentTypes
        /// example : http://localhost/api/AgileCollaboration/ContentTypes
        /// </summary>
        /// <returns>All content type names</returns>
        [HttpGet]
        public IEnumerable<string> ContentTypes()
        {
            return _contentTypesService.GetContentTypes();
        }

        /// <summary>
        /// POST api/AgileCollaboration/Login
        /// example : http://localhost/api/AgileCollaboration/Login 
        /// </summary>
        /// <param name="userNameOrEmail"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPost]
        [AlwaysAccessible]
        public HttpResponseMessage Login(dynamic userInfo)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                if (userInfo != null)
                {
                    var userNameOrEmail = Convert.ToString(userInfo.userNameOrEmail);
                    var password = Convert.ToString(userInfo.password);
                    IUser user = _membershipService.ValidateUser(userNameOrEmail, password);
                    var result = new
                    {
                        user.Id,
                        user.UserName,
                        user.Email
                    };
                    response.Content = Serialize(result, response);
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                Logger.Error("Error occurs when Login :" + ex.Message);
            }
            return response;
        }

        /// <summary>
        /// GET api/AgileCollaboration/GetMyProjects?userName=
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetMyProjects(string userName)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                var user = _membershipService.GetUser(userName);
                if (user != null)
                {
                    Services.WorkContext.CurrentUser = user;

                    var projects = _projectService.GetProjects(null).Select(item =>
                    new {
                        item.As<ProjectPart>().Id,
                        item.As<ProjectPart>().Title,
                        item.As<ProjectPart>().Description,
                        item.As<CommonPart>().CreatedUtc,
                        item.As<CommonPart>().PublishedUtc,
                        item.As<CommonPart>().ModifiedUtc,
                        item.As<CommonPart>().VersionCreatedUtc,
                        item.As<CommonPart>().VersionModifiedUtc,
                        item.As<CommonPart>().VersionPublishedUtc,
                        item.As<CommonPart>().Owner.UserName
                    });
                   
                    response.Content = Serialize(projects, response);
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                Logger.Error("Error occurs when GetMyProjects :" + ex.Message);
            }
            return response;
        }

        /// <summary>
        /// GET api/AgileCollaboration/GetDashBoardViewModel?userName=
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetDashBoardViewModel(string userName)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                var user = _membershipService.GetUser(userName);
                if (user != null)
                {
                    Services.WorkContext.CurrentUser = user;
                    var result = _agileCollaborationService.GetDashBoardViewModel();
                    response.Content = Serialize(result, response);
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                Logger.Error("Error occurs when GetDashBoardViewModel :" + ex.Message);
            }
            return response;
        }
        /// <summary>
        /// GET api/AgileCollaboration/SearchTickets?DueDate=Overdue 
        /// GET api/AgileCollaboration/SearchTickets?Status=1 
        /// </summary>
        /// <param name="pagerParameters"></param>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage Search([FromUri]PostedTicketSearchViewModel searchModel)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                PagerParametersWithSortFields pagerParameters = new PagerParametersWithSortFields();
                // A simple solution for the bug of sending page paramemter via querystring, if searchModel has value, with unknown reason, the page will not be set
                if (pagerParameters != null && pagerParameters.Page == null && Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value).ContainsKey("page"))
                {
                    if (!string.IsNullOrEmpty(Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value)["page"]))
                    {
                        int page;
                        if (int.TryParse(Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value)["page"], out page))
                        {
                            pagerParameters.Page = page;
                        } 
                    }
                }

                if (this._crmContentOwnershipService.IsCurrentUserCustomer())
                {
                    searchModel.Unassigned = false;
                    searchModel.Users = new int[] { };
                    searchModel.IncludeAllVisibleItemsBySelectedGroupsAndUsers = false;
                }

                // add default sort field, if it is not provided
                if (string.IsNullOrEmpty(pagerParameters.SortField))
                {
                    pagerParameters.SortField = TicketPart.IdentityFieldName;
                    pagerParameters.Descending = true;
                }

                if (searchModel != null)
                {
                    searchModel.Users = searchModel.Users ?? new int[] { };
                    searchModel.BusinessUnits = searchModel.BusinessUnits ?? new int[] { };
                }

                if (!string.IsNullOrEmpty(searchModel.Status) && !this._basicDataService.GetStatusRecords().Any(c => c.Id.ToString() == searchModel.Status))
                {
                    searchModel.Status = string.Empty;
                }

                SearchTicketsViewModel result = null;
                // full text search will be done by lucene
                if (string.IsNullOrEmpty(searchModel.Term))
                {
                     result =  this.SearchByHqlQuery(pagerParameters, searchModel);
                }
                else
                {
                     //result = this.SearchByLucene(pagerParameters, searchModel);
                }

                response.Content = Serialize(result, response);
            }
            catch (Exception ex)
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                Logger.Error("Error occurs when SearchTickets :" + ex.Message);
            }
            return response;
        }

        private SearchTicketsViewModel SearchByHqlQuery(PagerParametersWithSortFields pagerParameters, PostedTicketSearchViewModel searchModel)
        {
            Pager pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            int totalCount = this._searchTicketService.CountByDatabase(searchModel);
            var contentItems = this._searchTicketService.SearchByDatabase(pagerParameters, searchModel);

            SearchTicketsViewModel model = this.CreateSearchModel(contentItems, pager, searchModel, pagerParameters, totalCount);

            return model;
        }

        private SearchTicketsViewModel CreateSearchModel(IEnumerable<IContent> contentItems, Pager pager, PostedTicketSearchViewModel postedSearchModel, PagerParametersWithSortFields pagerParameters, int totalCount)
        {
            SearchTicketsViewModel model = new SearchTicketsViewModel();
            model.Term = postedSearchModel.Term;
            model.IsAdminUser = this.Services.Authorizer.Authorize(Permissions.AdvancedOperatorPermission);
            if (postedSearchModel.DueDate == PostedTicketSearchViewModel.OverDueDate)
            {
                model.Overdue = true;
            }
            else
            {
                model.Overdue = false;
                DateTime value;
                if (DateTime.TryParse(postedSearchModel.DueDate, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out value))
                {
                    model.DueDate = value;
                }
            }

            model.StatusId = postedSearchModel.Status;
            model.UnStatus = postedSearchModel.UnStatus;
            model.PagerParameters = pagerParameters;
            model.RelatedContentItemId = postedSearchModel.RelatedContentItemId;

            // related contentItem
            if (postedSearchModel.RelatedContentItemId.HasValue)
            {
                var relatedContentItem = this._contentManager.Get(postedSearchModel.RelatedContentItemId.Value);
                var titlePart = relatedContentItem.As<TitlePart>();
                if (titlePart != null)
                {
                    model.RelatedContentItemTitle = titlePart.Title;
                }
            }

            model.Pager = this.Services.New.Pager(pager).TotalItemCount(totalCount);

            model.Items = new List<dynamic>();
            foreach (var contentItem in contentItems)
            {
                // ignore search results which content item has been removed or unpublished
                if (contentItem == null)
                {
                    totalCount--;
                    continue;
                }

                var itemModel = this._contentManager.BuildDisplay(contentItem, "TableRow");
                itemModel.Metadata.Type = "Ticket_TableRow_Container";
                model.Items.Add(itemModel);
                itemModel.IsEditable = this._crmContentOwnershipService.CurrentUserCanEditContent(contentItem);
            }

            model.Unassigned = postedSearchModel.Unassigned;
            this.FillBusinessUnitsAndUsers(model, postedSearchModel);

            // Projects
            if (this._projectService.IsTicketsRelatedToProjects())
            {
                model.IsProjectForTicketsSupported = true;
                model.ProjectId = postedSearchModel.ProjectId;
                var projects = this._projectService.GetProjects(null);
                Converter.Fill(model.Projects, projects.AsPart<ProjectPart>());
            }

            if (this._crmContentOwnershipService.IsCurrentUserCustomer())
            {
                model.IsCustomerUser = true;
                model.Users.Clear();
                model.BusinessUnits.ToList().ForEach(c => c.Teams.Clear());
            }

            var statusRecords = this._basicDataService.GetStatusRecords().ToList();
            model.ClosedStatusId = statusRecords.First(c => c.StatusTypeId == StatusRecord.ClosedStatus).Id;
            model.OpenStatusId = statusRecords.First(c => c.StatusTypeId == StatusRecord.OpenStatus).Id;

            // IncludeAllVisibleItemsBySelectedGroupsAndUsers  is meaningful, if there is a selected user or business unit
            model.IncludeAllVisibleItemsBySelectedGroupsAndUsers = postedSearchModel.IncludeAllVisibleItemsBySelectedGroupsAndUsers &&
                (model.Users.Any(c => c.Checked) || model.BusinessUnits.Any(c => c.Checked) || model.BusinessUnits.SelectMany(c => c.Teams).Any(c => c.Checked));

            model.SearchDescription = this.GetSearchDescription(model, statusRecords);

            return model;
        }
        /// <summary>
        /// Fills the model with the businessUnits that the user has granted to view them
        /// </summary>
        private void FillBusinessUnitsAndUsers(SearchTicketsViewModel model, PostedTicketSearchViewModel postedSearchModel)
        {
            bool restrictToUserPermissions = !this.Services.Authorizer.Authorize(Permissions.AdvancedOperatorPermission);
            this._businessUnitService.Fill(model.BusinessUnits, restrictToUserPermissions);

            var selectedBusinessUnits = postedSearchModel.BusinessUnits.ToList();

            // TeamIds of the search
            var teams = new List<int>();

            // set checkes of businessUnits
            model.BusinessUnits
                .ToList()
                .ForEach(c => c.Checked = selectedBusinessUnits.Count(d => d == c.BusinessUnitId) > 0);

            // set checks of teams
            model.BusinessUnits
                .SelectMany(c => c.Teams)
                .ToList()
                .ForEach(c => c.Checked = teams.Count(d => d == c.TeamId) > 0);

            IEnumerable<IUser> users = null;

            users = this._basicDataService.GetOperators().ToList();

            foreach (var user in users)
            {
                SearchTicketsViewModel.UserViewModel userViewModel = new SearchTicketsViewModel.UserViewModel
                {
                    Id = user.Id,
                    Username = CRMHelper.GetFullNameOfUser(user.As<UserPart>()),
                    Checked = postedSearchModel.Users.Count(c => c == user.Id) > 0,
                    IsAdminOrOperator = true
                };

                model.Users.Add(userViewModel);
            }
        }


        private string GetSearchDescription(SearchTicketsViewModel model, IEnumerable<StatusRecord> statusList)
        {
            List<string> parts = new List<string>();

            string format = "<span class='label'>{0}</span>: {1}";
            string oneParameterFormat = "<span class='label'>{0}</span>";

            // groups
            var selectedBusinessUnits = model.BusinessUnits.Where(c => c.Checked);
            var selectedTeams = model.BusinessUnits.SelectMany(c => c.Teams.Where(d => d.Checked));
            var groups = selectedBusinessUnits.Select(c => c.Name).Union(selectedTeams.Select(d => d.Name));
            if (groups.Any())
            {
                string groupsString = string.Format(CultureInfo.CurrentUICulture, format, T("Groups").Text, string.Join(", ", groups));
                parts.Add(groupsString);
            }

            // project
            if (model.ProjectId.HasValue && this._projectService.IsTicketsRelatedToProjects())
            {
                var project = this._projectService.GetProject(model.ProjectId.Value);
                string projectName = project != null ? project.Record.Title : this.T("UNKNOWN Project").Text;
                string projectNameString = string.Format(CultureInfo.CurrentUICulture, format, T("Project").Text, projectName);
                parts.Add(projectNameString);
            }

            // related ContentItem
            if (model.RelatedContentItemId.HasValue)
            {
                if (!string.IsNullOrEmpty(model.RelatedContentItemTitle))
                {
                    string text = string.Format(CultureInfo.CurrentUICulture, format, T("Tickets related to").Text, model.RelatedContentItemTitle);
                    parts.Add(text);
                }
                else
                {
                    string text = string.Format(CultureInfo.CurrentUICulture, format, T("Tickets related to ContentId").Text, model.RelatedContentItemId.Value.ToString(CultureInfo.InvariantCulture));
                    parts.Add(text);
                }
            }

            // users
            var selectedUsers = model.Users.Where(c => c.Checked).Select(c => c.Username);
            if (selectedUsers.Any())
            {
                string selectedUsersString = string.Format(CultureInfo.CurrentUICulture, format, T("Users").Text, string.Join(", ", selectedUsers));
                parts.Add(selectedUsersString);
            }

            // Unassigned
            if (model.Unassigned)
            {
                parts.Add(string.Format(CultureInfo.CurrentUICulture, oneParameterFormat, T("Unassigned").Text));
            }

            // Status
            if (!string.IsNullOrEmpty(model.StatusId))
            {
                var status = statusList.FirstOrDefault(c => c.Id.ToString(CultureInfo.InvariantCulture).ToUpper(CultureInfo.InvariantCulture) == model.StatusId.ToUpper(CultureInfo.InvariantCulture));
                if (status != null)
                {
                    parts.Add(string.Format(CultureInfo.CurrentUICulture, format, T("Status").Text, status.Name));
                }
            }

            // Unstatus
            if (model.UnStatus)
            {
                parts.Add(string.Format(CultureInfo.CurrentUICulture, oneParameterFormat, T("No Status").Text));
            }

            // Due date
            if (model.DueDate.HasValue)
            {
                if (model.DueDate.Value.Date > DateTime.UtcNow)
                {
                    parts.Add(string.Format(CultureInfo.CurrentUICulture, oneParameterFormat, T("Not Overdue").Text));
                }

                parts.Add(string.Format(CultureInfo.CurrentUICulture, format, T("Due Date").Text, model.DueDate.Value.ToString("yyyy/MM/dd")));
            }

            // Overdue
            if (model.Overdue)
            {
                parts.Add(string.Format(CultureInfo.CurrentUICulture, oneParameterFormat, T("Overdue").Text));
            }

            // IncludeAllVisibleItemsBySelectedGroupsAndUsers
            if (model.IncludeAllVisibleItemsBySelectedGroupsAndUsers)
            {
                parts.Add(string.Format(CultureInfo.CurrentUICulture, oneParameterFormat, this.T("All visible Tickets by the selected agents or groups").Text));
            }

            // Term
            if (!string.IsNullOrEmpty(model.Term))
            {
                parts.Add(string.Format(CultureInfo.CurrentUICulture, format, T("Term").Text, model.Term));
            }

            return string.Join(", ", parts);
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
