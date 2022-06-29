using Newtonsoft.Json;
using PostSharp.Community.Packer;
using Soothsilver.Random;
using System;
using System.Diagnostics;
using Xunit;

[assembly: Packer(LoadAtModuleInit = true, ExcludeAssemblies = new string[] { "nonexistent-assembly" })]

namespace TestAssembly.WithReferences
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Debugger.Launch();
            Debugger.Break();

            Delay();
            Console.ReadLine();
        }

        private static void Delay()
        {
            ThenUse.Stuff();
        }
    }

    internal class ThenUse
    {
        public static void Stuff()
        {
            var srls = JsonConvert.SerializeObject(new string[] { "he", "ha" });
            var r = srls + R.Next(0, 1).ToString();
            Assert.Equal(@"[""he"",""ha""]0", r);
            Console.WriteLine("This is still working: " + r);
        }
    }
}