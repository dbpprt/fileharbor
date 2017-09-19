using System.Collections.Generic;
using JetBrains.Annotations;

namespace Fileharbor.Common.Configuration
{
    [UsedImplicitly]
    public class LanguageConfiguration
    {
        public IEnumerable<int> AvailableLanguages { get; set; }
    }
}