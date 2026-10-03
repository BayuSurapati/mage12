using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiniGame_MitigasiBencana : MonoBehaviour
{
    [Header("Referensi Sistem Manager")]
    public MiniGameManager miniGameManager;

    [Header("Pengaturan Game")]
    public int sequenceLength = 4;
    public float timeLimit = 5.0f;
    public float flashSpeed = .3f;

    [Header("Referensi UI")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI timerText;

    [Tooltip("Masukkan 4 tonbol secara berurutan, dari kiri ke kanan")]
    public Button[] colorButtons;

    //Variabel internal
    private List<int> targetSequence = new List<int>();
    private int playerInputIndex = 0;
    private float timeRemaining;

    private bool isPlayerTurn = false;
    private bool isGameActive = false;

    //Untuk menyimpan warna asli tombol agar bisa dikembalikan setelah di-flash
    private Color[] originalColor;

    void Awake()
    {
        // Simpan referensi tombol asli
        originalColor = new Color[colorButtons.Length];
        for (int i = 0; i < colorButtons.Length; i++)
        {
            originalColor[i] = colorButtons[i].GetComponent<Image>().color;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
   

    public void StartMiniGame()
    {
        isGameActive = true;
        isPlayerTurn = false;
        playerInputIndex = 0;
        timeRemaining = timeLimit;

        timerText.text = "Waktu: --";
        statusText.text = "Perhatikan urutan tombol!";
        statusText.color = Color.yellow;

        SetButtonsInteractable(false);
        GenerateSequence();
        StartCoroutine(PlaySequence());
    }

    void Update()
    {
        // Timer HANYA berjalan jika ini adalah giliran pemain dan game masih aktif
        if (isGameActive && isPlayerTurn)
        {
            timeRemaining -= Time.deltaTime;
            timerText.text = "Waktu: " + Mathf.CeilToInt(timeRemaining).ToString() + "s";

            if (timeRemaining <= 0)
            {
                timerText.text = "Waktu Habis!";
                EndMiniGame(false); // Kalah karena kehabisan waktu
            }
        }
    }

    public void GenerateSequence()
    {
        targetSequence.Clear();
        for (int i = 0; i < sequenceLength; i++)
        {
            // Pilih angka acak dari 0 sampai jumlah tombol (misal 0 sampai 3)
            int randomColorIndex = Random.Range(0, colorButtons.Length);
            targetSequence.Add(randomColorIndex);
        }
    }

    private IEnumerator PlaySequence()
    {
        yield return new WaitForSeconds(0.5f);

        foreach (int colorIndex in targetSequence)
        {
            yield return StartCoroutine(FlashButton(colorIndex));
            yield return new WaitForSeconds(0.2f); // Jeda antar tombol
        }

        isPlayerTurn = true;
        statusText.text = "Giliran Anda! Tekan tombol sesuai urutan!";
        statusText.color= Color.red;

        //Tombol Interactable
        SetButtonsInteractable(true);

    }

    private IEnumerator FlashButton(int index)
    {
        Image btnImage = colorButtons[index].GetComponent<Image>();
        btnImage.color = Color.white;
        yield return new WaitForSeconds(flashSpeed);
        btnImage.color = originalColor[index];

    }

    public void OnButtonPressed(int buttonIndex)
    {
        if(!isPlayerTurn || !isGameActive)
            return;

        if(buttonIndex == targetSequence[playerInputIndex])
        {
            playerInputIndex++;

            StartCoroutine(FlashButton(buttonIndex));

            if (playerInputIndex >= sequenceLength)
            {
                statusText.text = "MITIGASI Berhasil";
                statusText.color = Color.green;
                EndMiniGame(true);
            }
        }
        else
        {
            statusText.text = "KODE SALAH!";
            statusText.color = Color.red;
            EndMiniGame(false);
        }
    }

    private void SetButtonsInteractable(bool state)
    {
        foreach (Button btn in colorButtons)
        {
            btn.interactable = state;
        }
    }

    private void EndMiniGame(bool isWin)
    {
        isGameActive = false;
        isPlayerTurn = false;
        SetButtonsInteractable(false);

        miniGameManager.CompleteMiniGame(isWin);
    }
}
