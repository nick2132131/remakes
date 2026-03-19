using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Synapse_UI_WPF
{
    public partial class App : Application
    {
        static App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CefResolver;
        }

        private static Assembly CefResolver(object sender, ResolveEventArgs args)
        {
            if (!args.Name.StartsWith("CefSharp")) return null;

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var shortName = args.Name.Split(',')[0]; // e.g. "CefSharp"

            // 1. Try output folder exact match
            var inOutput = Path.Combine(baseDir, shortName + ".dll");
            if (File.Exists(inOutput)) return Assembly.LoadFrom(inOutput);

            // 2. CefSharp.dll lives in NuGet cache under cefsharp.common
            //    Try all common framework subfolders
            var nugetBase = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".nuget", "packages", "cefsharp.common", "145.0.260", "lib");

            foreach (var sub in new[] { "net462", "net40", "net45", "net46", "net48", "netstandard2.0" })
            {
                var candidate = Path.Combine(nugetBase, sub, shortName + ".dll");
                if (File.Exists(candidate)) return Assembly.LoadFrom(candidate);
            }

            // 3. Fallback: scan all subdirs of cefsharp.common lib folder
            if (Directory.Exists(nugetBase))
            {
                foreach (var dir in Directory.GetDirectories(nugetBase))
                {
                    var candidate = Path.Combine(dir, shortName + ".dll");
                    if (File.Exists(candidate)) return Assembly.LoadFrom(candidate);
                }
            }

            return null;
        }
    }
}