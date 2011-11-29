@echo off
SETLOCAL
set TARGET_PATH=%~dp1
set IN_FILENAME="%TARGET_PATH%Version.txt"
set /p BUILD= < %IN_FILENAME%
set BUILD=%BUILD:~22%
if /I "%BUILD%" == "" set BUILD=0
set /a BUILD=%BUILD%+1
echo #define VERSION_BUILD %BUILD% > %IN_FILENAME%
ENDLOCAL