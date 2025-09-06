namespace Sigapi.OpenApi.Extensions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
public sealed class RequireAuthenticationAttribute : Attribute;