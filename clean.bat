@echo off
REM Clean all obj and bin directories

echo Cleaning AxisCore solution...

REM Remove all obj and bin directories
for /d /r %%d in (obj,bin) do @if exist "%%d" rd /s /q "%%d"

echo Restoring packages...
dotnet restore

echo Done! You can now build the solution.
pause
