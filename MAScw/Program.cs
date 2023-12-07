using ActressMas;
using System;

namespace EdgeComputingAuction
{
    class Program
    {
        static void Main(string[] args)
        {
            var environment = new EnvironmentMas();

            // Initialize the auctioneer agent
            var auctioneer = new AuctioneerAgent();
            environment.Add(auctioneer, "Auctioneer");

            // Initialize bidder agents (device agents)
            for (int i = 1; i <= 25; i++)
            {
                int dataRequirement = new Random().Next(10, 50);
                int valuation = new Random().Next(150, 1000);
                int distance = new Random().Next(10, 200);

                var bidderAgent = new BidderAgent(dataRequirement, valuation, distance);
                environment.Add(bidderAgent, $"Bidder{i}");
            }

            // Initialize edge server agents (seller agents)
            for (int i = 1; i <= 25; i++)
            {
                int capacity = new Random().Next(20, 100); 
                int costPerUnit = new Random().Next(50, 250);
                int distance = new Random().Next(10, 200);


                var edgeServerAgent = new EdgeServerAgent(capacity, costPerUnit, distance);
                environment.Add(edgeServerAgent, $"EdgeServer{i}");
            }

            System.Threading.Thread.Sleep(1000); // 1000 milliseconds delay to allow agents to initialize

            auctioneer.StartAuction();

            // Start the environment
            environment.Start();

            Console.WriteLine("Double Auction process has started.");

            Console.ReadLine();
        }
    }
}
