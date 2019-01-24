namespace Stamp.Fody.Tests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Mono.Cecil;
    using NUnit.Framework;

    [TestFixture]
    public abstract class PatchAssemblyTestsBase
    {
        private readonly string _patchedAssemblyPath;
        private readonly Assembly _patchedAssembly;
        private readonly string _assemblyToPatchPath;

        
        protected PatchAssemblyTestsBase(string assemblyName)
        {
            GetAssemblyToPatchPath(assemblyName);

            var guid = Guid.NewGuid().ToString();

            _assemblyToPatchPath = GetAssemblyToPatchPath(assemblyName);

            _patchedAssemblyPath = _assemblyToPatchPath.Replace(".dll", $"{guid}.dll");
            File.Copy(_assemblyToPatchPath, _patchedAssemblyPath, true);

            using (var moduleDefinition = ModuleDefinition.ReadModule(_assemblyToPatchPath))
            {
                var currentDirectory = AssemblyLocation.CurrentDirectory();

                var weavingTask = new ModuleWeaver
                {
                    ModuleDefinition = moduleDefinition,
                    AddinDirectoryPath = currentDirectory,
                    SolutionDirectoryPath = currentDirectory,
                    AssemblyFilePath = _patchedAssemblyPath
                };

                weavingTask.Execute();
                moduleDefinition.Write(_patchedAssemblyPath);

                weavingTask.AfterWeaving();
            }

            _patchedAssembly = Assembly.LoadFile(_patchedAssemblyPath);
        }

        protected abstract void AssertProductionVersion(string version);

        protected abstract void AssertFileVersion(string version);

        protected abstract void AssertInformationalVersion(string version);

        [Test]
        public void InformationalVersionSetCorrectly()
        {
            var customAttributes = GetAssemblyInformationalVersionAttribute(_patchedAssembly);

            // Assert
            var version = customAttributes.InformationalVersion;
            AssertInformationalVersion(version);
        }
        
        [Test]
        public void FileVersionSetCorrectly()
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(_patchedAssemblyPath);
            var version = versionInfo.FileVersion;
            AssertFileVersion(version);
        }
        
        [Test]
        public void ProductionVersionSetCorrectly()
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(_patchedAssemblyPath);

            // Assert
            var version = versionInfo.ProductVersion;
            AssertProductionVersion(version);
        }
        
#if(DEBUG)
        [Test]
        public void PeVerify()
        {
            Verifier.Verify(_assemblyToPatchPath, _patchedAssemblyPath);
        }
#endif

        private static string GetAssemblyToPatchPath(string assemblyName)
        {

            var path = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, $@"..\..\..\{assemblyName}\bin\Debug\{assemblyName}.dll"));
#if (!DEBUG)
            path = path.Replace("Debug", "Release");
#endif

            return path;
        }

        private static AssemblyInformationalVersionAttribute GetAssemblyInformationalVersionAttribute(Assembly assembly)
        {
            var customAttributes = (AssemblyInformationalVersionAttribute)assembly
                .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                .First();
            return customAttributes;
        }

    }
}