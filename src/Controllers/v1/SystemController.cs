using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Fileharbor.Common;
using Fileharbor.Common.Configuration;
using Fileharbor.ViewModels.v1.System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;

namespace Fileharbor.Controllers.v1
{
    [Route("api/v1/system")]
    public class SystemController : BaseController
    {
        private readonly IOptions<LanguageConfiguration> _languageConfiguration;

        public SystemController(CurrentPrincipal currentPrincipal,
            IOptions<LanguageConfiguration> languageConfiguration) : base(currentPrincipal)
        {
            _languageConfiguration = languageConfiguration;
        }

        [HttpGet]
        [Route("version")]
        public string GetVersion()
        {
            return PlatformServices.Default.Application.ApplicationVersion;
        }

        [HttpGet]
        [Route("languages")]
        [AllowAnonymous]
        public IEnumerable<AvailableLanguagesResponse> GetAvailableLanguages()
        {
            return _languageConfiguration.Value.AvailableLanguages.Select(_ => new CultureInfo(_)).Select(_ =>
                new AvailableLanguagesResponse
                {
                    Name = _.Name,
                    Lcid = _.LCID,
                    LocalName = _.DisplayName
                });
        }
    }
}