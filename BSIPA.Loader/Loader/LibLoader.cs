using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using IPA.Logging;
using IPA.Utilities;
using Mono.Cecil;

namespace IPA.Loader
{
    internal class CecilLibLoader : BaseAssemblyResolver
    {
        private static readonly string CurrentAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        private static readonly string CurrentAssemblyPath = Assembly.GetExecutingAssembly().Location;

        public override AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            LibLoader.SetupAssemblyFilenames();

			if (name.Name == CurrentAssemblyName || name.Name == "IPA.Loader")
			{
                return AssemblyDefinition.ReadAssembly(CurrentAssemblyPath, parameters);
			}

            if (LibLoader.FilenameLocations.TryGetValue($"{name.Name}.dll", out var path))
            {
                if (File.Exists(path))
                    return AssemblyDefinition.ReadAssembly(path, parameters);
            }
            else if (LibLoader.FilenameLocations.TryGetValue($"{name.Name}.{name.Version}.dll", out path))
            {
                if (File.Exists(path))
                    return AssemblyDefinition.ReadAssembly(path, parameters);
            }


            return base.Resolve(name, parameters);
        }
    }

    internal static class LibLoader
    {
        internal static string LibraryPath => UnityGame.LibraryPath;
        internal static string NativeLibraryPath => UnityGame.NativeLibraryPath;
        internal static Dictionary<string, string> FilenameLocations;

        internal static void Configure()
        {
            SetupAssemblyFilenames(true);
            AppDomain.CurrentDomain.AssemblyResolve -= AssemblyLibLoader;
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyLibLoader;
        }

        internal static void SetupAssemblyFilenames(bool force = false)
        {
            if (FilenameLocations == null || force)
            {
                FilenameLocations = new Dictionary<string, string>();

                foreach (var fn in TraverseTree(LibraryPath, s => s != NativeLibraryPath))
                    if (FilenameLocations.ContainsKey(fn.Name))
                        Log(Logger.Level.Critical, $"Multiple instances of {fn.Name} exist in Libs! Ignoring {fn.FullName}");
                    else FilenameLocations.Add(fn.Name, fn.FullName);
            }
        }

        public static Assembly AssemblyLibLoader(object source, ResolveEventArgs e)
        {
            var asmName = new AssemblyName(e.Name);
            return LoadLibrary(asmName);
        }

        internal static Assembly LoadLibrary(AssemblyName asmName)
        {
            Log(Logger.Level.Debug, $"Resolving library {asmName}");

            SetupAssemblyFilenames();

            var testFile = $"{asmName.Name}.dll";
            Log(Logger.Level.Debug, $"Looking for file {asmName.Name}.dll");

			if (asmName.Name == "IPA.Loader")
			{
				Log(Logger.Level.Debug, "Resolved to self");
                return typeof(BSIPALoaderPlugin).Assembly;
			}

            if (FilenameLocations.TryGetValue(testFile, out var path))
            {
                Log(Logger.Level.Debug, $"Found file {testFile} as {path}");
                if (File.Exists(path))
                    return Assembly.LoadFrom(path);

                Log(Logger.Level.Critical, $"but {path} no longer exists!");
            }
            else if (FilenameLocations.TryGetValue(testFile = $"{asmName.Name}.{asmName.Version}.dll", out path))
            {
                Log(Logger.Level.Debug, $"Found file {testFile} as {path}");
                Log(Logger.Level.Warning, $"File {testFile} should be renamed to just {asmName.Name}.dll");
                if (File.Exists(path))
                    return Assembly.LoadFrom(path);

                Log(Logger.Level.Critical, $"but {path} no longer exists!");
            }

            Log(Logger.Level.Critical, $"No library {asmName} found");

            return null;
        }

        internal static void Log(Logger.Level lvl, string message)
        { // multiple proxy methods to delay loading of assemblies until it's done
            if (Logger.LogCreated)
                AssemblyLibLoaderCallLogger(lvl, message);
            else
                if (((byte)lvl & (byte)StandardLogger.PrintFilter) != 0)
                    Console.WriteLine($"[{lvl}] {message}");
        }
        internal static void Log(Logger.Level lvl, Exception message)
        { // multiple proxy methods to delay loading of assemblies until it's done
            if (Logger.LogCreated)
                AssemblyLibLoaderCallLogger(lvl, message);
            else
                if (((byte)lvl & (byte)StandardLogger.PrintFilter) != 0)
                Console.WriteLine($"[{lvl}] {message}");
        }

        private static void AssemblyLibLoaderCallLogger(Logger.Level lvl, string message) => Logger.libLoader.Log(lvl, message);
        private static void AssemblyLibLoaderCallLogger(Logger.Level lvl, Exception message) => Logger.libLoader.Log(lvl, message);

        // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/file-system/how-to-iterate-through-a-directory-tree
        private static IEnumerable<FileInfo> TraverseTree(string root, Func<string, bool> dirValidator = null)
        {
            if (dirValidator == null) dirValidator = s => true;

            Stack<string> dirs = new Stack<string>(32);

            if (!Directory.Exists(root))
                throw new ArgumentException();
            dirs.Push(root);

            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                string[] subDirs;
                try
                {
                    subDirs = Directory.GetDirectories(currentDir);
                }
                catch (UnauthorizedAccessException)
                { continue; }
                catch (DirectoryNotFoundException)
                { continue; }

                string[] files;
                try
                {
                    files = Directory.GetFiles(currentDir);
                }
                catch (UnauthorizedAccessException)
                { continue; }
                catch (DirectoryNotFoundException)
                { continue; }
                
                foreach (string str in subDirs)
                    if (dirValidator(str)) dirs.Push(str);

                foreach (string file in files)
                {
                    FileInfo nextValue;
                    try
                    {
                        nextValue = new FileInfo(file);
                    }
                    catch (FileNotFoundException)
                    { continue; }

                    yield return nextValue;
                }
            }
        }
    }
}
