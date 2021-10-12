using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyProduct("Nitrox Bootloader")]
[assembly: AssemblyTitle("Simple loader that initiates code to be loaded into the game from Nitrox's install location")]
[assembly: AssemblyDescription("Simple loader that initiates code to be loaded into the game from Nitrox's install location")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
// COMMON: [assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("3d257315-8072-44f4-9859-6dea57ea1ad6")]

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
// COMMON: [assembly: AssemblyVersion("X.X.X.X")]
// COMMON: [assembly: AssemblyFileVersion("X.X.X.X")]

[assembly: InternalsVisibleTo("NitroxTest")]