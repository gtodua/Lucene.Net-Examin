param (
	[Parameter(Mandatory=$true)]
	[ValidatePattern("\d+?\.\d+?\.\d+?\.\d")]
	[string]
	$ReleaseVersionNumber
)

$PSScriptFilePath = (Get-Item $MyInvocation.MyCommand.Path).FullName

$SolutionRoot = Split-Path -Path $PSScriptFilePath -Parent

$Is64BitSystem = (Get-WmiObject -Class Win32_OperatingSystem).OsArchitecture -eq "64-bit"
$Is64BitProcess = [IntPtr]::Size -eq 8

$RegistryArchitecturePath = ""
if ($Is64BitProcess) { $RegistryArchitecturePath = "\Wow6432Node" }

$FrameworkArchitecturePath = ""
if ($Is64BitSystem) { $FrameworkArchitecturePath = "64" }

$ClrVersion = (Get-ItemProperty -path "HKLM:\SOFTWARE$RegistryArchitecturePath\Microsoft\VisualStudio\10.0")."CLR Version"

$MSBuild = "$Env:SYSTEMROOT\Microsoft.NET\Framework$FrameworkArchitecturePath\$ClrVersion\MSBuild.exe"

# Make sure we don't have a release folder for this version already
$BuildFolder = Join-Path -Path $SolutionRoot -ChildPath "build";
$ReleaseFolder = Join-Path -Path $BuildFolder -ChildPath "Releases\v$ReleaseVersionNumber";
if ((Get-Item $ReleaseFolder -ErrorAction SilentlyContinue) -ne $null)
{
	Write-Warning "$ReleaseFolder already exists on your local machine. It will now be deleted."
	Remove-Item $ReleaseFolder -Recurse
}

# Set the version number in SolutionInfo.cs
$SolutionInfoPath = Join-Path -Path $SolutionRoot -ChildPath "SolutionInfo.cs"
(gc -Path $SolutionInfoPath) `
	-replace "(?<=Version\(`")[.\d]*(?=`"\))", $ReleaseVersionNumber |
	sc -Path $SolutionInfoPath -Encoding UTF8

# Build the solution in release mode
$SolutionPath = Join-Path -Path $SolutionRoot -ChildPath "Examine.sln"
& $MSBuild "$SolutionPath" /p:Configuration=Release /maxcpucount /t:Clean
if (-not $?)
{
	throw "The MSBuild process returned an error code."
}
& $MSBuild "$SolutionPath" /p:Configuration=Release /maxcpucount
if (-not $?)
{
	throw "The MSBuild process returned an error code."
}

$CoreExamineFolder = Join-Path -Path $ReleaseFolder -ChildPath "Examine";
$UmbExamineFolder = Join-Path -Path $ReleaseFolder -ChildPath "UmbracoExamine";
$UmbExaminePDFFolder = Join-Path -Path $ReleaseFolder -ChildPath "UmbracoExaminePDF";
$WebExamineFolder = Join-Path -Path $ReleaseFolder -ChildPath "ExamineWebDemo";
$ExamineAzureFolder = Join-Path -Path $ReleaseFolder -ChildPath "Examine.Azure";
$UmbracoExamineAzureFolder = Join-Path -Path $ReleaseFolder -ChildPath "UmbracoExamine.Azure";
$UmbracoExaminePDFAzureFolder  = Join-Path -Path $ReleaseFolder -ChildPath "UmbracoExamine.PDF.Azure";


New-Item $CoreExamineFolder -Type directory
New-Item $UmbExamineFolder -Type directory
New-Item $UmbExaminePDFFolder -Type directory
New-Item $WebExamineFolder -Type directory
New-Item $ExamineAzureFolder -Type directory
New-Item $UmbracoExamineAzureFolder -Type directory
New-Item $UmbracoExaminePDFAzureFolder -Type directory

$include = @('*Examine*.dll','*Lucene*.dll','ICSharpCode.SharpZipLib.dll')
$CoreExamineBinFolder = Join-Path -Path $SolutionRoot -ChildPath "Projects\Examine\bin\Release";
Copy-Item "$CoreExamineBinFolder\*.dll" -Destination $CoreExamineFolder -Include $include

$UmbExamineBinFolder = Join-Path -Path $SolutionRoot -ChildPath "Projects\UmbracoExamine\bin\Release";
Copy-Item "$UmbExamineBinFolder\*.dll" -Destination $UmbExamineFolder -Include $include

$include = @('UmbracoExamine.PDF.dll','itextsharp.dll')
$UmbExaminePDFBinFolder = Join-Path -Path $SolutionRoot -ChildPath "Projects\UmbracoExamine.PDF\bin\Release";
Copy-Item "$UmbExaminePDFBinFolder\*.dll" -Destination $UmbExaminePDFFolder -Include $include

$include = @('*Examine*.dll','*Lucene*.dll', '*Azure*.dll','ICSharpCode.SharpZipLib.dll')
$ExamineAzureBinFolder = Join-Path -Path $SolutionRoot -ChildPath "Projects\Examine.Azure\bin\Release";
Copy-Item "$ExamineAzureBinFolder\*.dll" -Destination $ExamineAzureFolder -Include $include

$UmbracoExamineAzureBinFolder = Join-Path -Path $SolutionRoot -ChildPath "Projects\UmbracoExamine.Azure\bin\Release";
Copy-Item "$UmbracoExamineAzureBinFolder\*.dll" -Destination $UmbracoExamineAzureFolder -Include $include

$include = @('UmbracoExamine.PDF.Azure.dll','UmbracoExamine.PDF.dll','itextsharp.dll')
$UmbExaminePDFAzureBinFolder = Join-Path -Path $SolutionRoot -ChildPath "Projects\UmbracoExamine.PDF.Azure\bin\Release";
Copy-Item "$UmbExaminePDFAzureBinFolder\*.dll" -Destination $UmbracoExaminePDFAzureFolder -Include $include


$ExamineWebDemoFolder = Join-Path -Path $SolutionRoot -ChildPath "Projects\Examine.Web.Demo";
Copy-Item "$ExamineWebDemoFolder\*" -Destination $WebExamineFolder -Recurse
$IndexSet = Join-Path $WebExamineFolder -ChildPath "App_Data\SimpleIndexSet2";
$include = @('*.sdf','SimpleIndexSet2*')
Remove-Item $IndexSet -Recurse
$SqlCeDb = Join-Path $WebExamineFolder -ChildPath "App_Data\Database1.sdf";
Remove-Item $SqlCeDb 

""
"Build $ReleaseVersionNumber is done!"