using System;
using System.Collections.Generic;
using UnityEngine;

public class Simulator : MonoBehaviour {
    private void Start() {
        int handSize = 5;
        int batchSize = 100000000; // 每次處理X筆數

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
        Debug.LogErrorFormat("完成花費:{0}秒 共有{1}種組合", (DateTime.Now - start).TotalSeconds, combinationCounter);
        Debug.LogError("能組成同花順的數量:" + straightFlushCounter);

        // 測試手牌是否為同花順
        //List<Card> hand1 = new List<Card> {
        //    new Card(0, 1), new Card(0, 2), new Card(0, 3), new Card(0, 4), new Card(0, 5)
        //};
        //List<Card> hand2 = new List<Card> {
        //    new Card(0, 10), new Card(0, 11), new Card(0, 12), new Card(0, 13), new Card(0, 1)
        //};

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
                Debug.Log(string.Join(",", hand));
                straightFlushCounter++;
            }

            // 生成下一組合
            int k = handSize - 1;
            while (k >= 0 && indices[k] == k + n - handSize) {
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
        // 计数排序：统计每种花色的牌的数量
        int[] counts = new int[4];
        foreach (Card card in hand) {
            counts[card.Suit]++;
        }

        // 遍历每种花色，检查是否有同花顺
        for (int suit = 0; suit < 4; suit++) {
            if (counts[suit] < 5) continue; // 超过5张同花色才考虑同花顺

            // 计数排序：统计每种牌值的数量
            int[] valueCounts = new int[14]; // 0 不使用，从1到13代表牌值
            foreach (Card card in hand) {
                if (card.Suit == suit) {
                    valueCounts[card.Value]++;
                    if (card.Value == 1) { // 如果是 A，则同时视为 14
                        valueCounts[14]++;
                    }
                }
            }

            // 在每种花色的牌中找出同花顺
            int straightCount = 0; // 记录连续的牌数
            for (int value = 1; value <= 13; value++) {
                if (valueCounts[value] > 0) {
                    straightCount++;
                    if (straightCount == 5) return true; // 找到同花顺
                } else {
                    straightCount = 0; // 重置连续的牌数
                }
            }
        }

        return false; // 没有找到同花顺
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

