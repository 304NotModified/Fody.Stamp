nunit3-console "Tests\bin\Release\net462\Tests.dll"
dotnet test "Tests\Tests.csproj"  -f netcoreapp2.1 --configuration Release --no-build /property:Platform=AnyCPU