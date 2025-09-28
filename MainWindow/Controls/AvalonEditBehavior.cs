using ICSharpCode.AvalonEdit;
using Microsoft.Xaml.Behaviors;
using System.Windows;

namespace SFTemplateGenerator.MainWindow.Controls
{
    public sealed class AvalonEditBehavior : Behavior<TextEditor>
    {
        public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register("Text", typeof(string), typeof(AvalonEditBehavior), new PropertyMetadata(string.Empty, OnTextChanged));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as AvalonEditBehavior;
            if (behavior.AssociatedObject != null)
            {
                behavior.AssociatedObject.Document.Text = e.NewValue as string ?? string.Empty;
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
            {
                AssociatedObject.TextChanged += OnTextChanged;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
            {
                AssociatedObject.TextChanged -= OnTextChanged;
            }
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            Text = AssociatedObject.Document.Text;
        }
    }
}
