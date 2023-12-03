using ActressMas;
using System;

namespace EdgeComputingAuction
{
    public class BidderAgent : Agent
    {
        public int DataRequirement { get; private set; }
        public int Valuation { get; private set; }
        private bool _hasWonBid;

        public BidderAgent(int dataRequirement, int valuation)
        {
            DataRequirement = dataRequirement;
            Valuation = valuation;
            _hasWonBid = false;
        }

        public override void Setup()
        {
            Console.WriteLine($"{Name} set up with data requirement {DataRequirement} Mb and valuation {Valuation}p");
            Send("Auctioneer", $"{Name} join {DataRequirement}MB");
        }

        public override void Act(Message message)
        {
            Console.WriteLine($"{Name} received message: {message.Content}");

            if (message.Content == "start")
            {
                MakeBid();
            }
            else if (message.Content.StartsWith("result"))
            {
                ProcessResult(message.Content);
            }
            // Add handling for offers here if necessary
        }

        private void MakeBid()
        {
            if (!_hasWonBid)
            {
                int bidAmount = CalculateBid();
                Console.WriteLine($"{Name} is bidding {bidAmount} pence");
                Send("Auctioneer", $"bid {bidAmount}");
            }
        }

        private int CalculateBid()
        {
  
            int averageBidEstimate = GetAverageBidEstimate();

            double bidModifier = CalculateBidModifier(averageBidEstimate);

            int finalBid = Math.Min((int)(Valuation * bidModifier), Valuation);

            return finalBid;
        }
        private double CalculateBidModifier(int averageBidEstimate)
        {
            if (Valuation > averageBidEstimate)
            {
                return 1.1;
            }
            else
            {
                return 0.9; 
            }
        }

        private int GetAverageBidEstimate()
        {
            return 200; 
        }

        private void ProcessResult(string resultMessage)
        {
            string[] resultInfo = resultMessage.Split(' ');

            if (resultInfo[1] == Name)
            {
                Console.WriteLine($"{Name} won the bid at price {resultInfo[2]}");
                _hasWonBid = true;
                // Further processing if required, like adjusting strategies or confirming resource allocation.
            }
            else
            {
                Console.WriteLine($"{Name} did not win. Adjusting strategy for next round.");
                // Adjust strategy for the next round if needed.
                _hasWonBid = false; // Reset win status for next round
            }
        }
    }
}
