using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class CardSwipe : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("Data Kartu saat ini")]
    public CardData activeCard;

    [Header("Referensi UI Teks")]
    public TextMeshProUGUI narrativeTextUI;

    [Header("Referensi Modul Lain")]
    public GameManager gameManager;

    [Header("Pengaturan Geser")]
    public float swipeThreshold = 150f;
    public float telegraphThreshold = 50f;
    public float throwSpeed = 2500f;

    [Header("Pengaturan Visual Sinyal (Ukuran)")]
    public float minDotScale = 0.5f;
    public float maxDotScale = 1.8f;
    public float maxEffectValue = 15f;

    [Header("Referensi UI Sinyal")]
    public GameObject sinyalBencana;
    public GameObject sinyalEkosistem;
    public GameObject sinyalKeuangan;
    public GameObject sinyalTeknologi;

    [Header("Referensi UI Teks Pilihan")]
    public TextMeshProUGUI teksPilihanKiri;
    public TextMeshProUGUI teksPilihanKanan;

    public Image imageKarakterUI;

    private Vector2 defaultPosition;
    private RectTransform rectTransform;
    private bool isAnimating = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        defaultPosition = rectTransform.anchoredPosition;
        HideAllSignals();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isAnimating) return;
    }

    public void UpdateCardDisplay()
    {
        if (activeCard != null && narrativeTextUI != null)
        {
            narrativeTextUI.text = activeCard.narasiCerita;

            if (teksPilihanKiri != null)
            {
                teksPilihanKiri.text = activeCard.teksGeserKiri;
                SetTextAlpha(teksPilihanKiri, 1f);
            }
            if (teksPilihanKanan != null)
            {
                teksPilihanKanan.text = activeCard.teksGeserKanan;
                SetTextAlpha(teksPilihanKanan, 1f);
            }
            if (imageKarakterUI != null && activeCard.gambarKarakter != null)
            {
                imageKarakterUI.sprite = activeCard.gambarKarakter;
            }

            SetTextAlpha(teksPilihanKiri, 0f);
            SetTextAlpha(teksPilihanKanan, 0f);
        }
    }

    // ==========================================
    // 1. ANIMASI HANYA UNTUK KARTU PERTAMA KALI
    // ==========================================
    public void AnimateCardEntrance()
    {
        isAnimating = true;
        HideAllSignals();
        SetTextAlpha(teksPilihanKiri, 0f);
        SetTextAlpha(teksPilihanKanan, 0f);

        // Siapkan suara acak dan mainkan DENGAN DELAY 0.5 detik
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SiapkanSuaraKarakterBaru();
            AudioManager.Instance.MulaiBergumamDenganDelay();
        }

        rectTransform.anchoredPosition = new Vector2(0, 1500f);
        rectTransform.rotation = Quaternion.Euler(0, 0, Random.Range(-15f, 15f));
        rectTransform.localScale = Vector3.one;

        rectTransform.DOAnchorPos(defaultPosition, 0.8f).SetEase(Ease.OutQuart);
        rectTransform.DORotate(Vector3.zero, 0.8f).OnComplete(() =>
        {
            isAnimating = false;
        });
    }

    // ==========================================
    // 2. MUNCULKAN KARTU UNTUK BULAN KE-2 DST.
    // ==========================================
    public void InstantResetForNextCard()
    {
        isAnimating = true;
        HideAllSignals();
        SetTextAlpha(teksPilihanKiri, 0f);
        SetTextAlpha(teksPilihanKanan, 0f);

        // Siapkan suara acak baru dan mainkan DENGAN DELAY 0.5 detik
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SiapkanSuaraKarakterBaru();
            AudioManager.Instance.MulaiBergumamDenganDelay();
        }

        rectTransform.anchoredPosition = defaultPosition;
        rectTransform.rotation = Quaternion.identity;
        rectTransform.localScale = Vector3.zero;

        rectTransform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            isAnimating = false;
        });
    }

    // ==========================================
    // 3. KARTU TIDAK JADI DIGESER (SNAP BACK)
    // ==========================================
    public void ResetCardPosition()
    {
        AudioManager.Instance.MainkanSFX("Popp");
        isAnimating = true;
        HideAllSignals();
        SetTextAlpha(teksPilihanKiri, 0f);
        SetTextAlpha(teksPilihanKanan, 0f);

        rectTransform.DOAnchorPos(defaultPosition, 0.3f).SetEase(Ease.OutBack);
        rectTransform.DORotate(Vector3.zero, 0.3f).OnComplete(() =>
        {
            isAnimating = false;
        });
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isAnimating) return;

        rectTransform.anchoredPosition += eventData.delta;

        float rotationAngle = rectTransform.anchoredPosition.x * -0.05f;
        rectTransform.rotation = Quaternion.Euler(0, 0, rotationAngle);

        float dragDistanceX = rectTransform.anchoredPosition.x - defaultPosition.x;

        UpdateTelegraphing(dragDistanceX);

        float fadeRatio = Mathf.Clamp01(Mathf.Abs(dragDistanceX) / swipeThreshold);

        if (dragDistanceX > 10f || dragDistanceX < -10f)
        {
            if (dragDistanceX > 10f)
            {
                SetTextAlpha(teksPilihanKanan, fadeRatio);
                SetTextAlpha(teksPilihanKiri, 0f);
            }
            else
            {
                SetTextAlpha(teksPilihanKiri, fadeRatio);
                SetTextAlpha(teksPilihanKanan, 0f);
            }
            // Suara tidak lagi dipanggil di sini!
        }
        else
        {
            SetTextAlpha(teksPilihanKiri, 0f);
            SetTextAlpha(teksPilihanKanan, 0f);
            HideAllSignals();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isAnimating) return;

        AudioManager.Instance.MainkanSFX("Whoosh");

        float dragDistanceX = rectTransform.anchoredPosition.x - defaultPosition.x;

        if (dragDistanceX > swipeThreshold)
        {
            StartCoroutine(ThrowCardOffScreen(true));
        }
        else if (dragDistanceX < -swipeThreshold)
        {
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

        if (AudioManager.Instance != null)
        {
            // Matikan suara gumaman jika kebetulan pemain melempar kartu sangat cepat
            AudioManager.Instance.HentikanBergumam();
            AudioManager.Instance.MainkanSuaraKertas();
        }

        float targetOffScreenX = toRight ? Screen.width + 1000f : -Screen.width - 1000f;
        Vector2 targetPosition = new Vector2(targetOffScreenX, rectTransform.anchoredPosition.y);

        while (Vector2.Distance(rectTransform.anchoredPosition, targetPosition) > 10f)
        {
            rectTransform.anchoredPosition = Vector2.MoveTowards(rectTransform.anchoredPosition, targetPosition, throwSpeed * Time.deltaTime);
            yield return null;
        }

        if (gameManager != null)
        {
            gameManager.ProcessDecision(toRight, activeCard);
        }
    }

    private void UpdateTelegraphing(float dragDistanceX)
    {
        HideAllSignals();

        if (activeCard == null) return;

        if (dragDistanceX > telegraphThreshold)
        {
            if (activeCard.efekKananBencana != 0) ActivateAndScaleSignal(sinyalBencana, activeCard.efekKananBencana);
            if (activeCard.efekKananEkosistem != 0) ActivateAndScaleSignal(sinyalEkosistem, activeCard.efekKananEkosistem);
            if (activeCard.efekKananKeuangan != 0) ActivateAndScaleSignal(sinyalKeuangan, activeCard.efekKananKeuangan);
            if (activeCard.efekKananTeknologi != 0) ActivateAndScaleSignal(sinyalTeknologi, activeCard.efekKananTeknologi);
        }
        else if (dragDistanceX < -telegraphThreshold)
        {
            if (activeCard.efekKiriBencana != 0) ActivateAndScaleSignal(sinyalBencana, activeCard.efekKiriBencana);
            if (activeCard.efekKiriEkosistem != 0) ActivateAndScaleSignal(sinyalEkosistem, activeCard.efekKiriEkosistem);
            if (activeCard.efekKiriKeuangan != 0) ActivateAndScaleSignal(sinyalKeuangan, activeCard.efekKiriKeuangan);
            if (activeCard.efekKiriTeknologi != 0) ActivateAndScaleSignal(sinyalTeknologi, activeCard.efekKiriTeknologi);
        }
    }

    private void ActivateAndScaleSignal(GameObject signal, int effectValue)
    {
        if (signal == null) return;
        signal.SetActive(true);
        float absValue = Mathf.Abs(effectValue);
        absValue = Mathf.Clamp(absValue, 0, maxEffectValue);
        float calculatedScale = Mathf.Lerp(minDotScale, maxDotScale, absValue / maxEffectValue);
        signal.GetComponent<RectTransform>().localScale = new Vector3(calculatedScale, calculatedScale, 1f);
    }

    private void SetTextAlpha(TextMeshProUGUI textUI, float alpha)
    {
        if (textUI != null)
        {
            Color c = textUI.color;
            c.a = alpha;
            textUI.color = c;
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