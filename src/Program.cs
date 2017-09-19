using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Fileharbor
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        private static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureServices(Startup.ConfigureStartupServices)
                .UseStartup<Startup>()
                .Build();
        }
    }
}