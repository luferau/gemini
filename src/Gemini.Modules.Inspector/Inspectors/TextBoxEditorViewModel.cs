using System.Windows.Controls;
using System.Windows.Input;

namespace Gemini.Modules.Inspector.Inspectors
{
    public class TextBoxEditorViewModel<T> : EditorBase<T>, ILabelledInspector
    {
        public void ValueChanged(Key key, TextBox textBox)
        {
            if (key != Key.Enter) return;

            textBox?.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }
    }
}