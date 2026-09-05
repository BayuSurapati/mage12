using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    [Header("Indikator Daerah (0-100)")]
    public int statBencana = 50;
    public int statEkosistem  = 50;
    public int statKeuangan = 50;
    public int statTeknologi = 50;

    [Header("Referensi Modul UI")]
    public UIManager uiManager;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetStats()
    {
        statBencana = 50;
        statEkosistem = 50;
        statKeuangan = 50;
        statTeknologi = 50;

        if(uiManager != null)
        {
            uiManager.UpdateProgressBars();
        }
    }

    public void ApplyCardEffects(bool isRightChoice, CardData playedCard)
    {
        if (isRightChoice)
        {
            statBencana += playedCard.efekKananBencana;
            statEkosistem += playedCard.efekKananEkosistem;
            statKeuangan += playedCard.efekKananKeuangan;
            statTeknologi += playedCard.efekKananTeknologi;
        }
        else
        {
            statBencana += playedCard.efekKiriBencana;
            statEkosistem += playedCard.efekKiriEkosistem;
            statKeuangan += playedCard.efekKiriKeuangan;
            statTeknologi += playedCard.efekKiriTeknologi;
        }

        ClampStats();

        if(uiManager != null)
        {
            uiManager.UpdateProgressBars();
        }

        Debug.Log($"[StatsManager] Status Baru -> Bencana: {statBencana} | Eko: {statEkosistem} | Uang: {statKeuangan} | Tech: {statTeknologi}");
    }

    public void ApplyMiniGameEffects(int efekBencana, int efekEkosistem, int efekKeuangan, int efekTeknologi)
    {
        statBencana += efekBencana;
        statEkosistem += efekEkosistem;
        statKeuangan += efekKeuangan;
        statTeknologi += efekTeknologi;

        ClampStats();

        if (uiManager != null)
        {
            uiManager.UpdateProgressBars();
        }
        Debug.Log($"[StatsManager] Status Baru (Mini-Game) -> Bencana: {statBencana} | Eko: {statEkosistem} | Uang: {statKeuangan} | Tech: {statTeknologi}");
    }

    private void ClampStats()
    {
        statBencana = Mathf.Clamp(statBencana, 0, 100);
        statEkosistem = Mathf.Clamp(statEkosistem, 0, 100);
        statKeuangan = Mathf.Clamp(statKeuangan, 0, 100);
        statTeknologi = Mathf.Clamp(statTeknologi, 0, 100);
    }

    //Lapor ke game manager kalau game sudah selesai
    public bool isGameOver()
    {
        return statBencana <= 0 || statBencana >= 100 ||
               statEkosistem <= 0 || statEkosistem >= 100 ||
               statKeuangan <= 0 || statKeuangan >= 100 ||
               statTeknologi <= 0 || statTeknologi >= 100;
    }
}
