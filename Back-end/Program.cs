using Nancy.Hosting.Self;
using System.Threading;

namespace DENMAP_SERVER
{
    class Program
    {
        static void Main(string[] args)
        {
            var uri = new Uri("http://localhost:80");

            using (var host = new NancyHost(uri))
            {
                host.Start();
                Console.WriteLine($"NancyFX is running on {uri}.");
                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}
