using MemeManager.Extensions;

namespace MemeManager;

public class ViewLocator : ViewLocatorBase
{
    /// <inheritdoc />
    protected override string GetViewName(object viewModel) =>
        viewModel.GetType().FullName!.ReplaceLastOccurrence("ViewModel", "");
}
