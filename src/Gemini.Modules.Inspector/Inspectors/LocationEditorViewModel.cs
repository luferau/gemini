using Telerik.Windows.Controls.Map;

namespace Gemini.Modules.Inspector.Inspectors
{
    public class LocationEditorViewModel : EditorBase<Location>, ILabelledInspector
    {
        public double Latitude
        {
            get => Value.Latitude;
            set
            {
                Value = new Location(value, Value.Longitude);
                NotifyOfPropertyChange(() => Latitude);
            }
        }

        public double Longitude
        {
            get => Value.Longitude;
            set
            {
                Value = new Location(Value.Latitude, value);
                NotifyOfPropertyChange(() => Longitude);
            }
        }

        public override void NotifyOfPropertyChange(string propertyName)
        {
            if (propertyName == "Value")
            {
                NotifyOfPropertyChange(() => Latitude);
                NotifyOfPropertyChange(() => Longitude);
            }
            base.NotifyOfPropertyChange(propertyName);
        }
    }
}
