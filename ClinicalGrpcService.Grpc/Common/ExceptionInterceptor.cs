using System.Diagnostics;
using ClinicalGrpcService.Domain.Exceptions;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace ClinicalGrpcService.Grpc.Common;

public class ExceptionInterceptor(ILogger<ExceptionInterceptor> logger) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "Invalid argument in {Method}", context.Method);
            Activity.Current?.SetStatus(ActivityStatusCode.Error, ex.Message);
            Activity.Current?.AddException(ex);
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (ConflictException ex)
        {
            // Expected business case (duplicate), not a server error:
            // warning only, no stack trace, span not marked as failed
            logger.LogWarning("{ConflictType} in {Method}", ex.GetType().Name, context.Method);
            Activity.Current?.SetTag("conflict.type", ex.GetType().Name);
            throw new RpcException(new Status(StatusCode.AlreadyExists, ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception in {Method}", context.Method);
            Activity.Current?.SetStatus(ActivityStatusCode.Error, ex.Message);
            Activity.Current?.AddException(ex);
            throw new RpcException(new Status(StatusCode.Internal, "An unexpected error occurred."));
        }
    }
}