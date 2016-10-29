using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle ("boosteriferous")]
[assembly: AssemblyDescription ("Thrust profiles for SRBs")]
[assembly: AssemblyConfiguration ("")]
[assembly: AssemblyCompany ("")]
[assembly: AssemblyProduct ("")]
[assembly: AssemblyCopyright ("Copyright © 2015-16, soundnfury")]
[assembly: AssemblyTrademark ("")]
[assembly: AssemblyCulture ("")]

// Don't include the patch revision in the AssemblyVersion - as this will break any dependent
// DLLs any time it changes.  Breaking on a minor revision is probably acceptable - it's
// unlikely that there wouldn't be other breaking changes on a minor version change.
[assembly: AssemblyVersion("0.2")]
[assembly: AssemblyFileVersion("0.2.3")]

// Use KSPAssembly to allow other DLLs to make this DLL a dependency in a
// non-hacky way in KSP.  Format is (AssemblyProduct, major, minor), and it
// does not appear to have a hard requirement to match the assembly version.
[assembly: KSPAssembly("boosteriferous", 0, 2)]
