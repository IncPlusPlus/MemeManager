using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
using MemeManager.Services.Abstractions;
using MemeManager.ViewModels.Interfaces;
using ReactiveUI;

namespace MemeManager.ViewModels.Implementations;

public class CategoriesListViewModel : ReactiveObject, ICategoriesListViewModel
{
    private ICategoryService _categoryService;
    private IFilterObserverService _filterObserver;
    private FolderIconConverter? _folderIconConverter;

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
                        "Category",
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
                new TextColumn<CategoryTreeNodeModel, int>(
                    "Number of memes",
                    x => x.MemeCount
                )
            }
        };

        // Although we won't properly display the right memes for multiple categories, allowing multiselect
        // will let users delete categories in bulk in the future
        Source.RowSelection!.SingleSelect = false;
        Source.RowSelection.SelectionChanged += OnSelectionChanged;

        // TODO: Implement a way to show uncategorized memes. Maybe a default row in the categories table.
        rootCategories = _categoryService.GetTopLevelCategories().Select(i => new CategoryTreeNodeModel(i)).ToList();

        Source.Items = rootCategories;
    }

    public HierarchicalTreeDataGridSource<CategoryTreeNodeModel> Source { get; }

    public void ClearCategorySelection()
    {
        Source.RowSelection?.Clear();
    }

    public void OnSelectionChanged(object? sender,
        TreeSelectionModelSelectionChangedEventArgs<CategoryTreeNodeModel> args)
    {
        _filterObserver.CurrentCategory = args.SelectedItems.FirstOrDefault()?.Category;
    }

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

    private class FolderIconConverter : IMultiValueConverter
    {
        private readonly Bitmap _file;
        private readonly Bitmap _folderCollapsed;
        private readonly Bitmap _folderExpanded;

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