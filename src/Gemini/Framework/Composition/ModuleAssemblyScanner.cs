using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Gemini.Framework.Composition
{
    /// <summary>
    /// Replaces the unguarded <c>new DirectoryCatalog("./").Parts</c> scan that terminated the
    /// process during App.xaml parsing whenever a single assembly in the deployment folder could
    /// not load all of its types. Discovery keeps MEF's semantics for healthy assemblies
    /// (<see cref="AssemblyCatalog"/>, identity-based loading, and an assembly joins
    /// <c>AssemblySource</c> only when it contributes at least one part) and skips — with a report —
    /// any assembly that cannot be used.
    /// </summary>
    public class ModuleAssemblyScanner
    {
        private readonly string _directory;
        private readonly Action<ScanFailure> _reportFailure;
        private readonly Func<Assembly, Type[]> _getTypes;

        /// <summary>Primary constructor: the probe is the runtime's own type enumeration.</summary>
        public ModuleAssemblyScanner(string directory, Action<ScanFailure> reportFailure, Func<Assembly, Type[]> getTypes)
        {
            _directory = directory;
            _reportFailure = reportFailure;
            _getTypes = getTypes;
        }

        /// <summary>Secondary constructor for callers that want the real probe.</summary>
        public ModuleAssemblyScanner(string directory, Action<ScanFailure> reportFailure)
            : this(directory, reportFailure, assembly => assembly.GetTypes())
        {
        }

        /// <summary>
        /// Loads every managed <c>*.dll</c> in the folder exactly as MEF's <c>DirectoryCatalog</c>
        /// does — <see cref="AssemblyName.GetAssemblyName"/> supplies the identity and its CodeBase,
        /// then <see cref="Assembly.Load(AssemblyName)"/> resolves it through the normal binding
        /// rules — and returns the assemblies that can be composed.
        /// </summary>
        public IReadOnlyList<ScannedModuleAssembly> Scan()
        {
            var accepted = new List<ScannedModuleAssembly>();
            var identities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            string[] files;
            try
            {
                files = Directory.GetFiles(_directory, "*.dll");
            }
            catch (Exception error)
            {
                Report(_directory, error);
                return accepted;
            }

            foreach (var path in files.OrderBy(file => file, StringComparer.OrdinalIgnoreCase))
            {
                AssemblyName name;
                try
                {
                    name = AssemblyName.GetAssemblyName(path);
                }
                catch (BadImageFormatException)
                {
                    continue; // native library (for example HDFql.dll) - MEF skips these too
                }
                catch (FileNotFoundException)
                {
                    continue;
                }
                catch (FileLoadException)
                {
                    continue;
                }

                if (name == null || !identities.Add(name.FullName))
                    continue;

                Assembly assembly;
                try
                {
                    assembly = Assembly.Load(name);
                }
                catch (Exception error)
                {
                    Report(name.FullName, error);
                    continue;
                }

                ComposablePartCatalog catalog;
                if (!TryCreateCatalog(assembly, out catalog))
                    continue;

                if (!catalog.Parts.Any())
                    continue; // mirror DirectoryCatalog.Parts: only assemblies that contribute a part

                accepted.Add(new ScannedModuleAssembly(assembly, catalog));
            }

            return accepted;
        }

        /// <summary>
        /// Probes the assembly before anything is composed. <see cref="AssemblyCatalog"/> builds its
        /// inner catalog lazily, so an assembly with one unresolvable type throws later, out of the
        /// enumerator; probing here is what turns that into a reported skip.
        /// </summary>
        public bool TryCreateCatalog(Assembly assembly, out ComposablePartCatalog catalog)
        {
            catalog = null;

            if (assembly == null)
                return false;

            try
            {
                _getTypes(assembly);
            }
            catch (Exception error)
            {
                Report(assembly.FullName, error);
                return false;
            }

            try
            {
                catalog = new AssemblyCatalog(assembly);
                return true;
            }
            catch (Exception error)
            {
                Report(assembly.FullName, error);
                catalog = null;
                return false;
            }
        }

        private void Report(string source, Exception error)
        {
            if (_reportFailure != null)
                _reportFailure(new ScanFailure(source, error));
        }
    }
}
