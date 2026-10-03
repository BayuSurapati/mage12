using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiniGame_CyberAttack : MonoBehaviour
{
    [Header("Referensi Sistem Inti")]
    public MiniGameManager miniGameManager;

    [Header("Pengaturan Game")]
    public float timeLimit = 10f; // Batas waktu 10 detik
    public int totalPopups = 8;   // Total 8 iklan virus

    [Header("Referensi UI")]
    public GameObject popupPrefab;     // Masukkan Prefab Popup_Virus ke sini
    public Transform popupContainer;   // Masukkan Panel_CyberAttack ke sini (sebagai induk)
    public TextMeshProUGUI timerText;  // Teks hitung mundur
    public Slider progressBar;         // Progress bar (Slider)

    // Variabel internal
    private float timeRemaining;
    private int popupsClosed = 0;
    private bool isGameActive = false;

    // --- FUNGSI MEMULAI GAME ---
    public void StartMiniGame()
    {
        // Reset semua nilai
        timeRemaining = timeLimit;
        popupsClosed = 0;

        progressBar.minValue = 0;
        progressBar.maxValue = totalPopups;
        progressBar.value = 0;

        SpawnAllPopups(); // Munculkan 8 iklan sekaligus

        isGameActive = true;
    }

    void Update()
    {
        if (!isGameActive) return; // Jika game belum mulai atau sudah selesai, abaikan

        // Jalankan hitung mundur
        timeRemaining -= Time.deltaTime;

        // Tampilkan teks dengan membulatkan angka ke atas (misal 9.8 jadi 10)
        timerText.text = "Waktu Tersisa: " + Mathf.CeilToInt(timeRemaining).ToString() + "s";

        // Cek jika waktu habis
        if (timeRemaining <= 0)
        {
            timerText.text = "Waktu Habis!";
            EndGame(false); // Kalah
        }
    }

    // --- FUNGSI MUNCULKAN IKLAN ---
    private void SpawnAllPopups()
    {
        for (int i = 0; i < totalPopups; i++)
        {
            // Cetak Prefab ke dalam Container
            GameObject newPopup = Instantiate(popupPrefab, popupContainer);

            // Acak posisinya di layar
            RectTransform rect = newPopup.GetComponent<RectTransform>();

            // Catatan: Angka ini tergantung resolusi layar Canvas Anda (misal 1080x1920)
            // Sesuaikan range ini agar pop-up tidak keluar batas layar
            float randomX = Random.Range(-350f, 500f);
            float randomY = Random.Range(-400f, 160f);
            rect.anchoredPosition = new Vector2(randomX, randomY);

            // Tambahkan perintah klik ke tombol secara otomatis lewat skrip
            Button popupBtn = newPopup.GetComponent<Button>();
            popupBtn.onClick.AddListener(() => OnPopupClicked(newPopup));
        }
    }

    // --- FUNGSI SAAT PEMAIN MENEKAN IKLAN ---
    private void OnPopupClicked(GameObject popup)
    {
        if (!isGameActive) return;

        Destroy(popup); // Hancurkan objek pop-up dari layar

        popupsClosed++; // Tambah skor
        progressBar.value = popupsClosed; // Update UI Slider

        // Cek kondisi menang
        if (popupsClosed >= totalPopups)
        {
            EndGame(true); // Menang
        }
    }

    // --- FUNGSI MENGAKHIRI GAME ---
    private void EndGame(bool isWin)
    {
        isGameActive = false;

        // Bersihkan sisa pop-up di layar (jika kalah karena kehabisan waktu)
        foreach (Transform child in popupContainer)
        {
            // Jangan hapus Slider atau Text, hanya hapus objek yang punya komponen Button (Pop-up)
            if (child.GetComponent<Button>() != null)
            {
                Destroy(child.gameObject);
            }
        }

        // Lapor ke MiniGameManager bahwa game selesai (mengirim True atau False)
        miniGameManager.CompleteMiniGame(isWin);
    }
}