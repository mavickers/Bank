@echo off

if "%1" == "-local" nuget add %2 "d:\Local Packages" && goto:eof
if "%1" == "-nuget" nuget push %2 -source https://api.nuget.org/v3/index.json && goto:eof

:usage

echo.
echo USAGE
echo.
echo publish -[local^|^|nuget] [nupkg file]
echo.

:eof