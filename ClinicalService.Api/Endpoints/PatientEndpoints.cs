namespace ClinicalService.Api.Endpoints;

public static class PatientEndpoints
{
    public static IEndpointRouteBuilder MapPatientEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/patients").WithTags("Patients");

        // Patient data: browsers and proxies must never cache these responses
        group.AddEndpointFilter(async (context, next) =>
        {
            context.HttpContext.Response.Headers.CacheControl = "no-store";
            return await next(context);
        });

        group.MapPost("/", CreateAsync)
            .WithName("CreatePatient")
            .WithSummary("Creates a patient, or reactivates an inactive patient with the same MRN.")
            .Produces<PatientResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithValidation<PatientRequest>();

        group.MapPost("/bulk", CreateRangeAsync)
            .WithName("CreatePatients")
            .WithSummary("Creates up to 1000 patients; returns an outcome for each row.")
            .Produces<CreatePatientsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithValidation<CreatePatientsRequest>();

        group.MapGet("/", GetAllAsync)
            .WithName("GetPatients")
            .WithSummary("Lists the organization's active patients, one page at a time.")
            .Produces<IReadOnlyList<PatientResponse>>(StatusCodes.Status200OK)
            .WithValidation<GetPatientsRequest>();

        group.MapGet("/{patientId:guid}", GetByIdAsync)
            .WithName("GetPatient")
            .WithSummary("Gets one active patient of the organization.")
            .Produces<PatientResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // POST, not GET: search values are patient data and must not appear in URLs
        group.MapPost("/search", SearchAsync)
            .WithName("SearchPatients")
            .WithSummary("Searches by MRN, email, phone and/or whole-word name.")
            .Produces<IReadOnlyList<PatientResponse>>(StatusCodes.Status200OK)
            .WithValidation<SearchPatientsRequest>();

        group.MapPut("/{patientId:guid}", UpdateAsync)
            .WithName("UpdatePatient")
            .WithSummary("Updates an active patient's details.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithValidation<PatientRequest>();

        group.MapDelete("/{patientId:guid}", DeleteAsync)
            .WithName("DeletePatient")
            .WithSummary("Soft-deletes an active patient.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{patientId:guid}/recordings", LinkRecordingAsync)
            .WithName("LinkPatientRecording")
            .WithSummary("Links a recording to an active patient.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithValidation<LinkRecordingRequest>();

        return app;
    }

    private static async Task<Created<PatientResponse>> CreateAsync(
        PatientRequest request,
        [FromHeader(Name = GatewayHeaders.OrganizationId)] Guid organizationId,
        [FromHeader(Name = GatewayHeaders.UserId)] Guid? userId,
        IPatientService patientService,
        CancellationToken ct)
    {
        var patient = await patientService.CreateAsync(
            request.ToPatient(OrganizationId.Of(organizationId), userId), ct);

        return TypedResults.Created($"/api/patients/{patient.PatientId.Value}", patient.ToResponse());
    }

    private static async Task<Ok<CreatePatientsResponse>> CreateRangeAsync(
        CreatePatientsRequest request,
        [FromHeader(Name = GatewayHeaders.OrganizationId)] Guid organizationId,
        [FromHeader(Name = GatewayHeaders.UserId)] Guid? userId,
        IPatientService patientService,
        CancellationToken ct)
    {
        var orgId = OrganizationId.Of(organizationId);
        var patients = request.Patients.Select(p => p.ToPatient(orgId, userId)).ToList();

        var result = await patientService.CreateRangeAsync(orgId, patients, ct);
        return TypedResults.Ok(result.ToResponse());
    }

    private static async Task<Ok<IReadOnlyList<PatientResponse>>> GetAllAsync(
        [AsParameters] GetPatientsRequest request,
        IPatientService patientService,
        CancellationToken ct)
    {
        var patients = await patientService.GetAllAsync(
            OrganizationId.Of(request.OrganizationId), request.PageNumber, request.PageSize, ct);

        return TypedResults.Ok(patients.ToResponse());
    }

    private static async Task<Results<Ok<PatientResponse>, NotFound>> GetByIdAsync(
        Guid patientId,
        [FromHeader(Name = GatewayHeaders.OrganizationId)] Guid organizationId,
        IPatientService patientService,
        CancellationToken ct)
    {
        var patient = await patientService.GetByIdAsync(
            PatientId.Of(patientId), OrganizationId.Of(organizationId), ct);

        return patient is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(patient.ToResponse());
    }

    private static async Task<Ok<IReadOnlyList<PatientResponse>>> SearchAsync(
        SearchPatientsRequest request,
        [FromHeader(Name = GatewayHeaders.OrganizationId)] Guid organizationId,
        IPatientService patientService,
        CancellationToken ct)
    {
        var patients = await patientService.SearchAsync(
            OrganizationId.Of(organizationId),
            request.Mrn, request.Email, request.Phone, request.Name,
            request.PageNumber, request.PageSize, ct);

        return TypedResults.Ok(patients.ToResponse());
    }

    private static async Task<Results<NoContent, NotFound>> UpdateAsync(
        Guid patientId,
        PatientRequest request,
        [FromHeader(Name = GatewayHeaders.OrganizationId)] Guid organizationId,
        [FromHeader(Name = GatewayHeaders.UserId)] Guid? userId,
        IPatientService patientService,
        CancellationToken ct)
    {
        var patient = request.ToPatient(OrganizationId.Of(organizationId), userId);
        patient.PatientId = PatientId.Of(patientId);

        return await patientService.UpdateAsync(patient, ct)
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        Guid patientId,
        [FromHeader(Name = GatewayHeaders.OrganizationId)] Guid organizationId,
        [FromHeader(Name = GatewayHeaders.UserId)] Guid? userId,
        IPatientService patientService,
        CancellationToken ct)
    {
        return await patientService.DeleteAsync(
                PatientId.Of(patientId), OrganizationId.Of(organizationId), userId, ct)
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }

    private static async Task<Results<NoContent, NotFound>> LinkRecordingAsync(
        Guid patientId,
        LinkRecordingRequest request,
        [FromHeader(Name = GatewayHeaders.UserId)] Guid? userId,
        IPatientService patientService,
        CancellationToken ct)
    {
        return await patientService.LinkRecordingAsync(
                PatientId.Of(patientId), RecordingId.Of(request.RecordingId), userId, ct)
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }
}