using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MemeManager.Views.Main;

public partial class MemeListView : UserControl
{
public MemeListView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }    
}