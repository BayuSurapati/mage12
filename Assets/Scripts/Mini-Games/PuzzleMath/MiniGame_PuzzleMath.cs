using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening; // WAJIB: Impor DOTween

public class MiniGame_PuzzleMath : MonoBehaviour
{
    [Header("Referensi Sistem Inti")]
    public MiniGameManager miniGameManager;

    [Header("Pengaturan Game")]
    public float timeLimit = 15f;

    [Header("Referensi UI Gameplay")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI operatorText;
    public TextMeshProUGUI resultText;

    [Header("Referensi Animasi UI")]
    public RectTransform[] equationElements; // Masukkan Slot 1, Teks Operator, Slot 2, Teks Sama Dengan, dll.

    [Header("Referensi Slot & Balok")]
    public MathSlot slot1;
    public MathSlot slot2;
    public MathBlock[] answerBlocks;

    [Header("UI Hasil (PANEL MENANG & KALAH)")]
    public GameObject panelMenang;
    public GameObject panelKalah;
    public RectTransform rectMenang;
    public RectTransform rectKalah;
    public CanvasGroup canvasGroupMenang;
    public CanvasGroup canvasGroupKalah;

    [Header("Animasi Hasil (Audit Sukses - MENANG)")]
    public RectTransform[] panahProfit;  // Masukkan gambar panah hijau naik / tanda $ yang melayang
    public RectTransform papanGrafikNaik; // Papan yang menampilkan kurva profit

    [Header("Animasi Hasil (Audit Gagal - KALAH)")]
    public RectTransform[] grafikBankrut; // Masukkan gambar garis merah yang merosot / tanda $ merah
    public CanvasGroup[] keringatKalah;   // Efek keringat dingin/air mata pada karakter
    public CanvasGroup warningIconKalah;  // Ikon panah merah ke bawah

    [HideInInspector] public bool isGameActive = false;

    private float timeRemaining;
    private int targetResult;
    private string currentOp;

    // Variabel penyimpan posisi awal animasi hasil
    private Vector2[] originalProfitPos;
    private Vector2[] originalBankrutPos;

    private void Awake()
    {
        // Simpan posisi awal elemen animasi MENANG (Panah Profit)
        if (panahProfit != null && panahProfit.Length > 0)
        {
            originalProfitPos = new Vector2[panahProfit.Length];
            for (int i = 0; i < panahProfit.Length; i++)
            {
                if (panahProfit[i] != null) originalProfitPos[i] = panahProfit[i].anchoredPosition;
            }
        }

        // Simpan posisi awal elemen animasi KALAH (Grafik Turun)
        if (grafikBankrut != null && grafikBankrut.Length > 0)
        {
            originalBankrutPos = new Vector2[grafikBankrut.Length];
            for (int i = 0; i < grafikBankrut.Length; i++)
            {
                if (grafikBankrut[i] != null) originalBankrutPos[i] = grafikBankrut[i].anchoredPosition;
            }
        }
    }

    public void StartMiniGame()
    {
        isGameActive = false; // Kunci game selama intro
        timeRemaining = timeLimit;

        statusText.text = "AUDIT KEUANGAN!";
        statusText.color = Color.red;
        timerText.text = "Waktu: 15s";

        // Pastikan panel hasil tersembunyi
        if (panelMenang != null) panelMenang.SetActive(false);
        if (panelKalah != null) panelKalah.SetActive(false);

        // Hentikan sisa animasi hasil sebelumnya (jika game dimainkan ulang)
        if (panahProfit != null) foreach (var panah in panahProfit) if (panah != null) DOTween.Kill(panah);
        if (grafikBankrut != null) foreach (var grafik in grafikBankrut) if (grafik != null) DOTween.Kill(grafik);
        if (keringatKalah != null) foreach (var keringat in keringatKalah) if (keringat != null) DOTween.Kill(keringat);
        if (warningIconKalah != null) DOTween.Kill(warningIconKalah);

        // Pastikan semua slot kosong dan blok di bawah
        foreach (MathBlock block in answerBlocks) block.SnapBack();

        GenerateMathProblem();
        StartCoroutine(PlayIntroAnimation());
    }

    private IEnumerator PlayIntroAnimation()
    {
        // 1. Sembunyikan semua elemen (skala 0)
        if (statusText != null) statusText.transform.localScale = Vector3.zero;
        if (timerText != null) timerText.transform.localScale = Vector3.zero;

        if (equationElements != null)
        {
            foreach (RectTransform eq in equationElements)
                if (eq != null) eq.localScale = Vector3.zero;
        }

        foreach (MathBlock block in answerBlocks)
            if (block != null) block.transform.localScale = Vector3.zero;

        // 2. Animasikan Teks Instruksi masuk
        if (statusText != null) statusText.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
        if (timerText != null) timerText.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetDelay(0.1f);

        yield return new WaitForSeconds(0.2f);

        // 3. Animasikan komponen persamaan (slot & tanda) dari kiri ke kanan
        if (equationElements != null)
        {
            for (int i = 0; i < equationElements.Length; i++)
            {
                if (equationElements[i] != null)
                {
                    equationElements[i].DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        yield return new WaitForSeconds(0.2f);

        // 4. Animasikan keempat blok pilihan jawaban bermunculan
        for (int i = 0; i < answerBlocks.Length; i++)
        {
            if (answerBlocks[i] != null)
            {
                answerBlocks[i].transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
                yield return new WaitForSeconds(0.15f);
            }
        }

        yield return new WaitForSeconds(0.3f);
        isGameActive = true;
    }

    void Update()
    {
        if (!isGameActive) return;

        timeRemaining -= Time.deltaTime;
        timerText.text = "Waktu: " + Mathf.CeilToInt(timeRemaining).ToString() + "s";

        if (timeRemaining <= 0)
        {
            timerText.text = "Waktu Habis!";
            statusText.text = "AUDIT GAGAL!";
            statusText.color = Color.red;
            EndGame(false);
        }
    }

    private void GenerateMathProblem()
    {
        int num1 = 0, num2 = 0;
        int opType = Random.Range(0, 4);

        if (opType == 0) { num1 = Random.Range(1, 26); num2 = Random.Range(1, 26); targetResult = num1 + num2; currentOp = "+"; }
        else if (opType == 1) { num1 = Random.Range(1, 26); num2 = Random.Range(1, 26); if (num1 < num2) { int temp = num1; num1 = num2; num2 = temp; } targetResult = num1 - num2; currentOp = "-"; }
        else if (opType == 2) { num1 = Random.Range(1, 11); num2 = Random.Range(1, 11); targetResult = num1 * num2; currentOp = "x"; }
        else if (opType == 3) { targetResult = Random.Range(1, 11); num2 = Random.Range(1, 11); num1 = targetResult * num2; currentOp = "/"; }

        operatorText.text = currentOp;
        resultText.text = "= " + targetResult.ToString();

        List<int> choices = new List<int> { num1, num2 };
        while (choices.Count < 4) { int randomNum = Random.Range(1, 26); if (!choices.Contains(randomNum)) choices.Add(randomNum); }
        for (int i = 0; i < choices.Count; i++) { int temp = choices[i]; int randomIndex = Random.Range(i, choices.Count); choices[i] = choices[randomIndex]; choices[randomIndex] = temp; }

        for (int i = 0; i < 4; i++) { answerBlocks[i].manager = this; answerBlocks[i].SetupBlock(choices[i]); }
    }

    public void CheckWinCondition()
    {
        if (slot1.currenBlock != null && slot2.currenBlock != null)
        {
            int val1 = slot1.currenBlock.blockValue;
            int val2 = slot2.currenBlock.blockValue;
            bool isCorrect = false;

            if (currentOp == "+") isCorrect = (val1 + val2 == targetResult);
            else if (currentOp == "-") isCorrect = (val1 - val2 == targetResult || val2 - val1 == targetResult);
            else if (currentOp == "x") isCorrect = (val1 * val2 == targetResult);
            else if (currentOp == "/") isCorrect = (val1 / (float)val2 == targetResult || val2 / (float)val1 == targetResult);

            if (isCorrect)
            {
                statusText.text = "AUDIT SESUAI!";
                statusText.color = Color.green;
                statusText.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0f), 0.3f, 1);
                EndGame(true);
            }
            else
            {
                statusText.text = "SALAH HITUNG!";
                statusText.color = Color.red;
                statusText.transform.DOShakePosition(0.5f, 10f, 20, 90f, false, true); // Efek error

                slot1.currenBlock.SnapBackError();
                slot2.currenBlock.SnapBackError();
            }
        }
    }

    private void EndGame(bool isWin)
    {
        isGameActive = false;

        // Sembunyikan elemen UI Gameplay
        if (statusText != null) statusText.transform.DOScale(Vector3.zero, 0.3f);
        if (timerText != null) timerText.transform.DOScale(Vector3.zero, 0.3f);
        if (equationElements != null) foreach (var eq in equationElements) if (eq != null) eq.DOScale(Vector3.zero, 0.3f);
        foreach (MathBlock block in answerBlocks) if (block != null) block.transform.DOScale(Vector3.zero, 0.3f);

        DOVirtual.DelayedCall(0.4f, () =>
        {
            miniGameManager.CompleteMiniGame(isWin);
            ShowResultPanelAnimated(isWin);
        });
    }

    private void ShowResultPanelAnimated(bool isWin)
    {
        if (isWin)
        {
            if (panelMenang != null)
            {
                panelMenang.SetActive(true);
                if (rectMenang != null) { rectMenang.anchoredPosition = new Vector2(0, -1000f); rectMenang.DOAnchorPosY(0, 0.8f).SetEase(Ease.OutBack); }
                if (canvasGroupMenang != null) { canvasGroupMenang.alpha = 0f; canvasGroupMenang.DOFade(1f, 0.8f); }

                // ==========================================
                // ANIMASI KEUNTUNGAN (Grafik Hijau Naik)
                // ==========================================

                // 1. Papan Grafik Membal Masuk
                if (papanGrafikNaik != null)
                {
                    papanGrafikNaik.localScale = Vector3.zero;
                    papanGrafikNaik.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetDelay(0.8f);
                }

                // 2. Panah / Tanda Dollar Melayang Naik
                if (panahProfit != null && panahProfit.Length > 0)
                {
                    for (int i = 0; i < panahProfit.Length; i++)
                    {
                        if (panahProfit[i] != null)
                        {
                            panahProfit[i].anchoredPosition = originalProfitPos[i];

                            // Animasi melayang sedikit ke atas lalu berulang (Yoyo)
                            float floatSpeed = Random.Range(1f, 2f);
                            panahProfit[i].DOAnchorPosY(originalProfitPos[i].y + 15f, floatSpeed)
                                .SetEase(Ease.InOutSine)
                                .SetLoops(-1, LoopType.Yoyo)
                                .SetDelay(1f); // Mulai setelah papan muncul
                        }
                    }
                }
            }
        }
        else
        {
            if (panelKalah != null)
            {
                panelKalah.SetActive(true);
                if (rectKalah != null) { rectKalah.anchoredPosition = new Vector2(0, -1000f); rectKalah.DOAnchorPosY(0, 0.8f).SetEase(Ease.OutBack); }
                if (canvasGroupKalah != null) { canvasGroupKalah.alpha = 0f; canvasGroupKalah.DOFade(1f, 0.8f); }

                // Ikon peringatan turun berkedip
                if (warningIconKalah != null)
                {
                    warningIconKalah.alpha = 1f;
                    warningIconKalah.DOFade(0.2f, 0.3f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetDelay(0.8f);
                }

                // ==========================================
                // ANIMASI KEBANGKRUTAN (Grafik Merah Turun)
                // ==========================================

                // 1. Garis Grafik Merah Melorot
                if (grafikBankrut != null && grafikBankrut.Length > 0)
                {
                    for (int i = 0; i < grafikBankrut.Length; i++)
                    {
                        if (grafikBankrut[i] != null)
                        {
                            grafikBankrut[i].anchoredPosition = originalBankrutPos[i];

                            // Animasi meluncur turun (seolah grafik anjlok drastis)
                            grafikBankrut[i].DOAnchorPosY(originalBankrutPos[i].y - 30f, 2f)
                                .SetEase(Ease.OutBounce)
                                .SetLoops(-1, LoopType.Yoyo)
                                .SetDelay(0.8f);
                        }
                    }
                }

                // 2. Keringat Karakter Berkedip (Efek Panik)
                if (keringatKalah != null && keringatKalah.Length > 0)
                {
                    for (int i = 0; i < keringatKalah.Length; i++)
                    {
                        if (keringatKalah[i] != null)
                        {
                            keringatKalah[i].alpha = 0f;

                            // Animasi keringat muncul lalu memudar cepat
                            keringatKalah[i].DOFade(1f, 0.3f)
                                .SetEase(Ease.InOutSine)
                                .SetLoops(-1, LoopType.Yoyo)
                                .SetDelay(0.8f);
                        }
                    }
                }
            }
        }
    }
}