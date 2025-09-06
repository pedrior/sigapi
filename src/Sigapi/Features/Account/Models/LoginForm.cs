using Sigapi.Common.Scraping;

namespace Sigapi.Features.Account.Models;

[UsedImplicitly(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.Members)]
public sealed class LoginForm
{
    [ValueSelector("#form", Attribute = "action")]
    public string Action { get; set; } = string.Empty;
    
    [AttributeSelector("#form input", Key = "name", Value = "value")]
    public Dictionary<string, string> Data { get; set; } = new();

    public IDictionary<string, string> BuildSubmissionData(string username, string password)
    {
        return new Dictionary<string, string>(Data)
        {
            ["form:login"] = username,
            ["form:senha"] = password,
            ["form:width"] = "1920",
            ["form:height"] = "1080"
        };
    }
}