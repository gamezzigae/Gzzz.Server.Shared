using Gzzz;
using Gzzz.Authentication;
using Gzzz.AwsFunctionUrlInvoker.Models;
using Gzzz.Controllers;
using System.Text.Json.Serialization;
namespace Project1;

[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(object))]
[JsonSerializable(typeof(DateTimeOffset))]
[JsonSerializable(typeof(ApiContext))]
//
[JsonSerializable(typeof(AuthenticationConfig))]
[JsonSerializable(typeof(SignResponse))]
public partial class CustomJsonContext : JsonSerializerContext
{
}
