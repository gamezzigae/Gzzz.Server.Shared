using Gzzz.AwsFunctionUrlInvoker.Models;
using System.Text.Json.Serialization;

namespace Gzzz.AwsFunctionUrlInvoker;

[JsonSerializable(typeof(FunctionUrlRequest))]
[JsonSerializable(typeof(FunctionUrlResponse))]
public partial class FunctionUrlJsonContext : JsonSerializerContext
{
}
