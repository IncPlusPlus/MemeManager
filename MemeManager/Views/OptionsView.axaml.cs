using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MemeManager.Views;

public partial class OptionsView : UserControl
{
    public OptionsView()
    {
        this.InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}