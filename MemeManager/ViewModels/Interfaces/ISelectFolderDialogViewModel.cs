using HanumanInstitute.MvvmDialogs;

namespace MemeManager.ViewModels.Interfaces;

public interface IImportFolderDialogViewModel : IModalDialogViewModel, ICloseable
{
    string? Path { get; }
}
