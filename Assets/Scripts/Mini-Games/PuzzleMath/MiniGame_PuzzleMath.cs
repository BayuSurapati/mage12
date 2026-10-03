using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MiniGame_PuzzleMath : MonoBehaviour
{
    [Header("Referensi Sistem Inti")]
    public MiniGameManager miniGameManager;

    [Header("Pengaturan Game")]
    public float timeLimit = 15f; // Batas waktu dalam detik

    [Header("Referensi UI")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI operatorText; // Menampilkan logo +, -, x, atau /
    public TextMeshProUGUI resultText;   // Menampilkan target hasil ( = X )

    [Header("Referensi Slot & Balok")]
    public MathSlot slot1;
    public MathSlot slot2;
    public MathBlock[] answerBlocks; // Masukkan 4 kotak jawaban ke sini

    [HideInInspector] public bool isGameActive = false;

    private float timeRemaining;
    private int targetResult;
    private string currentOp;

    public void StartMiniGame()
    {
        isGameActive = true;
        timeRemaining = timeLimit;

        statusText.text = "AUDIT KEUANGAN!";
        statusText.color = Color.yellow;
        timerText.text = "Waktu: 15s";

        // Pastikan semua slot kosong dan blok di bawah
        foreach (MathBlock block in answerBlocks) block.SnapBack();

        GenerateMathProblem();
    }

    void Update()
    {
        if (!isGameActive) return;

        timeRemaining -= Time.deltaTime;
        timerText.text = "Waktu: " + Mathf.CeilToInt(timeRemaining).ToString() + "s";

        if (timeRemaining <= 0)
        {
            timerText.text = "Waktu Habis!";
            EndGame(false);
        }
    }

    private void GenerateMathProblem()
    {
        int num1 = 0, num2 = 0;
        int opType = Random.Range(0, 4); // 0=Tambah, 1=Kurang, 2=Kali, 3=Bagi

        if (opType == 0) // PENJUMLAHAN
        {
            num1 = Random.Range(1, 26);
            num2 = Random.Range(1, 26);
            targetResult = num1 + num2;
            currentOp = "+";
        }
        else if (opType == 1) // PENGURANGAN
        {
            num1 = Random.Range(1, 26);
            num2 = Random.Range(1, 26);
            if (num1 < num2) { int temp = num1; num1 = num2; num2 = temp; } // Pastikan hasilnya positif
            targetResult = num1 - num2;
            currentOp = "-";
        }
        else if (opType == 2) // PERKALIAN
        {
            num1 = Random.Range(1, 11); // Angka diperkecil agar bisa dihitung otak
            num2 = Random.Range(1, 11);
            targetResult = num1 * num2;
            currentOp = "x";
        }
        else if (opType == 3) // PEMBAGIAN
        {
            targetResult = Random.Range(1, 11);
            num2 = Random.Range(1, 11);
            num1 = targetResult * num2; // Memastikan pembagian bulat tanpa desimal
            currentOp = "/";
        }

        operatorText.text = currentOp;
        resultText.text = "= " + targetResult.ToString();

        // Siapkan 4 pilihan (2 Benar, 2 Salah)
        List<int> choices = new List<int> { num1, num2 };

        while (choices.Count < 4)
        {
            int randomNum = Random.Range(1, 26);
            if (!choices.Contains(randomNum)) choices.Add(randomNum);
        }

        // Acak urutan 4 pilihan tersebut
        for (int i = 0; i < choices.Count; i++)
        {
            int temp = choices[i];
            int randomIndex = Random.Range(i, choices.Count);
            choices[i] = choices[randomIndex];
            choices[randomIndex] = temp;
        }

        // Berikan angka ke balok UI
        for (int i = 0; i < 4; i++)
        {
            answerBlocks[i].manager = this;
            answerBlocks[i].SetupBlock(choices[i]);
        }
    }

    public void CheckWinCondition()
    {
        // Mengecek HANYA KETIKA kedua slot sudah terisi
        if (slot1.currenBlock != null && slot2.currenBlock != null)
        {
            int val1 = slot1.currenBlock.blockValue;
            int val2 = slot2.currenBlock.blockValue;
            bool isCorrect = false;

            // Logika Evaluasi Ganda (Membaca Kiri ke Kanan ATAU Kanan ke Kiri)
            if (currentOp == "+") isCorrect = (val1 + val2 == targetResult);
            else if (currentOp == "-") isCorrect = (val1 - val2 == targetResult || val2 - val1 == targetResult);
            else if (currentOp == "x") isCorrect = (val1 * val2 == targetResult);
            else if (currentOp == "/") isCorrect = (val1 / (float)val2 == targetResult || val2 / (float)val1 == targetResult);

            if (isCorrect)
            {
                statusText.text = "AUDIT SESUAI!";
                statusText.color = Color.green;
                EndGame(true);
            }
            else
            {
                // Jika salah, buang waktu dengan snap back tanpa kurangi timer
                statusText.text = "SALAH HITUNG!";
                statusText.color = Color.red;
                slot1.currenBlock.SnapBack();
                slot2.currenBlock.SnapBack();
            }
        }
    }

    private void EndGame(bool isWin)
    {
        isGameActive = false;
        miniGameManager.CompleteMiniGame(isWin);
    }
}
