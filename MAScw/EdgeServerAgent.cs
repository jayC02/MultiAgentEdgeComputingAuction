using ActressMas;
using System;

namespace EdgeComputingAuction
{
    public class EdgeServerAgent : Agent
    {
        public int Capacity { get; private set; }
        public int CostPerUnit { get; private set; }

        public EdgeServerAgent(int capacity, int costPerUnit)
        {
            Capacity = capacity;
            CostPerUnit = costPerUnit;
        }

        public override void Setup()
        {
            Console.WriteLine($"Edge Server {Name} set up with capacity {Capacity} Mb at cost {CostPerUnit} per unit.");
            Send("Auctioneer", $"join {Capacity}");
        }

        public override void Act(Message message)
        {
            Console.WriteLine($"Edge Server {Name} received message: {message.Content}");

            if (message.Content == "start")
            {
                MakeOffer();
            }
            else if (message.Content.StartsWith("result"))
            {
                ProcessResult(message.Content);
            }
        }

        private void MakeOffer()
        {
            // Example logic for making an offer
            int offerPrice = CalculateOfferPrice();
            Console.WriteLine($"Edge Server {Name} is offering {Capacity} Mb at {offerPrice}");
            Send("Auctioneer", $"offer {Capacity} {offerPrice}");
        }

        private int CalculateOfferPrice()
        {
            // Implement your pricing strategy here. This is a simple example.
            // Adjust pricing strategy as per your auction rules
            return CostPerUnit * Capacity; // Simple example: total cost based on capacity and cost per unit
        }

        private void ProcessResult(string resultMessage)
        {
            string[] resultInfo = resultMessage.Split(' ');

            if (resultInfo[1] == Name)
            {
                Console.WriteLine($"Edge Server {Name} sold capacity at price {resultInfo[2]}");
                // Further processing if required, like adjusting capacity or confirming resource allocation.
            }
            else
            {
                Console.WriteLine($"Edge Server {Name} did not make a sale. Adjusting strategy for next round.");
                // Adjust strategy for the next round if needed.
            }
        }
    }
}
