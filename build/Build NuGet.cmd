
@echo off
"%~dp0..\tools\nuget.exe" restore "%~dp0..\Dianoga.sln"
"%~dp0..\tools\nuget.exe" pack "%~dp0..\src\Dianoga\Dianoga_8.csproj" -Build -Symbols -Properties Configuration=Kentico8
"%~dp0..\tools\nuget.exe" pack "%~dp0..\src\Dianoga\Dianoga_9.csproj" -Build -Symbols -Properties Configuration=Kentico9
"%~dp0..\tools\nuget.exe" pack "%~dp0..\src\Dianoga\Dianoga_10.csproj" -Build -Symbols -Properties Configuration=Kentico10
PAUSE