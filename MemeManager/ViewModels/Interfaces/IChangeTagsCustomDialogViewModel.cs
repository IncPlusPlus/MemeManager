using System.Collections.Generic;
using HanumanInstitute.MvvmDialogs;
using MemeManager.Persistence.Entity;

namespace MemeManager.ViewModels.Interfaces;

public interface IChangeTagsCustomDialogViewModel : IModalDialogViewModel, ICloseable
{
    IEnumerable<Meme> TargetMemes { get; set; }
}
