using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using MemeManager.ViewModels.Implementations;

namespace MemeManager.Views.Main;

public partial class StatusBarView : ReactiveUserControl<StatusBarViewModel>

{
    public StatusBarView()
    {
        this.InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
