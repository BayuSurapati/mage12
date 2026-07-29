using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardSwipe : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("Data Kartu saat ini")]
    [Tooltip("Data ini nantinya akan diisi otomatis oleh Game Manager")]
    public CardData activeCard;

    [Header("Referensi UI Teks")]
    [Tooltip("Masukkan objek CardNarrative Teks kesini")]
    public TextMeshProUGUI narrativeTextUI;

    [Header("Pengaturan Geser")]
    public float swipeThreshold = 150f; // Jarak untuk mengeksekusi pilihan (Setuju/Tolak)
    public float telegraphThreshold = 50f; // Jarak untuk memunculkan Sinyal UI (Mengintip)

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

    private Vector2 defaultPosition;
    private RectTransform rectTransform;

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        defaultPosition = rectTransform.anchoredPosition;
        HideAllSignals();
        UpdateCardDisplay();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {

    }

    public void UpdateCardDisplay()
    {
        if (activeCard != null && narrativeTextUI != null)
        {
            narrativeTextUI.text = activeCard.narasiCerita;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta;

        float rotationAngle = rectTransform.anchoredPosition.x * -0.05f;
        rectTransform.rotation = Quaternion.Euler(0,0, rotationAngle);

        float dragDistanceX = rectTransform.anchoredPosition.x - defaultPosition.x;

        UpdateTelegraphing(dragDistanceX);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float dragDistanceX = rectTransform.anchoredPosition.x - defaultPosition.x;

        if(dragDistanceX > swipeThreshold)
        {
            Debug.Log("Swipe Kanan Valid: Pilihan Setuju!");
        }
        else if (dragDistanceX < -swipeThreshold)
        {
            Debug.Log("Swipe Kiri Valid: Pilihan Tolak!");
        }
        else
        {
            ResetCardPosition();
        }
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

    private void ResetCardPosition()
    {
        rectTransform.anchoredPosition = defaultPosition;
        rectTransform.rotation = Quaternion.identity;
        HideAllSignals();
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
