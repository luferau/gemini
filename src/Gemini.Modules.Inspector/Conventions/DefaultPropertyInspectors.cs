using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Gemini.Modules.Inspector.Inspectors;
using Telerik.Windows.Controls.Map;

namespace Gemini.Modules.Inspector.Conventions
{
    public static class DefaultPropertyInspectors
    {
        private static readonly List<PropertyEditorBuilder> _inspectorBuilders;

        public static List<PropertyEditorBuilder> InspectorBuilders
        {
            get { return _inspectorBuilders; }
        }

        static DefaultPropertyInspectors()
        {
            _inspectorBuilders = new List<PropertyEditorBuilder>
            {
                new RangePropertyEditorBuilder(),
                new EnumPropertyEditorBuilder(),

                new StandardPropertyEditorBuilder<bool, CheckBoxEditorViewModel>(),
                new StandardPropertyEditorBuilder<Color, ColorEditorViewModel>(),
                new StandardPropertyEditorBuilder<double, TextBoxEditorViewModel<double>>(),
                new StandardPropertyEditorBuilder<float, TextBoxEditorViewModel<float>>(),
                new StandardPropertyEditorBuilder<int, TextBoxEditorViewModel<int>>(),
                new StandardPropertyEditorBuilder<short, TextBoxEditorViewModel<short>>(),
                new StandardPropertyEditorBuilder<ushort, TextBoxEditorViewModel<ushort>>(),
                new StandardPropertyEditorBuilder<long, TextBoxEditorViewModel<long>>(),
                new StandardPropertyEditorBuilder<double?, TextBoxEditorViewModel<double?>>(),
                new StandardPropertyEditorBuilder<float?, TextBoxEditorViewModel<float?>>(),
                new StandardPropertyEditorBuilder<int?, TextBoxEditorViewModel<int?>>(),
                new StandardPropertyEditorBuilder<Point3D, Point3DEditorViewModel>(),
                new StandardPropertyEditorBuilder<string, TextBoxEditorViewModel<string>>(),

                new StandardPropertyEditorBuilder<BigInteger, TextBoxEditorViewModel<BigInteger>>(),

                new StandardPropertyEditorBuilder<BitmapSource, BitmapSourceEditorViewModel>(),
                new StandardPropertyEditorBuilder<InspectorList, InspectorListEditorViewModel>(),
                new StandardPropertyEditorBuilder<FilePath, FilePathEditorViewModel>(),
                new StandardPropertyEditorBuilder<Location, LocationEditorViewModel>()
            };
        }

        public static IEditor CreateEditor(PropertyDescriptor propertyDescriptor)
        {
            foreach (var inspectorBuilder in _inspectorBuilders)
            {
                if (inspectorBuilder.IsApplicable(propertyDescriptor))
                    return inspectorBuilder.BuildEditor(propertyDescriptor);
            }
            return null;
        }
    }
}
