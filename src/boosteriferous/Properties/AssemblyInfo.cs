using System.Reflection;
using System.Runtime.CompilerServices;

// Information about this assembly is defined by the following attributes.
// Change them to the values specific to your project.

[assembly: AssemblyTitle ("boosteriferous")]
[assembly: AssemblyDescription ("Thrust profiles for SRBs")]
[assembly: AssemblyConfiguration ("")]
[assembly: AssemblyCompany ("")]
[assembly: AssemblyProduct ("")]
[assembly: AssemblyCopyright ("Copyright © 2015, soundnfury")]
[assembly: AssemblyTrademark ("")]
[assembly: AssemblyCulture ("")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]

// Don't include the patch revision in the AssemblyVersion - as this will break any dependent
// DLLs any time it changes.  Breaking on a minor revision is probably acceptable - it's
// unlikely that there wouldn't be other breaking changes on a minor version change.
[assembly: AssemblyVersion("0.1")]
[assembly: AssemblyFileVersion("0.1.0")]

// Use KSPAssembly to allow other DLLs to make this DLL a dependency in a
// non-hacky way in KSP.  Format is (AssemblyProduct, major, minor), and it
// does not appear to have a hard requirement to match the assembly version.
[assembly: KSPAssembly("boosteriferous", 0, 1)]
