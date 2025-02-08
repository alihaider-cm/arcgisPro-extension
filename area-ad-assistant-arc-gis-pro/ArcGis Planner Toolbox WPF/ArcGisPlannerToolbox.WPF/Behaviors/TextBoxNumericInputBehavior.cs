using Microsoft.Xaml.Behaviors;
using System.Windows.Controls;
using System.Windows.Input;

namespace ArcGisPlannerToolbox.WPF.Behaviors;

public class TextBoxNumericInputBehavior : Behavior<TextBox>
{
    protected override void OnAttached()
    {
        AssociatedObject.KeyDown += AssociatedObject_KeyDown;
    }
    protected override void OnDetaching()
    {
        AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
    }
    private void AssociatedObject_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if ((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) &&
            e.Key is >= Key.D0 and <= Key.D9)
            e.Handled = true;
        else if (e.Key is >= Key.D0 and <= Key.D9 or >= Key.NumPad0 and <= Key.NumPad9)
            e.Handled = false;
        else
            e.Handled = true;
    }
}
