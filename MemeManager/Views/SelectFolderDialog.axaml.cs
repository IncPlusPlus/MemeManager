using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MemeManager.Views;

public partial class SelectFolderDialog : Window
{
    public SelectFolderDialog()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

