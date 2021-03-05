// Build target list here https://docs.microsoft.com/en-us/dotnet/core/rid-catalog

dotnet build --configuration Release --runtime linux-x64
dotnet build --configuration Release --runtime win-x64
dotnet build --configuration Release --runtime osx-x64

PAUSE