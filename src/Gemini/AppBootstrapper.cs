using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Linq;
using Caliburn.Micro;
using Gemini.Framework.Composition;
using Gemini.Framework.Services;

namespace Gemini
{
    using System.Globalization;
    using System.Reflection;
    using System.Threading;
    using System.Windows;
    using Gu.Localization;

    public class AppBootstrapper : BootstrapperBase
    {
        private List<Assembly> _priorityAssemblies;
        private ModuleAssemblyScanner _scanner;
        private IReadOnlyList<ScannedModuleAssembly> _scannedAssemblies;
        private List<ScanFailure> _scanFailures;

		protected CompositionContainer Container { get; set; }

        internal IList<Assembly> PriorityAssemblies => _priorityAssemblies;

        public AppBootstrapper()
        {
            this.PreInitialize();
            this.Initialize();
        }

        protected virtual void PreInitialize()
        {
            if (Properties.Settings.Default.UpdateSettings)
            {
                try
                {
                    Properties.Settings.Default.Upgrade();
                }
                catch (System.Configuration.ConfigurationErrorsException ex)
                {
                    // User settings file is corrupted — delete it and reset to defaults.
                    var filename = ex.InnerException is System.Configuration.ConfigurationErrorsException innerEx
                        ? innerEx.Filename
                        : ((ex as System.Configuration.ConfigurationErrorsException)?.Filename);

                    if (!string.IsNullOrEmpty(filename) && System.IO.File.Exists(filename))
                    {
                        System.IO.File.Delete(filename);
                    }

                    Properties.Settings.Default.Reload();
                }

                Properties.Settings.Default.UpdateSettings = false;
                Properties.Settings.Default.Save();
            }

            var code = Properties.Settings.Default.LanguageCode;

            if (!string.IsNullOrWhiteSpace(code))
            {
                var culture = CultureInfo.GetCultureInfo(code);
                Translator.Culture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                Thread.CurrentThread.CurrentCulture = culture;
            }
        }

		/// <summary>
		/// By default, we are configured to use MEF
		/// </summary>
		protected override void Configure()
		{
            // Discover the modules in the application folder. This replaces a bare
            // new DirectoryCatalog("./") scan: enumerating its Parts made MEF call
            // Assembly.GetTypes() on every DLL, and one assembly whose dependency resolved to
            // the wrong version then threw ReflectionTypeLoadException out of this constructor,
            // i.e. out of App.xaml parsing, before any window or handler existed.
            _scanFailures = new List<ScanFailure>();
            _scanner = new ModuleAssemblyScanner(AppDomain.CurrentDomain.BaseDirectory, failure =>
            {
                _scanFailures.Add(failure);
                ReportScanFailure(failure);
            });
            _scannedAssemblies = _scanner.Scan();

            AssemblySource.Instance.AddRange(_scannedAssemblies
                .Select(scanned => scanned.Assembly)
                .Where(assembly => !AssemblySource.Instance.Contains(assembly)));

            // Prioritize the executable assembly. This allows the client project to override exports, including IShell.
            // The client project can override SelectAssemblies to choose which assemblies are prioritised.
            _priorityAssemblies = SelectAssemblies().Where(assembly => assembly != null).ToList();
		    var priorityCatalog = new AggregateCatalog(_priorityAssemblies.Select(CatalogFor));
		    var priorityProvider = new CatalogExportProvider(priorityCatalog);
            
            // Now get all other assemblies (excluding the priority assemblies).
			var mainCatalog = new AggregateCatalog(
                AssemblySource.Instance
                    .Where(assembly => !_priorityAssemblies.Contains(assembly))
                    .Select(CatalogFor));
		    var mainProvider = new CatalogExportProvider(mainCatalog);

			Container = new CompositionContainer(priorityProvider, mainProvider);
		    priorityProvider.SourceProvider = Container;
		    mainProvider.SourceProvider = Container;

			var batch = new CompositionBatch();

		    BindServices(batch);
            batch.AddExportedValue(mainCatalog);

			Container.Compose(batch);

            ValidateRequiredExports();

            OnScanCompleted(_scanFailures);
		}

        /// <summary>
        /// Called once, after discovery finished and the container is composed. A host application
        /// overrides this to record and report what discovery had to skip; the default writes the
        /// grouped summary to the trace listeners.
        /// </summary>
        protected virtual void OnScanCompleted(IReadOnlyList<ScanFailure> failures)
        {
            if (failures == null || failures.Count == 0)
                return;

            Trace.WriteLine(new ScanFailureSummary(failures).DescribeSummary());
        }

        /// <summary>
        /// Catalog for one assembly: the pair discovery already accepted, or a probe of the
        /// assembly itself (the priority assembly is the entry assembly and need not be in the folder).
        /// </summary>
        private ComposablePartCatalog CatalogFor(Assembly assembly)
        {
            var scanned = _scannedAssemblies.FirstOrDefault(candidate => candidate.Assembly == assembly);
            if (scanned != null)
                return scanned.Catalog;

            ComposablePartCatalog catalog;
            if (_scanner.TryCreateCatalog(assembly, out catalog))
                return catalog;

            OnStartupFailed(new InvalidOperationException(
                "The entry assembly could not be loaded: " + (assembly == null ? "(none)" : assembly.FullName)));

            return new TypeCatalog(new List<Type>());
        }

        /// <summary>
        /// The shell cannot run without these contracts, so a deployment that cannot supply them
        /// fails with a report instead of an opaque composition error later.
        /// </summary>
        private void ValidateRequiredExports()
        {
            var missing = new List<string>();

            if (!HasExport(typeof(IMainWindow)))
                missing.Add(typeof(IMainWindow).FullName);

            if (!HasExport(typeof(IShell)))
                missing.Add(typeof(IShell).FullName);

            if (missing.Count > 0)
                OnStartupFailed(new InvalidOperationException(
                    "These required modules are not available: " + string.Join(", ", missing)));
        }

        /// <summary>Asks the container whether a contract has an export, without creating one.</summary>
        private bool HasExport(Type contractType)
        {
            try
            {
                return Container.GetExports<object>(AttributedModelServices.GetContractName(contractType)).Any();
            }
            catch (ImportCardinalityMismatchException)
            {
                return false;
            }
        }

        /// <summary>Called once per assembly that module discovery could not use.</summary>
        protected virtual void ReportScanFailure(ScanFailure failure)
        {
            Trace.WriteLine(failure.ToString());
        }

        /// <summary>
        /// Called when the application cannot start. The default writes to the trace listeners;
        /// a host application overrides this to write its own log and tell the operator.
        /// </summary>
        protected virtual void OnStartupFailed(Exception error)
        {
            Trace.WriteLine("Startup failed: " + error);
        }

	    protected virtual void BindServices(CompositionBatch batch)
        {
            batch.AddExportedValue<IWindowManager>(new WindowManager());
            batch.AddExportedValue<IEventAggregator>(new EventAggregator());
            batch.AddExportedValue(Container);
	        batch.AddExportedValue(this);
        }

		protected override object GetInstance(Type serviceType, string key)
		{
			string contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
			var exports = Container.GetExports<object>(contract);

			if (exports.Any())
				return exports.First().Value;

			throw new Exception($"Could not locate any instances of contract {contract}.");
		}

		protected override IEnumerable<object> GetAllInstances(Type serviceType)
		{
			return Container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
		}

		protected override void BuildUp(object instance)
		{
			Container.SatisfyImportsOnce(instance);
		}

	    protected override void OnStartup(object sender, StartupEventArgs e)
	    {
            try
            {
	        base.OnStartup(sender, e);
                DisplayRootViewFor<IMainWindow>();
            }
            catch (Exception error)
            {
                OnStartupFailed(error);
            }
	    }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            return new[] { Assembly.GetEntryAssembly() };

            /*
            // TODO do so that assemblies are added from a specific folder (setting) or something like this
            var directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory.Replace(@"\Shell\", @"\MissileElements\"));
            var files = directory.GetFiles("*.dll", SearchOption.AllDirectories);
            var assemblies = files.Where(f => f.Name.Contains("MissileElements")).Select(f => Assembly.LoadFile(f.FullName)).ToList();

            assemblies.Add(Assembly.GetEntryAssembly());

            return assemblies;
            */
        }
	}
}
