using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class MiniGame_CyberAttack : MonoBehaviour
{
    [Header("Referensi Sistem Inti")]
    public MiniGameManager miniGameManager;

    [Header("Pengaturan Game")]
    public float timeLimit = 10f;
    public int totalPopups = 8;

    [Header("Referensi UI Gameplay")]
    public Transform popupContainer;
    public TextMeshProUGUI timerText;
    public Slider progressBar;

    [Header("Varian Prefab Virus")]
    public GameObject[] popupPrefabs;

    [Header("UI Hasil (PANEL MENANG)")]
    public GameObject panelMenang;
    public CanvasGroup canvasGroupMenang;
    public RectTransform rectMenang;

    [Header("UI Hasil (PANEL KALAH)")]
    public GameObject panelKalah;
    public CanvasGroup canvasGroupKalah;
    public RectTransform rectKalah;

    // 👇 BARU: Referensi untuk segitiga peringatan kuning 👇
    public CanvasGroup warningTriangleIcon;

    // Variabel internal
    private float timeRemaining;
    private int popupsClosed = 0;
    private bool isGameActive = false;

    public void StartMiniGame()
    {
        timeRemaining = timeLimit;
        popupsClosed = 0;
        progressBar.minValue = 0;
        progressBar.maxValue = totalPopups;
        progressBar.value = 0;
        isGameActive = false;

        // Matikan panel
        if (panelMenang != null) panelMenang.SetActive(false);
        if (panelKalah != null) panelKalah.SetActive(false);

        // Hentikan animasi berkedip jika game diulang
        if (warningTriangleIcon != null) DOTween.Kill(warningTriangleIcon);

        SpawnAllPopupsAnimated();
    }

    void Update()
    {
        if (!isGameActive) return;

        timeRemaining -= Time.deltaTime;
        timerText.text = "Waktu Tersisa: " + Mathf.CeilToInt(timeRemaining).ToString() + "s";

        if (timeRemaining <= 0)
        {
            timerText.text = "Waktu Habis!";
            EndGame(false);
        }
    }

    private void SpawnAllPopupsAnimated()
    {
        if (popupPrefabs == null || popupPrefabs.Length == 0) return;

        for (int i = 0; i < totalPopups; i++)
        {
            int randomIndex = Random.Range(0, popupPrefabs.Length);
            GameObject newPopup = Instantiate(popupPrefabs[randomIndex], popupContainer);
            RectTransform rect = newPopup.GetComponent<RectTransform>();

            float randomX = Random.Range(-700f, 755f);
            float randomY = Random.Range(-300f, 180f);
            rect.anchoredPosition = new Vector2(randomX, randomY);

            rect.localScale = Vector3.zero;
            rect.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetDelay(i * 0.1f);

            Button popupBtn = newPopup.GetComponent<Button>();
            if (popupBtn != null)
            {
                GameObject capturedPopup = newPopup;
                popupBtn.onClick.AddListener(() => OnPopupClickedAnimated(capturedPopup));
            }
        }

        float totalIntroTime = (totalPopups * 0.1f) + 0.4f;
        DOVirtual.DelayedCall(totalIntroTime, () => isGameActive = true);
    }

    private void OnPopupClickedAnimated(GameObject popup)
    {
        if (!isGameActive) return;

        popup.GetComponent<Button>().interactable = false;

        popup.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
        {
            Destroy(popup);
            popupsClosed++;
            progressBar.value = popupsClosed;

            progressBar.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0f), 0.2f, 1);

            if (popupsClosed >= totalPopups)
            {
                EndGame(true);
            }
        });
    }

    private void EndGame(bool isWin)
    {
        isGameActive = false;

        foreach (Transform child in popupContainer)
        {
            if (child.GetComponent<Button>() != null)
            {
                child.DOScale(Vector3.zero, 0.2f).OnComplete(() => Destroy(child.gameObject));
            }
        }

        miniGameManager.CompleteMiniGame(isWin);

        DOVirtual.DelayedCall(0.5f, () => ShowResultPanelAnimated(isWin));
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

                // 👇 BARU: Animasi Segitiga Berkedip 👇
                if (warningTriangleIcon != null)
                {
                    // Mulai dari terlihat jelas (Alpha = 1)
                    warningTriangleIcon.alpha = 1f;

                    // Fade ke agak transparan (Alpha = 0.2) dalam 0.3 detik
                    warningTriangleIcon.DOFade(0.2f, 0.3f)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo) // Mengulang tanpa henti, mondar-mandir (Yoyo)
                        .SetDelay(0.8f); // Baru mulai berkedip setelah panel selesai meluncur
                }
            }
        }
    }
}