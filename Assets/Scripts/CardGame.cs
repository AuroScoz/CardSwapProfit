using Cysharp.Threading.Tasks;
using Scoz.Func;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro.Examples;
using UnityEditor.iOS;
using UnityEngine;
using UnityEngine.UI;



public class CardGame : MonoBehaviour {

    [SerializeField] Text StartText;
    [SerializeField] GameObject StartGO;
    [SerializeField] GameObject PlayingGO;
    [SerializeField] CardPrefab[] HandPrefabs;
    [SerializeField] CardPrefab[] RewardHandPrefabs;
    [SerializeField] Text PlayrPT;
    [SerializeField] Text SwapCost;
    [SerializeField] Text Reward;
    [SerializeField] Text HandTypeText;
    [SerializeField] GameObject PoolCardParent;
    [SerializeField] GameObject CardPrefab;
    [SerializeField] GameObject CardPoolGO;
    [SerializeField] Animator AddPTTextAni;
    [SerializeField] Text AddPTText;
    [SerializeField] Button PlayAgainBtn;
    [SerializeField] Button SwapBtn;
    [SerializeField] Button ConfirmBtn;
    [SerializeField] AudioSource MyAudioSource;
    [SerializeField] GameObject TipGO;
    [SerializeField] GameObject LastResultGO;

    [SerializeField] int DefaultPlayerPT = 100;
    [SerializeField] int GameCost = 10;
    [SerializeField] int BaseSwapCost = 1;
    [SerializeField] int SwapCostAdd = 1;

    [SerializeField] GameObject RewardGO;
    [SerializeField] Animator RewardAni;
    [SerializeField] Text RewardNumberText;
    [SerializeField] Animator RewardTextAni;


    bool firstGame = true;

    enum GameState {
        Start,
        Playing,
        End
    }

    private void Start() {
        StartText.text = $"花費{GameCost}";
        playerPT = DefaultPlayerPT;
        GoState(GameState.Start);
        InitDeck();
    }
    void GoState(GameState state) {
        switch (state) {
            case GameState.Start:
                TipGO.SetActive(false);
                LastResultGO.SetActive(false);
                RewardGO.SetActive(false);
                StartGO.SetActive(true);
                PlayingGO.SetActive(false);
                PlayAgainBtn.gameObject.SetActive(false);
                SwapBtn.gameObject.SetActive(true);
                ConfirmBtn.gameObject.SetActive(true);
                break;
            case GameState.Playing:
                TipGO.SetActive(true);
                LastResultGO.SetActive(false);
                RewardGO.SetActive(false);
                StartGO.SetActive(false);
                PlayingGO.SetActive(true);
                PlayAgainBtn.gameObject.SetActive(false);
                SwapBtn.gameObject.SetActive(true);
                ConfirmBtn.gameObject.SetActive(true);
                SetHandPrefabsCanReaction(true);
                break;
            case GameState.End:
                TipGO.SetActive(false);
                LastResultGO.SetActive(true);
                RewardGO.SetActive(false);
                StartGO.SetActive(false);
                PlayingGO.SetActive(true);
                PlayAgainBtn.gameObject.SetActive(true);
                SwapBtn.gameObject.SetActive(false);
                ConfirmBtn.gameObject.SetActive(false);
                SetHandPrefabsCanReaction(false);
                break;
        }
    }
    void SetHandPrefabsCanReaction(bool _interactable) {
        foreach (var hand in HandPrefabs) {
            hand.SelectToggle.interactable = _interactable;
        }
    }

    public void OnPlayClick() {
        StartNewGame(); // 開始新遊戲
        GoState(GameState.Playing);
        ShowHandsMatchEffect();
    }

    public void OnSwapClick() {
        List<int> idxs = new List<int>();
        for (int i = 0; i < HandPrefabs.Length; i++) {
            if (HandPrefabs[i].SelectToggle.isOn) {
                idxs.Add(i);
            }
            HandPrefabs[i].SelectToggle.isOn = false;
        }
        if (idxs.Count == 0) return;
        SwapCard(idxs.ToArray());

        RefreshHandsUI();
        RefreshBottomUI();
        ShowHandsMatchEffect();
    }

    public void OnConfirmClick() {
        if (hands.GetHandType() == HandType.FourOfAKind) {
            PlayRewardVoice();
            PlayRewardAni();
        } else {
            int gainPT = hands.GetHandType().GetOdds();
            AddPlayerPT(gainPT);
            RefreshBottomUI();
            GoState(GameState.End);
        }

        for (int i = 0; i < RewardHandPrefabs.Length; i++) {
            RewardHandPrefabs[i].SetImg(hands[i].GetCardSprite());
        }
    }
    void PlayRewardVoice() {
        switch (hands.GetHandType()) {
            case HandType.FourOfAKind:
                MyAudioSource.clip = Resources.Load<AudioClip>("Audios/Annie/Annie Original Taunt 2");
                MyAudioSource.Play();
                break;
        }
    }
    public void OnPlayAgainClick() {
        StartNewGame(); //開始新遊戲
        GoState(GameState.Playing);
        ShowHandsMatchEffect();
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
        for (int i = 0; i < hands.Count; i++) {
            HandPrefabs[i].SetImg(hands[i].GetCardSprite());
            HandPrefabs[i].SelectToggle.SetIsOnWithoutNotify(false);
        }
    }

    void RefreshBottomUI() {
        PlayrPT.text = playerPT.ToString();
        SwapCost.text = $"花費:{swapCost}";
        Reward.text = $"獎勵:{hands.GetHandType().GetOdds()}";
        HandTypeText.text = $"目前牌型: {hands.GetHandType().ToStr()}";
    }

    void ShowHandsMatchEffect() {
        List<int> indices = new List<int>();

        switch (hands.GetHandType()) {
            case HandType.HighCard:
                break;
            case HandType.Pair:
                indices = hands.GetIsPairIndices();
                break;
            case HandType.ThreeOfAKind:
                indices = hands.GetThreeOfAKindIndices();
                break;
            case HandType.Straight:
                indices = hands.GetStraightIndices();
                break;
            case HandType.Flush:
                indices = hands.GetFlushIndices();
                break;
            case HandType.FullHouse:
                indices = hands.GetFullHouseIndices();
                break;
            case HandType.FourOfAKind:
                indices = hands.GetFourOfAKindIndices();
                break;
            case HandType.StraightFlush:
                indices = hands.GetStraightFlushIndices();
                break;
            default:
                break;
        }

        for (int i = 0; i < indices.Count; i++) {
            HandPrefabs[indices[i]].PlayMatchEffect();
        }

    }


    List<Image> poolCardImgs = new List<Image>();
    private List<Card> deck; // 牌池
    private List<Card> hands; // 玩家手牌
    private Dictionary<int, bool> cardPool; // 查看牌池

    private int playerPT; // 玩家點數
    private int swapCost; // 換牌成本
    private int swapCount; // 換牌次數



    void StartNewGame() {

        ShuffleDeck();
        DrawInitialHand();
        AddPlayerPT(-GameCost);
        swapCost = BaseSwapCost;
        swapCount = 0;
        RefreshHandsUI();
        RefreshBottomUI();
    }

    void AddPlayerPT(int _value) {
        if (_value == 0) return;
        playerPT += _value;
        string aniTrigger = "add";
        if (_value < 0) {
            aniTrigger = "reduce";
        }
        AddPTTextAni.SetTrigger(aniTrigger);
        if (_value > 0) AddPTText.text = "+" + _value.ToString();
        else AddPTText.text = _value.ToString();
    }

    void InitDeck() {
        deck = new List<Card>();
        cardPool = new Dictionary<int, bool>();
        for (int suit = 0; suit < 4; suit++) {
            for (int number = 1; number <= 13; number++) {
                var newCard = new Card((SuitType)suit, number);
                deck.Add(newCard);
                cardPool[newCard.Idx] = true;

                var go = Instantiate(CardPrefab, PoolCardParent.transform);
                var cover = go.transform.Find("cover").GetComponent<Image>();
                var img = go.transform.Find("Image").GetComponent<Image>();
                go.transform.GetComponent<Toggle>().interactable = false;
                img.sprite = newCard.GetCardSprite();
                //var text = go.transform.Find("Text").GetComponent<Text>();
                //text.text = newCard.ToString();
                poolCardImgs.Add(cover);
            }
        }
    }
    void ResetCardPool() {
        if (cardPool == null) return;
        foreach (var key in cardPool.Keys.ToList()) {
            cardPool[key] = true;
        }
    }
    void ShuffleDeck() {

        for (int i = 0; i < deck.Count; i++) {
            int randomIndex = UnityEngine.Random.Range(0, deck.Count);
            Card temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
        ResetCardPool();
    }

    void DrawInitialHand() {
        hands = new List<Card>();

        if (firstGame) {
            DrawCard(1);
            DrawCard(14);
            DrawCard(27);
            DrawCard(40);
            DrawCard(15);
            DrawCard(30);
            DrawCard(25);
            firstGame = false;
        } else {
            for (int i = 0; i < 7; i++) {
                DrawCard();
            }
        }
    }




    void DrawCard(int _idx = 0) {
        if (_idx != 0) {
            var findIdx = deck.FindIndex(a => a.Idx == _idx);
            if (findIdx == -1) {
                Debug.LogError("牌池無此idx的牌: " + _idx);
                return;
            }
            hands.Add(deck[findIdx]);
            cardPool[deck[findIdx].Idx] = false; // 更新牌池
            deck.RemoveAt(findIdx);
        } else {
            if (deck.Count > 0) {
                Card drawnCard = deck[0];
                hands.Add(drawnCard);
                cardPool[drawnCard.Idx] = false; // 更新牌池
                deck.RemoveAt(0);
            }
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
            if (handIdx >= 0 && handIdx < hands.Count) {
                // 抽一張新牌
                if (deck.Count > 0) {
                    Card newCard = deck[0];
                    deck.RemoveAt(0);
                    newCards.Add(newCard);

                    // 直接替換手牌中的卡片
                    hands[handIdx] = newCard;
                    cardPool[newCard.Idx] = false; // 更新牌池
                }
            }
        }
        AddPlayerPT(-swapCost);
        swapCount++;
        swapCost = BaseSwapCost + swapCount * SwapCostAdd; // 更新換牌成本
    }

    public Dictionary<int, bool> GetCardPool() {
        return cardPool;
    }

    void PlayRewardAni() {
        RewardGO.SetActive(true);
        RewardAni.SetTrigger("Play");
        RewardNumberText.text = "0";
    }

    public void StartPlayRewardNumberAni() {
        UniTask.Void(async () => {
            int targetNum = hands.GetHandType().GetOdds();
            int curNum = 0;
            int maxAniTime = 1000;
            int delay = 20;
            int addNum = Mathf.Clamp(Mathf.RoundToInt((float)targetNum / ((float)maxAniTime / (float)delay)), 1, int.MaxValue);
            while (curNum < targetNum) {
                curNum += addNum;
                RewardNumberText.text = curNum.ToString();
                await UniTask.Delay(delay);
            }

            RewardTextAni.SetTrigger("Play");
            PointRewardEffect.Instance.PlayReward(hands.GetHandType());
            PostProcessingManager.Instance.SetChromaticAberrationDecayEffect(1f, 1f);
            PlayPostProcessingEffect(1.5f, 0.3f, 1.5f).Forget();

            await UniTask.Delay(1000);
            GoState(GameState.End);
            RewardAni.SetTrigger("End");
            EndRewardAni();
        });

    }

    public void EndRewardAni() {
        RewardGO.SetActive(false);
        UniTask.Void(async () => {
            await UniTask.Delay(1000);
            int gainPT = hands.GetHandType().GetOdds();
            AddPlayerPT(gainPT);
            RefreshBottomUI();
        });

    }


    async UniTaskVoid PlayPostProcessingEffect(float _targetIntensity, float _targetThreshold, float _duration) {
        var bloom = PostProcessingManager.Instance.GetBloom();
        if (bloom == null) return;
        float leftTime = _duration;
        float interval = 0.04f;
        float originalIntensity = bloom.intensity.value;
        float originalThreshold = bloom.threshold.value;
        float addIntensity = (originalIntensity - _targetIntensity) / (_duration / interval);
        float addThreshold = (originalThreshold - _targetThreshold) / (_duration / interval);
        bloom.intensity.value = _targetIntensity;
        bloom.threshold.value = _targetThreshold;
        while (leftTime > 0) {
            await UniTask.Delay(TimeSpan.FromSeconds(interval));
            bloom.intensity.value += addIntensity;
            bloom.threshold.value += addThreshold;
            leftTime -= interval;
        }
        bloom.intensity.value = originalIntensity;
        bloom.threshold.value = originalThreshold;
    }
}
