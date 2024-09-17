$net = "net9.0-windows"
$FullPath = [System.IO.Path]::GetFullPath("$PWD\bin\release\$net\Keysharp.dll")
$DLLBytes = [System.IO.File]::ReadAllBytes($FullPath)
$Assembly = [System.Reflection.Assembly]::Load($DLLBytes)
$AssemblyVersion = $Assembly.GetName().Version.ToString()

Copy-Item ".\Keysharp.Install\Release\Keysharp.msi" -Destination ".\Keysharp_$AssemblyVersion.msi" -Force
Compress-Archive -LiteralPath `
 ".\bin\release\$net\Keysharp.Core.dll" `
,".\bin\release\$net\Keysharp.exe" `
,".\bin\release\$net\Keysharp.dll" `
,".\bin\release\$net\Keyview.exe" `
,".\bin\release\$net\Keyview.dll" `
,".\bin\release\$net\Interop.IWshRuntimeLibrary.dll" `
,".\bin\release\$net\Microsoft.CodeAnalysis.CSharp.dll" `
,".\bin\release\$net\Microsoft.CodeAnalysis.dll" `
,".\bin\release\$net\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll" `
,".\bin\release\$net\Microsoft.Extensions.DependencyModel.dll" `
,".\bin\release\$net\Microsoft.NET.HostModel.dll" `
,".\bin\release\$net\Scintilla.NET.dll" `
,".\bin\release\$net\System.Management.dll" `
,".\bin\release\$net\Keysharp.Core.deps.json" `
,".\bin\release\$net\Keysharp.deps.json" `
,".\bin\release\$net\Keysharp.runtimeconfig.json" `
,".\bin\release\$net\Keyview.runtimeconfig.json" `
,".\bin\release\$net\x64" `
-DestinationPath ".\Keysharp_$AssemblyVersion.zip" -Force