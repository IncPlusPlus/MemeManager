using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MemeManager.Views.Main;

public partial class CategoriesListView : UserControl
{
    public CategoriesListView()
    {
        this.InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }    
}