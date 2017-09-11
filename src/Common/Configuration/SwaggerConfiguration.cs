using JetBrains.Annotations;

namespace Fileharbor.Common.Configuration
{
    [UsedImplicitly]
    public class SwaggerConfiguration
    {
        public string Title { get; set; }

        public string SwaggerGenEndpoint { get; set; }
    }
}
