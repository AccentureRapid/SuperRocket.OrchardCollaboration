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

    public class DiscussionController : ApiController
    {

        private static readonly char[] separator = new[] { '{', '}', ',' };
        private readonly ISiteService _siteService;
        private readonly IMilestoneService _milestoneService;

        public DiscussionController(
            IOrchardServices _services,
            ISiteService siteService,
            IMilestoneService milestoneService
            )
        {


            Services = _services;
            _siteService = siteService;
            _milestoneService = milestoneService;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public IOrchardServices Services { get; set; }

        /// <summary>
        /// GET api/Discussion/GetOpenMilestones?projectId=110
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetDiscussions(int projectId)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                //TODO find how to query Discussions
                var result = _milestoneService.GetOpenMilestones(projectId).Select(
                    x => new
                    {
                        x.Id,
                        x.As<TitlePart>().Title,
                        x.As<MilestonePart>().Record.StartTime,
                        x.As<MilestonePart>().Record.EndTime,
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
                Logger.Error("Error occurs when GetOpenMilestones :" + ex.StackTrace);
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
