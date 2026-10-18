using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // WAJIB: Pastikan Anda telah mengimpor DOTween

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

    [Header("Referensi UI Gameplay")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI statusText;

    public RectTransform targetArea; // Area Hijau
    public RectTransform pointer;    // Jarum Merah
    public RectTransform timingBar;  // Bar Kuning Keseluruhan

    [Header("Referensi Animasi (Orang Demo)")]
    [Tooltip("Masukkan gambar-gambar orang/tangan mengepal ke dalam array ini")]
    public RectTransform[] kerumunanDemo;

    [Header("UI Hasil (PANEL MENANG)")]
    public GameObject panelMenang;
    public CanvasGroup canvasGroupMenang;
    public RectTransform rectMenang;

    [Header("Animasi Hasil (Lingkungan Aman - MENANG)")]
    [Tooltip("Masukkan gambar asap pabrik putih")]
    public RectTransform[] asapPabrik;

    [Header("UI Hasil (PANEL KALAH)")]
    public GameObject panelKalah;
    public CanvasGroup canvasGroupKalah;
    public RectTransform rectKalah;
    public CanvasGroup warningIconKalah; // Ikon Peringatan/Sirine

    [Header("Animasi Hasil (Kerusuhan - KALAH)")]
    [Tooltip("Masukkan gambar asap tebal abu-abu di atas gedung")]
    public RectTransform[] asapKerusuhan;
    [Tooltip("Masukkan gambar kobaran api di gedung")]
    public CanvasGroup[] apiKerusuhan;

    // Variabel internal
    private bool isGameActive = false;
    private float timeRemaining;
    private int successfulActions;

    private float currentSpeed;
    private int moveDirection = 1; // 1 untuk kanan, -1 untuk kiri
    private float stunTimer = 0f;

    private Vector2 originalTargetSize;
    private Vector2[] originalKerumunanPos;

    // Penyimpan posisi & skala animasi hasil
    private Vector2[] originalAsapPabrikPos;
    private Vector3[] originalAsapPabrikScale;
    private Vector2[] originalAsapRusuhPos;
    private Vector3[] originalAsapRusuhScale;

    private void Awake()
    {
        if (targetArea != null)
        {
            // Simpan ukuran asli area target agar bisa dikembalikan saat game di-reset
            originalTargetSize = targetArea.sizeDelta;
        }

        // Simpan posisi awal setiap gambar orang/kerumunan demo
        if (kerumunanDemo != null && kerumunanDemo.Length > 0)
        {
            originalKerumunanPos = new Vector2[kerumunanDemo.Length];
            for (int i = 0; i < kerumunanDemo.Length; i++)
            {
                if (kerumunanDemo[i] != null) originalKerumunanPos[i] = kerumunanDemo[i].anchoredPosition;
            }
        }

        // Simpan posisi dan skala awal untuk animasi panel hasil
        if (asapPabrik != null && asapPabrik.Length > 0)
        {
            originalAsapPabrikPos = new Vector2[asapPabrik.Length];
            originalAsapPabrikScale = new Vector3[asapPabrik.Length];
            for (int i = 0; i < asapPabrik.Length; i++)
            {
                if (asapPabrik[i] != null)
                {
                    originalAsapPabrikPos[i] = asapPabrik[i].anchoredPosition;
                    originalAsapPabrikScale[i] = asapPabrik[i].localScale;
                }
            }
        }

        if (asapKerusuhan != null && asapKerusuhan.Length > 0)
        {
            originalAsapRusuhPos = new Vector2[asapKerusuhan.Length];
            originalAsapRusuhScale = new Vector3[asapKerusuhan.Length];
            for (int i = 0; i < asapKerusuhan.Length; i++)
            {
                if (asapKerusuhan[i] != null)
                {
                    originalAsapRusuhPos[i] = asapKerusuhan[i].anchoredPosition;
                    originalAsapRusuhScale[i] = asapKerusuhan[i].localScale;
                }
            }
        }
    }

    public void StartMiniGame()
    {
        isGameActive = false; // Kunci game selama animasi intro berjalan
        timeRemaining = timeLimit;
        successfulActions = 0;
        currentSpeed = basePointerSpeed;
        stunTimer = 0f;

        // Kembalikan ukuran target ke ukuran asli saat game dimulai
        if (targetArea != null) targetArea.sizeDelta = originalTargetSize;

        statusText.text = "Negosiasi Dimulai!";
        statusText.color = Color.red;
        timerText.text = "Waktu: 15s";

        // Matikan panel hasil
        if (panelMenang != null) panelMenang.SetActive(false);
        if (panelKalah != null) panelKalah.SetActive(false);

        // Hentikan sisa animasi sebelumnya
        if (warningIconKalah != null) DOTween.Kill(warningIconKalah);
        if (pointer != null) DOTween.Kill(pointer);

        if (asapPabrik != null) foreach (var asap in asapPabrik) if (asap != null) DOTween.Kill(asap);
        if (asapKerusuhan != null) foreach (var asap in asapKerusuhan) if (asap != null) DOTween.Kill(asap);
        if (apiKerusuhan != null) foreach (var api in apiKerusuhan) if (api != null) DOTween.Kill(api);

        // Mulai animasi perkenalan dan orasi kerumunan
        StartCoroutine(PlayIntroAnimation());
        MulaiAnimasiDemo();
    }

    private IEnumerator PlayIntroAnimation()
    {
        // Sembunyikan elemen UI Gameplay (skala 0)
        if (statusText != null) statusText.transform.localScale = Vector3.zero;
        if (timerText != null) timerText.transform.localScale = Vector3.zero;
        if (timingBar != null) timingBar.localScale = Vector3.zero;

        // Animasikan elemen membal masuk
        if (statusText != null) statusText.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
        if (timerText != null) timerText.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetDelay(0.1f);

        yield return new WaitForSeconds(0.2f);

        if (timingBar != null) timingBar.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(0.5f);

        isGameActive = true;
    }

    private void MulaiAnimasiDemo()
    {
        if (kerumunanDemo == null || kerumunanDemo.Length == 0) return;

        for (int i = 0; i < kerumunanDemo.Length; i++)
        {
            if (kerumunanDemo[i] != null)
            {
                kerumunanDemo[i].anchoredPosition = originalKerumunanPos[i];

                // Animasi bergetar/melompat-lompat seolah sedang demo
                float jumpSpeed = UnityEngine.Random.Range(0.3f, 0.6f);
                float jumpHeight = UnityEngine.Random.Range(5f, 15f);

                kerumunanDemo[i].DOAnchorPosY(originalKerumunanPos[i].y + jumpHeight, jumpSpeed)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }
        }
    }

    void Update()
    {
        if (!isGameActive) return;

        // 1. Logika Timer Keseluruhan
        timeRemaining -= Time.deltaTime;
        timerText.text = "Waktu: " + Mathf.CeilToInt(timeRemaining).ToString() + "s";

        if (timeRemaining <= 0)
        {
            timerText.text = "Waktu Habis!";
            statusText.text = "Demo Ricuh!";
            statusText.color = Color.red;
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

        if (pointerX >= minX && pointerX <= maxX)
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

        // Animasi jarum membal saat tepat sasaran
        pointer.DOPunchScale(new Vector3(0.5f, 0.5f, 0f), 0.2f, 1);

        // Animasi target area berkedip putih sekejap
        Image targetImg = targetArea.GetComponent<Image>();
        if (targetImg != null)
        {
            Color origColor = targetImg.color;
            targetImg.DOColor(Color.white, 0.1f).OnComplete(() => targetImg.color = origColor);
        }

        if (successfulActions >= targetKesuksesan)
        {
            statusText.text = "Negosiasi Berhasil!";
            statusText.color = Color.green;
            statusText.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0f), 0.3f, 1);
            EndGame(true);
        }
        else
        {
            statusText.text = $"Berhasil! ({successfulActions}/{targetKesuksesan})";
            statusText.color = Color.blue;
            statusText.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0f), 0.2f, 1);

            // Tingkatkan kesulitan dengan memperkecil area target dan meningkatkan kecepatan pointer
            currentSpeed *= speedMultiplier;
            targetArea.DOSizeDelta(new Vector2(targetArea.sizeDelta.x * targetShrinkFactor, targetArea.sizeDelta.y), 0.3f).SetEase(Ease.OutBack);

            AcakPosisiTarget();
        }
    }

    public void Meleset()
    {
        statusText.text = "Meleset!";
        statusText.color = Color.red;

        // Efek teks bergetar (error)
        statusText.transform.DOShakePosition(0.5f, 10f, 20, 90f, false, true);

        // Animasi jarum memerah sekilas karena stun
        Image pointerImg = pointer.GetComponent<Image>();
        if (pointerImg != null)
        {
            Color origColor = pointerImg.color; // Simpan warna merah bawaannya (asumsi jarum sudah merah)
            pointerImg.DOColor(Color.black, 0.2f).SetLoops(4, LoopType.Yoyo).OnComplete(() => pointerImg.color = origColor);
        }

        // Kena stun sesuai durasi stun yang ditentukan
        stunTimer = stunDuration;
    }

    public void AcakPosisiTarget()
    {
        // Supaya letak area hijau tidak statis di tengah terus, kita acak posisinya
        float maxPos = (timingBar.rect.width / 2f) - (targetArea.rect.width / 2f);
        float randomX = UnityEngine.Random.Range(-maxPos, maxPos);

        // Gerakkan target area secara mulus ke posisi baru
        targetArea.DOAnchorPosX(randomX, 0.3f).SetEase(Ease.OutSine);
    }

    private void EndGame(bool isWin)
    {
        isGameActive = false;

        // Hentikan animasi demo kerumunan
        if (kerumunanDemo != null)
        {
            foreach (var orang in kerumunanDemo)
            {
                if (orang != null) DOTween.Kill(orang);
            }
        }

        // Sembunyikan UI Gameplay (Teks dan Bar)
        if (statusText != null) statusText.transform.DOScale(Vector3.zero, 0.3f);
        if (timerText != null) timerText.transform.DOScale(Vector3.zero, 0.3f);
        if (timingBar != null) timingBar.DOScale(Vector3.zero, 0.3f);

        // Lapor poin ke manager utama dan panggil panel hasil
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
                if (rectMenang != null)
                {
                    rectMenang.anchoredPosition = new Vector2(0, -1000f);
                    rectMenang.DOAnchorPosY(0, 0.8f).SetEase(Ease.OutBack);
                }
                if (canvasGroupMenang != null)
                {
                    canvasGroupMenang.alpha = 0f;
                    canvasGroupMenang.DOFade(1f, 0.8f);
                }

                // ==========================================
                // ANIMASI LINGKUNGAN STABIL (Asap Pabrik Putih)
                // ==========================================
                if (asapPabrik != null && asapPabrik.Length > 0)
                {
                    for (int i = 0; i < asapPabrik.Length; i++)
                    {
                        if (asapPabrik[i] != null)
                        {
                            asapPabrik[i].anchoredPosition = originalAsapPabrikPos[i];
                            asapPabrik[i].localScale = originalAsapPabrikScale[i];

                            // Asap mengepul ke atas secara perlahan lalu diulang
                            asapPabrik[i].DOAnchorPosY(originalAsapPabrikPos[i].y + 30f, 4f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
                            asapPabrik[i].DOScale(originalAsapPabrikScale[i] * 1.1f, 4f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);

                            CanvasGroup asapGroup = asapPabrik[i].GetComponent<CanvasGroup>();
                            if (asapGroup != null)
                            {
                                asapGroup.alpha = 1f;
                                asapGroup.DOFade(0f, 4f).SetEase(Ease.InSine).SetLoops(-1, LoopType.Restart);
                            }
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
                if (rectKalah != null)
                {
                    rectKalah.anchoredPosition = new Vector2(0, -1000f);
                    rectKalah.DOAnchorPosY(0, 0.8f).SetEase(Ease.OutBack);
                }
                if (canvasGroupKalah != null)
                {
                    canvasGroupKalah.alpha = 0f;
                    canvasGroupKalah.DOFade(1f, 0.8f);
                }

                if (warningIconKalah != null)
                {
                    warningIconKalah.alpha = 1f;
                    warningIconKalah.DOFade(0.2f, 0.3f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetDelay(0.8f);
                }

                // ==========================================
                // ANIMASI KERUSUHAN (Api & Asap Tebal Abu-Abu)
                // ==========================================

                // 1. Asap Kerusuhan Mengepul
                if (asapKerusuhan != null && asapKerusuhan.Length > 0)
                {
                    for (int i = 0; i < asapKerusuhan.Length; i++)
                    {
                        if (asapKerusuhan[i] != null)
                        {
                            asapKerusuhan[i].anchoredPosition = originalAsapRusuhPos[i];
                            asapKerusuhan[i].localScale = originalAsapRusuhScale[i];

                            asapKerusuhan[i].DOAnchorPosY(originalAsapRusuhPos[i].y + 40f, 3f).SetEase(Ease.OutSine).SetLoops(-1, LoopType.Restart);
                            asapKerusuhan[i].DOScale(originalAsapRusuhScale[i] * 1.3f, 3f).SetEase(Ease.OutSine).SetLoops(-1, LoopType.Restart);

                            CanvasGroup asapKalahGroup = asapKerusuhan[i].GetComponent<CanvasGroup>();
                            if (asapKalahGroup != null)
                            {
                                asapKalahGroup.alpha = 1f;
                                asapKalahGroup.DOFade(0f, 3f).SetEase(Ease.InExpo).SetLoops(-1, LoopType.Restart);
                            }
                        }
                    }
                }

                // 2. Api Berkedip (Flickering)
                if (apiKerusuhan != null && apiKerusuhan.Length > 0)
                {
                    for (int i = 0; i < apiKerusuhan.Length; i++)
                    {
                        if (apiKerusuhan[i] != null)
                        {
                            apiKerusuhan[i].alpha = 1f;
                            apiKerusuhan[i].transform.localScale = Vector3.one;

                            float kecepatanKelip = UnityEngine.Random.Range(0.2f, 0.5f);
                            apiKerusuhan[i].DOFade(0.4f, kecepatanKelip).SetEase(Ease.InOutBounce).SetLoops(-1, LoopType.Yoyo);
                            apiKerusuhan[i].transform.DOScale(1.1f, kecepatanKelip * 1.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
                        }
                    }
                }
            }
        }
    }
}