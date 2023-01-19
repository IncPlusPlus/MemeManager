using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MemeManager.Views;

public partial class NewCategoryDialog : Window
{
    public NewCategoryDialog()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        var textBox = this.FindControl<TextBox>("CategoryName");
        if (textBox != null)
        {
            // When the NewCategoryDialog opens, put the cursor inside the TextBox in the AutoCompleteBox
            textBox.AttachedToVisualTree += (s, e) => textBox.Focus();
        }
    }
}

