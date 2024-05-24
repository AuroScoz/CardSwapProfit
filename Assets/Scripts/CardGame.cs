using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum Suit { Clubs, Diamonds, Hearts, Spades } // 梅花, 方塊, 紅心, 黑桃
public enum HandType {
    HighCard,
    Pair,
    Flush,
    Straight,
    ThreeOfAKind,
    FullHouse,
    StraightFlush,
}
public static class Extends {
    public static string ToStr(this Suit suit) {
        switch (suit) {
            case Suit.Spades:
                return "黑桃";
            case Suit.Hearts:
                return "紅心";
            case Suit.Diamonds:
                return "方塊";
            case Suit.Clubs:
                return "梅花";
            default:
                return "尚未定義";
        }
    }
    public static string ToStr(this HandType suit) {
        switch (suit) {
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
            case HandType.StraightFlush:
                return "同花順";
            default:
                return "尚未定義";
        }
    }
}
public class Card {

    public int Idx;//=suit * 13 + number
    public Suit suit; // 花色
    public int number; // 數字

    public Card(Suit suit, int number) {
        this.suit = suit;
        this.number = number;
        Idx = (int)suit * 13 + number;
    }
    public override string ToString() {
        return $"{suit.ToStr()}{number}";
    }
}
public class CardGame : MonoBehaviour {

    [SerializeField] Text StartText;
    [SerializeField] GameObject StartGO;
    [SerializeField] GameObject PlayingGO;
    [SerializeField] GameObject ResultGO;
    [SerializeField] Text[] HandTexts;
    [SerializeField] Toggle[] HandToggles;
    [SerializeField] Text PlayrPT;
    [SerializeField] Text SwapCost;
    [SerializeField] Text Reward;
    [SerializeField] Text HandTypeText;
    [SerializeField] GameObject PoolCardParent;
    [SerializeField] GameObject CardPrefab;
    [SerializeField] GameObject CardPoolGO;

    // 結果
    [SerializeField] Text ResultRewardText;
    [SerializeField] Text ResultHandTypeText;
    [SerializeField] Text PlayAgainText;

    [SerializeField] int DefaultPlayerPT = 100;
    [SerializeField] int GameCost = 10;
    [SerializeField] int BaseSwapCost = 1;
    [SerializeField] int SwapCostAdd = 1;



    enum GameState {
        Start,
        Playing,
        End
    }

    private void Start() {
        StartText.text = $"花費{GameCost}";
        PlayAgainText.text = $"花費{GameCost}";
        playerPT = DefaultPlayerPT;
        GoState(GameState.Start);
    }
    void GoState(GameState state) {
        switch (state) {
            case GameState.Start:
                StartGO.SetActive(true);
                PlayingGO.SetActive(false);
                ResultGO.SetActive(false);
                break;
            case GameState.Playing:
                StartGO.SetActive(false);
                PlayingGO.SetActive(true);
                ResultGO.SetActive(false);
                break;
            case GameState.End:
                StartGO.SetActive(false);
                PlayingGO.SetActive(false);
                ResultGO.SetActive(true);
                break;
        }
    }

    public void OnPlayClick() {
        StartNewGame(); // 開始新遊戲
        GoState(GameState.Playing);
    }

    public void OnSwapClick() {
        List<int> idxs = new List<int>();
        for (int i = 0; i < HandToggles.Length; i++) {
            if (HandToggles[i].isOn) {
                idxs.Add(i);
            }
            HandToggles[i].isOn = false;
        }
        SwapCard(idxs.ToArray());

        RefreshHandsUI();
        RefreshBottomUI();
    }

    public void OnConfirmClick() {
        int gainPT = CalculateReward();
        ResultHandTypeText.text = $"牌型: {CurHandType.ToStr()}";
        ResultRewardText.text = $"獎勵:{gainPT}";
        playerPT += gainPT;
        GoState(GameState.End);
    }
    public void OnCheckCardPoolClick(bool _show) {
        CardPoolGO.SetActive(_show);
        if (_show) {
            int i = 0;
            foreach (var available in cardPool.Values) {
                poolCardImgs[i].enabled = !available;
                i++;
            }
        }
    }

    void RefreshHandsUI() {
        for (int i = 0; i < hand.Count; i++) {
            HandTexts[i].text = hand[i].ToString();
        }
    }

    void RefreshBottomUI() {
        PlayrPT.text = playerPT.ToString();
        SwapCost.text = $"花費:{swapCost}";
        Reward.text = $"獎勵:{CalculateReward()}";
        HandTypeText.text = $"目前牌型: {CurHandType.ToStr()}";
    }


    List<Image> poolCardImgs = new List<Image>();
    private List<Card> deck; // 牌池
    private List<Card> hand; // 玩家手牌
    private Dictionary<int, bool> cardPool; // 查看牌池

    private int playerPT; // 玩家點數
    private int swapCost; // 換牌成本
    private int swapCount; // 換牌次數
    private HandType CurHandType;//目前牌型



    void StartNewGame() {
        // 初始化牌池
        InitDeck();
        ShuffleDeck();
        DrawInitialHand();
        playerPT -= GameCost;
        swapCost = BaseSwapCost;
        swapCount = 0;
        RefreshHandsUI();
        RefreshBottomUI();
    }

    void InitDeck() {
        deck = new List<Card>();
        cardPool = new Dictionary<int, bool>();
        for (int suit = 0; suit < 4; suit++) {
            for (int number = 1; number <= 13; number++) {
                var newCard = new Card((Suit)suit, number);
                deck.Add(newCard);
                cardPool[newCard.Idx] = true;

                var go = Instantiate(CardPrefab, PoolCardParent.transform);
                var cover = go.transform.Find("cover").GetComponent<Image>();
                var text = go.transform.Find("Text").GetComponent<Text>();
                text.text = newCard.ToString();
                poolCardImgs.Add(cover);
            }
        }
    }

    void ShuffleDeck() {
        for (int i = 0; i < deck.Count; i++) {
            int randomIndex = Random.Range(0, deck.Count);
            Card temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    void DrawInitialHand() {
        hand = new List<Card>();
        for (int i = 0; i < 7; i++) {
            DrawCard();
        }
    }

    void DrawCard() {
        if (deck.Count > 0) {
            Card drawnCard = deck[0];
            hand.Add(drawnCard);
            cardPool[drawnCard.Idx] = false; // 更新牌池
            deck.RemoveAt(0);
        }
    }


    public void SwapCard(params int[] handIdxs) {
        if (handIdxs == null || handIdxs.Length <= 0) {
            Debug.LogError("傳入參數錯誤");
            return;
        }
        if (playerPT < swapCost) {
            Debug.LogError("點數不夠");
            return;
        }

        var newCards = new List<Card>();

        for (int i = 0; i < handIdxs.Length; i++) {
            int handIdx = handIdxs[i];
            // 將手牌替換
            if (handIdx >= 0 && handIdx < hand.Count) {
                Card oldCard = hand[handIdx];

                // 抽一張新牌
                if (deck.Count > 0) {
                    Card newCard = deck[0];
                    deck.RemoveAt(0);
                    newCards.Add(newCard);

                    // 直接替換手牌中的卡片
                    hand[handIdx] = newCard;
                    cardPool[newCard.Idx] = false; // 更新牌池
                }
            }
        }

        playerPT -= swapCost; // 扣除換牌成本
        swapCount++;
        swapCost = BaseSwapCost + swapCount * SwapCostAdd; // 更新換牌成本
    }

    public void CheckRewards() {
        int rewardPoints = CalculateReward();
        playerPT += rewardPoints;
        StartNewGame(); // 出牌後重置遊戲
    }

    int CalculateReward() {

        int reward = 0;

        if (IsStraightFlush(hand)) {
            reward = 500;
            CurHandType = HandType.StraightFlush;
        } else if (IsFullHouse(hand)) {
            reward = 100;
            CurHandType = HandType.FullHouse;
        } else if (IsThreeOfAKind(hand)) {
            reward = 40;
            CurHandType = HandType.ThreeOfAKind;
        } else if (IsStraight(hand)) {
            reward = 30;
            CurHandType = HandType.Straight;
        } else if (IsFlush(hand)) {
            reward = 20;
            CurHandType = HandType.Flush;
        } else if (IsPair(hand)) {
            reward = 5;
            CurHandType = HandType.Pair;
        } else {
            reward = 0;
            CurHandType = HandType.HighCard;
        }
        return reward;
    }

    bool IsPair(List<Card> cards) {
        var groups = cards.GroupBy(card => card.number);
        return groups.Any(group => group.Count() == 2);
    }

    bool IsFlush(List<Card> cards) {
        var groups = cards.GroupBy(card => card.suit);
        return groups.Any(group => group.Count() >= 5);
    }

    bool IsStraight(List<Card> cards) {

        // 將牌號取出
        var numbers = cards.Select(card => card.number).Distinct().OrderBy(n => n).ToList();
        if (numbers.Contains(1)) {
            numbers.Add(14); // A也可以作為14
        }

        // 檢查是否有連續5個數字的牌
        for (int i = 0; i <= numbers.Count - 5; i++) {
            if (numbers[i + 4] - numbers[i] == 4) {
                return true;
            }
        }

        return false;
    }


    bool IsThreeOfAKind(List<Card> cards) {
        var groups = cards.GroupBy(card => card.number);
        return groups.Any(group => group.Count() == 3);
    }

    bool IsFullHouse(List<Card> cards) {
        var groups = cards.GroupBy(card => card.number);
        bool hasThreeOfAKind = groups.Any(group => group.Count() == 3);
        bool hasPair = groups.Any(group => group.Count() == 2);
        return hasThreeOfAKind && hasPair;
    }

    bool IsStraightFlush(List<Card> cards) {
        var suitedGroups = cards.GroupBy(card => card.suit);
        foreach (var group in suitedGroups) {
            if (group.Count() >= 5) {
                var numbers = group.Select(card => card.number).Distinct().OrderBy(n => n).ToList();
                for (int i = 0; i < numbers.Count - 4; i++) {
                    if (numbers[i + 4] - numbers[i] == 4) {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public Dictionary<int, bool> GetCardPool() {
        return cardPool;
    }
}
