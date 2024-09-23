# Before running:
#   Initial install: dotnet tool install --global dotnet-ef
#   Update: dotnet tool update --global dotnet-ef

dotnet ef dbcontext optimize --output-dir Database/Model/Api/Compiled --project Sovereign.Core --context BansContext
dotnet ef dbcontext optimize --output-dir Database/Model/Bans/Compiled --project Sovereign.Core --context GameBansContext