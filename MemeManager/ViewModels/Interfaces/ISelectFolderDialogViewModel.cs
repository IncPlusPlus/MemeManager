using HanumanInstitute.MvvmDialogs;

namespace MemeManager.ViewModels.Interfaces;

public interface ISelectFolderDialogViewModel : IModalDialogViewModel, ICloseable
{
    string? Path { get; }
}
