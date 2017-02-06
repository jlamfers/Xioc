$strPath = [System.IO.Path]::GetFullPath($PSScriptRoot + "/Xioc.dll");
$Assembly = [Reflection.Assembly]::Loadfile($strPath)
$AssemblyName = $Assembly.GetName()
$Assemblyversion = $AssemblyName.version

&$PSScriptRoot"/nuget.exe" pack $PSScriptRoot/Xioc.nuspec -Version $Assemblyversion
&$PSScriptRoot"/nuget.exe" pack $PSScriptRoot/Xioc.Mvc5.nuspec -Version $Assemblyversion
&$PSScriptRoot"/nuget.exe" pack $PSScriptRoot/Xioc.WebApi2.nuspec -Version $Assemblyversion