using Avalonia.Controls;
using Avalonia.Input;
using MemeManager.DependencyInjection;
using Microsoft.Extensions.Logging;
using Splat;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace MemeManager.Views.Extras;

public abstract class DraggableControl : UserControl
{
    private ILogger _log = Locator.Current.GetRequiredService<ILogger>();
    // private Point? _initialPointerPosition;
    // private bool _dragStarted;
    // private DataObject dragData;

    public DraggableControl()
    {
        PointerPressed += DoDrag;
        /*
         * Everything commented out below is how this would be implemented if issue #34 wasn't in the way (this includes
         * the commented out methods as well). I'm keeping this around so I've got an alternate solution if I can manage to solve that issue.
         */

        // Empty object so the field is always at least initialized
        // dragData = new DataObject();
        // PointerPressed += OnPointerPressed;
        // PointerMoved += OnPointerMoved;
        // PointerReleased += OnPointerReleased;
    }

    // private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    // {
    //     _dragStarted = false;
    //     _initialPointerPosition = null;
    // }

    /*
     * Disabled until I can solve #34
     */
    // private async void OnPointerMoved(object? sender, PointerEventArgs e)
    // {
    //     if (_initialPointerPosition != null && !_dragStarted)
    //     {
    //         var initialPosition = (Point)_initialPointerPosition;
    //         // Calculate distance between initial and updated mouse position.
    //         // Modeled from https://stackoverflow.com/a/46954394/1687436
    //         Vector movedDistance = (initialPosition - e.GetPosition(this));
    //         var length = movedDistance.Length;
    //         if (length > 3)
    //         {
    //             _dragStarted = true;
    //             // var dragData = CreateDataObject();
    //             // Factory is from the sample app https://github.com/AvaloniaUI/Avalonia/blob/f63ed9cf6bf19b8490c55d2f9bf6cc6b9ae05fb3/samples/ControlCatalog/Pages/DragAndDropPage.xaml.cs#L35.
    //             // I'd have to implement a function that actually adds the required data to a DataObject.
    //             // factory(dragData);
    //             var result = await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Copy);
    //             switch (result)
    //             {
    //                 case DragDropEffects.Move:
    //                     Console.WriteLine();
    //                     // dragState.Text = "Data was moved";
    //                     break;
    //                 case DragDropEffects.Copy:
    //                     Console.WriteLine();
    //                     // dragState.Text = "Data was copied";
    //                     break;
    //                 case DragDropEffects.Link:
    //                     Console.WriteLine();
    //                     // dragState.Text = "Data was linked";
    //                     break;
    //                 case DragDropEffects.None:
    //                     Console.WriteLine();
    //                     // dragState.Text = "The drag operation was canceled";
    //                     break;
    //                 default:
    //                     Console.WriteLine();
    //                     // dragState.Text = "Unknown result";
    //                     break;
    //             }
    //         }
    //     }
    // }

    // From the sample app https://github.com/AvaloniaUI/Avalonia/blob/f63ed9cf6bf19b8490c55d2f9bf6cc6b9ae05fb3/samples/ControlCatalog/Pages/DragAndDropPage.xaml.cs.
    // Calling DragDrop.DoDragDrop immediately causes issue #34 but is the only way way I can implement this behavior for now.
    private async void DoDrag(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        var pointProps = e.GetCurrentPoint(this).Properties;
        if (!pointProps.IsLeftButtonPressed)
        {
            _log.LogTrace("Not starting DoDrag because pointer event was not left mouse button. Pointer update event was {UpdateType}", pointProps.PointerUpdateKind);
        }
        else
        {
            _log.LogTrace("Call to DoDrag has begun");
            var dragData = CreateDataObject();
            var result = await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Copy);
            _log.LogTrace("Drag & Drop operation completed with DragDropEffect.{DragDropEffect}", result);
        }
    }

    // private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    // {
    //     dragData = CreateDataObject();
    //     _initialPointerPosition = e.GetPosition(this);
    // }

    protected abstract DataObject CreateDataObject();
}
