using UnityEngine;

public class MiniGameManager : MonoBehaviour
{
    [Header("Referensi Modul Utama")]
    public GameManager gameManager;
    public StatsManager statsManager;
    public CardSwipe cardSwipeScript;

    [Header("Referensi Intro UI Mini Games")]
    public MiniGame_IntroUI introUI;

    [Header("Referensi Script Mini-Games")]
    public MiniGame_CyberAttack cyberAttackGame;
    public MiniGame_MitigasiBencana mitigasiBencanaGame;
    public MiniGame_SortirSampah sampahGame;
    public MiniGame_DemoMassal demoMassalGame;
    public MiniGame_PuzzleMath puzzleMathGame;

    [HideInInspector] public CardData currentMiniGameCard;

    public void TriggerMiniGame(CardData cardData)
    {
        currentMiniGameCard = cardData;
        cardSwipeScript.enabled = false; // Kunci kartu

        if (introUI != null)
        {
            introUI.ShowIntro(cardData);
        }
    }

    public void StartMiniGame()
    {
        if (currentMiniGameCard.cardID == "MG-01" && cyberAttackGame != null)
        {
            cyberAttackGame.gameObject.SetActive(true);
            cyberAttackGame.StartMiniGame();
        }
        else if (currentMiniGameCard.cardID == "MG-02" && mitigasiBencanaGame != null)
        {
            mitigasiBencanaGame.gameObject.SetActive(true);
            mitigasiBencanaGame.StartMiniGame();
        }
        else if (currentMiniGameCard.cardID == "MG-03" && sampahGame != null)
        {
            sampahGame.gameObject.SetActive(true);
            sampahGame.StartMiniGame();
        }
        else if (currentMiniGameCard.cardID == "MG-04" && demoMassalGame != null)
        {
            demoMassalGame.gameObject.SetActive(true);
            demoMassalGame.StartMiniGame();
        }
        else if (currentMiniGameCard.cardID == "MG-05" && puzzleMathGame != null)
        {
            puzzleMathGame.gameObject.SetActive(true);
            puzzleMathGame.StartMiniGame();
        }
    }

    public void CompleteMiniGame(bool isWin)
    {
        // 1. Terapkan efek stat seketika saat game selesai
        if (isWin)
        {
            statsManager.ApplyMiniGameEffects(
                currentMiniGameCard.efekWinBencana,
                currentMiniGameCard.efekWinEkosistem,
                currentMiniGameCard.efekWinKeuangan,
                currentMiniGameCard.efekWinTeknologi
            );
        }
        else
        {
            statsManager.ApplyMiniGameEffects(
                currentMiniGameCard.efekLoseBencana,
                currentMiniGameCard.efekLoseEkosistem,
                currentMiniGameCard.efekLoseKeuangan,
                currentMiniGameCard.efekLoseTeknologi
            );
        }
        // PENTING: Jangan matikan panel game di sini, agar panel Hasil (Menang/Kalah) tetap terlihat!
    }

    // 👇 FUNGSI BARU UNTUK MENUTUP PANEL & LANJUT BULAN 👇
    public void LanjutKeBulanBerikutnya()
    {
        // 1. Matikan semua panel mini-game dari layar (Ini yang membuat panel tertutup)
        if (cyberAttackGame != null) cyberAttackGame.gameObject.SetActive(false);
        if (mitigasiBencanaGame != null) mitigasiBencanaGame.gameObject.SetActive(false);
        if (sampahGame != null) sampahGame.gameObject.SetActive(false);
        if (demoMassalGame != null) demoMassalGame.gameObject.SetActive(false);
        if (puzzleMathGame != null) puzzleMathGame.gameObject.SetActive(false);

        // 2. Buka kunci kartu
        cardSwipeScript.enabled = true;

        // 3. Maju ke bulan berikutnya
        if (!statsManager.isGameOver())
        {
            gameManager.AdvanceMonth();
        }
    }
}