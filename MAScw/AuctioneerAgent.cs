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

        private HashSet<string> _unmatchedBidders;
        private HashSet<string> _unmatchedSellers;

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

            // Console.WriteLine($"{message.Content} ----> Auctioneer");

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
                _offersCollected = false;
                _bidsCollected = false;
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
            if (bidParts.Length < 3)
            {
                Console.WriteLine($"Invalid bid message format: {bidMessage}");
                return;
            }

            int bidAmount = int.Parse(bidParts[1]);
            int dataRequirement = int.Parse(bidParts[2]);

            _bids.Add(new Bid
            {
                Bidder = bidder,
                Amount = bidAmount,
                DataRequirement = dataRequirement
            });

            //Console.WriteLine($"Received bid from {bidder} for {bidAmount} with data requirement {dataRequirement}.");
        }

        private void HandleOffer(string seller, string offerMessage)
        {
            string[] offerParts = offerMessage.Split(' ');
            if (offerParts.Length < 3 ||
                !int.TryParse(offerParts[1], out int offerAmount) ||
                !int.TryParse(offerParts[2], out int capacity))
            {
                Console.WriteLine($"Invalid offer message format: {offerMessage}");
                return; // Exit the method if the message format is not as expected
            }

            _offers.Add(new Offer
            {
                Seller = seller,
                Amount = offerAmount,
                Capacity = capacity
            });

            Console.WriteLine($"Received offer from {seller} for {offerAmount} pence for {capacity}MB");
        }


        private void PerformAuction()
        {
            var sortedBids = _bids.OrderByDescending(b => b.Amount).ToList();
            var sortedOffers = _offers.OrderBy(o => o.Amount).ToList();

            _unmatchedBidders ??= new HashSet<string>();
            _unmatchedSellers ??= new HashSet<string>();

            _unmatchedBidders.Clear();
            _unmatchedSellers.Clear();

            foreach (var bid in _bids)
            {
                _unmatchedBidders.Add(bid.Bidder);
            }
            foreach (var offer in _offers)
            {
                _unmatchedSellers.Add(offer.Seller);
            }

            foreach (var bid in sortedBids)
            {
                Offer? bestMatch = null;
                int bestMatchDifference = int.MaxValue;

                foreach (var offer in sortedOffers)
                {
                    if (bid.Amount >= offer.Amount && bid.DataRequirement <= offer.Capacity)
                    {
                        int difference = Math.Abs(bid.Amount - offer.Amount);
                        if (difference < bestMatchDifference)
                        {
                            bestMatch = offer;
                            bestMatchDifference = difference;
                        }
                    }
                }

                if (bestMatch != null)
                {
                    Console.WriteLine($"Auction Match: Bidder {bid.Bidder} wins with bid {bid.Amount}. Seller: {bestMatch.Value.Seller} with offer {bestMatch.Value.Amount}");
                    Send(bid.Bidder, $"win {bid.Amount}");
                    Send(bestMatch.Value.Seller, $"sold {bestMatch.Value.Amount}");

                    Send(bid.Bidder, $"result win {bestMatch.Value.Amount}");
                    Send(bestMatch.Value.Seller, $"result sold {bid.Amount}");

                    _unmatchedBidders.Remove(bid.Bidder);
                    _unmatchedSellers.Remove(bestMatch.Value.Seller);

                    sortedOffers.Remove(bestMatch.Value);
                }
            }

            foreach (var bidder in _unmatchedBidders)
            {
                Send(bidder, "result loss");
                Send(bidder, "new round");
            }
            foreach (var seller in _unmatchedSellers)
            {
                Send(seller, "result loss");
                Send(seller, "new round");
            }

            _round++;
            Console.WriteLine("Next auction round will start.----------------------------------------------------------");

            _bids.Clear();
            _offers.Clear();
        }

        private struct Bid
        {
            public string Bidder { get; set; }
            public int Amount { get; set; }
            public int DataRequirement { get; set; }
        }

        private struct Offer
        {
            public string Seller { get; set; }
            public int Amount { get; set; }
            public int Capacity { get; set; }
        }
    }

}