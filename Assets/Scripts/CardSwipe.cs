using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.Properties;

public class CardSwipe : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("Data Kartu saat ini")]
    [Tooltip("Data ini nantinya akan diisi otomatis oleh Game Manager")]
    public CardData activeCard;

    [Header("Referensi UI Teks")]
    [Tooltip("Masukkan objek CardNarrative Teks kesini")]
    public TextMeshProUGUI narrativeTextUI;

    [Header("Referensi Modul Lain")]
    public GameManager gameManager;

    [Header("Pengaturan Geser")]
    public float swipeThreshold = 150f; // Jarak untuk mengeksekusi pilihan (Setuju/Tolak)
    public float telegraphThreshold = 50f; // Jarak untuk memunculkan Sinyal UI (Mengintip)
    [Tooltip("Kecepatan kartu terlempar keluar setelah dilepas")]
    public float throwSpeed = 2500f;

    [Header("Pengaturan Visual Sinyal (Ukuran)")]
    [Tooltip("Skala ukuran titik paling kecil (untuk efek mendekati 0)")]
    public float minDotScale = 0.5f;
    [Tooltip("Skala ukuran titik paling besar (untuk efek maksimal)")]
    public float maxDotScale = 1.8f;
    [Tooltip("Nilai maksimal efek kartu biasa sebagai acuan pembagi")]
    public float maxEffectValue = 15f;

    [Header("Referensi UI Sinyal")]
    public GameObject sinyalBencana;
    public GameObject sinyalEkosistem;
    public GameObject sinyalKeuangan;
    public GameObject sinyalTeknologi;

    [Header("Referensi UI Teks Pilihan")]
    public TextMeshProUGUI teksPilihanKiri;
    public TextMeshProUGUI teksPilihanKanan;

    [Tooltip("Masukkan komponen UI Image karakter di sini")]
    public Image imageKarakterUI;

    private Vector2 defaultPosition;
    private RectTransform rectTransform;

    private bool isAnimating = false;

    // Start is called before the first frame update
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        defaultPosition = rectTransform.anchoredPosition;
        HideAllSignals();
        //UpdateCardDisplay();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isAnimating) return;
    }

    public void UpdateCardDisplay()
    {
        Debug.Log($"teksPilihanKiri: {teksPilihanKiri}");  // Akan keluar "None" jika null
        Debug.Log($"teksPilihanKanan: {teksPilihanKanan}");  // Akan keluar "None" jika null

        if (activeCard != null && narrativeTextUI != null)
        {
            narrativeTextUI.text = activeCard.narasiCerita;

            if (teksPilihanKiri != null)
            {
                teksPilihanKiri.text = activeCard.teksGeserKiri;
                Debug.Log($"teksPilihanKiri text: {teksPilihanKiri.text}");
                Debug.Log($"teksPilihanKiri enabled: {teksPilihanKiri.enabled}");
                Debug.Log($"teksPilihanKiri gameObject active: {teksPilihanKiri.gameObject.activeInHierarchy}");
                SetTextAlpha(teksPilihanKiri, 1f);
            }
            else
            {
                Debug.LogError("teksPilihanKiri adalah NULL!");
            }

            if (teksPilihanKanan != null)
            {
                teksPilihanKanan.text = activeCard.teksGeserKanan;
                Debug.Log($"teksPilihanKanan text: {teksPilihanKanan.text}");
                Debug.Log($"teksPilihanKanan enabled: {teksPilihanKanan.enabled}");
                Debug.Log($"teksPilihanKanan gameObject active: {teksPilihanKanan.gameObject.activeInHierarchy}");
                SetTextAlpha(teksPilihanKanan, 1f);
            }
            else
            {
                Debug.LogError("teksPilihanKanan adalah NULL!");
            }

            if (imageKarakterUI != null && activeCard.gambarKarakter != null)
            {
                imageKarakterUI.sprite = activeCard.gambarKarakter;
            }

            SetTextAlpha(teksPilihanKiri, 0f);
            SetTextAlpha(teksPilihanKanan, 0f);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isAnimating) return;

        rectTransform.anchoredPosition += eventData.delta;

        float rotationAngle = rectTransform.anchoredPosition.x * -0.05f;
        rectTransform.rotation = Quaternion.Euler(0,0, rotationAngle);

        float dragDistanceX = rectTransform.anchoredPosition.x - defaultPosition.x;
        Debug.Log($"dragDistanceX: {dragDistanceX}"); // Debug
        UpdateTelegraphing(dragDistanceX);

        //Jika jarak geser lebih dari 10 pixel, baru mulai menampilkan teks pilihan
        float fadeRatio = Mathf.Clamp01(Mathf.Abs(dragDistanceX) / swipeThreshold);
        Debug.Log($"fadeRatio: {fadeRatio}"); // Debug

        if (dragDistanceX > 10f) // Tarik ke Kanan
        {
            Debug.Log($"Setting right text alpha to: {fadeRatio}"); // Debug
            SetTextAlpha(teksPilihanKanan, fadeRatio);
            SetTextAlpha(teksPilihanKiri, 0f);
        }
        else if (dragDistanceX < -10f) // Tarik ke Kiri
        {
            Debug.Log($"Setting left text alpha to: {fadeRatio}"); // Debug
            SetTextAlpha(teksPilihanKiri, fadeRatio);
            SetTextAlpha(teksPilihanKanan, 0f);
        }
        else // Di area netral tengah
        {
            SetTextAlpha(teksPilihanKiri, 0f);
            SetTextAlpha(teksPilihanKanan, 0f);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isAnimating) return;

        float dragDistanceX = rectTransform.anchoredPosition.x - defaultPosition.x;
        //bool hasMadeChoice = false;

        if(dragDistanceX > swipeThreshold)
        {
            Debug.Log("Swipe Kanan Valid: Pilihan Setuju!");
            StartCoroutine(ThrowCardOffScreen(true));
        }
        else if (dragDistanceX < -swipeThreshold)
        {
            Debug.Log("Swipe Kiri Valid: Pilihan Tolak!");
            StartCoroutine(ThrowCardOffScreen(false));
        }
        else
        {
            ResetCardPosition();
        }
    }

    private IEnumerator ThrowCardOffScreen(bool toRight)
    {
        isAnimating = true;
        HideAllSignals();

        // 1. Tentukan titik target di luar layar
        // Jika toRight = true, targetnya jauh di sebelah kanan layar. Jika false, sebelah kiri.
        float targetOffScreenX = toRight ? Screen.width + 1000f : -Screen.width -1000f;
        Vector2 targetPosition = new Vector2(targetOffScreenX, rectTransform.anchoredPosition.y);

        // 2. Lakukan pergerakan bertahap (frame by frame)
        // Kartu akan terus bergerak sampai jaraknya ke target sudah sangat dekat
        while (Vector2.Distance(rectTransform.anchoredPosition, targetPosition) > 10f)
        {
            rectTransform.anchoredPosition = Vector2.MoveTowards(rectTransform.anchoredPosition, targetPosition, throwSpeed * Time.deltaTime);
            yield return null;
        }

        // 3. Setelah kartu benar-benar hilang dari layar:
        if (gameManager != null)
        {
            gameManager.ProcessDecision(toRight, activeCard);
        }

        // 4. Kembalikan posisi fisik kartu ke tengah secara instan (diam-diam)
        //rectTransform.anchoredPosition = defaultPosition;
        //rectTransform.rotation = Quaternion.identity;

        //isAnimating = false;
    }

    private void UpdateTelegraphing(float dragDistanceX)
    {
        HideAllSignals();

        if(activeCard == null)
        {
            return;
        }

        if (dragDistanceX > telegraphThreshold) // KANAN
        {
            if (activeCard.efekKananBencana != 0) ActivateAndScaleSignal(sinyalBencana, activeCard.efekKananBencana);
            if (activeCard.efekKananEkosistem != 0) ActivateAndScaleSignal(sinyalEkosistem, activeCard.efekKananEkosistem);
            if (activeCard.efekKananKeuangan != 0) ActivateAndScaleSignal(sinyalKeuangan, activeCard.efekKananKeuangan);
            if (activeCard.efekKananTeknologi != 0) ActivateAndScaleSignal(sinyalTeknologi, activeCard.efekKananTeknologi);
        }
        else if (dragDistanceX < -telegraphThreshold) // KIRI
        {
            if (activeCard.efekKiriBencana != 0) ActivateAndScaleSignal(sinyalBencana, activeCard.efekKiriBencana);
            if (activeCard.efekKiriEkosistem != 0) ActivateAndScaleSignal(sinyalEkosistem, activeCard.efekKiriEkosistem);
            if (activeCard.efekKiriKeuangan != 0) ActivateAndScaleSignal(sinyalKeuangan, activeCard.efekKiriKeuangan);
            if (activeCard.efekKiriTeknologi != 0) ActivateAndScaleSignal(sinyalTeknologi, activeCard.efekKiriTeknologi);
        }
    }

    // --- FUNGSI BARU: Menyalakan Sinyal Sekaligus Mengubah Ukurannya ---
    private void ActivateAndScaleSignal(GameObject signal, int effectValue)
    {
        if (signal == null) return;

        // 1. Nyalakan objek sinyal
        signal.SetActive(true);

        // 2. Dapatkan nilai absolut dari efek (karena minus atau plus sama-sama dihitung "besar")
        float absValue = Mathf.Abs(effectValue);

        // 3. Batasi nilai maksimal agar tidak melebihi batas (untuk antisipasi nilai mini-game yang mencapai 25)
        absValue = Mathf.Clamp(absValue, 0, maxEffectValue);

        // 4. Hitung skala menggunakan Lerp
        // Jika absValue = 0, skala = minDotScale. Jika absValue = 15, skala = maxDotScale.
        float calculatedScale = Mathf.Lerp(minDotScale, maxDotScale, absValue / maxEffectValue);

        // 5. Terapkan skala ke komponen RectTransform UI
        signal.GetComponent<RectTransform>().localScale = new Vector3(calculatedScale, calculatedScale, 1f);
    }

    private void SetTextAlpha(TextMeshProUGUI textUI, float alpha)
    {
        if(textUI != null)
        {
            Color c = textUI.color;
            c.a = alpha;
            textUI.color = c;
        }
    }

    public void ResetCardPosition()
    {
        rectTransform.anchoredPosition = defaultPosition;
        rectTransform.rotation = Quaternion.identity;
        isAnimating = false;
        HideAllSignals();

        SetTextAlpha(teksPilihanKiri, 0f);
        SetTextAlpha(teksPilihanKanan, 0f);
    }

    private void ShowSignals(GameObject[] signalsToActivate)
    {
        HideAllSignals();
        foreach (GameObject signal in signalsToActivate)
        {
            if(signal != null)
            {
                signal.SetActive(true);
            }
        }
    }

    private void HideAllSignals()
    {
        if (sinyalBencana != null) sinyalBencana.SetActive(false);
        if (sinyalEkosistem != null) sinyalEkosistem.SetActive(false);
        if (sinyalKeuangan != null) sinyalKeuangan.SetActive(false);
        if (sinyalTeknologi != null) sinyalTeknologi.SetActive(false);
    }
}
