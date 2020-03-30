using System;
using Newtonsoft.Json;
using PostSharp.Community.JustOneExe;
using Soothsilver.Random;
using Xunit;

[assembly: JustOneExe]

namespace TestAssembly.WithReferences
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            string srls = JsonConvert.SerializeObject(new string[] {"he", "ha"});
            string r = srls + R.Next(0, 1).ToString();
            Assert.Equal(@"[""he"",""ha""]0", r);
            Console.WriteLine("This is still working: " + r);
        }
    }
}