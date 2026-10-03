using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class MathBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public MiniGame_PuzzleMath manager;
    public TextMeshProUGUI numberText;

    [HideInInspector] public int blockValue; // Nilai blok matematika ini
    [HideInInspector] public MathSlot currentSlot; // Slot saat ini tempat blok berada

    private Vector3 originalPosition; // Posisi asli blok sebelum di-drag
    private RectTransform rectTransform; // Komponen RectTransform dari blok
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

        if(canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void SetupBlock(int value)
    {
        blockValue = value;
        numberText.text = value.ToString();
        originalPosition = rectTransform.anchoredPosition; // Simpan posisi asli saat setup
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(!manager.isGameActive)
        {
            return;
        }

        transform.SetAsLastSibling(); // Bawa blok ke depan saat di-drag
        canvasGroup.alpha = 0.6f; // Buat blok sedikit transparan saat di-drag
        canvasGroup.blocksRaycasts = false; // Nonaktifkan raycast agar blok bisa dilewati pointer

        if(currentSlot != null)
        {
            currentSlot.currenBlock = null; // Lepaskan blok dari slot saat ini
            currentSlot = null;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(!manager.isGameActive)
        {
            return;
        }

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor; // Update posisi blok saat di-drag
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(!manager.isGameActive)
        {
            return;
        }
        canvasGroup.alpha = 1f; // Kembalikan transparansi blok
        canvasGroup.blocksRaycasts = true; // Aktifkan kembali raycast

        GameObject objekDibawah = eventData.pointerCurrentRaycast.gameObject;

        if (objekDibawah != null)
        {
            MathSlot slot = objekDibawah.GetComponent<MathSlot>();

            if(slot != null && slot.currenBlock == null)
            {
                // Jika slot valid dan kosong, tempatkan blok di slot tersebut
                rectTransform.position = slot.transform.position;
                currentSlot = slot;
                slot.currenBlock = this;

                manager.CheckWinCondition(); // Periksa kondisi kemenangan setelah menempatkan blok
                return;
            }
        }
        SnapBack(); // Kembalikan blok ke posisi asli jika tidak ditempatkan di slot yang valid
    }

    public void SnapBack()
    {
        rectTransform.anchoredPosition = originalPosition; // Kembalikan blok ke posisi asli
        if(currentSlot != null)
        {
            currentSlot.currenBlock = null; // Lepaskan blok dari slot saat ini
            currentSlot = null;
        }
    }

}
