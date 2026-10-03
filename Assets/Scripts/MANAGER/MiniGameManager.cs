using UnityEngine;

public class MiniGameManager : MonoBehaviour
{
    [Header("Referensi Modul Utama")]
    public GameManager gameManager;
    public StatsManager statsManager;
    public CardSwipe cardSwipeScript;

    [Header("Referensi Intro & Result UI Mini Games")]
    public MiniGame_IntroUI introUI;
    public MiniGames_ResultUI resultUI;

    [Header("Referensi Script Mini-Games")]
    // Masukkan referensi skrip mini-game spesifik di sini
    public MiniGame_CyberAttack cyberAttackGame;
    public MiniGame_MitigasiBencana mitigasiBencanaGame;
    public MiniGame_SortirSampah sampahGame;
    public MiniGame_DemoMassal demoMassalGame;
    public MiniGame_PuzzleMath puzzleMathGame;
    // public MiniGame_Karhutla karhutlaGame; (untuk nanti)

    private CardData currentMiniGameCard;

    public void TriggerMiniGame(CardData cardData)
    {
        currentMiniGameCard = cardData;
        cardSwipeScript.enabled = false; // Kunci kartu

        //Buka Intro
        if(introUI != null)
        {
            introUI.ShowIntro(cardData);
        }
    }

    public void StartMiniGame()
    {
        // Logika untuk menentukan mini-game mana yang dimainkan
        // (Bisa menggunakan ID kartu atau Enum jenis mini-game dari CardData)

        if (currentMiniGameCard.cardID == "MG-01") // Anggap MG-01 adalah ID untuk Cyber Attack
        {
            // Panel_CyberAttack harusnya diaktifkan di dalam fungsi StartMiniGame() ini
            if (cyberAttackGame != null)
            {
                cyberAttackGame.gameObject.SetActive(true); // Nyalakan panel spesifiknya
                cyberAttackGame.StartMiniGame();
            }
        }

        if(currentMiniGameCard.cardID == "MG-02") // Anggap MG-02 adalah ID untuk Mitigasi Bencana
        {
            if (mitigasiBencanaGame != null)
            {
                mitigasiBencanaGame.gameObject.SetActive(true); // Nyalakan panel spesifiknya
                mitigasiBencanaGame.StartMiniGame();
            }
        }

        if (currentMiniGameCard.cardID == "MG-03") // Anggap MG-03 adalah ID untuk Sortir Sampah
        {
            if (sampahGame != null)
            {
                sampahGame.gameObject.SetActive(true); // Nyalakan panel spesifiknya
                sampahGame.StartMiniGame();
            }
        }

        if (currentMiniGameCard.cardID == "MG-04") // Anggap MG-04 adalah ID untuk Demo Massal
        {
            if (demoMassalGame != null)
            {
                demoMassalGame.gameObject.SetActive(true); // Nyalakan panel spesifiknya
                demoMassalGame.StartMiniGame();
            }
        }

        if (currentMiniGameCard.cardID == "MG-05") // Anggap MG-05 adalah ID untuk Puzzle Math
        {
            if (puzzleMathGame != null)
            {
                puzzleMathGame.gameObject.SetActive(true); // Nyalakan panel spesifiknya
                puzzleMathGame.StartMiniGame();
            }
        }
        // else if (cardData.cardID == "MG-01") { jalankan mini-game banjir... }
    }

    public void CompleteMiniGame(bool isWin)
    {
        // Matikan panel spesifik setelah selesai
        if (cyberAttackGame != null && cyberAttackGame.gameObject.activeSelf)
        {
            cyberAttackGame.gameObject.SetActive(false);
        }

        if (mitigasiBencanaGame != null && mitigasiBencanaGame.gameObject.activeSelf)
        {
            mitigasiBencanaGame.gameObject.SetActive(false);
        }

        if (sampahGame != null &&   sampahGame.gameObject.activeSelf)
        {
            sampahGame.gameObject.SetActive(false);
        }

        if (demoMassalGame != null && demoMassalGame.gameObject.activeSelf)
        {
            demoMassalGame.gameObject.SetActive(false);
        }

        if (puzzleMathGame != null && puzzleMathGame.gameObject.activeSelf)
        {
            puzzleMathGame.gameObject.SetActive(false);
        }

        if (resultUI != null)
        {
            resultUI.ShowResult(isWin, currentMiniGameCard);
        }
    }

    public void ContinueAfterMiniGame(bool isWin)
    {
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

        // Buka kunci kartu dan lanjutkan game
        cardSwipeScript.enabled = true;

        if (!statsManager.isGameOver())
        {
            gameManager.AdvanceMonth();
        }
    }
}