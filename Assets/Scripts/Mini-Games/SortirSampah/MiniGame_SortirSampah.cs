using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MiniGame_SortirSampah : MonoBehaviour
{
    [Header("Referensi Sistem inti")]
    public MiniGameManager miniGameManager;

    [Header("Pengaturan Game")]
    public float timeLimit = 8f;
    private int targetSampah = 12;


    [Header("Referensi UI")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI timerText;

    [Tooltip("Masukkan semua item sampah yang akan disortir")]
    public List<TrashItems> daftarSampah;

    [HideInInspector] public bool isGameActive = false;

    private float timeRemaining;
    private int sampahTersortir = 0;

    // Start is called before the first frame update
    public void StartMiniGame()
    {
        isGameActive = true;
        timeRemaining = timeLimit;
        sampahTersortir = 0;

        statusText.text = "Sortir Sampah!";
        statusText.color = Color.yellow;
        timerText.text = "Waktu|: 8s";

        foreach (TrashItems sampah in daftarSampah)
        {
            sampah.gameObject.SetActive(true);
            sampah.manager = this;
            sampah.SimpanPosisiAwal();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isGameActive)
        {
            timeRemaining -= Time.deltaTime;
            timerText.text = $"Waktu: {Mathf.CeilToInt(timeRemaining)}s";

            if(timeRemaining <= 0)
            {
                //isGameActive = false;
                statusText.text = "Waktu Habis!";
                statusText.color = Color.red;
                EndGame(false);
            }
        }
    }

    public void ItemBerhasilDisortir()
    {
        sampahTersortir++;
        if (sampahTersortir >= targetSampah)
        {
            statusText.text = "Berhasil!";
            statusText.color = Color.green;
            EndGame(true);
        } 
    }

    private void EndGame(bool isWin)
    {
        isGameActive = false;
        miniGameManager.CompleteMiniGame(isWin);
    }
}
