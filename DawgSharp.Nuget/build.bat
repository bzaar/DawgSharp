echo ************************ Build the project *************************************************

dotnet restore
dotnet build -c Release

echo ************************ Create Nuget package *************************************************
del /F /Q *.nupkg
nuget.exe pack DawgSharp.nuspec -NonInteractive -Verbosity detailed