// See https://aka.ms/new-console-template for more information
using Gzzz.Db.DynamoDb;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Hello, World!");

var serviceProvider = new ServiceCollection()
	.AddSingleton()
	.BuildServiceProvider()
