@echo off
SETLOCAL
set TARGET_PATH=%~dp1
set IN_FILENAME="%TARGET_PATH%Version.txt"
set OUT_FILENAME="%TARGET_PATH%Version.cs"
set /p BUILD= < %IN_FILENAME%
set BUILD=%BUILD:~22%
if /I "%BUILD%" == "" set BUILD=0
set /a BUILD=%BUILD%+1
echo using System; > %OUT_FILENAME%
echo namespace idTech4 { >> %OUT_FILENAME%
echo  internal class idVersion { >> %OUT_FILENAME%
echo    public const int BuildCount   = %BUILD%; >> %OUT_FILENAME%
echo    public const string BuildDate   = "%DATE%"; >> %OUT_FILENAME%
echo    public const string BuildTime   = "%TIME%"; >> %OUT_FILENAME%
echo  } >> %OUT_FILENAME%
echo } >> %OUT_FILENAME%
ENDLOCAL