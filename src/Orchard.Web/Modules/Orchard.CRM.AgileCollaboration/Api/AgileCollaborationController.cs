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
        public AgileCollaborationController(
            IContentManager contentManager,
            IContentTypesService contentTypesService,
            IHtmlModuleService moduleService,
            IUserEventHandler userEventHandler,
            IMembershipService membershipService,
            IProjectService projectService,
            IOrchardServices _services
            )
        {
            _contentManager = contentManager;
            _contentTypesService = contentTypesService;
            _moduleService = moduleService;
            _userEventHandler = userEventHandler;
            _membershipService = membershipService;
            _projectService = projectService;
            Services = _services;

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
        [Route("GetContentTypeDefinition")]
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
        [Route("ContentTypes")]
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
        [Route("Login")]
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
        [Route("GetMyProjects")]
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
