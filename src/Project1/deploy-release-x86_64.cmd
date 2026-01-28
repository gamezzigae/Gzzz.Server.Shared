@echo off


dotnet lambda deploy-function ^
--profile "newp" ^
--region "ap-northeast-1" ^
--configuration "Release" ^
--function-name "test3" ^
--function-runtime "dotnet10" ^
--framework "net10.0" ^
--function-memory-size 512 ^
--function-timeout "5" ^
--function-handler "Project1" ^
--function-architecture "x86_64" ^
--package-type "Zip" ^
--msbuild-parameters "--self-contained true" ^
--container-image-for-build public.ecr.aws/sam/build-dotnet10:latest


pause