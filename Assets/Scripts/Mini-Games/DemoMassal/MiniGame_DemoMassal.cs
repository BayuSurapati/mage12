using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class MiniGame_DemoMassal : MonoBehaviour
{
    [Header("Referensi Sistem Inti")]
    public MiniGameManager miniGameManager;

    [Header("Pengaturan Game")]
    public float timeLimit = 15f; // Batas waktu dalam detik
    public int targetKesuksesan = 3; // Target jumlah aksi sukses

    [Header("Pengaturan Timing Bar")]
    public float basePointerSpeed = 300f; // Kecepatan dasar pointer
    public float speedMultiplier = 1.3f; // Faktor pengali kecepatan
    public float targetShrinkFactor = 0.8f; // Faktor penyusutan target
    public float stunDuration = 1f; // Durasi stun dalam detik

    [Header("Referensi UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI statusText;

    public RectTransform targetArea;
    public RectTransform pointer;
    public RectTransform timingBar;

    //Variabel internal
    private bool isGameActive = false;
    private float timeRemaining;
    private int successfulActions;

    private float currentSpeed;
    private int moveDirection = 1; // 1 untuk kanan, -1 untuk kiri
    private float stunTimer = 0f;

    private Vector2 originalTargetSize;
    private void Awake()
    {
        if(targetArea != null)
        {
            // Simpan ukuran asli area target agar bisa dikembalikan saat game di-reset
            originalTargetSize = targetArea.sizeDelta;
        }
    }

    // Start is called before the first frame update
    public void StartMiniGame()
    {
        isGameActive = true;
        timeRemaining = timeLimit;
        successfulActions = 0;
        currentSpeed = basePointerSpeed;
        stunTimer = 0f;

        //kembalikan ukuran target ke ukuran asli saat game dimulai
        targetArea.sizeDelta = originalTargetSize;

        statusText.text = "Negosiasi Dimulai!";
        statusText.color = Color.yellow;
        timerText.text = "Waktu:10s";
    }

    // Update is called once per frame
    void Update()
    {
        if (!isGameActive) return;

        // 1. Logika Timer Keseluruhan
        timeRemaining -= Time.deltaTime;
        timerText.text = "Waktu: " + Mathf.CeilToInt(timeRemaining).ToString() + "s";

        if (timeRemaining <= 0)
        {
            timerText.text = "Waktu Habis!";
            EndGame(false); // Gagal negosiasi
            return;
        }

        // 2. Logika Penalti (Stun)
        if (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
            return; // Hentikan pergerakan jarum selama masa penalti
        }

        // 3. Logika Pergerakan Jarum (Bolak-balik)
        float maxX = (timingBar.rect.width / 2f) - (pointer.rect.width / 2f);

        // Gerakkan jarum berdasarkan kecepatan dan arah
        pointer.anchoredPosition += new Vector2(currentSpeed * moveDirection * Time.deltaTime, 0);

        // Jika jarum menabrak ujung kanan, pantulkan ke kiri
        if (pointer.anchoredPosition.x >= maxX)
        {
            pointer.anchoredPosition = new Vector2(maxX, pointer.anchoredPosition.y);
            moveDirection = -1;
        }
        // Jika jarum menabrak ujung kiri, pantulkan ke kanan
        else if (pointer.anchoredPosition.x <= -maxX)
        {
            pointer.anchoredPosition = new Vector2(-maxX, pointer.anchoredPosition.y);
            moveDirection = 1;
        }
    }

    public void OnHitButton()
    {
        if (!isGameActive || stunTimer > 0f) return;

        // Menghitung batas kiri dan kanan dari Target Area saat ini
        float targetHalfWidth = targetArea.rect.width / 2f;
        float minX = targetArea.anchoredPosition.x - targetHalfWidth;
        float maxX = targetArea.anchoredPosition.x + targetHalfWidth;

        float pointerX = pointer.anchoredPosition.x;

        if(pointerX >= minX && pointerX <= maxX)
        {
            Berhasil();
        }
        else
        {
           Meleset();
        }
    }

    public void Berhasil()
    {
        successfulActions++;

        if(successfulActions >= targetKesuksesan)
        {
            statusText.text = "Demo Selesai!";
            statusText.color = Color.green;
            EndGame(true);
        }
        else
        {
            statusText.text = $"Berhasil! ({successfulActions}/{targetKesuksesan})";
            statusText.color = Color.blue;

            //Tingkatkan kesulitan dengan memperkecil area target dan meningkatkan kecepatan pointer
            currentSpeed *= speedMultiplier;
            targetArea.sizeDelta = new Vector2(targetArea.sizeDelta.x * targetShrinkFactor, targetArea.sizeDelta.y);//Makin Kecil

            AcakPosisiTarget();
        }

    }

    public void Meleset()
    {
        statusText.text = "Meleset!";
        statusText.color = Color.red;

        //Kena stun sesuai durasi stun yang ditentukan
        stunTimer = stunDuration;
    }

    public void AcakPosisiTarget()
    {
        // Supaya letak area hijau tidak statis di tengah terus, kita acak posisinya\
        float maxPos = (timingBar.rect.width / 2f) - (targetArea.rect.width / 2f);
        float randomX = UnityEngine.Random.Range(-maxPos, maxPos);
        targetArea.anchoredPosition = new Vector2(randomX, targetArea.anchoredPosition.y);
    }

    private void EndGame(bool isWin)
    {
        isGameActive = false;
        miniGameManager.CompleteMiniGame(isWin);
    }
}
