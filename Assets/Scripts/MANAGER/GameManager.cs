using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Database Kartu")]
    [Tooltip("Masukkan semua file ScriptableObject Kartu Biasa ke sini")]
    public List<CardData> allNormalCards;
    [Tooltip("Masukkan semua file ScriptableObject Kartu Mini-Game ke sini")]
    public List<CardData> allMiniGameCards;

    [Header("Data sesi saat ini")]
    public List<CardData> currentSessionDeck = new List<CardData>();
    public int currentMonth = 1;
    private const int MAX_MONTHS = 30;

    [Header("Referensi Modul Lain")]
    public CardSwipe cardSwipeScript;
    public StatsManager statsManager;
    public MiniGameManager miniGameManager;

    // Start is called before the first frame update
    void Start()
    {
        if(statsManager != null)
        {
            statsManager.ResetStats();
        }
        GenerateSessionDeck();
        DrawCard();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GenerateSessionDeck()
    {
        currentSessionDeck.Clear();
        currentMonth = 1;

        //Wadah sementara
        List<CardData> tempNormalCards = new List<CardData>(allNormalCards);
        List<CardData> tempMiniGameCards = new List<CardData>(allMiniGameCards);

        //Shuffle the lists
        ShuffleList(tempNormalCards);
        ShuffleList(tempMiniGameCards);

        List<CardData> selectedDeck = new List<CardData>();

        for (int i = 0; i < 25 && i < tempNormalCards.Count; i++)
        {
            selectedDeck.Add(tempNormalCards[i]);
        }

        int[] miniGameIntervals = { 3, 8, 15, 23, 28 };

        for (int i = 0; i < 5 && i < tempMiniGameCards.Count; i++)
        {
            int insertIndex = miniGameIntervals[i];
            if (insertIndex < selectedDeck.Count)
            {
                selectedDeck.Insert(insertIndex, tempMiniGameCards[i]);
            }
            else
            {
                selectedDeck.Add(tempMiniGameCards[i]);
            }
        }

        //Masukkan dalam deck permainan
        currentSessionDeck = selectedDeck;
        Debug.Log($"Sesi Baru Dimulai! Total Kartu di Deck: {currentSessionDeck.Count}");
    }

    public void ProcessDecision(bool isRightChoice, CardData playedCard)
    {
        if (statsManager == null)
        {
            return;
        }

        //  CEKATAN KARTU PAKSAAN (MINI-GAME) 
        if (playedCard.isMiniGameCard)
        {
            // Jangan hitung efek stat biasa, langsung lempar ke Mini-Game
            if (miniGameManager != null)
            {
                miniGameManager.TriggerMiniGame(playedCard);
            }

            // Hentikan fungsi di sini! Pergantian bulan (AdvanceMonth) akan 
            // dieksekusi nanti oleh MiniGameResultUI setelah pemain selesai bermain.
            return;
        }

        //Minta statsmanager untuk menghitung efeknya
        statsManager.ApplyCardEffects(isRightChoice, playedCard);

        //Tanya stats manager soal kondisi game over
        if (statsManager.isGameOver())
        {
            Debug.Log("Permainan Selesai! Pemain Kalah!");
            return;
        }

        AdvanceMonth();
    }

    public void DrawCard()
    {
        if(currentMonth > MAX_MONTHS)
        {
            Debug.Log("Permainan Selesai! Pemain Bertahan 30 Bulan!");
            // TODO: Panggil fungsi Game Over (Win Condition)
            return;
        }

        if(currentSessionDeck.Count > 0)
        {
            CardData nextCard = currentSessionDeck[0];
            currentSessionDeck.RemoveAt(0);
            Debug.Log($"[CEK KARTU] Di tangan: {nextCard.cardID} | Di pucuk deck untuk bulan depan: {currentSessionDeck[0].cardID}");

            cardSwipeScript.gameObject.SetActive(true);
            cardSwipeScript.activeCard = nextCard;
            cardSwipeScript.UpdateCardDisplay();
            cardSwipeScript.ResetCardPosition();
            Debug.Log($"Bulan ke-{currentMonth}: Memainkan Kartu {nextCard.cardID}");
        }
    }

    public void AdvanceMonth()
    {
        StartCoroutine(AdvanceMonthRoutine());
    }

    private IEnumerator AdvanceMonthRoutine()
    {
        yield return new WaitForSeconds(.5f); // Tunggu 1 detik sebelum melanjutkan
        currentMonth++;
        DrawCard();
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
