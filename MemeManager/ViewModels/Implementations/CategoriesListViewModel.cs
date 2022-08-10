using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MemeManager.Models;
using MemeManager.Persistence;
using MemeManager.Persistence.Entity;
using MemeManager.Services.Abstractions;
using MemeManager.ViewModels.Interfaces;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;

namespace MemeManager.ViewModels.Implementations;

public class CategoriesListViewModel : ReactiveObject, ICategoriesListViewModel
{
    private Category? _selectedCategory;
    private FolderIconConverter? _folderIconConverter;
    private ICategoryService _categoryService;
    private IFilterObserverService _filterObserver;

    public CategoriesListViewModel(IFilterObserverService filterObserverService, ICategoryService categoryService)
    {
        _filterObserver = filterObserverService;
        _categoryService = categoryService;
        List<CategoryTreeNodeModel> rootCategories;
        var assetLoader = AvaloniaLocator.Current.GetService<IAssetLoader>();

            if (assetLoader is not null)
            {
                using (var fileStream = assetLoader.Open(new Uri("avares://MemeManager/Assets/file.png")))
                using (var folderStream = assetLoader.Open(new Uri("avares://MemeManager/Assets/folder.png")))
                using (var folderOpenStream =
                       assetLoader.Open(new Uri("avares://MemeManager/Assets/folder-open.png")))
                {
                    var fileIcon = new Bitmap(fileStream);
                    var folderIcon = new Bitmap(folderStream);
                    var folderOpenIcon = new Bitmap(folderOpenStream);

                    _folderIconConverter = new FolderIconConverter(fileIcon, folderOpenIcon, folderIcon);
                }
            }

            Source = new HierarchicalTreeDataGridSource<CategoryTreeNodeModel>(Array.Empty<CategoryTreeNodeModel>())
            {
                Columns =
                {
                    new HierarchicalExpanderColumn<CategoryTreeNodeModel>(
                        new TemplateColumn<CategoryTreeNodeModel>(
                            "Categories",
                            new FuncDataTemplate<CategoryTreeNodeModel>(FileNameTemplate, true),
                            new GridLength(1, GridUnitType.Star),
                            new ColumnOptions<CategoryTreeNodeModel>
                            {
                                CompareAscending = CategoryTreeNodeModel.SortAscending(x => x.Name),
                                CompareDescending = CategoryTreeNodeModel.SortDescending(x => x.Name),
                            })
                        {
                            IsTextSearchEnabled = true,
                            TextSearchValueSelector = x => x.Name
                        },
                        x => x.Children,
                        x => x.HasChildren,
                        x => x.IsExpanded),
                    new TextColumn<CategoryTreeNodeModel,int>(
                        "Number of memes",
                        x => x.MemeCount
                    )
                }
            };

            Source.RowSelection!.SingleSelect = false;
            Source.RowSelection.SelectionChanged += SelectionChanged;

            rootCategories=_categoryService.GetTopLevelCategories().Select(i => new CategoryTreeNodeModel(i)).ToList();

            // Get the top level categories and set the TreeDataGrid's source to them
            // using (var context = new MemeManagerContext())
            // {
            //     rootCategories = context.Categories
            //         /*
            //          * Loading just the categories is a simple operation and we don't want to pepper the DB with queries
            //          * when we want to expand a bunch of categories so we explicitly/eagerly load the children of the
            //          * categories.
            //          * See https://docs.microsoft.com/en-us/ef/core/querying/related-data/ for more info
            //          */
            //         .Include(c => c.Children)
            //         .Where(category => category.Parent==null)
            //         .Select(i => new CategoryTreeNodeModel(i)).ToList();
            // }
            Source.Items = rootCategories;

            Source.RowSelection.SelectionChanged += OnSelectionChanged;
    }

    public Category? SelectedCategory
    {
        get => _selectedCategory;
        set => this.RaiseAndSetIfChanged(ref _selectedCategory, value);
    }

    public void ClearCategorySelection()
    {
        Source.RowSelection?.Clear();
    }

    public void OnSelectionChanged(object? sender, TreeSelectionModelSelectionChangedEventArgs<CategoryTreeNodeModel> args)
    {
        _filterObserver.CurrentCategory = args.SelectedItems.FirstOrDefault()?.Category;
    }
    
    public HierarchicalTreeDataGridSource<CategoryTreeNodeModel> Source { get; }

        private IControl FileNameTemplate(CategoryTreeNodeModel node, INameScope ns)
        {
            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new Image
                    {
                        [!Image.SourceProperty] = new MultiBinding
                        {
                            Bindings =
                            {
                                // new Binding(nameof(node.IsDirectory)),
                                new Binding(nameof(node.IsExpanded)),
                            },
                            Converter = _folderIconConverter,
                        },
                        Margin = new Thickness(0, 0, 4, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                    },
                    new TextBlock
                    {
                        [!TextBlock.TextProperty] = new Binding(nameof(CategoryTreeNodeModel.Name)),
                        VerticalAlignment = VerticalAlignment.Center,
                    }
                }
            };
        }

        private void SelectionChanged(object? sender, TreeSelectionModelSelectionChangedEventArgs<CategoryTreeNodeModel> e)
        {
            var selectedCategory = Source.RowSelection?.SelectedItem?.Category;
            this.RaiseAndSetIfChanged(ref _selectedCategory, selectedCategory, nameof(SelectedCategory));

            foreach (var i in e.DeselectedItems)
                System.Diagnostics.Trace.WriteLine($"Deselected '{i?.Category}'");
            foreach (var i in e.SelectedItems)
                System.Diagnostics.Trace.WriteLine($"Selected '{i?.Category}'");
        }

        private class FolderIconConverter : IMultiValueConverter
        {
            private readonly Bitmap _file;
            private readonly Bitmap _folderExpanded;
            private readonly Bitmap _folderCollapsed;

            public FolderIconConverter(Bitmap file, Bitmap folderExpanded, Bitmap folderCollapsed)
            {
                _file = file;
                _folderExpanded = folderExpanded;
                _folderCollapsed = folderCollapsed;
            }

            public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
            {
                if (values.Count == 2 &&
                    values[0] is bool isDirectory &&
                    values[1] is bool isExpanded)
                {
                    if (!isDirectory)
                        return _file;
                    else
                        return isExpanded ? _folderExpanded : _folderCollapsed;
                }

                return null;
            }
        }
}