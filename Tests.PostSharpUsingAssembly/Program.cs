using System;
using System.Diagnostics;
using Newtonsoft.Json;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Community.Packer;
using Soothsilver.Random;
using Xunit;

[assembly: Packer(LoadAtModuleInit = false)]

namespace TestAssembly.WithReferences
{
    internal class Program
    {    
        [LogMe]

        public static void Main(string[] args)
        {
            //Debugger.Launch();
            PackerUtility.Initialize();
            Delay();
        }

        private static void Delay()
        {
            ThenUse.Stuff();
        }
    }

    [Serializable]
    internal class LogMeAttribute : OnMethodBoundaryAspect
    {
        public override void OnEntry(MethodExecutionArgs args)
        {
            Console.WriteLine("On entry");
            base.OnEntry(args);
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