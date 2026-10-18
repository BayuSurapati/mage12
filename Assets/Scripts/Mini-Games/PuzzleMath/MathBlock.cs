using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI; // WAJIB: Impor DOTween

public class MathBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public MiniGame_PuzzleMath manager;
    public TextMeshProUGUI numberText;

    [HideInInspector] public int blockValue;
    [HideInInspector] public MathSlot currentSlot;

    private Vector3 originalPosition;
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        originalPosition = rectTransform.anchoredPosition;
    }

    public void SetupBlock(int value)
    {
        blockValue = value;
        numberText.text = value.ToString();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!manager.isGameActive) return;

        transform.SetAsLastSibling();

        // Hentikan animasi yang mungkin sedang berjalan
        DOTween.Kill(rectTransform);

        // Buat balok membesar sedikit saat digenggam
        rectTransform.DOScale(Vector3.one * 1.1f, 0.1f);
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        if (currentSlot != null)
        {
            currentSlot.currenBlock = null;
            currentSlot = null;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!manager.isGameActive) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!manager.isGameActive) return;

        // Kembalikan ukuran balok
        rectTransform.DOScale(Vector3.one, 0.1f);
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        GameObject objekDibawah = eventData.pointerCurrentRaycast.gameObject;

        if (objekDibawah != null)
        {
            MathSlot slot = objekDibawah.GetComponent<MathSlot>();

            if (slot != null && slot.currenBlock == null)
            {
                // Menganimasikan perpindahan balok ke slot seperti ditarik magnet
                rectTransform.DOMove(slot.transform.position, 0.2f).SetEase(Ease.OutBack);
                currentSlot = slot;
                slot.currenBlock = this;

                manager.CheckWinCondition();
                return;
            }
        }
        SnapBack();
    }

    public void SnapBack()
    {
        // Animasi kembali mulus ke tempat asal
        rectTransform.DOAnchorPos(originalPosition, 0.3f).SetEase(Ease.OutBack);

        if (currentSlot != null)
        {
            currentSlot.currenBlock = null;
            currentSlot = null;
        }
    }

    // Fungsi baru untuk dipanggil GameManager saat jawaban salah (efek warna merah sejenak)
    public void SnapBackError()
    {
        Image blockImage = GetComponent<Image>();
        if (blockImage != null)
        {
            Color origColor = blockImage.color;
            blockImage.DOColor(Color.red, 0.2f).SetLoops(2, LoopType.Yoyo).OnComplete(() => blockImage.color = origColor);
        }
        SnapBack();
    }
}