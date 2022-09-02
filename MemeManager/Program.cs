using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using MemeManager.DependencyInjection;
using Microsoft.Extensions.Logging;
using Splat;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace MemeManager
{
    class Program
    {
        private const int TimeoutSeconds = 3;
        private static ILogger? log;

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            var mutex = new Mutex(false, typeof(Program).FullName);

            try
            {
                if (!mutex.WaitOne(TimeSpan.FromSeconds(TimeoutSeconds), true))
                {
                    return;
                }

                SubscribeToDomainUnhandledEvents();
                RegisterDependencies();

                log = Locator.Current.GetRequiredService<ILogger>();
                log.LogInformation("MemeManager starting up!");

                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        private static void SubscribeToDomainUnhandledEvents() =>
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var ex = (Exception)args.ExceptionObject;

                log?.LogCritical("Unhandled application error: {Exception}", ex);
            };

        private static void RegisterDependencies() =>
            Bootstrapper.Register(Locator.CurrentMutable, Locator.Current);

        // Avalonia configuration, don't remove; also used by visual designer.
        private static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}
