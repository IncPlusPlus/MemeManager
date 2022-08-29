using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MemeManager.DependencyInjection;
using MemeManager.ViewModels.Interfaces;
using MemeManager.Views;
using Splat;

namespace MemeManager
{
    public class App : Application
    {
        private static IClassicDesktopStyleApplicationLifetime? _lifetime;
        public static event EventHandler<ControlledApplicationLifetimeExitEventArgs>? ShuttingDown;
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                DataContext = GetRequiredService<IMainWindowViewModel>();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = DataContext
                };
                _lifetime = desktop;
                _lifetime.Exit += OnExit;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private static T GetRequiredService<T>() => Locator.Current.GetRequiredService<T>();

        private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            var handler = ShuttingDown;
            handler?.Invoke(this, e);
            if (_lifetime != null)
                _lifetime.Exit -= OnExit;
        }
    }
}
