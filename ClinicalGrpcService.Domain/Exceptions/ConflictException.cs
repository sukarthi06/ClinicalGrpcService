namespace ClinicalGrpcService.Domain.Exceptions;

/// <summary>
/// Base class for "this already exists" business errors. The gRPC interceptor
/// maps it to StatusCode.AlreadyExists.
/// The message is sent to the client and written to the logs,
/// so it must never contain patient data (name, MRN, email, phone and so on).
/// </summary>
public abstract class ConflictException(string message) : Exception(message);