
using Gzzz;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

public class LambdaJsonSizeComparison
{
	public static void Main()
	{
		int[] sizes = { 50, 100, 500, 1000, 2000, 5000, 10000 }; // JSON 길이(byte) 기준
		Console.WriteLine("Size\tUTF8\tBase64\tGZip+Base64");

		foreach (int size in sizes)
		{
			var sample = new SampleData
			{
				Id = RandomX.GetRandom(),
				Name = "Test Object",
				Description = RandomX.GetRandomText(size)
			};

			// 1️⃣ JSON → UTF8
			byte[] jsonBytes = JsonSerializer.SerializeToUtf8Bytes(sample);
			string utf8String = Encoding.UTF8.GetString(jsonBytes);

			// 2️⃣ JSON → Base64
			string base64String = Convert.ToBase64String(jsonBytes);

			// 3️⃣ JSON → GZip → Base64
			string gzipBase64String = Convert.ToBase64String(CompressGzip(jsonBytes, CompressionLevel.Optimal));
			string gzipBase64String2 = Convert.ToBase64String(CompressGzip(jsonBytes, CompressionLevel.SmallestSize));

			Console.WriteLine($"{jsonBytes.Length}\t{utf8String.Length}\t{base64String.Length}\t{gzipBase64String.Length}\t{gzipBase64String2.Length}");
		}
	}

	public static byte[] CompressGzip(byte[] data, CompressionLevel compressionLevel)
	{
		using var output = new MemoryStream();
		using (var gzip = new GZipStream(output, compressionLevel, leaveOpen: true))
		{
			gzip.Write(data, 0, data.Length);
		}
		return output.ToArray();
	}

	public class SampleData
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
	}
}
