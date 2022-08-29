using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MemeManager.Views.Main;

public partial class SearchbarView : UserControl
{
    public SearchbarView()
    {
        this.InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
