namespace ClinicalService.Api.Security;

// Set by the gateway from the JWT; the gateway strips any client-sent values
public static class GatewayHeaders
{
    public const string UserId = "X-User-Id";
    public const string OrganizationId = "X-Organization-Id";
}