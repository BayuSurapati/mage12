using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening; // WAJIB: Pastikan Anda telah mengimpor DOTween

public class MiniGame_SortirSampah : MonoBehaviour
{
    [Header("Referensi Sistem Inti")]
    public MiniGameManager miniGameManager;

    [Header("Pengaturan Game")]
    public float timeLimit = 8f;
    private int targetSampah = 12;

    [Header("Referensi UI Gameplay")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI timerText;
    [Tooltip("Masukkan 3 tong sampah di sini (Hijau, Kuning, Merah)")]
    public RectTransform[] daftarTongSampah;
    [Tooltip("Masukkan semua item sampah yang akan disortir")]
    public List<TrashItems> daftarSampah;

    [Header("UI Hasil (PANEL MENANG)")]
    public GameObject panelMenang;
    public CanvasGroup canvasGroupMenang;
    public RectTransform rectMenang;

    [Header("Animasi Menang (Lingkungan Bersih)")]
    public RectTransform[] awanMenang;   // Gambar awan
    public RectTransform[] burungMenang; // Gambar burung

    [Header("UI Hasil (PANEL KALAH)")]
    public GameObject panelKalah;
    public CanvasGroup canvasGroupKalah;
    public RectTransform rectKalah;

    [Header("Animasi Kalah (Lingkungan Kotor)")]
    public RectTransform alatExcavator;  // Gambar ekskavator
    public RectTransform[] sampahBerserakan; // Gambar tumpukan sampah di panel kalah

    [HideInInspector] public bool isGameActive = false;

    private float timeRemaining;
    private int sampahTersortir = 0;

    // Variabel penyimpan posisi awal animasi hasil
    private Vector2[] originalAwanPos;
    private Vector2[] originalBurungPos;
    private Vector2 originalExcavatorPos;
    private Vector3 originalExcavatorRot;
    private Vector2[] originalSampahKalahPos;

    void Awake()
    {
        // Simpan posisi awal elemen animasi MENANG (Awan & Burung)
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

        // Simpan posisi awal elemen animasi KALAH (Excavator & Tumpukan Sampah)
        if (alatExcavator != null)
        {
            originalExcavatorPos = alatExcavator.anchoredPosition;
            originalExcavatorRot = alatExcavator.eulerAngles;
        }

        if (sampahBerserakan != null && sampahBerserakan.Length > 0)
        {
            originalSampahKalahPos = new Vector2[sampahBerserakan.Length];
            for (int i = 0; i < sampahBerserakan.Length; i++)
            {
                if (sampahBerserakan[i] != null) originalSampahKalahPos[i] = sampahBerserakan[i].anchoredPosition;
            }
        }
    }

    public void StartMiniGame()
    {
        isGameActive = false;
        timeRemaining = timeLimit;
        sampahTersortir = 0;

        statusText.text = "Sortir Sampah!";
        statusText.color = Color.black;
        timerText.text = "Waktu: 8s";

        // Pastikan panel hasil disembunyikan di awal
        if (panelMenang != null) panelMenang.SetActive(false);
        if (panelKalah != null) panelKalah.SetActive(false);

        // Matikan sisa animasi sebelumnya jika game diulang
        if (alatExcavator != null) DOTween.Kill(alatExcavator);
        if (awanMenang != null) foreach (var awan in awanMenang) if (awan != null) DOTween.Kill(awan);
        if (burungMenang != null) foreach (var burung in burungMenang) if (burung != null) DOTween.Kill(burung);
        if (sampahBerserakan != null) foreach (var s in sampahBerserakan) if (s != null) DOTween.Kill(s);

        // Mulai animasi perkenalan UI (Tong & Sampah membal masuk)
        StartCoroutine(PlayIntroAnimation());
    }

    private IEnumerator PlayIntroAnimation()
    {
        // Sembunyikan elemen
        if (statusText != null) statusText.transform.localScale = Vector3.zero;
        if (timerText != null) timerText.transform.localScale = Vector3.zero;

        foreach (RectTransform tong in daftarTongSampah)
        {
            if (tong != null) tong.localScale = Vector3.zero;
        }

        foreach (TrashItems sampah in daftarSampah)
        {
            if (sampah != null)
            {
                sampah.gameObject.SetActive(true);
                sampah.manager = this;
                sampah.transform.localScale = Vector3.zero;
                sampah.SimpanPosisiAwal();
            }
        }

        // Animasikan Teks membal masuk
        if (statusText != null) statusText.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
        if (timerText != null) timerText.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetDelay(0.1f);
        yield return new WaitForSeconds(0.2f);

        // Animasikan Tong Sampah bermunculan dari kiri ke kanan
        for (int i = 0; i < daftarTongSampah.Length; i++)
        {
            if (daftarTongSampah[i] != null)
            {
                daftarTongSampah[i].DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
                yield return new WaitForSeconds(0.1f);
            }
        }
        yield return new WaitForSeconds(0.2f);

        // Animasikan Item Sampah bermunculan (seperti dimuntahkan)
        for (int i = 0; i < daftarSampah.Count; i++)
        {
            if (daftarSampah[i] != null)
            {
                daftarSampah[i].transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                yield return new WaitForSeconds(0.05f);
            }
        }
        yield return new WaitForSeconds(0.3f);

        isGameActive = true; // Mulai waktu bermain
    }

    void Update()
    {
        if (isGameActive)
        {
            timeRemaining -= Time.deltaTime;
            timerText.text = $"Waktu: {Mathf.CeilToInt(timeRemaining)}s";

            if (timeRemaining <= 0)
            {
                statusText.text = "Waktu Habis!";
                statusText.color = Color.red;
                EndGame(false);
            }
        }
    }

    public void ItemBerhasilDisortir()
    {
        sampahTersortir++;
        statusText.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0f), 0.2f, 1);

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

        // Hilangkan elemen UI gameplay dari layar
        if (statusText != null) statusText.transform.DOScale(Vector3.zero, 0.3f);
        if (timerText != null) timerText.transform.DOScale(Vector3.zero, 0.3f);

        foreach (RectTransform tong in daftarTongSampah)
        {
            if (tong != null) tong.DOScale(Vector3.zero, 0.3f);
        }

        foreach (TrashItems sampah in daftarSampah)
        {
            if (sampah != null && sampah.gameObject.activeSelf)
            {
                sampah.transform.DOScale(Vector3.zero, 0.3f);
            }
        }

        // Lapor poin dan luncurkan panel hasil
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
                // ANIMASI LINGKUNGAN BERSIH (Awan & Burung)
                // ==========================================
                if (awanMenang != null && awanMenang.Length > 0)
                {
                    for (int i = 0; i < awanMenang.Length; i++)
                    {
                        if (awanMenang[i] != null)
                        {
                            awanMenang[i].anchoredPosition = originalAwanPos[i];
                            float randomSpeed = Random.Range(3f, 6f);
                            float randomDistance = Random.Range(20f, 40f);
                            awanMenang[i].DOAnchorPosX(originalAwanPos[i].x + randomDistance, randomSpeed)
                                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
                        }
                    }
                }

                if (burungMenang != null && burungMenang.Length > 0)
                {
                    for (int i = 0; i < burungMenang.Length; i++)
                    {
                        if (burungMenang[i] != null)
                        {
                            burungMenang[i].anchoredPosition = originalBurungPos[i];
                            float terbangSpeed = Random.Range(4f, 7f);
                            float terbangJarak = 300f;

                            burungMenang[i].DOAnchorPosX(originalBurungPos[i].x + terbangJarak, terbangSpeed)
                                .SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);

                            burungMenang[i].DOAnchorPosY(originalBurungPos[i].y + 15f, 1f)
                                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
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

                // ==========================================
                // ANIMASI LINGKUNGAN KOTOR (Excavator & Sampah)
                // ==========================================

                // 1. Alat Berat Bergerak (Maju-Mundur & Mengeruk)
                if (alatExcavator != null)
                {
                    alatExcavator.anchoredPosition = originalExcavatorPos;
                    alatExcavator.eulerAngles = originalExcavatorRot;

                    // Menggeser excavator maju sedikit ke arah tumpukan sampah lalu mundur
                    alatExcavator.DOAnchorPosX(originalExcavatorPos.x + 25f, 2f)
                        .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

                    // Sedikit efek rotasi agar terlihat berat dan beroperasi
                    alatExcavator.DORotate(new Vector3(0, 0, -3f), 1f)
                        .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
                }

                // 2. Sampah Berserakan & Bergetar
                if (sampahBerserakan != null && sampahBerserakan.Length > 0)
                {
                    for (int i = 0; i < sampahBerserakan.Length; i++)
                    {
                        if (sampahBerserakan[i] != null)
                        {
                            sampahBerserakan[i].anchoredPosition = originalSampahKalahPos[i];

                            // Sampah memantul/bergetar kecil untuk memberi kesan berjatuhan dan berantakan
                            float bounceSpeed = Random.Range(0.4f, 0.8f);
                            sampahBerserakan[i].DOAnchorPosY(originalSampahKalahPos[i].y - 5f, bounceSpeed)
                                .SetEase(Ease.InOutBounce).SetLoops(-1, LoopType.Yoyo);

                            // Putar sedikit (seolah berserakan tertiup angin/terdorong excavator)
                            float rotAmount = Random.Range(-10f, 10f);
                            sampahBerserakan[i].DORotate(new Vector3(0, 0, rotAmount), bounceSpeed * 2f)
                                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
                        }
                    }
                }
            }
        }
    }
}