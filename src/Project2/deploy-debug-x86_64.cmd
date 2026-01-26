@echo off


dotnet lambda deploy-function ^
--profile "newp" ^
--region "ap-northeast-1" ^
--configuration "Debug" ^
--function-name "test3" ^
--function-runtime "dotnet10" ^
--framework "net10.0" ^
--function-memory-size 512 ^
--function-timeout "5" ^
--function-handler "Project2" ^
--function-architecture "x86_64" ^
--package-type "Zip" ^
--msbuild-parameters "--self-contained true"



pause