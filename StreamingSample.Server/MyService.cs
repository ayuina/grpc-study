using Grpc.Core;
using StreamingSample.Server;
using StreamingSample.Common;

namespace StreamingSample.Server;

public class MyService : StreamingSample.Common.MyService.MyServiceBase
{
    private readonly ILogger<MyService> _logger;
    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    public override async Task<HelloResponse> HelloWorld(HelloRequest request, Grpc.Core.ServerCallContext context)
    {
        _logger.LogInformation($"{context.Host} {context.Method}");
        // _logger.LogInformation("{0} : {1} = {2}", nameof(HelloWorld), nameof(context.Deadline), context.Deadline);
        // _logger.LogInformation("{0} : {1} = {2}", nameof(HelloWorld), nameof(context.Host), context.Host);
        // _logger.LogInformation("{0} : {1} = {2}", nameof(HelloWorld), nameof(context.Method), context.Method);
        // _logger.LogInformation("{0} : {1} = {2}", nameof(HelloWorld), nameof(context.Peer), context.Peer);
        // _logger.LogInformation("{0} : {1} = {2}", nameof(HelloWorld), nameof(context.Status), context.Status);
        // _logger.LogInformation("{0} : {1} = {2}", nameof(HelloWorld), nameof(context.UserState), context.UserState);

        var res = new HelloResponse()
        {
            Message = $"Hello {request.Name}, this is {Environment.MachineName} ruuning on {context.Host}"
        };
        return await Task.FromResult(res);
    }

    public override async Task HelloServerStream(HelloRequest request, IServerStreamWriter<HelloResponse> responseStream, ServerCallContext context)
    {
        _logger.LogInformation($"Starting {context.Method} on {Environment.MachineName} for {request.Name}[{context.Peer}]");
        var sw = System.Diagnostics.Stopwatch.StartNew();

        await responseStream.WriteAsync(new HelloResponse() { Message = $"Starting server streaming for {request.Name} ..." });
        var rand = new Random();
        for(int i = 0; i < request.Count; i++)
        {
            //_logger.LogInformation("sending {0}", i);
            await Task.Delay(rand.Next(100));
            if (!context.CancellationToken.IsCancellationRequested)
            {
                await responseStream.WriteAsync(new HelloResponse() { Message = $"Hello {i}" });
            }
            else
            {
                break;
            }
        }

        sw.Stop();
        _logger.LogInformation($"Stopping {context.Method} on {Environment.MachineName} for {request.Name}[{context.Peer}], duration {sw.Elapsed.TotalSeconds}");

    }

    public override async Task CalcPiStream(HelloRequest request, IServerStreamWriter<HelloResponse> responseStream, ServerCallContext context)
    {
        _logger.LogInformation($"Starting {context.Method} on {Environment.MachineName} for {request.Name}[{context.Peer}]");
        var sw = System.Diagnostics.Stopwatch.StartNew();

        await responseStream.WriteAsync(new HelloResponse() { Message = $"Starting calculate pi for {request.Name} ..." });
        var rand = new Random();
        long inner = 0;
        for (long count = 1; count < long.MaxValue; count++)
        {
            var x = rand.NextDouble();
            var y = rand.NextDouble();
            var length = Math.Sqrt(x*x + y*y);
            if (length <= 1) inner++;
            if(context.CancellationToken.IsCancellationRequested)
            {
                break;
            }
            else 
            {
                if(count % request.Count == 0)
                {
                    await responseStream.WriteAsync(new HelloResponse() { Message = $"Pi = {4* (double)inner / (double)count}" });
                }
            }
        }

        sw.Stop();
        _logger.LogInformation($"Stopping {context.Method} on {Environment.MachineName} for {request.Name}[{context.Peer}], duration {sw.Elapsed.TotalSeconds}");
    }

}
