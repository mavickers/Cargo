@echo off

if "%1" == "-local" nuget add Cargo\bin\Release\LightPath.Cargo.%2.nupkg -source "d:\Local Packages" && goto:eof
if "%1" == "-nuget" nuget push Cargo\bin\Release\LightPath.Cargo.%2.nupkg -source https://api.nuget.org/v3/index.json && goto:eof

:usage

echo.
echo USAGE
echo.
echo publish -[local^|^|nuget] [version]
echo.

:eof