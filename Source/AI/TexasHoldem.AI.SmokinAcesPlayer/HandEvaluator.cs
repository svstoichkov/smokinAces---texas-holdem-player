﻿namespace TexasHoldem.AI.SmokinAcesPlayer
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Logic.Helpers;

    using TexasHoldem.Logic.Cards;
    using TexasHoldem.Logic.Extensions;
    
    public static class HandEvaluator
    {
        private static readonly object locker = new object();

        public static double CalculateHandValue(IList<Card> myHand, IList<Card> communityCards)
        {
            if (communityCards.Count == 0)
            {
                if ((int) myHand[0].Type < 5 && (int) myHand[1].Type < 5 && myHand[0].Type != myHand[1].Type)
                {
                    return 0;
                }
            }
            
            double wins = 0;

            Parallel.For(0, Settings.GameSimulationsCount, i =>
            //for (int i = 0; i < Settings.GameSimulationsCount; i++)
            {
                var player1Hand = new List<Card>(myHand);
                var player2Hand = new List<Card>();

                var remainingCommunityCards = new List<Card>();

                var deck = new List<Card>(Deck.AllCards);
                deck.CustomRemoveRange(myHand);
                deck.CustomRemoveRange(communityCards);
                deck = deck.Shuffle().ToList();

                //generate remaining community cards
                for (int j = 0; j < 5 - communityCards.Count; j++)
                {
                    remainingCommunityCards.Add(deck.Deal());
                }

                //add hole/community cards to the AI
                player1Hand.AddRange(communityCards);
                player1Hand.AddRange(remainingCommunityCards);

                //add cards to player2
                player2Hand.Add(deck.Deal());
                player2Hand.Add(deck.Deal());
                player2Hand.AddRange(communityCards);
                player2Hand.AddRange(remainingCommunityCards);
                
                //compare hands
                if (Helpers.CompareCards(player1Hand, player2Hand) == 1)
                {
                    lock (locker)
                    {
                        wins++;
                    }
                }
            });

            return wins / Settings.GameSimulationsCount;
        }
    }
}