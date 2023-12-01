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
            for (int i = 1; i <= 10; i++) // Assuming 5 bidder agents
            {
                int dataRequirement = new Random().Next(10, 50); // Random data requirement between 10 and 50 Mb
                int valuation = new Random().Next(100, 500); // Random valuation between 100 and 500

                var bidderAgent = new BidderAgent(dataRequirement, valuation);
                environment.Add(bidderAgent, $"Bidder{i}");
            }

            // Initialize edge server agents (seller agents)
            for (int i = 1; i <= 10; i++) // Assuming 3 edge server agents
            {
                int capacity = new Random().Next(20, 100); // Random capacity between 20 and 100 Mb
                int costPerUnit = new Random().Next(5, 25); // Random cost per unit between 5 and 25

                var edgeServerAgent = new EdgeServerAgent(capacity, costPerUnit);
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
