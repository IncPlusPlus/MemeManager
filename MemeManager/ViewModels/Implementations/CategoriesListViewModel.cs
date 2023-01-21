using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DynamicData;
using HanumanInstitute.MvvmDialogs;
using MemeManager.DependencyInjection;
using MemeManager.Extensions;
using MemeManager.Models;
using MemeManager.Persistence.Entity;
using MemeManager.Services.Abstractions;
using MemeManager.ViewModels.Interfaces;
using ReactiveUI;
using Splat;

namespace MemeManager.ViewModels.Implementations;

public class CategoriesListViewModel : ReactiveObject, ICategoriesListViewModel
{
    private readonly IObservable<EventPattern<DbChangeEventArgs>> _dbChangedObservable;
    private readonly IDbChangeNotifier _dbChangeNotifier;
    private readonly IDialogService _dialogService;
    private ObservableCollection<Category> _categories;
    private ICategoryService _categoryService;
    private IFilterObserverService _filterObserver;
    private FolderIconConverter? _folderIconConverter;
    private IMemeService _memeService;
    private ReadOnlyObservableCollection<CategoryTreeNodeModel> _nodeViewModels;

    public CategoriesListViewModel(IDialogService dialogService, IFilterObserverService filterObserverService, IDbChangeNotifier dbChangeNotifier,
        ICategoryService categoryService, IMemeService memeService)
    {
        _dialogService = dialogService;
        _filterObserver = filterObserverService;
        _dbChangeNotifier = dbChangeNotifier;
        _categoryService = categoryService;
        _memeService = memeService;
        var assetLoader = AvaloniaLocator.Current.GetService<IAssetLoader>();

        SelectedNodes = new ObservableCollection<CategoryTreeNodeModel>();

        // TODO: Determine how much of this icon code is still needed. It was carried over from the example I made this from.
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

        _dbChangedObservable = Observable.FromEventPattern<EventHandler<DbChangeEventArgs>, DbChangeEventArgs>(
            handler => _dbChangeNotifier.EntitiesUpdated += handler,
            handler => _dbChangeNotifier.EntitiesUpdated -= handler);

        _categories = new ObservableCollection<Category>(_categoryService.GetTopLevelCategories());

        var models = new SourceCache<Category, int>(c => c.Id);

        // Modeled from https://stackoverflow.com/a/53874449/1687436
        var transformed = models
            .Connect()
            /*
             * Avoids recreating the TreeNodeViewModel by instead changing its category property. While this might not
             * change the value (since the EF proxy object will be the same instance just with updated values), it does
             * provide two nice features.
             *
             * 1. When the models variable is updated (through models.AddOrUpdate()), the existing TreeNodeViewModels
             * won't be replaced.
             * 2. We can perform hacky solutions like calling other methods inside the Category setter if we absolutely
             * have to.
             */
            .TransformWithInlineUpdate(u => new CategoryTreeNodeModel(u, categoryService),
                (previousViewModel, updatedCategory) =>
                {
                    previousViewModel.Category = updatedCategory;
                })
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _nodeViewModels)
            .Subscribe();

        models.AddOrUpdate(_categoryService.GetTopLevelCategories());

        this.WhenAnyObservable(x => x._dbChangedObservable)
            .Select(x => x.EventArgs)
            .Where(x => x.TypeRelevant(typeof(Category)))
            .Subscribe(x =>
            {
                var topLevelCategories = _categoryService.GetTopLevelCategories();
                // Determine if there have been any removed categories
                var removedCategories = NodeViewModels.Where(model =>
                    !topLevelCategories.Select(c => c.Id).Contains(model.Category.Id)).Select(n => n.Category);
                models.Remove(removedCategories);
                // In-place update the existing category models
                models.AddOrUpdate(topLevelCategories);
            });

        SelectedNodes.CollectionChanged += TreeView_OnSelectionChanged;
        NewCategoryCommand = ReactiveCommand.CreateFromTask(CreateNewCategory);
        DeleteCategoryCommand = ReactiveCommand.Create<Collection<CategoryTreeNodeModel>>(DeleteCategories);
        NewSubcategoryCommand = ReactiveCommand.CreateFromTask<CategoryTreeNodeModel>(CreateSubcategory);
    }

    public ReadOnlyObservableCollection<CategoryTreeNodeModel> NodeViewModels { get => _nodeViewModels; }
    public ObservableCollection<Category> Categories { get { return _categories; } }
    public ObservableCollection<CategoryTreeNodeModel> SelectedNodes { get; }
    public ReactiveCommand<Unit, Unit> NewCategoryCommand { get; }
    public ReactiveCommand<Collection<CategoryTreeNodeModel>, Unit> DeleteCategoryCommand { get; }
    public ReactiveCommand<CategoryTreeNodeModel, Unit> NewSubcategoryCommand { get; }

    public void ClearCategorySelection()
    {
        SelectedNodes.Clear();
    }

    private void TreeView_OnSelectionChanged(object? sender,
        NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
    {
        if (notifyCollectionChangedEventArgs.NewItems != null)
        {
            var firstItem = notifyCollectionChangedEventArgs.NewItems[0] as CategoryTreeNodeModel;
            _filterObserver.CurrentCategory = firstItem?.Category;
        }
        else
        {
            _filterObserver.CurrentCategory = null;
        }
    }

    public void SetCategory(Category category, IEnumerable<Meme> draggedMemes)
    {
        draggedMemes.ForEach(m => _memeService.SetCategory(m, category));
    }

    private async Task CreateNewCategory()
    {
        var dialogViewModel = _dialogService.CreateViewModel<INewCategoryDialogViewModel>();
        // Because I'm reusing a viewmodel, the existing instance could have text from previous uses. Clear it.
        dialogViewModel.Text = "";

        // The ownerViewModel is required to be a be the DataContext of a Window. I can't use 'this' like in the examples because MemesListViewModel is the DataContext of MemesListView which isn't a Window.
        var success = await _dialogService.ShowDialogAsync<NewCategoryDialog>(Locator.Current.GetRequiredService<IMainWindowViewModel>() as MainWindowViewModel, dialogViewModel).ConfigureAwait(true);
        if (success == true)
        {
            var name = dialogViewModel.Text;
            // TODO: This needs to be initialized with the correct parent directory
            _categoryService.Create(new Category() { Name = name });
        }
    }

    private async Task CreateSubcategory(CategoryTreeNodeModel node)
    {
        var parentCategory = node.Category;
        var dialogViewModel = _dialogService.CreateViewModel<INewCategoryDialogViewModel>();
        // Because I'm reusing a viewmodel, the existing instance could have text from previous uses. Clear it.
        dialogViewModel.Text = "";

        // The ownerViewModel is required to be a be the DataContext of a Window. I can't use 'this' like in the examples because MemesListViewModel is the DataContext of MemesListView which isn't a Window.
        var success = await _dialogService.ShowDialogAsync<NewCategoryDialog>(Locator.Current.GetRequiredService<IMainWindowViewModel>() as MainWindowViewModel, dialogViewModel).ConfigureAwait(true);
        if (success == true)
        {
            var name = dialogViewModel.Text;
            // TODO: This needs to be initialized with the correct parent directory
            _categoryService.Create(new Category() { Name = name, Parent = parentCategory });
        }
    }

    private void DeleteCategories(Collection<CategoryTreeNodeModel> nodeList)
    {
        _categoryService.Delete(nodeList.Select(nodeModel => nodeModel.Category).ToArray());
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
