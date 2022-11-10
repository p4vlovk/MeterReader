namespace MeterReader.Interceptors.Helpers;

using Grpc.Core;

public static class ExceptionHelpers
{
    public static RpcException Handle<T>(
        this Exception exception,
        ILogger<T> logger,
        Guid correlationId)
        => exception switch
        {
            TimeoutException timeoutException => HandleTimeoutException(timeoutException, logger, correlationId),
            RpcException rpcException => HandleRpcException(rpcException, logger, correlationId),
            _ => HandleDefault(exception, logger, correlationId)
        };

    private static RpcException HandleTimeoutException(
        Exception exception,
        ILogger logger,
        Guid correlationId)
    {
        logger.LogError(exception, "CorrelationId: {CorrelationId} - A timeout occurred", correlationId);
        var status = new Status(StatusCode.Internal, "An external resource did not answer within the time limit");
        var trailers = new Metadata { { "CorrelationId", correlationId.ToString() } };

        return new RpcException(status, trailers);
    }
    
    private static RpcException HandleRpcException(
        RpcException exception,
        ILogger logger,
        Guid correlationId)
    {
        logger.LogError(exception, "CorrelationId: {CorrelationId} - An error occurred", correlationId);
        var status = new Status(exception.StatusCode, exception.Message);
        var trailers = exception.Trailers;
        trailers.Add("CorrelationId", correlationId.ToString());

        return new RpcException(status, trailers);
    }
    
    private static RpcException HandleDefault(
        Exception exception,
        ILogger logger,
        Guid correlationId)
    {
        logger.LogError(exception, "CorrelationId: {CorrelationId} - An error occurred", correlationId);
        var status = new Status(StatusCode.Internal, exception.Message);
        var trailers = new Metadata { { "CorrelationId", correlationId.ToString() } };

        return new RpcException(status, trailers);
    }
}