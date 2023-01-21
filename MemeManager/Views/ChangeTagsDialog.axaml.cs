using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MemeManager.Views;

public partial class ChangeTagsDialog : Window
{
    public ChangeTagsDialog()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        var autoCompleteBox = this.FindControl<AutoCompleteBox>("TagSuggestionsBox");
        if (autoCompleteBox != null)
        {
            // When the ChangeTagsDialog opens, put the cursor inside the TextBox in the AutoCompleteBox
            autoCompleteBox.AttachedToVisualTree += (s, e) => autoCompleteBox.Focus();
        }
    }
}
