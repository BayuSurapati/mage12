using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class MiniGames_ResultUI : MonoBehaviour
{
    [Header("Referensi Panel")]
    [Tooltip("Masukkan panel hasil mini-game di sini")]
    public GameObject resultPanel;

    [Header("Referensi Teks UI")]
    public TextMeshProUGUI titleText; // Menampilkan "Berhasil!" atau "Gagal!"
    public TextMeshProUGUI detailBencanaText;
    public TextMeshProUGUI detailEkosistemText;
    public TextMeshProUGUI detailKeuanganText;
    public TextMeshProUGUI detailTeknologiText;

    [Header("Referensi Sistem")]
    public MiniGameManager miniGameManager;

    private bool currentResultWin;
    private CardData currentCardData;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowResult(bool isWin, CardData cardData)
    {
        currentResultWin = isWin;
        currentCardData = cardData;

        resultPanel.SetActive(true);

        // Update UI based on the result
        if (isWin)
        {
            titleText.text = "Berhasil!";
            detailBencanaText.text = $"Bencana: {cardData.efekWinBencana}";
            detailEkosistemText.text = $"Ekosistem: {cardData.efekWinEkosistem}";
            detailKeuanganText.text = $"Keuangan: {cardData.efekWinKeuangan}";
            detailTeknologiText.text = $"Teknologi: {cardData.efekWinTeknologi}";
        }
        else
        {
            titleText.text = "Gagal!";
            detailBencanaText.text = $"Bencana: {cardData.efekLoseBencana}";
            detailEkosistemText.text = $"Ekosistem: {cardData.efekLoseEkosistem}";
            detailKeuanganText.text = $"Keuangan: {cardData.efekLoseKeuangan}";
            detailTeknologiText.text = $"Teknologi: {cardData.efekLoseTeknologi}";
        }
    }

    public void SetStatsText(TextMeshProUGUI textUI, string statName, int value)
    {
        if(value > 0) {
            textUI.text = $"{statName}: +{value}";
            textUI.color = Color.green;
        } else if(value < 0)
        {
            textUI.text = $"{statName}: -{value}";
            textUI.color = Color.red;
        }
        else
        {
            textUI.text = $"{statName}: 0 (Tidak Berubah)";
            textUI.color = Color.gray; // Warna abu-abu jika efeknya 0
        }
    }

    public void NextButtonPressed()
    {
        resultPanel.SetActive(false);

        if(miniGameManager != null)
        {
            miniGameManager.ContinueAfterMiniGame(currentResultWin);
        }
    }
}
