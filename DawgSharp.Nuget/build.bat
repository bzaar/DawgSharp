echo ************************ Create the nuget package ************************

dotnet restore
dotnet pack -c Release
