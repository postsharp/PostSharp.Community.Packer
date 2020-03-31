using System;

namespace PostSharp.Community.Packer
{
    /// <summary>
    /// Contains the method used to initialize the Packer.
    /// </summary>
    public static class PackerUtility
    {
        /// <summary>
        /// Call this to initialize the Packer. Use this if you're not using a module initialize. If you use this,
        /// you must call this before using any class that references something from a packed-in assembly.
        /// </summary>
        public static void Initialize()
        {
            throw new Exception("The code in this method should have been replaced at build time by the Packer. If you see this, it's probably because you didn't configure Packer with an assembly-wide attribute.");
        }
    }
}