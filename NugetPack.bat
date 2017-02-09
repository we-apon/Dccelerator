
set versionSuffix=--version-suffix rc7
set output=publish
set buildConfiguration=Debug

del /F /Q %output%\*

#dotnet pack src\Dccelerator.Core -c %buildConfiguration% %versionSuffix% -o %output%
dotnet pack src\DataAccess\Dccelerator.DataAccess.Common -c %buildConfiguration% %versionSuffix% -o %output%
#dotnet pack src\DataAccess\Dccelerator.DataAccess.Lazy -c %buildConfiguration% %versionSuffix% -o %output%
#dotnet pack src\DataAccess\Dccelerator.DataAccess.BerkeleyDb -c %buildConfiguration% %versionSuffix% -o %output%
#dotnet pack src\DataAccess\Dccelerator.DataAccess.AdoNet -c %buildConfiguration% %versionSuffix% -o %output%
#dotnet pack src\DataAccess\Dccelerator.DataAccess.Adapters.Oracle -c %buildConfiguration% %versionSuffix% -o %output%
#dotnet pack src\DataAccess\Dccelerator.DataAccess.Adapters.MsSql -c %buildConfiguration% %versionSuffix% -o %output%

