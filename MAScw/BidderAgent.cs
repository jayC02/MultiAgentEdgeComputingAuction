using ActressMas;
using System;

namespace EdgeComputingAuction
{
    public class BidderAgent : Agent
    {
        public int DataRequirement { get; private set; }
        public int Valuation { get; private set; }
        private bool _hasWonBid;
        private int _roundsWithoutWin;

        public BidderAgent(int dataRequirement, int valuation)
        {
            DataRequirement = dataRequirement;
            Valuation = valuation;
            _hasWonBid = false;
            _roundsWithoutWin = 0;
        }

        public override void Setup()
        {
            Console.WriteLine($"{Name} set up with data requirement {DataRequirement} Mb and valuation {Valuation}p");
            Send("Auctioneer", $"{Name} join {DataRequirement}MB");
        }

        public override void Act(Message message)
        {
            Console.WriteLine($"{Name} received message: {message.Content}");

            if (message.Content == "start" || message.Content == "new round")
            {
                MakeBid();
            }
            else if (message.Content.StartsWith("result"))
            {
                ProcessResult(message.Content);
            }
            else if (message.Content == "loss")
            {
                // Handle the loss message
                _hasWonBid = false;
                _roundsWithoutWin++;
                //Console.WriteLine($"{Name} did not win. Adjusting strategy for next round.");
            }
        }

        private void MakeBid()
        {
            int bidAmount = CalculateDynamicBid();
            Console.WriteLine($"{Name} is bidding {bidAmount} pence with data requirement {DataRequirement} Mb");
            Send("Auctioneer", $"bid {bidAmount} {DataRequirement}");
        }

        private int CalculateDynamicBid()
        {
            // Starting bid is set to 50% of the valuation
            double bidPercentage = 0.65;

            // Increase the bid by 15% for each new round
            bidPercentage += 0.15 * _roundsWithoutWin;

            // Calculate the new bid
            int newBid = (int)(Valuation * bidPercentage);

            return newBid;
        }

        private void ProcessResult(string resultMessage)
        {
            string[] resultInfo = resultMessage.Split(' ');

            if (resultInfo[1] == Name)
            {
                Console.WriteLine($"{Name} won the bid at price {resultInfo[2]}");
                _hasWonBid = true;
                _roundsWithoutWin = 0; // Reset counter
            }
            else
            {
                Console.WriteLine($"{Name} did not win. Adjusting strategy for next round.");
                _hasWonBid = false;
                _roundsWithoutWin++; // Increment the counter
            }
        }
    }

}
