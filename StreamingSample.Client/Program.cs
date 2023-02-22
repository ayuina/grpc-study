using System.Diagnostics;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using StreamingSample.Common;

namespace StreamingSample.Client
{

    public class Program
    {
        public static async Task Main(string[] args)
        {
            using (var channel = GrpcChannel.ForAddress(args[0]))
            {
                //await CallUnary(channel);
                //await ReceiveStreaming(channel, args[1]);
                await CalcPiStream(channel, args[1]);
            }
        }

        public static async Task CallUnary(GrpcChannel channel, string name)
        {
            Console.WriteLine("Calling {0}", nameof(CallUnary));

            var client = new MyService.MyServiceClient(channel);
            var res = await client.HelloWorldAsync(new HelloRequest() { Name = name });
            Console.WriteLine(res.Message);
            
        }

        public static async Task ReceiveStreaming(GrpcChannel channel, string name)
        {
            Console.WriteLine("Calling {0}", nameof(ReceiveStreaming));

            var client = new MyService.MyServiceClient(channel);
            using(var streaming = client.HelloServerStream(new HelloRequest() { Name = name, Count = 100000 }))
            {
                await foreach(var res in streaming.ResponseStream.ReadAllAsync())
                {
                    Console.WriteLine(res.Message);
                }
                
            }
        }

        public static async Task CalcPiStream(GrpcChannel channel, string name)
        {
            Console.WriteLine("Calling {0}", nameof(CalcPiStream));

            var client = new MyService.MyServiceClient(channel);
            using (var streaming = client.CalcPiStream(new HelloRequest() { Name = name, Count = 1000000 }))
            {
                var regex = new System.Text.RegularExpressions.Regex("^Pi = (.*)$");
                await foreach (var res in streaming.ResponseStream.ReadAllAsync())
                {
                    var x = regex.Match(res.Message);
                    if(x.Success)
                    {
                        double result = double.Parse(x.Groups[1].Value);
                        Console.WriteLine("{0} : {1}%", res.Message, 100*(result / Math.PI));
                    }
                    else
                    {
                        Console.WriteLine(res.Message);
                    }
                }

            }
        }


    }
}
