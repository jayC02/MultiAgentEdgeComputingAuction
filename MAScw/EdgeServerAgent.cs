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
        private const int PriceDecreaseFactor = 100;
        private const int MinimumPricePerMB = 1; // Minimum price per MB to avoid negative pricing

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
                MadeSaleLastRound = false;
                MakeOffer();
            }
            else if (message.Content.StartsWith("result"))
            {
                ProcessResult(message.Content);
            }
        }

        private void MakeOffer()
        {
            double offerPrice = CalculateDynamicOfferPrice();
            int offerPriceInt = (int)Math.Round(offerPrice);
            Console.WriteLine($"Edge Server {Name} is offering {Capacity} Mb at {offerPriceInt}p");
            Send("Auctioneer", $"offer {offerPriceInt} {Capacity}");
        }

        private double CalculateDynamicOfferPrice()
        {
            double basePrice = CalculateBasePrice();
            double performanceAdjustment = DeterminePerformanceAdjustment();

            double dynamicPrice = basePrice + performanceAdjustment;
            return Math.Max(dynamicPrice, MinimumPricePerMB * Capacity);
        }

        private double CalculateBasePrice()
        {
            double costPerMB = CostPerUnit / 10.0;
            return costPerMB * Capacity;
        }

        private double DeterminePerformanceAdjustment()
        {
            if (MadeSaleLastRound)
            {
                RoundsWithoutSale = 0;
                return 0;
            }
            else
            {
                RoundsWithoutSale++;
                return -PriceDecreaseFactor * RoundsWithoutSale;
            }
        }

        private void ProcessResult(string resultMessage)
        {
            string[] resultInfo = resultMessage.Split(' ');

            if (resultInfo[1] == Name)
            {
                LastSalePrice = int.Parse(resultInfo[2]);
                MadeSaleLastRound = true;
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
