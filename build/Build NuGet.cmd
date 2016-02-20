
@echo off
"%~dp0..\tools\nuget.exe" restore "%~dp0..\Dianoga.sln"
"%~dp0..\tools\nuget.exe" pack "%~dp0..\src\Dianoga\Dianoga.csproj" -Build -Symbols -Properties Configuration=Release
"%~dp0..\tools\nuget.exe" pack "%~dp0..\src\Dianoga\Dianoga_8.2.csproj" -Build -Symbols -Properties Configuration=Release
PAUSE