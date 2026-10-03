using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.EventSystems;

public class TrashItems : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public JenisSampah jenisSampah;
    public MiniGame_SortirSampah manager;

    private Vector2 originalPosition;
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        if(canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void SimpanPosisiAwal()
    {
        originalPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(!manager.isGameActive)
        {
            return; // Do not allow dragging if the game is not active
        }

        transform.SetAsLastSibling(); // Bring the item to the front
        canvasGroup.alpha = 0.6f; // Make the item semi-transparent while dragging
        canvasGroup.blocksRaycasts = false; // Allow raycasts to pass through the item while dragging 
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(!manager.isGameActive)
        {
            return; // Do not allow dragging if the game is not active
        }

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(!manager.isGameActive)
        {
            return; // Do not allow dragging if the game is not active
        }

        canvasGroup.alpha = 1f; // Restore the item's opacity
        canvasGroup.blocksRaycasts = true;

        GameObject objekDiBawahJari = eventData.pointerCurrentRaycast.gameObject;

        if (objekDiBawahJari != null)
        {
            TrashBin tong = objekDiBawahJari.GetComponent<TrashBin>();

            if (tong != null)
            {
                // Jika masuk ke tong yang JENISNYA SAMA
                if (tong.jenisTong == this.jenisSampah)
                {
                    manager.ItemBerhasilDisortir();
                    gameObject.SetActive(false); // Hilangkan sampahnya
                    return; // Hentikan fungsi di sini agar tidak kembali ke awal
                }
            }
        }

        // Jika salah masuk tong atau dilepas sembarangan di luar tong -> KEMBALI KE POSISI AWAL (Snap Back)
        rectTransform.anchoredPosition = originalPosition;
    }
}
