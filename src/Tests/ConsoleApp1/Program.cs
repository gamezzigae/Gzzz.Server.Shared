using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.Json;
using BenchmarkDotNet.Attributes;

Console.WriteLine(	);
BenchmarkRunner.Run<TryCatchBenchmark>();
//
//
//
//


[MemoryDiagnoser]
public class TryCatchBenchmark
{


	[Benchmark]
	public void TryCatchNoException()
	{
		try
		{
		}
		catch (Exception)
		{
		}
	}

}
