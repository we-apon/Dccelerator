
set versionSuffix=

set options=--include-symbols --include-source

set curDir=%cd%
set output=%curdir%\publish
set buildConfiguration=Release

del /F /Q %output%\*

dotnet pack src\Dccelerator.Core -c %buildConfiguration% %options% %versionSuffix%  -o %output%
dotnet pack src\DataAccess\Dccelerator.DataAccess.Common -c %buildConfiguration% %options% %versionSuffix% -o %output%
dotnet pack src\DataAccess\Dccelerator.DataAccess.Lazy -c %buildConfiguration% %options% %versionSuffix% -o %output%
dotnet pack src\DataAccess\Dccelerator.DataAccess.BerkeleyDb -c %buildConfiguration% %options% %versionSuffix% -o %output%
dotnet pack src\DataAccess\Dccelerator.DataAccess.AdoNet -c %buildConfiguration% %options% %versionSuffix% -o %output%
dotnet pack src\DataAccess\Dccelerator.DataAccess.Adapters.Oracle -c %buildConfiguration% %options% %versionSuffix% -o %output%
dotnet pack src\DataAccess\Dccelerator.DataAccess.Adapters.MsSql -c %buildConfiguration% %options% %versionSuffix% -o %output%
dotnet pack src\Dccelerator.TraceSourceAspects -c %buildConfiguration% %options% %versionSuffix% -o %output%

