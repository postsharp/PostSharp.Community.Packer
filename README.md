## <img src="https://cdn4.iconfinder.com/data/icons/Hypic_Icon_Pack_by_shlyapnikova/64/plugin_64.png" width="32"> &nbsp; PostSharp.Community.JustOneExe 
An example add-in for [PostSharp](https://postsharp.net).

You can use the code in this repository as a base on which to build your own add-in for PostSharp.

*This is an add-in for [PostSharp](https://postsharp.net). It modifies your assembly during compilation by using IL weaving. The add-in functionality is in preview, and not yet public. This add-in might not work and is unsupported.*

## Creating an add-in
To create your PostSharp add-in using this template as a base, do the following:
1. Change "PostSharp.Community.JustOneExe" in all file and folder names to the name of your add-in.
2. Change the text "PostSharp.Community.JustOneExe" in all files to the name/namespace of your add-in.
3. Replace the `JustOneExeAttribute` in `PostSharp.Community.JustOneExe` with any attributes or classes that you want the user of your add-in to be able to access at IntelliSense time, build time and possibly runtime.
4. Update the class `JustOneExeTask` in `PostSharp.Community.JustOneExe.Weaver` with the code of your add-in.
5. Delete `LICENSE.md` or change it a license of your choice.

Then, if you want to distribute the add-in as a NuGet package:
1. Update any other files, such as the `nuspec` file.
2. Use `nuget pack PostSharp.Community.JustOneExe.nuspec` to create a NuGet package from your add-in.
3. To use the add-in, the user just needs to add and install that NuGet package.

Or, if you want to distribute the add-in as a project or an assembly:
1. The user of the add-in must reference the NuGet package PostSharp.
2. The user of the add-in must reference the client assembly or the client assembly project (`PostSharp.Community.JustOneExe`).
3. The directory that contains the weaver assembly (`PostSharp.Community.JustOneExe.Weaver`) must be placed on the PostSharp search path with the MSBuild property `PostSharpSearchPath`. See the project `PostSharp.Community.JustOneExe.Tests` for an example on how to do this.

#### Building
Restore and build the solution to build both the add-in and the tests.

You need at least a Community license of PostSharp to build the Tests project. You can get this license for free 
at https://www.postsharp.net/essentials.

#### Testing
The add-in is included in the project file of `PostSharp.Community.JustOneExe.Tests`. This means that the attributes you
define `PostSharp.Community.JustOneExe` (the client assembly) can be used in the test project and the weaver you define
in `PostSharp.Community.JustOneExe.Weaver` (the weaver assembly) processes that test project when the test project is built.

Tests you define in that project therefore already see the enhanced code. 

See the project file `PostSharp.Community.JustOneExe.Tests.csproj` for details on how add-in discovery works.

#### Debugging
You can attach a debugger to the compiler as an assembly is being built to debug your add-in.

To do this, add the following lines to the `.csproj` file of the project where you're using the weaver. In this add-in, that would be `PostSharp.Community.JustOneExe.Tests.csproj`:

```xml
<PropertyGroup>
    <PostSharpAttachDebugger>True</PostSharpAttachDebugger>
    <PostSharpHost>Native</PostSharpHost>
</PropertyGroup>
```
Or build the project with the command line arguments `/P:PostSharpAttachDebugger=True /P:PostSharpHost=Native`.

Also, change the `<TargetFrameworks>` line to only target one framework.

If you choose .NET Framework, then PostSharp will trigger the just-in-time debugger window prompting you to attach a debugger.

If you choose .NET Core or .NET Standard, then PostSharp will wait indefinitely until you attach a debugger using "Attach to Running Process" functionality of a debugger.

We tested debugging with both Visual Studio and JetBrains Rider.

## Documentation of the JustOneExe add-in
This add-in adds the line `Console.WriteLine("Hello, world!");` at the beginning of each target method in your code.
 
#### Example
Your code:
```csharp
[JustOneExe]
static int ReturnTheAnswer() 
{
    return 42;
}
```
What gets compiled:
```csharp
static int ReturnTheAnswer() 
{
    Console.WriteLine("Hello, world!");
    return 42;
}
```

#### Installation (as a user of this plugin)
1. Install the NuGet package: `PM> Install-Package PostSharp.Community.JustOneExe`
2. Get a free PostSharp Community license at https://www.postsharp.net/essentials
3. When you compile for the first time, you'll be asked to enter the license key.

#### How to use
Add `[JustOneExe]` to the methods or classes where you want it to apply, or apply it to the entire assembly with [multicasting](https://doc.postsharp.net/attribute-multicasting).

#### Copyright notices
This example of a PostSharp add-in is released to the public domain.

Other PostSharp add-ins are generally released under the MIT license.

* The code in this repository was created by PostSharp Technologies.
* Icon by Shlyapnikova, https://www.iconfinder.com/icons/51412/24_plugin_icon, CC BY 3.0