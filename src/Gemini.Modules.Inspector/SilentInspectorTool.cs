#if !MODULE_INSPECTOR
using System;
using System.ComponentModel.Composition;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace Gemini.Modules.Inspector
{
    /// <summary>
    /// Stands in for the inspector pane in a build that leaves the Inspector module out. The model
    /// designer, the map designer and the script editor publish their selection through
    /// <see cref="IInspectorTool"/>, so the contract must stay resolvable; there is no pane, and the
    /// selection is kept but never shown.
    /// </summary>
    [Export(typeof(IInspectorTool))]
    public sealed class SilentInspectorTool : Tool, IInspectorTool
    {
        private IInspectableObject _selectedObject;

        public event EventHandler SelectedObjectChanged;

        public override PaneLocation PreferredLocation
        {
            get { return PaneLocation.Right; }
        }

        public override double PreferredWidth
        {
            get { return 300; }
        }

        public IInspectableObject SelectedObject
        {
            get { return _selectedObject; }
            set
            {
                _selectedObject = value;
                var handler = SelectedObjectChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
        }
    }
}
#endif
