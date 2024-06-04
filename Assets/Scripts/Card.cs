using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    public Sprite GetCardSprite() {
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
    public static int GetOdds(this HandType _type) {
        switch (_type) {
            case HandType.HighCard:
                return 0;
            case HandType.Pair:
                return 2;
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
    public static HandType GetHandType(this List<Card> _cards) {
        if (_cards.IsStraightFlush()) {
            return HandType.StraightFlush;
        } else if (_cards.IsFourOfAKind()) {
            return HandType.FourOfAKind;
        } else if (_cards.IsFullHouse()) {
            return HandType.FullHouse;
        } else if (_cards.IsThreeOfAKind()) {
            return HandType.ThreeOfAKind;
        } else if (_cards.IsStraight()) {
            return HandType.Straight;
        } else if (_cards.IsFlush()) {
            return HandType.Flush;
        } else if (_cards.IsPair()) {
            return HandType.Pair;
        } else {
            return HandType.HighCard;
        }
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
    /// 是否為同花，並返回符合條件的牌的索引
    /// </summary>
    public static List<int> GetFlushIndices(this List<Card> _cards) {
        Dictionary<SuitType, List<int>> suitIndices = new Dictionary<SuitType, List<int>>();

        // 計算每個花色的索引
        for (int i = 0; i < _cards.Count; i++) {
            Card card = _cards[i];
            if (!suitIndices.ContainsKey(card.Suit))
                suitIndices[card.Suit] = new List<int>();
            suitIndices[card.Suit].Add(i);
            if (suitIndices[card.Suit].Count >= 5)
                return suitIndices[card.Suit].Take(5).ToList();
        }

        return new List<int>();
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
    /// 是否為三條，並返回符合條件的牌的索引
    /// </summary>
    public static List<int> GetThreeOfAKindIndices(this List<Card> _cards) {
        Dictionary<int, List<int>> numberIndices = new Dictionary<int, List<int>>();

        // 計算每個數字的索引
        for (int i = 0; i < _cards.Count; i++) {
            Card card = _cards[i];
            if (!numberIndices.ContainsKey(card.Number))
                numberIndices[card.Number] = new List<int>();
            numberIndices[card.Number].Add(i);
            if (numberIndices[card.Number].Count >= 3)
                return numberIndices[card.Number].Take(3).ToList();
        }

        return new List<int>();
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
    /// 是否為二條，並返回符合條件的牌的索引
    /// </summary>
    public static List<int> GetIsPairIndices(this List<Card> _cards) {
        Dictionary<int, List<int>> numberIndices = new Dictionary<int, List<int>>();

        // 計算每個數字的索引
        for (int i = 0; i < _cards.Count; i++) {
            Card card = _cards[i];
            if (!numberIndices.ContainsKey(card.Number))
                numberIndices[card.Number] = new List<int>();
            numberIndices[card.Number].Add(i);
            if (numberIndices[card.Number].Count >= 2)
                return numberIndices[card.Number].Take(2).ToList();
        }

        return new List<int>();
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
    /// 取得葫蘆的索引
    /// </summary>
    public static List<int> GetFullHouseIndices(this List<Card> _cards) {
        Dictionary<int, List<int>> numberIndices = new Dictionary<int, List<int>>();
        List<int> result = new List<int>();

        // 計算每個數字的索引
        for (int i = 0; i < _cards.Count; i++) {
            Card card = _cards[i];
            if (!numberIndices.ContainsKey(card.Number))
                numberIndices[card.Number] = new List<int>();
            numberIndices[card.Number].Add(i);
        }

        List<int> threeOfAKindIndices = null;
        List<int> pairIndices = null;

        // 查找是否有三條和一對
        foreach (var entry in numberIndices) {
            if (entry.Value.Count >= 3 && threeOfAKindIndices == null) {
                threeOfAKindIndices = entry.Value.Take(3).ToList();
            } else if (entry.Value.Count >= 2 && pairIndices == null) {
                pairIndices = entry.Value.Take(2).ToList();
            }
        }

        // 如果有三條和一對，則是葫蘆
        if (threeOfAKindIndices != null && pairIndices != null) {
            result.AddRange(threeOfAKindIndices);
            result.AddRange(pairIndices);
            return result;
        }

        // 檢查是否有兩組三條
        List<int> secondThreeOfAKindIndices = null;
        foreach (var entry in numberIndices) {
            if (entry.Value.Count >= 3) {
                if (threeOfAKindIndices == null) {
                    threeOfAKindIndices = entry.Value.Take(3).ToList();
                } else if (secondThreeOfAKindIndices == null) {
                    secondThreeOfAKindIndices = entry.Value.Take(3).ToList();
                    break;
                }
            }
        }

        // 如果有兩組三條，則是葫蘆
        if (threeOfAKindIndices != null && secondThreeOfAKindIndices != null) {
            result.AddRange(threeOfAKindIndices);
            result.AddRange(secondThreeOfAKindIndices.Take(2));
            return result;
        }

        return new List<int>();
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
    /// 取得四條的索引
    /// </summary>
    public static List<int> GetFourOfAKindIndices(this List<Card> _cards) {
        Dictionary<int, List<int>> rankCount = new Dictionary<int, List<int>>();
        for (int i = 0; i < _cards.Count; i++) {
            Card card = _cards[i];
            if (!rankCount.ContainsKey(card.Number))
                rankCount[card.Number] = new List<int>();
            rankCount[card.Number].Add(i);
            if (rankCount[card.Number].Count == 4)
                return rankCount[card.Number];
        }
        return new List<int>();
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




    /// <summary>
    /// 取得順子的索引
    /// </summary>
    public static List<int> GetStraightIndices(this List<Card> _cards) {
        List<int> values = _cards.Select(c => c.Number).Distinct().OrderBy(v => v).ToList();
        List<int> indices = new List<int>();

        for (int i = 0; i <= values.Count - 5; i++) {
            if (values[i + 4] - values[i] == 4) {
                // 找到順子，返回這些牌的索引
                indices = _cards.Select((card, index) => new { card, index })
                                .Where(x => values.GetRange(i, 5).Contains(x.card.Number))
                                .Select(x => x.index)
                                .ToList();
                return indices;
            }
        }

        // 檢查是否為 10, J, Q, K, A 順子
        if (values.Contains(1) && values.Contains(10) && values.Contains(11) && values.Contains(12) && values.Contains(13)) {
            indices = _cards.Select((card, index) => new { card, index })
                            .Where(x => new List<int> { 1, 10, 11, 12, 13 }.Contains(x.card.Number))
                            .Select(x => x.index)
                            .ToList();
            return indices;
        }

        return indices;
    }
    /// <summary>
    /// 是否為同花順，並返回符合條件的牌的索引
    /// </summary>
    public static List<int> GetStraightFlushIndices(this List<Card> _cards) {
        Dictionary<SuitType, List<int>> suitIndices = new Dictionary<SuitType, List<int>>();
        for (int i = 0; i < _cards.Count; i++) {
            Card card = _cards[i];
            if (!suitIndices.ContainsKey(card.Suit))
                suitIndices[card.Suit] = new List<int>();
            suitIndices[card.Suit].Add(i);
        }

        foreach (var suitEntry in suitIndices) {
            List<int> indices = suitEntry.Value;
            if (indices.Count < 5) continue;

            List<Card> sortedCards = indices.Select(index => _cards[index]).OrderBy(card => card.Number).ToList();
            List<int> straightFlushIndices = sortedCards.GetStraightIndices2();
            if (straightFlushIndices.Count > 0) {
                return straightFlushIndices.Select(index => indices[index]).ToList();
            }
        }
        return new List<int>();
    }
    /// <summary>
    /// 是否為順子，並返回符合條件的牌的索引
    /// </summary>
    static List<int> GetStraightIndices2(this List<Card> _cards) {
        List<int> values = _cards.Select(c => c.Number).Distinct().OrderBy(v => v).ToList();
        List<int> indices = new List<int>();

        for (int i = 0; i <= values.Count - 5; i++) {
            if (values[i + 4] - values[i] == 4) {
                // 找到順子，返回這些牌的索引
                indices = _cards.Select((card, index) => new { card, index })
                                .Where(x => values.GetRange(i, 5).Contains(x.card.Number))
                                .Select(x => x.index)
                                .ToList();
                return indices;
            }
        }

        // 檢查是否為 10, J, Q, K, A 順子
        if (values.Contains(1) && values.Contains(10) && values.Contains(11) && values.Contains(12) && values.Contains(13)) {
            indices = _cards.Select((card, index) => new { card, index })
                            .Where(x => new List<int> { 1, 10, 11, 12, 13 }.Contains(x.card.Number))
                            .Select(x => x.index)
                            .ToList();
            return indices;
        }

        return new List<int>();
    }

}