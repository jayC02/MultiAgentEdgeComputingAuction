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
            Console.WriteLine($"{Name} set up with data requirement {DataRequirement} Mb and valuation {Valuation}.");
            Send("Auctioneer", $"join {DataRequirement}");
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
                Console.WriteLine($"{Name} is bidding {bidAmount}");
                Send("Auctioneer", $"bid {bidAmount}");
            }
        }

        private int CalculateBid()
        {
            // Implement your bidding strategy here. This is a simple example.
            return Valuation - (new Random()).Next(0, Valuation / 10);
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
