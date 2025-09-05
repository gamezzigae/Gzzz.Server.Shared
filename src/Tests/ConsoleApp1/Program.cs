using BenchmarkDotNet.Running;
using ConsoleApp1;
using Gzzz;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


BenchmarkRunner.Run<AuthenticationTokensSerializeBenchmark>();
