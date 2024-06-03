using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Simulator : MonoBehaviour {
    private void Start() {
        int handSize = 6;
        int batchSize = 2000000000; // 每次處理X筆數

        // 初始化一副撲克牌
        List<Card> deck = new List<Card>();
        for (int suit = 0; suit < 4; suit++) {
            for (int value = 1; value <= 13; value++) {
                deck.Add(new Card(suit, value));
            }
        }

        // 初始化組合計數器
        int combinationCounter = 0;
        int straightFlushCounter = 0;

        // 開始分段處理生成組合
        DateTime start = DateTime.Now;
        GenerateCombinations(deck, handSize, batchSize, ref combinationCounter, ref straightFlushCounter);
        Debug.LogErrorFormat("完成花費:{0}秒 共考慮{1}種組合", (DateTime.Now - start).TotalSeconds, combinationCounter);
        Debug.LogError("能組成同花順的數量:" + straightFlushCounter);

        // 測試手牌是否為同花順
        //List<Card> hand1 = new List<Card> {
        //    new Card(0, 9), new Card(0, 2), new Card(0, 3), new Card(0, 4), new Card(0, 5), new Card(0, 8), new Card(0, 1)
        //};
        //List<Card> hand2 = new List<Card> {
        //    new Card(0, 11), new Card(0, 3), new Card(0, 4), new Card(0, 5), new Card(0, 6), new Card(0, 9), new Card(0, 2)
        //};
        //List<Card> hand3 = new List<Card> {
        //    new Card(0, 12), new Card(0, 4), new Card(0, 5), new Card(0, 6), new Card(0, 7), new Card(0, 10), new Card(0, 3)
        //};
        //List<Card> hand4 = new List<Card> {
        //    new Card(0, 2), new Card(0, 5), new Card(0, 6), new Card(0, 7), new Card(0, 8), new Card(0, 1), new Card(0, 4)
        //};
        //List<Card> hand5 = new List<Card> {
        //    new Card(0, 2), new Card(0, 6), new Card(0, 7), new Card(0, 8), new Card(0, 9), new Card(0, 1), new Card(0, 5)
        //};
        //List<Card> hand6 = new List<Card> {
        //    new Card(0, 3), new Card(0, 7), new Card(0, 8), new Card(0, 9), new Card(0, 10), new Card(0, 1), new Card(0, 6)
        //};
        //List<Card> hand7 = new List<Card> {
        //    new Card(0, 2), new Card(0, 8), new Card(0, 9), new Card(0, 10), new Card(0, 11), new Card(0, 4), new Card(0, 7)
        //};
        //List<Card> hand8 = new List<Card> {
        //    new Card(0, 2), new Card(0, 9), new Card(0, 10), new Card(0, 11), new Card(0, 12), new Card(0, 4), new Card(0, 8)
        //};
        //List<Card> hand9 = new List<Card> {
        //    new Card(0, 6), new Card(0, 10), new Card(0,11), new Card(0, 12), new Card(0, 13), new Card(0, 8), new Card(0, 9)
        //};
        //List<Card> hand10 = new List<Card> {
        //    new Card(0, 6), new Card(0, 11), new Card(0,12), new Card(0, 13), new Card(0, 1), new Card(0, 2), new Card(0, 10)
        //};

        //Debug.LogError("IsStraightFlush1=" + IsStraightFlush(hand1));
        //Debug.LogError("IsStraightFlush2=" + IsStraightFlush(hand2));
        //Debug.LogError("IsStraightFlush3=" + IsStraightFlush(hand3));
        //Debug.LogError("IsStraightFlush4=" + IsStraightFlush(hand4));
        //Debug.LogError("IsStraightFlush5=" + IsStraightFlush(hand5));
        //Debug.LogError("IsStraightFlush6=" + IsStraightFlush(hand6));
        //Debug.LogError("IsStraightFlush7=" + IsStraightFlush(hand7));
        //Debug.LogError("IsStraightFlush8=" + IsStraightFlush(hand8));
        //Debug.LogError("IsStraightFlush9=" + IsStraightFlush(hand9));
        //Debug.LogError("IsStraightFlush10=" + IsStraightFlush(hand10));
    }

    static void GenerateCombinations(List<Card> deck, int handSize, int batchSize, ref int combinationCounter, ref int straightFlushCounter) {
        int n = deck.Count;
        int[] indices = new int[handSize];

        for (int i = 0; i < handSize; i++) {
            indices[i] = i;
        }

        while (combinationCounter < batchSize) {
            List<Card> hand = new List<Card>();
            foreach (int index in indices) {
                hand.Add(deck[index]);
            }

            combinationCounter++;
            if (IsStraightFlush(hand)) {
                //Debug.Log(string.Join(",", hand));
                straightFlushCounter++;
            }

            // 生成下一組合
            int k = handSize - 1;
            while (k >= 0 && indices[k] == n - handSize + k) {
                k--;
            }

            if (k < 0) break;

            indices[k]++;
            for (int j = k + 1; j < handSize; j++) {
                indices[j] = indices[j - 1] + 1;
            }
        }
    }

    static bool IsStraightFlush(List<Card> hand) {
        Dictionary<int, List<Card>> suitCards = new Dictionary<int, List<Card>>();
        foreach (Card card in hand) {
            if (!suitCards.ContainsKey(card.Suit))
                suitCards.Add(card.Suit, new List<Card>());
            suitCards[card.Suit].Add(card);
        }

        foreach (var cards in suitCards.Values) {
            if (cards.Count < 5) continue;

            List<Card> sortedCards = cards.OrderBy(card => card.Value).ToList();
            if (IsStraight(sortedCards)) return true;
        }
        return false;
    }

    static bool IsStraight(List<Card> cards) {

        List<int> values = cards.Select(c => c.Value).Distinct().OrderBy(v => v).ToList();

        for (int i = 0; i <= values.Count - 5; i++) {
            if (values[i + 4] - values[i] == 4) {
                return true;
            }
        }

        if (values.Contains(1) && values.Contains(10) && values.Contains(11) && values.Contains(12) && values.Contains(13)) {
            return true;
        }

        return false;
    }
    static void ShowCards(List<Card> _cards) {
        for (int i = 0; i < _cards.Count; i++) {
            Debug.LogError("點數" + _cards[i].Value);
        }
    }

    class Card {
        public int Suit { get; private set; }
        public int Value { get; private set; }

        public Card(int suit, int value) {
            Suit = suit;
            Value = value;
        }

        public override string ToString() {
            return $"數值 {Value} 花色: {Suit}";
        }
    }
}

