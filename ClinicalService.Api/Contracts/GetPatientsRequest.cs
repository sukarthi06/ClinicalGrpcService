using ClinicalService.Api.Security;
using Microsoft.AspNetCore.Mvc;

namespace ClinicalService.Api.Contracts;

public sealed record GetPatientsRequest(
    [FromHeader(Name = GatewayHeaders.OrganizationId)] Guid OrganizationId,
    int PageNumber = 1,
    int PageSize = 50);