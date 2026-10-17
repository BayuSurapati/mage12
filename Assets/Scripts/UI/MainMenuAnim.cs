using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MainMenuAnim : MonoBehaviour
{
    [Header("Grup 1: Judul & Subjudul")]
    public CanvasGroup titleCanvasGroup;   // Bungkus Teks "RI 45" dan Subjudul ke sini
    public RectTransform titleRect;        // RectTransform dari pembungkus judul untuk efek geser

    [Header("Grup 2: Tombol Menu")]
    public CanvasGroup buttonsCanvasGroup; // Bungkus ketiga tombol ke sini
    public RectTransform[] menuButtons;    // Masukkan tombol Mulai, Pengaturan, Keluar

    [Header("Grup 3: Karakter & Ikon Pikiran")]
    public CanvasGroup characterCanvasGroup; // Bungkus Karakter dan Ikon ke sini
    public RectTransform characterImage;     // Gambar karakter botak
    public RectTransform[] thoughtIcons;     // Ikon-ikon pikiran (pohon, petir, dsb.)

    void Start()
    {
        // 1. SETUP AWAL (Menyembunyikan semuanya sebelum animasi mulai)
        SetupInitialState();

        // 2. MULAI ANIMASI (Dipanggil dengan delay sedikit agar tidak lag)
        DOVirtual.DelayedCall(0.2f, PlayIntroAnimation);
    }

    void SetupInitialState()
    {
        // Grup 1: Transparan & geser judul ke kiri
        if (titleCanvasGroup != null) titleCanvasGroup.alpha = 0f;
        if (titleRect != null) titleRect.anchoredPosition = new Vector2(titleRect.anchoredPosition.x - 100f, titleRect.anchoredPosition.y);

        // Grup 2: Transparan & perkecil semua tombol
        if (buttonsCanvasGroup != null) buttonsCanvasGroup.alpha = 0f;
        foreach (var btn in menuButtons)
        {
            btn.localScale = Vector3.zero;
        }

        // Grup 3: Transparan, geser karakter ke kanan, & perkecil ikon
        if (characterCanvasGroup != null) characterCanvasGroup.alpha = 0f;
        if (characterImage != null) characterImage.anchoredPosition = new Vector2(characterImage.anchoredPosition.x + 500f, characterImage.anchoredPosition.y);
        foreach (var icon in thoughtIcons)
        {
            icon.localScale = Vector3.zero;
        }
    }

    void PlayIntroAnimation()
    {
        // --- 1. ANIMASI GRUP 1 (Judul Kiri: Fade In + Geser ke Kanan) ---
        if (titleCanvasGroup != null) titleCanvasGroup.DOFade(1f, 1f);
        if (titleRect != null) titleRect.DOAnchorPosX(titleRect.anchoredPosition.x + 100f, 1f).SetEase(Ease.OutQuint);

        // --- 2. ANIMASI GRUP 3 (Karakter Masuk: Fade In + Geser ke Kiri) ---
        if (characterCanvasGroup != null) characterCanvasGroup.DOFade(1f, 1.2f).SetDelay(0.2f);
        if (characterImage != null)
        {
            characterImage.DOAnchorPosX(characterImage.anchoredPosition.x - 500f, 1.2f)
                .SetEase(Ease.OutBack)
                .SetDelay(0.2f);
        }

        // --- 3. ANIMASI GRUP 2 (Tombol Beruntun: Fade In Grup + Pop-up Satuan) ---
        if (buttonsCanvasGroup != null) buttonsCanvasGroup.DOFade(1f, 0.5f).SetDelay(0.4f);
        for (int i = 0; i < menuButtons.Length; i++)
        {
            menuButtons[i].DOScale(Vector3.one, 0.5f)
                .SetEase(Ease.OutBack)
                .SetDelay(0.4f + (i * 0.15f));
        }

        // --- 4. ANIMASI IKON PIKIRAN BERUNTUN & MELAYANG ---
        for (int i = 0; i < thoughtIcons.Length; i++)
        {
            int index = i;
            thoughtIcons[index].DOScale(Vector3.one, 0.4f)
                .SetEase(Ease.OutBack)
                .SetDelay(1f + (index * 0.1f))
                .OnComplete(() =>
                {
                    // Efek melayang pelan ke atas-bawah (Looping Yoyo)
                    thoughtIcons[index].DOAnchorPosY(thoughtIcons[index].anchoredPosition.y + 10f, 2f)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo);
                });
        }
    }
}
