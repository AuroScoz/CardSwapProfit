using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Card {

    public int Idx { get; private set; }//=suit * 13 + number
    public SuitType Suit { get; private set; } // 花色
    public int Number { get; private set; } // 數字

    public Card(SuitType suit, int number) {
        this.Suit = suit;
        this.Number = number;
        Idx = (int)suit * 13 + number;
    }
    public override string ToString() {
        return $"{Suit.ToStr()}{Number}";
    }
    public Sprite GetSprite() {
        return Resources.Load<Sprite>(string.Format("PokerImgs/{0}", Idx));
    }
}

public enum SuitType { Clubs, Diamonds, Hearts, Spades } // 梅花, 方塊, 紅心, 黑桃
public enum HandType {
    HighCard,
    Pair,
    ThreeOfAKind,
    Straight,
    Flush,
    FullHouse,
    FourOfAKind,
    StraightFlush,
}




public static class CardExtends {

    /// <summary>
    /// 取得花色文字
    /// </summary>
    public static string ToStr(this SuitType _suit) {
        switch (_suit) {
            case SuitType.Spades:
                return "黑桃";
            case SuitType.Hearts:
                return "紅心";
            case SuitType.Diamonds:
                return "方塊";
            case SuitType.Clubs:
                return "梅花";
            default:
                return "尚未定義";
        }
    }
    /// <summary>
    /// 取的牌型文字
    /// </summary>
    public static string ToStr(this HandType _type) {
        switch (_type) {
            case HandType.HighCard:
                return "高牌";
            case HandType.Pair:
                return "對子";
            case HandType.Flush:
                return "同花";
            case HandType.Straight:
                return "順子";
            case HandType.ThreeOfAKind:
                return "三條";
            case HandType.FullHouse:
                return "葫蘆";
            case HandType.FourOfAKind:
                return "四條";
            case HandType.StraightFlush:
                return "同花順";
            default:
                return "尚未定義";
        }
    }
    /// <summary>
    /// 取得牌型賠率
    /// </summary>
    public static float GetOdds(this HandType _type) {
        switch (_type) {
            case HandType.HighCard:
                return 0;
            case HandType.Pair:
                return 1.25f;
            case HandType.ThreeOfAKind:
                return 10;
            case HandType.Straight:
                return 20;
            case HandType.Flush:
                return 30;
            case HandType.FullHouse:
                return 50;
            case HandType.FourOfAKind:
                return 250;
            case HandType.StraightFlush:
                return 1000;
            default:
                Debug.LogError("尚未定義的HandType牌型賠率:" + _type);
                return 0;
        }
    }

    public static void ShowCards(this List<Card> _cards) {
        string str = "";
        for (int i = 0; i < _cards.Count; i++) {
            if (i != 0) str += ",";
            str += _cards[i].ToString();
        }
        Debug.LogError(str);
    }
    /// <summary>
    /// 是否為同花
    /// </summary>
    public static bool IsFlush(this List<Card> _cards) {
        Dictionary<SuitType, int> suitDics = new Dictionary<SuitType, int>();
        foreach (Card card in _cards) {
            if (!suitDics.ContainsKey(card.Suit)) {
                suitDics[card.Suit] = 1;
            } else {
                suitDics[card.Suit]++;
            }
            if (suitDics[card.Suit] >= 5) return true;
        }
        return false;
    }
    /// <summary>
    /// 是否為三條
    /// </summary>
    public static bool IsThreeOfAKind(this List<Card> _cards) {
        Dictionary<int, int> numberDic = new Dictionary<int, int>();

        // 計算每個數字的張數
        foreach (Card card in _cards) {
            if (!numberDic.ContainsKey(card.Number))
                numberDic[card.Number] = 1;
            else
                numberDic[card.Number]++;
            if (numberDic[card.Number] >= 3) return true;
        }
        return false;
    }
    /// <summary>
    /// 是否為二條
    /// </summary>
    public static bool IsPair(this List<Card> _cards) {
        Dictionary<int, int> numberDic = new Dictionary<int, int>();

        // 計算每個數字的張數
        foreach (Card card in _cards) {
            if (!numberDic.ContainsKey(card.Number))
                numberDic[card.Number] = 1;
            else
                numberDic[card.Number]++;
            if (numberDic[card.Number] >= 2) return true;
        }
        return false;
    }
    /// <summary>
    /// 是否為葫蘆
    /// </summary>
    public static bool IsFullHouse(this List<Card> _cards) {
        Dictionary<int, int> numberDic = new Dictionary<int, int>();

        // 計算每個數字的張數
        foreach (Card card in _cards) {
            if (!numberDic.ContainsKey(card.Number))
                numberDic[card.Number] = 0;
            numberDic[card.Number]++;
        }

        int threeOfAKindCount = 0;
        int pairCount = 0;

        // 檢查是否有三張和兩張相同數字的牌
        foreach (var count in numberDic.Values) {
            if (count >= 3) {
                threeOfAKindCount++;
            }
            if (count >= 2) {
                pairCount++;
            }
        }

        // 如果有至少一個三條和至少一個對子，則是葫蘆
        return (threeOfAKindCount >= 1 && pairCount >= 2) || (threeOfAKindCount > 1);
    }
    /// <summary>
    /// 是否為四條
    /// </summary>
    public static bool IsFourOfAKind(this List<Card> _cards) {
        Dictionary<int, int> rankCount = new Dictionary<int, int>();
        foreach (Card card in _cards) {
            if (!rankCount.ContainsKey(card.Number))
                rankCount[card.Number] = 0;
            rankCount[card.Number]++;
            if (rankCount[card.Number] == 4)
                return true;
        }
        return false;
    }
    /// <summary>
    /// 是否為同花順
    /// </summary>
    public static bool IsStraightFlush(this List<Card> _cards) {
        Dictionary<SuitType, List<Card>> suitCards = new Dictionary<SuitType, List<Card>>();
        foreach (Card card in _cards) {
            if (!suitCards.ContainsKey(card.Suit))
                suitCards.Add(card.Suit, new List<Card>());
            suitCards[card.Suit].Add(card);
        }

        foreach (var cards in suitCards.Values) {
            if (cards.Count < 5) continue;

            List<Card> sortedCards = cards.OrderBy(card => card.Number).ToList();
            if (sortedCards.IsStraight()) return true;
        }
        return false;
    }
    /// <summary>
    /// 是否為順子
    /// </summary>
    public static bool IsStraight(this List<Card> _cards) {
        List<int> values = _cards.Select(c => c.Number).Distinct().OrderBy(v => v).ToList();
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
}