using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AnimasiAwan : MonoBehaviour
{
    [Header("Senarai Awan")]
    [Tooltip("Masukkan semua objek UI awan anda ke dalam array ini")]
    public RectTransform[] senaraiAwan;

    [Header("Tetapan Animasi")]
    public float jarakPergerakan = 30f; // Jarak awan bergerak ke tepi (dalam pixel)
    public float masaPalingCepat = 4f;  // Masa pergerakan paling laju
    public float masaPalingLambat = 7f; // Masa pergerakan paling perlahan

    void Start()
    {
        // Jalankan animasi untuk setiap awan secara berasingan
        foreach (RectTransform awan in senaraiAwan)
        {
            // Cipta masa rawak supaya pergerakan awan tidak seragam antara satu sama lain
            float masaRawak = Random.Range(masaPalingCepat, masaPalingLambat);

            // Gerakkan awan pada paksi X (Kiri/Kanan)
            awan.DOAnchorPosX(awan.anchoredPosition.x + jarakPergerakan, masaRawak)
                .SetEase(Ease.InOutSine)      // Pergerakan menjadi perlahan di hujung (Smooth)
                .SetLoops(-1, LoopType.Yoyo); // -1 bermaksud infinity loop, Yoyo bermaksud patah balik
        }
    }
}
