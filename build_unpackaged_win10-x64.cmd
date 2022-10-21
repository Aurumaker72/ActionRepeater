@echo off
setlocal enabledelayedexpansion

for /f "usebackq tokens=*" %%i in (`vswhere.exe -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do (
  set msbuildPath="%%i"
  goto afterLoop
)

:afterLoop

set platform=x64
set runtimeIdentifier=win10-%platform%
%msbuildPath% ".\src\ActionRepeater.UI\ActionRepeater.UI.csproj" -t:restore,rebuild,publish -p:Configuration=Release -p:Platform=%platform% -p:RuntimeIdentifier=%runtimeIdentifier% -p:Version=0.1.0-alpha -p:SelfContained=False -p:PublishReadyToRun=False -p:SatelliteResourceLanguages=en

set publishDir=".\src\ActionRepeater.UI\bin\Release"
set targetPublishDir=".\bin\Release"

echo:
xcopy %publishDir% %targetPublishDir% /i /s /e /y /k /v /q
echo Copied publish directory to %targetPublishDir%

echo:
rmdir %publishDir% /s /q
echo Deleted old publish directory (%publishDir%)

echo:
pause
