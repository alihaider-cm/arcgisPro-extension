using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ArcGisPlannerToolbox.WPF.Behaviors;

public class TextBoxKeyStroke : Behavior<TextBox>
{
    public Key Key
    {
        get { return (Key)GetValue(TargetKeyProperty); }
        set { SetValue(TargetKeyProperty, value); }
    }
    public static readonly DependencyProperty TargetKeyProperty =
        DependencyProperty.Register(nameof(Key), typeof(Key), typeof(TextBoxKeyStroke), new PropertyMetadata(default));

    public ICommand Command
    {
        get { return (ICommand)GetValue(CommandProperty); }
        set { SetValue(CommandProperty, value); }
    }
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(TextBoxKeyStroke), new PropertyMetadata(default));

    protected override void OnAttached()
    {
        AssociatedObject.KeyDown += AssociatedObject_KeyDown;
    }

    private void AssociatedObject_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key)
            if (Command is not null)
                Command.Execute(null);
    }

    protected override void OnDetaching()
    {
        AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
    }
}
