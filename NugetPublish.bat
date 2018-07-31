
set sourcePath=publish
set version=0.0.21
set source=https://www.nuget.org/api/v2/package
set symbolSource=https://nuget.smbsrc.net

rem nuget push %sourcePath%\Dccelerator.Core.%version%.nupkg -ApiKey %1 -Source %source% -SymbolSource %symbolSource%
nuget push %sourcePath%\Dccelerator.DataAccess.Common.%version%.nupkg -ApiKey %1 -Source %source% -SymbolSource %symbolSource%
rem nuget push %sourcePath%\Dccelerator.DataAccess.Lazy.%version%.nupkg -ApiKey %1 -Source %source% -SymbolSource %symbolSource%
rem nuget push %sourcePath%\Dccelerator.DataAccess.BerkeleyDb.%version%.nupkg -ApiKey %1 -Source %source% -SymbolSource %symbolSource%
rem nuget push %sourcePath%\Dccelerator.DataAccess.AdoNet.%version%.nupkg -ApiKey %1 -Source %source% -SymbolSource %symbolSource%
rem nuget push %sourcePath%\Dccelerator.DataAccess.Adapters.Oracle.%version%.nupkg -ApiKey %1 -Source %source% -SymbolSource %symbolSource%
rem nuget push %sourcePath%\Dccelerator.DataAccess.Adapters.MsSql.%version%.nupkg -ApiKey %1 -Source %source% -SymbolSource %symbolSource%
rem nuget push %sourcePath%\Dccelerator.TraceSourceAspects.%version%.nupkg -ApiKey %1 -Source %source% -SymbolSource %symbolSource%
