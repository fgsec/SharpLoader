

function console {
    param($text,$type="info")
    $color = "gray"
    $block = "[#]"
    switch ($type){
        "error" { $color = "red";$block="[!]" }
        "ok" { $color = "green";$block="[+]" }
        "alert" { $color = "yellow";$block="[!]" }
    }
    Write-host "$block $text" -f $color
}

function Resolve-PathSafe {
    param ([string] $Path)
    $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Path)
}

function ConvertTo-Base64 {
    param (
        [string] $SourceFilePath,
        [string] $TargetFilePath
    )
 
    $SourceFilePath = Resolve-PathSafe $SourceFilePath
    $TargetFilePath = Resolve-PathSafe $TargetFilePath
     
    $bufferSize = 9000 # should be a multiplier of 3
    $buffer = New-Object byte[] $bufferSize
     
    $reader = [System.IO.File]::OpenRead($SourceFilePath)
    $writer = [System.IO.File]::CreateText($TargetFilePath)
     
    $bytesRead = 0
    do
    {
        $bytesRead = $reader.Read($buffer, 0, $bufferSize);
        $writer.Write([Convert]::ToBase64String($buffer, 0, $bytesRead));
    } while ($bytesRead -eq $bufferSize);
     
    $reader.Dispose()
    $writer.Dispose()
}

function output-log {
    param($output,$path)
    $output | Out-File "$path"
}