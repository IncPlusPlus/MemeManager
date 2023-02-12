using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MemeManager.Views;

public partial class ImportFolderDialog : Window
{
    public ImportFolderDialog()
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

