using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Xml.Serialization;
using Caliburn.Micro;

namespace Gemini.Modules.Inspector.Inspectors
{
    public class InspectorList : PropertyChangedBase
    {
        private ObservableCollection<string> _listCollection = new ObservableCollection<string>();
        [XmlIgnore]
        public ObservableCollection<string> ListCollection
        {
            get => _listCollection;
            set
            {
                _listCollection = value;
                NotifyOfPropertyChange(() => ListCollection);
            }
        }

        private string _selectedValue;
        public string SelectedValue
        {
            get => _selectedValue;
            set
            {
                _selectedValue = value;
                NotifyOfPropertyChange(() => SelectedValue); 
            }
        }

        public int SelectedValueAsInt
        {
            get
            {
                var result = int.TryParse(SelectedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var selectedValuesAsInt);
                return result ? selectedValuesAsInt : 0;
            }
        }

        public int Count => ListCollection.Count;
    }

    public class InspectorListEditorViewModel : EditorBase<InspectorList>, ILabelledInspector
    {

    }
}
