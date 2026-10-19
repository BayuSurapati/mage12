using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    [Header("Referensi UI (Bulan/Turn)")]
    public TextMeshProUGUI monthTextUI;

    [Header("Referensi Modul Lain")]
    public CardSwipe cardSwipeScript;
    public StatsManager statsManager;
    public MiniGameManager miniGameManager;

    [Header("UI Panel Ending Tunggal")]
    public GameObject panelEnding;
    public RectTransform rectPanelEnding;
    public CanvasGroup canvasGroupEnding;

    [Header("Elemen Teks Panel Ending")]
    public TextMeshProUGUI teksJudulEnding;
    public TextMeshProUGUI teksSebabEnding;
    public TextMeshProUGUI teksAkibatEnding;

    [Header("Visual 9 Ending (Opsional)")]
    [Tooltip("Masukkan 9 GameObject/Gambar background ending berurutan (1-9)")]
    public GameObject[] visualEnding; // Array untuk memunculkan 9 gambar berbeda

    // Variabel delay sebelum kembali ke Main Menu otomatis
    private float delaySebelumReset = 5f;

    void Start()
    {
        if (panelEnding != null) panelEnding.SetActive(false);

        if (statsManager != null)
        {
            statsManager.ResetStats();
        }
        GenerateSessionDeck();
        currentMonth = 1;
        UpdateTurnDisplay();

        DrawCard();
    }

    void Update()
    {

    }

    private void GenerateSessionDeck()
    {
        currentSessionDeck.Clear();
        currentMonth = 1;

        List<CardData> tempNormalCards = new List<CardData>(allNormalCards);
        List<CardData> tempMiniGameCards = new List<CardData>(allMiniGameCards);

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

        currentSessionDeck = selectedDeck;
        Debug.Log($"Sesi Baru Dimulai! Total Kartu di Deck: {currentSessionDeck.Count}");
    }

    public void ProcessDecision(bool isRightChoice, CardData playedCard)
    {
        if (statsManager == null) return;

        if (playedCard.isMiniGameCard)
        {
            if (miniGameManager != null) miniGameManager.TriggerMiniGame(playedCard);
            return;
        }

        statsManager.ApplyCardEffects(isRightChoice, playedCard);

        if (statsManager.isGameOver())
        {
            Debug.Log("Permainan Selesai! Pemain Kalah!");

            // Ambil ID tipe kekalahan dinamis dari StatsManager
            int tipeEndingKalah = statsManager.DapatkanTipeKekalahan();

            if (cardSwipeScript != null) cardSwipeScript.gameObject.SetActive(false);

            TampilkanEnding(tipeEndingKalah);
            return;
        }

        AdvanceMonth();
    }

    public void DrawCard()
    {
        if (currentMonth > MAX_MONTHS)
        {
            Debug.Log("Permainan Selesai! Pemain Bertahan 30 Bulan!");
            if (cardSwipeScript != null) cardSwipeScript.gameObject.SetActive(false);

            // Panggil Ending Kemenangan
            TampilkanEnding(9);
            return;
        }

        if (currentSessionDeck.Count > 0)
        {
            CardData nextCard = currentSessionDeck[0];
            currentSessionDeck.RemoveAt(0);
            Debug.Log($"[CEK KARTU] Di tangan: {nextCard.cardID}");

            cardSwipeScript.gameObject.SetActive(true);
            cardSwipeScript.activeCard = nextCard;
            cardSwipeScript.UpdateCardDisplay();
            cardSwipeScript.ResetCardPosition();
        }
    }

    public void AdvanceMonth()
    {
        StartCoroutine(AdvanceMonthRoutine());
    }

    private IEnumerator AdvanceMonthRoutine()
    {
        yield return new WaitForSeconds(.5f);
        currentMonth++;
        UpdateTurnDisplay();
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

    private void UpdateTurnDisplay()
    {
        if (monthTextUI != null)
        {
            monthTextUI.text = "Day-" + currentMonth.ToString();
            monthTextUI.transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                monthTextUI.transform.DOScale(1f, 0.2f);
            });
        }
    }

    // ==========================================
    // SISTEM ANIMASI PANEL ENDING OTOMATIS KELUAR
    // ==========================================
    public void TampilkanEnding(int tipeEnding)
    {
        // Tetap gunakan fungsi SetupTeksEnding milik Anda!
        SetupTeksEnding(tipeEnding);

        // Menyalakan visual/gambar yang tepat untuk ending tersebut
        if (visualEnding != null && visualEnding.Length > 0)
        {
            // Matikan semua gambar ilustrasi terlebih dahulu
            for (int i = 0; i < visualEnding.Length; i++)
            {
                if (visualEnding[i] != null) visualEnding[i].SetActive(false);
            }

            // Nyalakan HANYA gambar yang sesuai dengan tipe ending
            int indeksVisual = tipeEnding - 1;
            if (indeksVisual >= 0 && indeksVisual < visualEnding.Length)
            {
                if (visualEnding[indeksVisual] != null)
                    visualEnding[indeksVisual].SetActive(true);
            }
        }

        if (panelEnding != null)
        {
            panelEnding.SetActive(true);

            if (rectPanelEnding != null) rectPanelEnding.localScale = Vector3.zero;

            if (canvasGroupEnding != null)
            {
                canvasGroupEnding.alpha = 0f;
                canvasGroupEnding.DOFade(1f, 0.4f);
            }

            if (rectPanelEnding != null)
            {
                // Animasi membal masuk
                rectPanelEnding.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetDelay(0.3f);

                // Animasi mengecil dan pudar sebelum reset otomatis
                rectPanelEnding.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).SetDelay(delaySebelumReset - 0.5f);
                if (canvasGroupEnding != null) canvasGroupEnding.DOFade(0f, 0.5f).SetDelay(delaySebelumReset - 0.5f);
            }

            // Lakukan reset otomatis ke Main Menu
            DOVirtual.DelayedCall(delaySebelumReset, () =>
            {
                SceneManager.LoadScene("MainMenu");
            });
        }
    }

    private void SetupTeksEnding(int tipe)
    {
        switch (tipe)
        {
            case 1: teksJudulEnding.text = "DAERAH HANCUR"; teksSebabEnding.text = "Minimnya tanggap darurat dan mitigasi"; teksAkibatEnding.text = "Badai meratakan kota. Anda dicopot paksa."; break;
            case 2: teksJudulEnding.text = "KOTA BUNKER"; teksSebabEnding.text = "Anggaran dihabiskan untuk infrastruktur anti-kiamat"; teksAkibatEnding.text = "Warga stres hidup seperti di kamp militer."; break;
            case 3: teksJudulEnding.text = "DARURAT POLUSI"; teksSebabEnding.text = "Polusi merajalela dan udara beracun"; teksAkibatEnding.text = "Anda dievakuasi karena penyakit massal."; break;
            case 4: teksJudulEnding.text = "HUTAN BETON KEMBALI ALAMI"; teksSebabEnding.text = "Anda menutup seluruh industri demi penghijauan"; teksAkibatEnding.text = "Peradaban lumpuh. Anda diturunkan paksa."; break;
            case 5: teksJudulEnding.text = "KEBANGKRUTAN MASSAL"; teksSebabEnding.text = "Gaji pegawai tidak terbayar dan kas daerah kosong"; teksAkibatEnding.text = "Anda ditangkap atas dugaan salah urus anggaran."; break;
            case 6: teksJudulEnding.text = "REVOLUSI SOSIAL"; teksSebabEnding.text = "Keuntungan daerah yang terlalu ekstrem"; teksAkibatEnding.text = "Banyak korupsi di pemerintahan anda."; break;
            case 7: teksJudulEnding.text = "KOTA TERTINGGAL"; teksSebabEnding.text = "Sistem lumpuh karena menolak modernisasi"; teksAkibatEnding.text = "Pusat mencopot Anda karena dianggap tidak kompeten."; break;
            case 8: teksJudulEnding.text = "PENGANGGURAN AKIBAT AI"; teksSebabEnding.text = "Otomatisasi total menggusur lapangan pekerjaan"; teksAkibatEnding.text = "Kelompok pekerja yang marah menyerbu balai kota."; break;
            case 9: teksJudulEnding.text = "MASA BAKTI SELESAI"; teksSebabEnding.text = "30 bulan memimpin dengan penuh keseimbangan"; teksAkibatEnding.text = "Anda pensiun dengan tenang sebagai legenda."; break;
            default: teksJudulEnding.text = "LAPORAN"; teksSebabEnding.text = "Sebab tidak diketahui"; teksAkibatEnding.text = "Anda kehilangan jabatan."; break;
        }
    }
}