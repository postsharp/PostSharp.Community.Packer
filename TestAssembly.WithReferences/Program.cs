using System;
using Newtonsoft.Json;
using PostSharp.Community.Packer;
using Soothsilver.Random;
using Xunit;

[assembly: Packer(LoadAtModuleInit = false)]

namespace TestAssembly.WithReferences
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            PackerUtility.Initialize();
            Delay();
        }

        private static void Delay()
        {
            ThenUse.Stuff();
        }
    }

    class ThenUse
    {
        public static void Stuff()
        {
            
            string srls = JsonConvert.SerializeObject(new string[] {"he", "ha"});
            string r = srls + R.Next(0, 1).ToString();
            Assert.Equal(@"[""he"",""ha""]0", r);
            Console.WriteLine("This is still working: " + r);
        }
    }
}