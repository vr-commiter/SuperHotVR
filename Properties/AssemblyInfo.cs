using System.Reflection;
using MelonLoader;

[assembly: AssemblyTitle(SuperHotVR_TrueGear.BuildInfo.Description)]
[assembly: AssemblyDescription(SuperHotVR_TrueGear.BuildInfo.Description)]
[assembly: AssemblyCompany(SuperHotVR_TrueGear.BuildInfo.Company)]
[assembly: AssemblyProduct(SuperHotVR_TrueGear.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + SuperHotVR_TrueGear.BuildInfo.Author)]
[assembly: AssemblyTrademark(SuperHotVR_TrueGear.BuildInfo.Company)]
[assembly: AssemblyVersion(SuperHotVR_TrueGear.BuildInfo.Version)]
[assembly: AssemblyFileVersion(SuperHotVR_TrueGear.BuildInfo.Version)]
[assembly: MelonInfo(typeof(SuperHotVR_TrueGear.SuperHotVR_TrueGear), SuperHotVR_TrueGear.BuildInfo.Name, SuperHotVR_TrueGear.BuildInfo.Version, SuperHotVR_TrueGear.BuildInfo.Author, SuperHotVR_TrueGear.BuildInfo.DownloadLink)]
[assembly: MelonColor()]

// Create and Setup a MelonGame Attribute to mark a Melon as Universal or Compatible with specific Games.
// If no MelonGame Attribute is found or any of the Values for any MelonGame Attribute on the Melon is null or empty it will be assumed the Melon is Universal.
// Values for MelonGame Attribute can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonGame(null, null)]