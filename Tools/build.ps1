param([ValidateSet('Setup','Web','YouTube','Android','AndroidApk','IOS','Windows','ValidateLevels')][string]$Target='Web')
$ErrorActionPreference='Stop'
$projectPath=Split-Path $PSScriptRoot -Parent
$unityExe='C:\Program Files\Unity\Hub\Editor\6000.3.17f1\Editor\Unity.exe'
if(!(Test-Path -LiteralPath $unityExe)){throw 'Install Unity 6000.3.17f1 with Android, iOS, and WebGL modules, or update the editor path in Tools/build.ps1.'}
$logPath=Join-Path $projectPath ($Target.ToLower()+'.log')
$arguments=@('-batchmode','-nographics','-projectPath',('"'+$projectPath+'"'),'-executeMethod',('SockPairs.Editor.BuildTools.'+$Target),'-quit','-logFile',('"'+$logPath+'"'))
$process=Start-Process -FilePath $unityExe -ArgumentList $arguments -WindowStyle Hidden -PassThru -Wait
Get-Content -LiteralPath $logPath -Tail 30
if($process.ExitCode -ne 0){throw "Unity failed ($($process.ExitCode)). See $logPath"}
