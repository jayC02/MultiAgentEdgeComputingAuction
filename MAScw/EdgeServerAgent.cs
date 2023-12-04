using ActressMas;
using System;

namespace EdgeComputingAuction
{
    public class EdgeServerAgent : Agent
    {
        public int Capacity { get; private set; }
        public int CostPerUnit { get; private set; }

        private int LastSalePrice;
        private bool MadeSaleLastRound;
        private int RoundsWithoutSale;

        public EdgeServerAgent(int capacity, int costPerUnit)
        {
            Capacity = capacity;
            CostPerUnit = costPerUnit;
            LastSalePrice = 0;
            MadeSaleLastRound = false;
            RoundsWithoutSale = 0;
        }

        public override void Setup()
        {
            Console.WriteLine($"{Name} set up with capacity {Capacity} Mb at cost {CostPerUnit} pence per 10MB");
            Send("Auctioneer", $"join {Capacity}MB");
        }

        public override void Act(Message message)
        {
            Console.WriteLine($"{Name} received message: {message.Content}");

            if (message.Content == "start" || message.Content == "new round")
            {
                //reset sale status
                MadeSaleLastRound = false; 
                //create a new offer for new round
                MakeOffer();
            }
            else if (message.Content.StartsWith("result"))
            {
                ProcessResult(message.Content);
            }
        }

        private void MakeOffer()
        {
            double offerPrice = CalculateOfferPrice();
            int offerPriceInt = (int)Math.Round(offerPrice);
            Console.WriteLine($"Edge Server {Name} is offering {Capacity} Mb at {offerPriceInt}p");
            Send("Auctioneer", $"offer {offerPriceInt} {Capacity}");
        }


        private double CalculateOfferPrice()
        {
            double costPerMB = CostPerUnit / 10.0;

            double basePrice = costPerMB * Capacity;

            // Adjusting the price dynamically
            int marketAdjustment = DetermineMarketAdjustment();
            int performanceAdjustment = DeterminePerformanceAdjustment();

            double finalOfferPrice = basePrice + marketAdjustment + performanceAdjustment;

            // Return the exact decimal value
            return finalOfferPrice;
        }

        private int DetermineMarketAdjustment()
        {
            // Logic to determine market adjustment
            // This could involve analyzing overall demand and supply dynamics
            return 0; // Placeholder
        }

        private int DeterminePerformanceAdjustment()
        {
            // Adjust price based on previous round's performance
            if (MadeSaleLastRound)
            {
                // If sale was made, consider slight increment or stay stable
                return 5; // Example value
            }
            else
            {
                // If no sale was made, consider decrementing the price
                RoundsWithoutSale++;
                return -RoundsWithoutSale * 5; // Decrement more with each round without a sale
            }
        }

        private void ProcessResult(string resultMessage)
        {
            string[] resultInfo = resultMessage.Split(' ');

            if (resultInfo[1] == Name)
            {
                LastSalePrice = int.Parse(resultInfo[2]);
                MadeSaleLastRound = true;
                RoundsWithoutSale = 0;
                Console.WriteLine($"Edge Server {Name} sold capacity at price {LastSalePrice}");
            }
            else
            {
                MadeSaleLastRound = false;
                Console.WriteLine($"Edge Server {Name} did not make a sale. Adjusting strategy for next round.");
            }
        }
    }
}
