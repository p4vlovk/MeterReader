namespace MeterReader.Interceptors;

using Grpc.Core;
using Grpc.Core.Interceptors;
using MeterReader.Interceptors.Helpers;

public class ExceptionInterceptor : Interceptor
{
    private readonly ILogger<ExceptionInterceptor> logger;
    private readonly Guid correlationId;

    public ExceptionInterceptor(ILogger<ExceptionInterceptor> logger)
    {
        this.logger = logger;
        this.correlationId = Guid.NewGuid();
    }
    
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (Exception ex)
        {
            throw ex.Handle(this.logger, this.correlationId);
        }
    }

    public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        ServerCallContext context,
        ClientStreamingServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(requestStream, context);
        }
        catch (Exception ex)
        {
            throw ex.Handle(this.logger, this.correlationId);
        }
    }

    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            await continuation(request, responseStream, context);
        }
        catch (Exception ex)
        {
            throw ex.Handle(this.logger, this.correlationId);
        }
    }

    public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        DuplexStreamingServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            await continuation(requestStream, responseStream, context);
        }
        catch (Exception ex)
        {
            throw ex.Handle(this.logger, this.correlationId);
        }
    }
}