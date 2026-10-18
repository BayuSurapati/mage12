using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening; // WAJIB: Pastikan Anda telah mengimpor DOTween ke proyek Anda

public class MiniGame_MitigasiBencana : MonoBehaviour
{
    [Header("Referensi Sistem Manager")]
    public MiniGameManager miniGameManager;

    [Header("Pengaturan Game")]
    public int sequenceLength = 4;
    public float timeLimit = 5.0f;
    public float flashSpeed = .3f;

    [Header("Referensi UI Gameplay")]
    public TextMeshProUGUI statusText; // Teks instruksi: "Perhatikan..." / "Giliran Anda!"
    public TextMeshProUGUI timerText;  // Teks "Waktu: --"
    [Tooltip("Masukkan 4 tombol secara berurutan, dari kiri ke kanan")]
    public Button[] colorButtons;      // Keempat tombol zona (Merah, Kuning, Biru, Hijau)

    [Header("UI Hasil (PANEL MENANG)")]
    public GameObject panelMenang;
    public CanvasGroup canvasGroupMenang;
    public RectTransform rectMenang;

    [Header("Animasi Menang (PANEL MENANG)")]
    [Tooltip("Masukkan gambar awan-awan ke dalam array ini")]
    public RectTransform[] awanMenang;   // Array untuk objek UI awan
    [Tooltip("Masukkan sekumpulan burung ke dalam array ini")]
    public RectTransform[] burungMenang; // Array untuk objek UI kawanan burung

    [Header("UI Hasil (PANEL KALAH)")]
    public GameObject panelKalah;
    public CanvasGroup canvasGroupKalah;
    public RectTransform rectKalah;
    public CanvasGroup warningIconKalah; // Referensi untuk ikon sirine berkedip di panel kalah

    [Header("Animasi Gunung Berapi (PANEL KALAH)")]
    public RectTransform asapGunung; // Referensi untuk gambar asap
    public CanvasGroup[] apiPokok;   // Referensi untuk gambar api di pepohonan

    // Variabel internal permainan
    private List<int> targetSequence = new List<int>();
    private int playerInputIndex = 0;
    private float timeRemaining;
    private bool isPlayerTurn = false;
    private bool isGameActive = false;

    // Array untuk menyimpan warna asli dari masing-masing tombol
    private Color[] originalColor;

    // Variabel untuk menyimpan posisi/skala awal animasi Kalah
    private Vector2 originalAsapPos;
    private Vector3 originalAsapScale;

    // Variabel untuk menyimpan posisi awal animasi Menang
    private Vector2[] originalAwanPos;
    private Vector2[] originalBurungPos;

    void Awake()
    {
        // 1. Simpan warna awal setiap tombol saat game pertama kali dijalankan
        originalColor = new Color[colorButtons.Length];
        for (int i = 0; i < colorButtons.Length; i++)
        {
            if (colorButtons[i] != null)
            {
                originalColor[i] = colorButtons[i].GetComponent<Image>().color;
            }
        }

        // 2. Simpan posisi dan skala awal elemen animasi KALAH (Asap)
        if (asapGunung != null)
        {
            originalAsapPos = asapGunung.anchoredPosition;
            originalAsapScale = asapGunung.localScale;
        }

        // 3. Simpan posisi awal elemen animasi MENANG (Awan & Burung)
        if (awanMenang != null && awanMenang.Length > 0)
        {
            originalAwanPos = new Vector2[awanMenang.Length];
            for (int i = 0; i < awanMenang.Length; i++)
            {
                if (awanMenang[i] != null) originalAwanPos[i] = awanMenang[i].anchoredPosition;
            }
        }

        if (burungMenang != null && burungMenang.Length > 0)
        {
            originalBurungPos = new Vector2[burungMenang.Length];
            for (int i = 0; i < burungMenang.Length; i++)
            {
                if (burungMenang[i] != null) originalBurungPos[i] = burungMenang[i].anchoredPosition;
            }
        }
    }

    // --- FUNGSI 1: MEMULAI MINI GAME (Dipanggil oleh MiniGameManager) ---
    public void StartMiniGame()
    {
        isGameActive = false; // Kunci input/timer selama animasi intro berjalan
        isPlayerTurn = false;
        playerInputIndex = 0;
        timeRemaining = timeLimit;

        // Reset teks di awal
        timerText.text = "Waktu: --";
        statusText.text = "Perhatikan urutan zona bantuan!";
        statusText.color = Color.black; // Warna instruksi diubah menjadi hitam agar terbaca jelas

        // Kunci tombol agar tidak bisa diklik secara tidak sengaja
        SetButtonsInteractable(false);

        // Pastikan panel hasil disembunyikan
        if (panelMenang != null) panelMenang.SetActive(false);
        if (panelKalah != null) panelKalah.SetActive(false);

        // Hentikan sisa animasi sebelumnya (jika game dimainkan ulang)
        if (warningIconKalah != null) DOTween.Kill(warningIconKalah);
        if (asapGunung != null) DOTween.Kill(asapGunung);

        if (apiPokok != null)
        {
            foreach (var api in apiPokok) { if (api != null) DOTween.Kill(api); }
        }

        if (awanMenang != null)
        {
            foreach (var awan in awanMenang) { if (awan != null) DOTween.Kill(awan); }
        }

        if (burungMenang != null)
        {
            foreach (var burung in burungMenang) { if (burung != null) DOTween.Kill(burung); }
        }

        // Mulai animasi perkenalan
        StartCoroutine(PlayIntroAnimation());
    }

    void Update()
    {
        // Timer hanya akan berjalan jika sudah giliran pemain untuk menekan tombol
        if (isGameActive && isPlayerTurn)
        {
            timeRemaining -= Time.deltaTime;
            timerText.text = "Waktu: " + Mathf.CeilToInt(timeRemaining).ToString() + "s";

            // Jika kehabisan waktu
            if (timeRemaining <= 0)
            {
                timerText.text = "Waktu Habis!";
                statusText.text = "TERLAMBAT!";
                statusText.color = Color.red;
                EndMiniGame(false);
            }
        }
    }

    // --- FUNGSI 2: ANIMASI INTRO (UI MEMBAL MASUK) ---
    private IEnumerator PlayIntroAnimation()
    {
        // 1. Sembunyikan elemen terlebih dahulu (ubah skala jadi 0)
        if (statusText != null) statusText.transform.localScale = Vector3.zero;
        if (timerText != null) timerText.transform.localScale = Vector3.zero;

        foreach (Button btn in colorButtons)
        {
            if (btn != null) btn.transform.localScale = Vector3.zero;
        }

        // 2. Animasikan Teks Instruksi & Timer membal ke ukuran semula (skala 1)
        if (statusText != null)
            statusText.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);

        if (timerText != null)
            timerText.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetDelay(0.1f);

        yield return new WaitForSeconds(0.2f); // Jeda singkat

        // 3. Animasikan Keempat Tombol Zona secara berurutan (Staggering)
        for (int i = 0; i < colorButtons.Length; i++)
        {
            if (colorButtons[i] != null)
            {
                colorButtons[i].transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
                yield return new WaitForSeconds(0.15f); // Jeda antar kemunculan tombol
            }
        }

        yield return new WaitForSeconds(0.5f); // Jeda sebelum lampu mulai berkedip

        // 4. Buka kunci permainan dan jalankan logika memori
        isGameActive = true;
        GenerateSequence();
        StartCoroutine(PlaySequence());
    }

    // --- FUNGSI 3: MENGHASILKAN URUTAN ACAK ---
    public void GenerateSequence()
    {
        targetSequence.Clear();
        for (int i = 0; i < sequenceLength; i++)
        {
            // Ambil nomor urut tombol secara acak
            int randomColorIndex = Random.Range(0, colorButtons.Length);
            targetSequence.Add(randomColorIndex);
        }
    }

    // --- FUNGSI 4: MEMAINKAN URUTAN (LAMPU BERKEDIP) ---
    private IEnumerator PlaySequence()
    {
        yield return new WaitForSeconds(0.5f);

        // Menyalakan tombol satu per satu sesuai urutan yang dibuat
        foreach (int colorIndex in targetSequence)
        {
            yield return StartCoroutine(FlashButton(colorIndex));
            yield return new WaitForSeconds(0.2f); // Jeda istirahat antar kedipan
        }

        // Masuk ke Fase Input Pemain
        isPlayerTurn = true;
        statusText.text = "Giliran Anda! Tekan zona sesuai urutan!";
        statusText.color = Color.black; // Warna gelap agar mudah dibaca

        // Efek animasi "Punch" (membal) pada teks untuk menarik perhatian
        statusText.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0f), 0.3f, 1);

        // Buka kunci tombol agar bisa diklik pemain
        SetButtonsInteractable(true);
    }

    // Fungsi tambahan untuk mengubah warna tombol sekilas jadi GELAP/HITAM lalu kembali semula
    private IEnumerator FlashButton(int index)
    {
        if (colorButtons[index] != null)
        {
            Image btnImage = colorButtons[index].GetComponent<Image>();

            // Berkedip warna hitam agar kontras (diperbarui dari permintaan sebelumnya)
            btnImage.color = Color.black;

            yield return new WaitForSeconds(flashSpeed);

            // Kembali ke warna aslinya (merah/kuning/biru/hijau)
            btnImage.color = originalColor[index];
        }
    }

    // --- FUNGSI 5: MENANGANI INPUT PEMAIN (Dikaitkan ke Event OnClick Tombol) ---
    public void OnButtonPressed(int buttonIndex)
    {
        // Tolak input jika belum waktunya
        if (!isPlayerTurn || !isGameActive) return;

        // Jika pemain menebak ZONA YANG BENAR
        if (buttonIndex == targetSequence[playerInputIndex])
        {
            playerInputIndex++;

            // Animasi membal ke dalam saat tombol yang benar ditekan
            colorButtons[buttonIndex].transform.DOPunchScale(new Vector3(-0.1f, -0.1f, 0f), 0.2f, 1);

            // Nyalakan lampunya sekilas sebagai respons
            StartCoroutine(FlashButton(buttonIndex));

            // Jika pemain berhasil menyelesaikan seluruh urutan
            if (playerInputIndex >= sequenceLength)
            {
                statusText.text = "MITIGASI BERHASIL!";
                statusText.color = Color.green;
                statusText.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0f), 0.3f, 1);

                EndMiniGame(true); // Pemain Menang
            }
        }
        // Jika pemain menebak ZONA YANG SALAH
        else
        {
            statusText.text = "ZONA SALAH!";
            statusText.color = Color.red;

            // Efek getar layar/error pada teks
            statusText.transform.DOShakePosition(0.5f, 10f, 20, 90f, false, true);

            // Tombol yang salah akan berkedip merah sekilas
            Image wrongBtnImage = colorButtons[buttonIndex].GetComponent<Image>();
            if (wrongBtnImage != null)
            {
                wrongBtnImage.DOColor(Color.red, 0.2f).SetLoops(2, LoopType.Yoyo).OnComplete(() => wrongBtnImage.color = originalColor[buttonIndex]);
            }

            EndMiniGame(false); // Pemain Kalah
        }
    }

    // Fungsi praktis untuk mengunci/membuka semua tombol
    private void SetButtonsInteractable(bool state)
    {
        foreach (Button btn in colorButtons)
        {
            if (btn != null) btn.interactable = state;
        }
    }

    // --- FUNGSI 6: MENGAKHIRI GAME DAN BERSIH-BERSIH ---
    private void EndMiniGame(bool isWin)
    {
        isGameActive = false;
        isPlayerTurn = false;
        SetButtonsInteractable(false);

        // Hapus elemen UI dari layar dengan mengecilkan ukurannya (skala ke 0)
        if (statusText != null) statusText.transform.DOScale(Vector3.zero, 0.3f);
        if (timerText != null) timerText.transform.DOScale(Vector3.zero, 0.3f);

        foreach (Button btn in colorButtons)
        {
            if (btn != null) btn.transform.DOScale(Vector3.zero, 0.3f);
        }

        // Lapor ke manager utama setelah animasi menghilang selesai (0.3 detik)
        DOVirtual.DelayedCall(0.3f, () =>
        {
            // Catat stat kemenangan/kekalahan
            miniGameManager.CompleteMiniGame(isWin);

            // Munculkan UI Panel Hasil
            ShowResultPanelAnimated(isWin);
        });
    }

    // --- FUNGSI 7: MENAMPILKAN PANEL HASIL (MENANG/KALAH) ---
    private void ShowResultPanelAnimated(bool isWin)
    {
        if (isWin)
        {
            if (panelMenang != null)
            {
                panelMenang.SetActive(true);

                // Panel meluncur naik dari bawah
                if (rectMenang != null)
                {
                    rectMenang.anchoredPosition = new Vector2(0, -1000f);
                    rectMenang.DOAnchorPosY(0, 0.8f).SetEase(Ease.OutBack);
                }

                // Panel pudar masuk (fade in)
                if (canvasGroupMenang != null)
                {
                    canvasGroupMenang.alpha = 0f;
                    canvasGroupMenang.DOFade(1f, 0.8f);
                }

                // ==========================================
                // ANIMASI LINGKUNGAN ASRI (AWAN & BURUNG)
                // ==========================================

                // 1. Animasi Awan Mengapung
                if (awanMenang != null && awanMenang.Length > 0)
                {
                    for (int i = 0; i < awanMenang.Length; i++)
                    {
                        if (awanMenang[i] != null)
                        {
                            // Reset posisi ke tempat semula
                            awanMenang[i].anchoredPosition = originalAwanPos[i];

                            // Awan bergerak pelan ke kanan dan kiri (Yoyo Loop)
                            float randomSpeed = Random.Range(3f, 6f); // Variasi kecepatan
                            float randomDistance = Random.Range(20f, 40f); // Variasi jarak tempuh

                            awanMenang[i].DOAnchorPosX(originalAwanPos[i].x + randomDistance, randomSpeed)
                                .SetEase(Ease.InOutSine)
                                .SetLoops(-1, LoopType.Yoyo);
                        }
                    }
                }

                // 2. Animasi Kawanan Burung Melintas
                if (burungMenang != null && burungMenang.Length > 0)
                {
                    for (int i = 0; i < burungMenang.Length; i++)
                    {
                        if (burungMenang[i] != null)
                        {
                            // Reset posisi (misalnya ditaruh agak di luar layar kiri pada Unity Editor)
                            burungMenang[i].anchoredPosition = originalBurungPos[i];

                            // Burung terbang melintasi layar dari kiri ke kanan (atau bawah ke atas melengkung)
                            float terbangSpeed = Random.Range(4f, 7f);
                            float terbangJarak = 300f; // Sesuaikan dengan ukuran panel Anda

                            // Animasi bergerak horizontal ke kanan, lalu kembali ke posisi awal dan diulang
                            burungMenang[i].DOAnchorPosX(originalBurungPos[i].x + terbangJarak, terbangSpeed)
                                .SetEase(Ease.Linear)
                                .SetLoops(-1, LoopType.Restart);

                            // Tambahkan sedikit efek memantul/mengambang secara vertikal saat burung terbang
                            burungMenang[i].DOAnchorPosY(originalBurungPos[i].y + 15f, 1f)
                                .SetEase(Ease.InOutSine)
                                .SetLoops(-1, LoopType.Yoyo);
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

                // Panel meluncur naik dari bawah
                if (rectKalah != null)
                {
                    rectKalah.anchoredPosition = new Vector2(0, -1000f);
                    rectKalah.DOAnchorPosY(0, 0.8f).SetEase(Ease.OutBack);
                }

                // Panel pudar masuk (fade in)
                if (canvasGroupKalah != null)
                {
                    canvasGroupKalah.alpha = 0f;
                    canvasGroupKalah.DOFade(1f, 0.8f);
                }

                // Animasi Ikon Sirine/Bencana
                if (warningIconKalah != null)
                {
                    warningIconKalah.alpha = 1f;
                    warningIconKalah.DOFade(0.2f, 0.3f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetDelay(0.8f);
                }

                // ==========================================
                // ANIMASI GUNUNG BERAPI (ASAP & API)
                // ==========================================

                // 1. Animasi Asap Mengepul ke Atas
                if (asapGunung != null)
                {
                    // Reset posisi dan skala ke kondisi awal
                    asapGunung.anchoredPosition = originalAsapPos;
                    asapGunung.localScale = originalAsapScale;

                    // Buat asap melayang ke atas sambil membesar sedikit, kemudian diulang dari awal
                    asapGunung.DOAnchorPosY(originalAsapPos.y + 40f, 3f).SetEase(Ease.OutSine).SetLoops(-1, LoopType.Restart);
                    asapGunung.DOScale(originalAsapScale * 1.2f, 3f).SetEase(Ease.OutSine).SetLoops(-1, LoopType.Restart);

                    // Buat asap memudar perlahan saat naik
                    CanvasGroup asapCanvas = asapGunung.GetComponent<CanvasGroup>();
                    if (asapCanvas != null)
                    {
                        asapCanvas.alpha = 1f;
                        asapCanvas.DOFade(0f, 3f).SetEase(Ease.InExpo).SetLoops(-1, LoopType.Restart);
                    }
                }

                // 2. Animasi Api Berkobar (Flickering) pada Pepohonan
                if (apiPokok != null && apiPokok.Length > 0)
                {
                    for (int i = 0; i < apiPokok.Length; i++)
                    {
                        if (apiPokok[i] != null)
                        {
                            // Reset alpha dan skala sebelum animasi dimulai
                            apiPokok[i].alpha = 1f;
                            apiPokok[i].transform.localScale = Vector3.one;

                            // Kelip-kelip (alpha naik turun) dengan kadar kecepatan acak
                            float kecepatanKelip = Random.Range(0.2f, 0.5f);
                            apiPokok[i].DOFade(0.4f, kecepatanKelip).SetEase(Ease.InOutBounce).SetLoops(-1, LoopType.Yoyo);

                            // Sedikit efek membesar/mengecil agar api terlihat hidup bergerak
                            apiPokok[i].transform.DOScale(1.15f, kecepatanKelip * 1.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
                        }
                    }
                }
            }
        }
    }
}