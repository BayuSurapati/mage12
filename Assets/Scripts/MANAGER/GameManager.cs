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

    // Start is called before the first frame update
    void Start()
    {
        if(statsManager != null)
        {
            statsManager.ResetStats();
        }

        DrawCard();
        GenerateSessionDeck();
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

        //Add 20 normal cards and 10 mini games
        int totalNormalNeed = 20;
        int totalMiniGameNeed = 10;

        List<CardData> selectedDeck = new List<CardData>();

        for (int i = 0; i < totalNormalNeed && i < tempNormalCards.Count; i++)
        {
            selectedDeck.Add(tempNormalCards[i]);
        }
        for (int i = 0; i < totalMiniGameNeed && i < tempMiniGameCards.Count; i++)
        {
            selectedDeck.Add(tempMiniGameCards[i]);
        }

        //Shuffle 30 cards
        ShuffleList(selectedDeck);

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
            cardSwipeScript.activeCard = nextCard;
            cardSwipeScript.UpdateCardDisplay();

            Debug.Log($"Bulan ke-{currentMonth}: Memainkan Kartu {nextCard.cardID}");
        }
        else
        {
            Debug.LogError("Kehabisan Kartu di Deck!");
        }
    }

    public void AdvanceMonth()
    {
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
