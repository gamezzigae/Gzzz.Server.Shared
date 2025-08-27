@echo off

pushd Project1


dotnet lambda deploy-function ^
--region "ap-northeast-1" ^
--configuration "Release" ^
--function-name "test3" ^
--function-runtime "dotnet8" ^
--framework "net8.0" ^
--function-memory-size "512" ^
--function-timeout "10" ^
--function-handler "Project1" ^
--function-architecture "arm64" ^
--package-type "Zip" ^
--msbuild-parameters "--self-contained true"

popd

pause