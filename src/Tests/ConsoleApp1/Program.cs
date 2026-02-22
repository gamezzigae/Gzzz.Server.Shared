using Amazon.DynamoDBv2.Model;
using Gzzz.Controllers;
using Gzzz.Db.DynamoDb;
using System.Linq.Expressions;
using System.Reflection;

var xx = new ParameterInfoBenchmark();


await xx.Case1Async();
await xx.Case2Async();
BenchmarkRunner.Run<ParameterInfoBenchmark>();
