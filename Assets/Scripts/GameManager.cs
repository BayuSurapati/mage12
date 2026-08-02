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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GenerateSessionDeck()
    {

    }

    public void DrawCard()
    {

    }

    public void AdvanceMonth()
    {

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
