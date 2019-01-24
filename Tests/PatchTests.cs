using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using NUnit.Framework;

namespace Tests
{
    public class PatchTests
    {

        [Test]
        public void InformationalVersionSetCorrectly()
        {
            // Act
            var (assembly, path, appDomain) = PatchAssembly();

            try
            {
                var customAttributes = GetAssemblyInformationalVersionAttribute(assembly);

                // Assert
                AssertVersionWithSha(customAttributes.InformationalVersion);
            }
            finally
            {
                CleanupAssembly(path, appDomain);
            }
        }

        [Test]
        public void FileVersionSetCorrectly()
        {
            // Act
            var (_, path, appDomain) = PatchAssembly();

            try
            {
                // Assert
                var versionInfo = FileVersionInfo.GetVersionInfo(path);

                Assert.IsNotNull(versionInfo.FileVersion);
                Assert.IsNotEmpty(versionInfo.FileVersion);
                Assert.AreEqual("1.0.0.0", versionInfo.FileVersion);
            }
            finally
            {
                CleanupAssembly(path, appDomain);
            }
        }



        [Test]
        public void ProductionVersionSetCorrectly()
        {
            // Act
            var (_, path, appDomain) = PatchAssembly();

            try
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(path);

                // Assert
                AssertVersionWithSha(versionInfo.ProductVersion);
            }
            finally
            {
                CleanupAssembly(path, appDomain);
            }
        }



#if(DEBUG)
        [Test]
        public void PeVerify()
        {
            var beforeAssemblyPath = GetBeforeAssemblyPath();
            var (_, path, appDomain) = PatchAssembly();
            try
            {
                Verifier.Verify(beforeAssemblyPath, path);

            }
            finally
            {
                CleanupAssembly(path, appDomain);
            }
        }
#endif

        private static void AssertVersionWithSha(string version)
        {
            Assert.IsNotNull(version);
            Assert.IsNotEmpty(version);
            StringAssert.Contains("1.0.0.0", version, "Missing number");
            StringAssert.Contains("Head:", version, message: "Missing Head");
            StringAssert.Contains("Sha:", version, message: "Missing Sha");
            Trace.WriteLine(version);
        }

        private static (Assembly, string path, AppDomain) PatchAssembly()
        {



            var beforeAssemblyPath = GetBeforeAssemblyPath();

            var guid = Guid.NewGuid().ToString();


            var afterAssemblyPath = beforeAssemblyPath.Replace(".dll", $"{guid}.dll");
            File.Copy(beforeAssemblyPath, afterAssemblyPath, true);

            using (var moduleDefinition = ModuleDefinition.ReadModule(beforeAssemblyPath))
            {
                var currentDirectory = AssemblyLocation.CurrentDirectory();

                var weavingTask = new ModuleWeaver
                {
                    ModuleDefinition = moduleDefinition,
                    AddinDirectoryPath = currentDirectory,
                    SolutionDirectoryPath = currentDirectory,
                    AssemblyFilePath = afterAssemblyPath,
                };

                weavingTask.Execute();
                moduleDefinition.Write(afterAssemblyPath);

                weavingTask.AfterWeaving();
            }
            var appDomain = AppDomain.CreateDomain(guid);
            AssemblyName assemblyName = new AssemblyName
            {
                CodeBase = afterAssemblyPath
            };
            Assembly assembly = appDomain.Load(assemblyName);

            return (assembly, afterAssemblyPath, appDomain);
        }

        private static string GetBeforeAssemblyPath()
        {
            var beforeAssemblyPath = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, @"..\..\..\AssemblyToProcess\bin\Debug\AssemblyToProcess.dll"));
#if (!DEBUG)
       var beforeAssemblyPath = beforeAssemblyPath.Replace("Debug", "Release");
#endif
            return beforeAssemblyPath;
        }

        private static AssemblyInformationalVersionAttribute GetAssemblyInformationalVersionAttribute(Assembly assembly)
        {
            var customAttributes = (AssemblyInformationalVersionAttribute)assembly
                .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                .First();
            return customAttributes;
        }


        private static void CleanupAssembly(string path, AppDomain appDomain)
        {
            AppDomain.Unload(appDomain);

            File.Delete(path);
        }

    }
}