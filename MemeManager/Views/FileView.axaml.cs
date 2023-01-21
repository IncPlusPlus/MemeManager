using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using MemeManager.Persistence.Entity;
using MemeManager.ViewModels;
using MemeManager.Views.Extras;

namespace MemeManager.Views;

public partial class FileView : DraggableControl
{
    public const string MemeIdListFormat = "application/xxx-mememanager-meme-id-list";
    public FileView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override DataObject CreateDataObject()
    {
        var listBoxItem = this.GetLogicalParent();
        var thisFileViewVm = (listBoxItem.GetLogicalChildren().First() as FileView)?.DataContext as FileViewModel ?? throw new InvalidOperationException();
        var listbox = listBoxItem?.GetLogicalParent();
        var selection = (AvaloniaList<object>?)((ListBox?)listbox)?.SelectedItems;
        var memes = selection?.Select(item => ((FileViewModel)item).Meme).ToList() ?? new List<Meme>();
        /*
         * Due to issue #34, a user has to click once on a meme before clicking and dragging. If they simply start
         * clicking and dragging a meme without having clicked on it once prior, the ListBox selection won't have
         * changed yet and the DataObject would be empty. To make the irritating selection behavior slightly more
         * user friendly, we check if the object the user started dragging is included in the selection.
         *
         * The only drawback to this behavior is that if the user selects multiple memes and then starts dragging a
         * meme that wasn't part of the selection, that meme will be included among the selected memes. This behavior
         * may not be obvious to end-users but it's better than the behavior beforehand.
         */
        if (!memes.Contains(thisFileViewVm.Meme))
        {
            memes.Add(thisFileViewVm.Meme);
        }
        var data = new DataObject();
        // The DataObject also contains all the file paths for the dragged object. This is so that Discord will let us drop a meme onto its canvas.
        data.Set(DataFormats.FileNames, memes.Select(m => m.Path));
        data.Set(MemeIdListFormat, memes);
        return data;
    }
}
