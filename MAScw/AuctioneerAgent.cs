using ActressMas;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EdgeComputingAuction
{
    public class AuctioneerAgent : Agent
    {
        private List<Bid> _bids;
        private List<Offer> _offers;
        private int _round;
        private List<string> _participants;
        private int expectedNumberOfParticipants = 15;


        private bool _offersCollected;
        private bool _bidsCollected;
        private int expectedNumberOfOffers;
        private int expectedNumberOfBids;

        public AuctioneerAgent()
        {
            _participants = new List<string>();
            _bids = new List<Bid>();
            _offers = new List<Offer>();
            _round = 0;

            expectedNumberOfOffers = 10; 
            expectedNumberOfBids = 15;   
        }

        public override void Setup()
        {
            Broadcast("start");
            Console.WriteLine("---------------------------Auction Started---------------------------");
        }

        public override void Act(Message message)
        {
            Console.WriteLine($"{message.Content} ----> Auctioneer");

            if (message.Content.StartsWith("join"))
            {
                string participantName = message.Sender;
                _participants.Add(participantName);
                Console.WriteLine($"{participantName} has joined the auction.");
            }
            else if (message.Content == "start")
            {
                StartAuction();
            }
            else if (message.Content.StartsWith("bid"))
            {
                HandleBid(message.Sender, message.Content);
            }
            else if (message.Content.StartsWith("offer"))
            {
                HandleOffer(message.Sender, message.Content);
            }
            if (!_offersCollected && _offers.Count >= expectedNumberOfOffers) 
            {
                _offersCollected = true;
            }
            // Check if all bids have been received
            if (!_bidsCollected && _bids.Count >= expectedNumberOfBids)
            {
                _bidsCollected = true;
            }
            if (_round > 0 && _offersCollected && (_bidsCollected || _bids.Count > 0))
            {
                PerformAuction();
                _offersCollected = false; // Reset for next round
                _bidsCollected = false; // Reset for next round
            }
        }

        public void StartAuction()
        {
            _round = 1;
            foreach (var participant in _participants)
            {
                if (_participants.Count == expectedNumberOfParticipants)
                {
                    Broadcast("start auction");
                    Console.WriteLine("---------------Auction Started--------------------");
                }
            }
        }

        private void HandleBid(string bidder, string bidMessage)
        {
            string[] bidParts = bidMessage.Split(' ');
            int bidAmount = int.Parse(bidParts[1]);

            _bids.Add(new Bid
            {
                Bidder = bidder,
                Amount = bidAmount
            });

            Console.WriteLine($"Received bid from {bidder} for {bidAmount}.");
        }

        private void HandleOffer(string seller, string offerMessage)
        {
            string[] offerParts = offerMessage.Split(' ');
            int offerAmount = int.Parse(offerParts[1]);

            _offers.Add(new Offer
            {
                Seller = seller,
                Amount = offerAmount
            });

            Console.WriteLine($"Received offer from {seller} for {offerAmount} pence per MB");
        }

        private void PerformAuction()
        {
            var sortedBids = _bids.OrderByDescending(b => b.Amount).ToList();
            var sortedOffers = _offers.OrderBy(o => o.Amount).ToList();

            for (int bidIndex = 0; bidIndex < sortedBids.Count; bidIndex++)
            {
                var currentBid = sortedBids[bidIndex];

                for (int offerIndex = 0; offerIndex < sortedOffers.Count; offerIndex++)
                {
                    var currentOffer = sortedOffers[offerIndex];

                    if (currentBid.Amount >= currentOffer.Amount)
                    {
                        // Match found
                        Console.WriteLine($"Auction Match:{currentBid.Bidder} wins with bid {currentBid.Amount}. Seller: {currentOffer.Seller} with offer {currentOffer.Amount}");
                        Send(currentBid.Bidder, $"win {currentBid.Amount}");
                        Send(currentOffer.Seller, $"sold {currentOffer.Amount} pence");

                        // Remove matched bid and offer
                        sortedBids.RemoveAt(bidIndex);
                        sortedOffers.RemoveAt(offerIndex);

                        // Adjust bidIndex to account for the removed bid
                        bidIndex--;

                        break; // Break out of the inner loop and continue with the next bid
                    }
                }
            }
            _offers.Clear();
            _bids.Clear();
            _round++;
            Console.WriteLine("Next auction round will start.----------------------------------------------------------");
        }

        private struct Bid
        {
            public string Bidder { get; set; }
            public int Amount { get; set; }
        }

        private struct Offer
        {
            public string Seller { get; set; }
            public int Amount { get; set; }
        }
    }
}
