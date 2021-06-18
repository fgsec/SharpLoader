param($folder)

. $PSScriptRoot\shared_functions.ps1

. $PSScriptRoot\dotnettojscript\DotNetToJScript.exe  $folder\csharpbin.exe -c Sharploader.Execute --lang=Jscript --ver=v4 -o $folder\runner.js

# Since we have scripts that depends on this, let's do things all here!

if(Test-Path("$folder\runner.js")) {
    
    $jsrunner = get-content "$folder\runner.js"
    $xlsrunner = @"
<?xml version='1.0'?>
<stylesheet version="1.0"
xmlns="http://www.w3.org/1999/XSL/Transform"
xmlns:ms="urn:schemas-microsoft-com:xslt"
xmlns:user="http://mycompany.com/mynamespace">
<output method="text"/>
 <ms:script implements-prefix="user" language="JScript">
 <![CDATA[
  $jsrunner
 ]]>
 </ms:script>
</stylesheet>
"@

} 


New-Item -Path "$folder" -Name "runner.xsl" -ItemType "file" -Value $xlsrunner -force


$n = $MyInvocation.MyCommand.Name

if(Test-Path("$folder\runner.js")) {
    console -text "Executed: $n" -type "ok"
} else {
    console -text "Executed: $n" -type "error"
}