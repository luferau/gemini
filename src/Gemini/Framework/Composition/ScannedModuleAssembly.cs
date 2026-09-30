using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Reflection;

namespace Gemini.Framework.Composition
{
    /// <summary>
    /// An assembly that module discovery accepted, paired with the catalog built for it.
    /// The pair is carried through composition so no caller has to reflect over the assembly
    /// a second time — that second reflection is what made a partially loadable assembly
    /// throw again after it had already been classified.
    /// </summary>
    public class ScannedModuleAssembly
    {
        public ScannedModuleAssembly(Assembly assembly, ComposablePartCatalog catalog)
        {
            Assembly = assembly;
            Catalog = catalog;
        }

        public Assembly Assembly { get; }

        public ComposablePartCatalog Catalog { get; }
    }
}
