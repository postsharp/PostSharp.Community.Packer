using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace PostSharp.Community.Packer.Tests
{
    public class BasicTest
    {
        [Fact]
        public void TestTestAssemblyWithReferences()
        {
            string folder = @"..\..\..\..\TestAssembly.WithReferences\bin\Debug";
            string filename = "TestAssembly.WithReferences.exe";
            DeleteAllButOne(folder, filename);
            Process p = Process.Start(Path.Combine(folder, filename));
            Assert.True(p.WaitForExit(5000));
            Assert.Equal(0, p.ExitCode);
        }
        [Fact]
        public void TestWPF()
        {
            string folder = @"..\..\..\..\Tests.WPF\bin\Debug";
            string filename = "Tests.WPF.exe";
            DeleteAllButOne(folder, filename);
            Process p = Process.Start(Path.Combine(folder, filename));
            Assert.True(p.WaitForExit(5000));
            Assert.Equal(0, p.ExitCode);
        }

        private void DeleteAllButOne(string folder, string keepFileName)
        {
            foreach (string filename in Directory.EnumerateFiles(folder).ToList())
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