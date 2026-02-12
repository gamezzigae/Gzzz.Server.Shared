using BenchmarkDotNet.Running;
using ConsoleApp1;
using Gzzz;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var temp = new JsonCompressBenchmark();
Console.WriteLine(temp.Case1());
Console.WriteLine(await temp.Case2());

BenchmarkRunner.Run<JsonCompressBenchmark>();

