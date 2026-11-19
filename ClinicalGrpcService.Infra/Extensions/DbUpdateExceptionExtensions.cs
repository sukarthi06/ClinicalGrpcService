using Npgsql;

namespace ClinicalGrpcService.Infra.Extensions;

public static class DbUpdateExceptionExtensions
{
    // The only place that knows the database is Postgres
    public static bool IsUniqueViolation(this DbUpdateException ex) =>
        ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}