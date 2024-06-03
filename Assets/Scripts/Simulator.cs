using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Simulator : MonoBehaviour {
    private void Start() {
        int handSize = 7;

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
        GenerateCombinations(deck, handSize, ref combinationCounter, ref straightFlushCounter);
        Debug.LogErrorFormat("完成花費:{0}秒 共考慮{1}種組合", (DateTime.Now - start).TotalSeconds, combinationCounter);
        Debug.LogError("能組成同花順的數量:" + straightFlushCounter);
    }

    static void GenerateCombinations(List<Card> deck, int handSize, ref int combinationCounter, ref int straightFlushCounter) {
        int n = deck.Count;
        int[] indices = new int[handSize];

        // 初始化索引數組
        for (int i = 0; i < handSize; i++) {
            indices[i] = i;
        }

        while (true) {
            List<Card> handCards = new List<Card>();

            // 根據當前索引數組生成手牌
            foreach (int index in indices) {
                handCards.Add(deck[index]);
            }

            // 計數組合次數
            combinationCounter++;
            bool isStraightFlush = IsStraightFlush(handCards);
            if (isStraightFlush) {
                //ShowCards(handCards);
                straightFlushCounter++;
            }

            // 生成下一組合
            int k = handSize - 1;
            while (k >= 0 && indices[k] == n - handSize + k) {
                k--;
            }

            // 如果沒有更多組合則退出
            if (k < 0) break;

            indices[k]++;
            for (int j = k + 1; j < handSize; j++) {
                indices[j] = indices[j - 1] + 1;
            }
        }
    }


    static bool IsStraightFlush(List<Card> hands) {
        //ShowCards(hands);
        Dictionary<int, List<Card>> suitCards = new Dictionary<int, List<Card>>();
        foreach (Card card in hands) {
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
        bool isTarget = false;

        for (int i = 0; i <= values.Count - 5; i++) {
            if (values[i + 4] - values[i] == 4) {
                if (isTarget) Debug.LogError("true");
                return true;
            }
        }



        if (values.Contains(1) && values.Contains(10) && values.Contains(11) && values.Contains(12) && values.Contains(13)) {
            if (isTarget) Debug.LogError("true");
            return true;
        }
        if (isTarget) Debug.LogError("false");
        return false;
    }
    static void ShowCards(List<Card> _cards) {
        string s = "點數";
        for (int i = 0; i < _cards.Count; i++) {
            if (i != 0) s += ",";
            s += _cards[i].Value;
        }
        Debug.LogError(s);
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

