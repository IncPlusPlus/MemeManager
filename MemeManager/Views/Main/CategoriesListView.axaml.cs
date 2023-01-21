using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using MemeManager.Models;
using MemeManager.Persistence.Entity;
using MemeManager.ViewModels.Implementations;

namespace MemeManager.Views.Main;

public partial class CategoriesListView : UserControl
{
    public CategoriesListView()
    {
        this.InitializeComponent();

        AddHandler(DragDrop.DropEvent, Drop);
        AddHandler(DragDrop.DragOverEvent, DragOver);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private static void DragOver(object? sender, DragEventArgs e)
    {
        if (e.Source is TextBlock categoryTextBlock)
        {
            // Our TextBlock parent is StackPanel whose parent is a TreeViewItem (per CategoriesListView.axaml)
            if (categoryTextBlock.GetLogicalParent().GetLogicalParent() is TreeViewItem treeViewItem)
            {
                // var treeNodeVm = treeViewItem.DataContext as CategoryTreeNodeModel;
                e.DragEffects = e.DragEffects & (DragDropEffects.Copy);
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
            }
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }

        // Only allow memes to be dropped here
        if (!e.Data.Contains(FileView.MemeIdListFormat))
            e.DragEffects = DragDropEffects.None;
    }

    private static void Drop(object? sender, DragEventArgs e)
    {
        if (e.Source is TextBlock categoryTextBlock)
        {
            // Our TextBlock's parent is StackPanel whose parent is a TreeViewItem (per CategoriesListView.axaml)
            if (categoryTextBlock.GetLogicalParent().GetLogicalParent() is TreeViewItem treeViewItem)
            {
                // Check if an ancestor of this TreeViewItem is an instance of CategoriesListView
                // The braces are a null pattern check. See https://stackoverflow.com/a/71849657/1687436
                if (treeViewItem.FindLogicalAncestorOfType<CategoriesListView>() is { } categoriesList)
                {
                    e.DragEffects = e.DragEffects & (DragDropEffects.Copy);
                    var treeNodeVm = treeViewItem.DataContext as CategoryTreeNodeModel;
                    var categoriesListVm = categoriesList.DataContext as CategoriesListViewModel;
                    var draggedMemes = e.Data.Get(FileView.MemeIdListFormat) as IEnumerable<Meme>;
                    // Change the category of these memes
                    categoriesListVm?.SetCategory(treeNodeVm?.Category ?? throw new InvalidOperationException(),
                        draggedMemes ?? throw new InvalidOperationException());
                }
                else
                {
                    e.DragEffects = DragDropEffects.None;
                }
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
            }
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }

        // Only allow memes to be dropped here
        if (!e.Data.Contains(FileView.MemeIdListFormat))
            e.DragEffects = DragDropEffects.None;
    }
}
