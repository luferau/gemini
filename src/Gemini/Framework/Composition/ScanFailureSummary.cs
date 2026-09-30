using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gemini.Framework.Composition
{
    /// <summary>One cause shared by the assemblies that module discovery could not use.</summary>
    public class ScanFailureGroup
    {
        public ScanFailureGroup(string cause, IReadOnlyList<string> assemblies)
        {
            Cause = cause;
            Assemblies = assemblies;
        }

        public string Cause { get; }

        public IReadOnlyList<string> Assemblies { get; }

        public int Count
        {
            get { return Assemblies.Count; }
        }
    }

    /// <summary>
    /// Collapses what module discovery skipped into one entry per cause. A deployment can lose every
    /// assembly to a single loader-level reason - the files are blocked or remote - and a hundred
    /// identical entries plus a wall of text in a message box help nobody.
    /// </summary>
    public class ScanFailureSummary
    {
        private readonly IReadOnlyList<ScanFailure> _failures;
        private IReadOnlyList<ScanFailureGroup> _groups;
        private bool? _isLocationBlocked;

        public ScanFailureSummary(IReadOnlyList<ScanFailure> failures)
        {
            _failures = failures ?? new List<ScanFailure>();
        }

        public IReadOnlyList<ScanFailure> Failures
        {
            get { return _failures; }
        }

        public int Count
        {
            get { return _failures.Count; }
        }

        /// <summary>
        /// Failures grouped by cause shape - exception types and HResult, never message text, which is
        /// localized on a non-English Windows.
        /// </summary>
        public IReadOnlyList<ScanFailureGroup> Groups
        {
            get
            {
                if (_groups == null)
                    _groups = BuildGroups();

                return _groups;
            }
        }

        /// <summary>True when at least one assembly was refused by the loader because of its location.</summary>
        public bool IsLocationBlocked
        {
            get
            {
                if (!_isLocationBlocked.HasValue)
                    _isLocationBlocked = _failures.Any(failure => failure.IsLocationBlocked);

                return _isLocationBlocked.Value;
            }
        }

        /// <summary>Grouped report for the log. Per-assembly names are kept only for small drifts.</summary>
        public string DescribeSummary()
        {
            var text = new StringBuilder();

            text.Append(_failures.Count)
                .Append(_failures.Count == 1 ? " assembly" : " assemblies")
                .Append(" could not be loaded (")
                .Append(Groups.Count)
                .Append(Groups.Count == 1 ? " cause" : " causes")
                .AppendLine("):");

            foreach (var group in Groups)
            {
                text.Append("  ").Append(group.Count).Append(" x ").AppendLine(group.Cause);

                if (_failures.Count > 25)
                    continue;

                foreach (var assembly in group.Assemblies)
                    text.Append("      ").AppendLine(assembly);
            }

            if (_failures.Count > 25)
                text.AppendLine("  (per-assembly detail suppressed for this many failures; " +
                                "run Shell.exe --self-check for the full list)");

            return text.ToString().TrimEnd();
        }

        /// <summary>Operator-facing text, capped so a dialog can never become a wall of text.</summary>
        public string DescribeForOperator(int maxLines)
        {
            var lines = new List<string>();

            lines.Add(_failures.Count + (_failures.Count == 1 ? " module was" : " modules were") +
                      " not loaded in this session (" + Groups.Count + (Groups.Count == 1 ? " cause" : " causes") + "):");

            foreach (var group in Groups)
            {
                if (lines.Count >= maxLines)
                    break;

                lines.Add("  " + group.Count + " x " + group.Cause);

                foreach (var assembly in group.Assemblies)
                {
                    if (lines.Count >= maxLines)
                        break;

                    lines.Add("      " + ShortName(assembly));
                }
            }

            if (lines.Count >= maxLines)
                lines.Add("  ... and the rest are in the startup log");

            return string.Join(Environment.NewLine, lines);
        }

        private static string ShortName(string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
                return "(unknown)";

            var comma = assemblyName.IndexOf(',');
            return comma > 0 ? assemblyName.Substring(0, comma) : assemblyName;
        }

        private IReadOnlyList<ScanFailureGroup> BuildGroups()
        {
            var order = new List<string>();
            var causes = new Dictionary<string, string>(StringComparer.Ordinal);
            var assemblies = new Dictionary<string, List<string>>(StringComparer.Ordinal);

            foreach (var failure in _failures)
            {
                var error = failure.Error ?? new Exception("no error reported");
                var key = CauseKey(error);

                if (!assemblies.ContainsKey(key))
                {
                    order.Add(key);
                    causes.Add(key, DescribeCause(error));
                    assemblies.Add(key, new List<string>());
                }

                assemblies[key].Add(failure.Source);
            }

            var groups = new List<ScanFailureGroup>(order.Count);
            foreach (var key in order)
                groups.Add(new ScanFailureGroup(causes[key], assemblies[key]));

            return groups;
        }

        private static string CauseKey(Exception error)
        {
            return error.GetType().FullName + "|" + error.HResult.ToString("X8") + "|" +
                   Innermost(error).GetType().FullName;
        }

        private static string DescribeCause(Exception error)
        {
            return error.GetType().Name + " (0x" + error.HResult.ToString("X8") + ") caused by " +
                   Innermost(error).GetType().Name;
        }

        private static Exception Innermost(Exception error)
        {
            var current = error;
            var depth = 0;

            while (current.InnerException != null && depth < 8)
            {
                current = current.InnerException;
                depth++;
            }

            return current;
        }
    }
}
