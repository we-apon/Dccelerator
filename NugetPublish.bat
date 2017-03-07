
set sourcePath=publish
set version=0.0.12-rc9.symbols

#nuget push %sourcePath%\Dccelerator.Core.%version%.nupkg -ApiKey %1 -Source https://www.nuget.org/api/v2/package
#nuget push %sourcePath%\Dccelerator.DataAccess.Common.%version%.nupkg -ApiKey %1 -Source https://www.nuget.org/api/v2/package
#nuget push %sourcePath%\Dccelerator.DataAccess.Lazy.%version%.nupkg -ApiKey %1 -Source https://www.nuget.org/api/v2/package
#nuget push %sourcePath%\Dccelerator.DataAccess.BerkeleyDb.%version%.nupkg -ApiKey %1 -Source https://www.nuget.org/api/v2/package
#nuget push %sourcePath%\Dccelerator.DataAccess.AdoNet.%version%.nupkg -ApiKey %1 -Source https://www.nuget.org/api/v2/package
#nuget push %sourcePath%\Dccelerator.DataAccess.Adapters.Oracle.%version%.nupkg -ApiKey %1 -Source https://www.nuget.org/api/v2/package
#nuget push %sourcePath%\Dccelerator.DataAccess.Adapters.MsSql.%version%.nupkg -ApiKey %1 -Source https://www.nuget.org/api/v2/package
nuget push %sourcePath%\Dccelerator.TraceSourceAspects.%version%.nupkg -ApiKey %1 -Source https://www.nuget.org/api/v2/package
