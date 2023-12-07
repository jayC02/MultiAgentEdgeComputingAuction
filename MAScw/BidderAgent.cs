using ActressMas;
using System;

namespace EdgeComputingAuction
{
    public class BidderAgent : Agent
    {
        public int DataRequirement { get; private set; }
        public int Valuation { get; private set; }
        private bool _hasWonBid;
        public int Distance { get; private set; }

        private int _roundsWithoutWin;

        public BidderAgent(int dataRequirement, int valuation, int distance)
        {
            DataRequirement = dataRequirement;
            Valuation = valuation;
            _hasWonBid = false;
            Distance = distance;
            _roundsWithoutWin = 0;
        }

        public override void Setup()
        {
            Console.WriteLine($"{Name} set up with data requirement {DataRequirement} Mb , valuation {Valuation}p and distance {Distance} miles");
            Send("Auctioneer", $"{Name} join {DataRequirement}MB {Distance} miles");
        }

        public override void Act(Message message)
        {
            //Console.WriteLine($"{Name} received message: {message.Content}");

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
                _hasWonBid = false;
                _roundsWithoutWin++;
            }
        }

        private void MakeBid()
        {
            int bidAmount = CalculateDynamicBid();
            Console.WriteLine($"{Name} is bidding {bidAmount} pence with data requirement {DataRequirement} Mb with distance {Distance} miles");
            Send("Auctioneer", $"bid {bidAmount} {DataRequirement} {Distance}");
        }

        private int CalculateDynamicBid()
        {
            double bidPercentage = 0.65;

            bidPercentage += 0.25 * _roundsWithoutWin;

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
