$FullPath = [System.IO.Path]::GetFullPath("$PWD\bin\release\net7.0-windows\Keysharp.dll")
$DLLBytes = [System.IO.File]::ReadAllBytes($FullPath)
$Assembly = [System.Reflection.Assembly]::Load($DLLBytes)
$AssemblyVersion = $Assembly.GetName().Version.ToString()

Compress-Archive -LiteralPath `
 ".\bin\release\net7.0-windows\Keysharp.Core.dll" `
,".\bin\release\net7.0-windows\Keysharp.exe" `
,".\bin\release\net7.0-windows\Keysharp.dll" `
,".\bin\release\net7.0-windows\Keyview.exe" `
,".\bin\release\net7.0-windows\Keyview.dll" `
,".\bin\release\net7.0-windows\Interop.IWshRuntimeLibrary.dll" `
,".\bin\release\net7.0-windows\Microsoft.CodeAnalysis.CSharp.dll" `
,".\bin\release\net7.0-windows\Microsoft.CodeAnalysis.dll" `
,".\bin\release\net7.0-windows\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll" `
,".\bin\release\net7.0-windows\Microsoft.Extensions.DependencyModel.dll" `
,".\bin\release\net7.0-windows\Microsoft.NET.HostModel.dll" `
,".\bin\release\net7.0-windows\System.Management.dll" `
,".\bin\release\net7.0-windows\Keysharp.Core.deps.json" `
,".\bin\release\net7.0-windows\Keysharp.deps.json" `
,".\bin\release\net7.0-windows\Keysharp.runtimeconfig.json" `
,".\bin\release\net7.0-windows\Keyview.runtimeconfig.json" `
-DestinationPath ".\Keysharp_$AssemblyVersion.zip" -Force `
