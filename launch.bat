@echo off
rem Runs the web app (http profile). Roll forward so a newer .NET SDK/runtime can run it.
cd /d "%~dp0"
set DOTNET_ROLL_FORWARD=Major
dotnet run --project MyFathersWorkApp/MyFathersWorkApp --launch-profile http
