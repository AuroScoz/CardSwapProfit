using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Simulator : MonoBehaviour {
    private void Start() {
        int handSize = 7;
        int batchSize = 200000000; // 每次處理X筆數

        // 初始化一副撲克牌
        List<Card> deck = new List<Card>();
        for (int suit = 0; suit < 4; suit++) {
            for (int value = 1; value <= 13; value++) {
                deck.Add(new Card((SuitType)suit, value));
            }
        }

        // 初始化組合計數器
        int combinationCounter = 0;

        // 開始分段處理生成組合
        //DateTime start = DateTime.Now;
        //GenerateCombinations(deck, handSize, batchSize, ref combinationCounter, HandType.Pair);
        //Debug.LogErrorFormat("完成花費:{0}秒 共考慮{1}種組合", (DateTime.Now - start).TotalSeconds, combinationCounter);
    }

    static void GenerateCombinations(List<Card> deck, int handSize, int batchSize, ref int combinationCounter, HandType _handType) {
        int n = deck.Count;
        int[] indices = new int[handSize];
        int handTypeCount = 0;//組合數

        for (int i = 0; i < handSize; i++) {
            indices[i] = i;
        }

        while (combinationCounter < batchSize) {
            List<Card> hand = new List<Card>();
            foreach (int index in indices) {
                hand.Add(deck[index]);
            }

            combinationCounter++;
            switch (_handType) {
                case HandType.StraightFlush:
                    if (hand.IsStraightFlush()) {
                        //Debug.Log(string.Join(",", hand));
                        handTypeCount++;
                    }
                    break;
                case HandType.FourOfAKind:
                    if (hand.IsFourOfAKind()) {
                        //Debug.Log(string.Join(",", hand));
                        handTypeCount++;
                    }
                    break;
                case HandType.FullHouse:
                    if (hand.IsFullHouse()) {
                        //Debug.Log(string.Join(",", hand));
                        handTypeCount++;
                    }
                    break;
                case HandType.Flush:
                    if (hand.IsFlush()) {
                        //Debug.Log(string.Join(",", hand));
                        handTypeCount++;
                    }
                    break;
                case HandType.Straight:
                    if (hand.IsStraight()) {
                        //Debug.Log(string.Join(",", hand));
                        handTypeCount++;
                    }
                    break;
                case HandType.ThreeOfAKind:
                    if (hand.IsThreeOfAKind()) {
                        //Debug.Log(string.Join(",", hand));
                        handTypeCount++;
                    }
                    break;
                case HandType.Pair:
                    if (hand.IsPair()) {
                        //Debug.Log(string.Join(",", hand));
                        handTypeCount++;
                    }
                    break;
                case HandType.HighCard:
                    handTypeCount++;
                    break;
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

        Debug.LogError("牌型組合數:" + handTypeCount);
    }



}

