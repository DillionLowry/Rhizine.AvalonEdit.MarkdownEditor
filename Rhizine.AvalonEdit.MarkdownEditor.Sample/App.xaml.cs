using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Windows;

namespace Rhizine.AvalonEdit.MarkdownEditor.Sample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost _host;

        public static T GetService<T>() where T : class
        {
            var app = App.Current as App;
            if (app == null || app._host == null)
            {
                throw new InvalidOperationException("App or _host is not initialized.");
            }
            return app._host.Services.GetService(typeof(T)) as T;
        }

        protected void OnStartup(object sender, StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
            _host = Host.CreateDefaultBuilder()
                .UseSerilog(Log.Logger)
                .ConfigureServices(ConfigureServices)
                .Build();
            _host.Start();
        }

        private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddSingleton<MarkdownService>();
            services.AddSingleton<MainWindow>();

            services.AddTransient<MainViewModel>();
            services.AddTransient<MainPage>();
        }
    }
}