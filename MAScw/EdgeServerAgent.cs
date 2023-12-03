using ActressMas;
using System;

namespace EdgeComputingAuction
{
    public class EdgeServerAgent : Agent
    {
        public int Capacity { get; private set; }
        public int CostPerUnit { get; private set; }

        // Additional properties for strategic decision-making
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
            Send("Auctioneer", $"join {Capacity}");
        }

        public override void Act(Message message)
        {
            Console.WriteLine($"{Name} received message: {message.Content}");

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
            int offerPrice = CalculateOfferPrice();
            Console.WriteLine($"Edge Server {Name} is offering {Capacity} Mb at {offerPrice}p");
            Send("Auctioneer", $"offer {Capacity} {offerPrice} pence");
        }

        private int CalculateOfferPrice()
        {
            // Calculate the number of 10 MB units in the capacity
            int numberOfUnits = (int)Math.Ceiling((double)Capacity / 10);

            // Calculate the base price as cost per 10 MB unit times the number of units
            int basePrice = CostPerUnit * numberOfUnits;

            // Adjusting the price dynamically based on the market and past performance
            int marketAdjustment = DetermineMarketAdjustment();
            int performanceAdjustment = DeterminePerformanceAdjustment();

            int finalOfferPrice = basePrice + marketAdjustment + performanceAdjustment;
            finalOfferPrice = Math.Max(finalOfferPrice, basePrice);

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
