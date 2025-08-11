using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using System;
using System.IO;

namespace BenchmarkSuite1
{
	public class Benchmarks
	{
		readonly byte[] _bytes;
		readonly GeminiReusableMemoryStream _gemini = new GeminiReusableMemoryStream();
		readonly ClaudeReusableStream _claude = new ClaudeReusableStream();
		public Benchmarks()
		{
			_bytes = new byte[BufferSize];
			Random.Shared.NextBytes(_bytes);
		}

		[Params(4096, 8192, 16384)]
		public int BufferSize { get; set; }


		[Benchmark]
		public void Scenario1()
		{
			
			for(int i =0; i < 1000; i++)
			{
				_gemini.Reset();
				_gemini.Write(_bytes);
			}
		}

		[Benchmark]
		public void Scenario2()
		{
			for (int i = 0; i < 1000; i++)
			{
				_claude.Reset();
				_claude.Write(_bytes);
			}
		}
	}
}
