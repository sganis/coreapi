:: # configure http.sys web server
:: netsh http add urlacl url=http://*:5000/ user=Users
:: netsh http delete urlacl url=http://*:5000/

dotnet publish -c Release
xcopy /y /e %~dp0bin\Release\netcoreapp2.1\publish c:\coreapi