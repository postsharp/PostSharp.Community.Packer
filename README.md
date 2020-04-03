## <img src="icon.png" width="32"> &nbsp; PostSharp.Community.Packer 
**This add-in is not yet functioning!**

Embeds dependencies as resources so that you can have a standalone executable.




*This is an add-in for [PostSharp](https://postsharp.net). It modifies your assembly during compilation by using IL weaving.*

![CI badge](https://github.com/postsharp/PostSharp.Community.Packer/workflows/Full%20Pipeline/badge.svg)

#### Example
Your project would normally result in `MyProject.exe` which requires `Newtonsoft.Json.dll` and `Soothsilver.Random.dll` as dependencies because you used those NuGet packages.

If you use this add-in, instead those two DLLs will be embedded into `MyProject.exe` as resources and loaded from there. 
#### Installation 
1. Install the NuGet package: `PM> Install-Package PostSharp.Community.Packer`
2. Get a free PostSharp Community license at https://www.postsharp.net/essentials
3. When you compile for the first time, you'll be asked to enter the license key.
4. Add `[assembly: Packer]` somewhere in your code.

You can then distribute just the main output assembly file. It will be enough.

There are documented configuration options in the Packer attribute. Set them in your source code to change them from their defaults.

#### Copyright notices
Published under the MIT license.

* Copyright © PostSharp Technologies, Simon Cropp, and contributors 
* Icon by goescat, https://www.iconfinder.com/goescat, licensed under CC BY 3.0