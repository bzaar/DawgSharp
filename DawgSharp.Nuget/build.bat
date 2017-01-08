set MSBUILD_PATH="%~1"

echo ************************ Build DawgSharp with TargetFramework .Net 4.0 ************************
%MSBUILD_PATH% DawgSharp.msbuild /t:Clean,Build

echo ************************ Build DawgSharp with TargetFramework .Net 3.5 ************************
%MSBUILD_PATH% DawgSharp.msbuild /t:Clean,Build /p:Configuration="Release 3.5"

echo ************************ Create Nuget package *************************************************
del /F /Q *.nupkg
nuget.exe pack DawgSharp.nuspec -NonInteractive -Verbosity detailed