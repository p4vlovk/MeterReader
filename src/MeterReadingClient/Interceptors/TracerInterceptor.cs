namespace MeterReadingClient.Interceptors;

using Grpc.Core;
using Grpc.Core.Interceptors;

public class TracerInterceptor : Interceptor
{
    private readonly ILogger<TracerInterceptor> logger;

    public TracerInterceptor(ILogger<TracerInterceptor> logger)
        => this.logger = logger;

    public override TResponse BlockingUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        where TRequest : class
        where TResponse : class
    {
        this.logger.LogInformation(
            "Calling {MethodName} {MethodType} method. Payload received: {Type} : {Request}",
            context.Method.Name,
            context.Method.Type,
            request.GetType(),
            request);
        
        this.logger.LogInformation(
            "Calling {MethodName} {MethodType} method at {UtcNow} UTC from machine {MachineName}",
            context.Method.Name,
            context.Method.Type,
            DateTime.UtcNow,
            Environment.MachineName);

        return continuation(request, context);
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        where TRequest : class
        where TResponse : class
    {
        this.logger.LogInformation(
            "Calling {MethodName} {MethodType} method. Payload received: {Type} : {Request}",
            context.Method.Name,
            context.Method.Type,
            request.GetType(),
            request);
        
        this.logger.LogInformation(
            "Calling {MethodName} {MethodType} method at {UtcNow} UTC from machine {MachineName}",
            context.Method.Name,
            context.Method.Type,
            DateTime.UtcNow,
            Environment.MachineName);

        return continuation(request, context);
    }

    public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
        where TRequest : class
        where TResponse : class
    {
        this.logger.LogDebug(
            "Calling {MethodName} {MethodType} method at {UtcNow} UTC from machine {MachineName}",
            context.Method.Name,
            context.Method.Type,
            DateTime.UtcNow,
            Environment.MachineName);
        
        return continuation(context);
    }

    public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        where TRequest : class
        where TResponse : class
    {
        this.logger.LogDebug(
            "Calling {MethodName} {MethodType} method. Payload received: {Type} : {Request}",
            context.Method.Name,
            context.Method.Type,
            request.GetType(),
            request);
        
        this.logger.LogDebug(
            "Calling {MethodName} {MethodType} method at {UtcNow} UTC from machine {MachineName}",
            context.Method.Name,
            context.Method.Type,
            DateTime.UtcNow,
            Environment.MachineName);
        
        return continuation(request, context);
    }

    public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        this.logger.LogDebug(
            "Calling {MethodName} {MethodType} method at {UtcNow} UTC from machine {MachineName}",
            context.Method.Name,
            context.Method.Type,
            DateTime.UtcNow,
            Environment.MachineName);
        
        return continuation(context);
    }
}