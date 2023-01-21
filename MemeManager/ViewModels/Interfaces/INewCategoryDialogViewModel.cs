using HanumanInstitute.MvvmDialogs;

namespace MemeManager.ViewModels.Interfaces;

public interface INewCategoryDialogViewModel : IModalDialogViewModel, ICloseable
{
    string Text { get; set; }
}
