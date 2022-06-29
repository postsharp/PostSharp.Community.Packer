using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace PostSharp.Community.Packer.Tests
{
    public class BasicTests
    {
#if DEBUG
        private const string ConfigFolder = "Debug";
#else
        private const string ConfigFolder = "Release";
#endif

        [Fact]
        public void TestTestAssemblyWithReferences()
        {
            var folder = @"..\..\..\..\TestAssembly.WithReferences\bin\" + ConfigFolder;
            var filename = "TestAssembly.WithReferences.exe";
            DeleteAllButOne(folder, filename);
            var p = Process.Start(Path.Combine(folder, filename));
            Assert.True(p.WaitForExit(5000));
            Assert.Equal(0, p.ExitCode);
        }

        [Fact]
        public void TestWPF()
        {
            var folder = @"..\..\..\..\Tests.WPF\bin\" + ConfigFolder;
            var filename = "Tests.WPF.exe";
            DeleteAllButOne(folder, filename);
            var p = Process.Start(Path.Combine(folder, filename));
            Assert.True(p.WaitForExit(35000));
            Assert.Equal(0, p.ExitCode);
        }

        private void DeleteAllButOne(string folder, string keepFileName)
        {
            foreach (var filename in Directory.EnumerateFiles(folder).ToList())
            {
                if (filename.EndsWith(keepFileName))
                {
                    // keep
                }
                else
                {
                    File.Delete(filename);
                }
            }
        }
    }
}