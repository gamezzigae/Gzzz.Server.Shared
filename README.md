[![github package 배포](https://github.com/gamezzigae/Gzzz.Server.Shared/actions/workflows/Gzzz.AwsFunctionUrlInvoker%20Deployment.yml/badge.svg?branch=main)](https://github.com/gamezzigae/Gzzz.Server.Shared/actions/workflows/Gzzz.AwsFunctionUrlInvoker%20Deployment.yml)
[![unit test](https://github.com/gamezzigae/Gzzz.Server.Shared/actions/workflows/Gzzz.AwsFunctionUrlInvoker.Test%20Deployment.yml/badge.svg?branch=develop)](https://github.com/gamezzigae/Gzzz.Server.Shared/actions/workflows/Gzzz.AwsFunctionUrlInvoker.Test%20Deployment.yml)
[![샘플ARM서버배포](https://github.com/gamezzigae/Gzzz.Server.Shared/actions/workflows/DeployARM64SampleServer.yml/badge.svg?branch=main)](https://github.com/gamezzigae/Gzzz.Server.Shared/actions/workflows/DeployARM64SampleServer.yml)
[![샘플X86_64서버배포](https://github.com/gamezzigae/Gzzz.Server.Shared/actions/workflows/DeployX86_64SampleServer.yml/badge.svg?branch=main)](https://github.com/gamezzigae/Gzzz.Server.Shared/actions/workflows/DeployX86_64SampleServer.yml)


# Gzzz.Server.Shared
aws lambda nativeAoT : aws lambda의 단점을 보완하고 최적의 서비스를 사용하기 위해 개발한 프로젝트

## 왜 AWS Lambda?
- 가격 : 적은 이용량에서 0원, 적당한 이용량이라도 일반적인 컴퓨팅 비용을 넘지 않음
- serverless : 0에서 무제한에 가까운 scaleout을 지원
- 관리비용 : aws cloudwatch를 통하여 대부분의 모니터링을 별도의 작업 없이 즉시 배포하고 이용할 수 있으며 다양한 장애상태를 스스로 극복함. 제한된 인력상황에서 매우 강력한 장점
#### 단점보완
cold start는 기본적으로 가용할 수 있는 유닛이 없는 상태에서 발생한다.
1.활성화된 lambda unit이 없을 때
2.활성화된 lambda unit들이 모두 사용중일 때
Monolithic Lambda를 사용하여 동일한 형상의 lambda에서 집중적으로 메세지를 처리하게 하여 unit의 수를 확보하고 nativeAoT 를 활용하여 빠른 warm up을 가능하게 한다. 2번 케이스의 경우를 최소화 하기 위해서는 짧은 처리시간이 필요하다. 대부분의 처리시간은 db연산에서 발생하기 때문에 일반적이지 않은 db형상이지만 한번만 읽고, 한번만 쓰는것으로 모든 기능을 처리할 수 있도록 설계하였다.
  
## Features
- Idempotancy : 한번의 읽기,쓰기만으로 데이터저장, 멱등성보장, 낙관적 동시성 제어를 수행하여 db비용 최소화
- benchmark.net : 최적의 결과를 얻기 위해 여러방법들을 테스트하고 적용
- Serialize/Deserialize : span, json, Messagepack, Zstd, gzip등
- dependency injection 활용
- xUnit : test project를 mocking없이 di + docker container만 사용하여 테스트환경과 실제 런타임 환경이 매우 유사함
- Redis,Dynamodb : Repository Pattern을 사용하며 낙관적 동시성 제어(Optimistic Concurrency Control) 지원

## 기타
github packages를 통해 배포되며 nuget package manager에서 등록하여 사용가능
