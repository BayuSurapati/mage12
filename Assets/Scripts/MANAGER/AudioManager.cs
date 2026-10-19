using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Komponen Audio")]
    public AudioSource audioSourceVoice; // Pengeras suara untuk gumaman
    public AudioSource audioSourceSFX;   // Pengeras suara untuk efek suara (kertas, dll)
    public AudioSource audioSourceBGM;   // Pengeras suara khusus untuk Background Music

    [Header("Daftar Musik Latar (BGM)")]
    [Tooltip("Masukkan semua file BGM di sini")]
    public AudioClip[] daftarBGM;

    [Header("Daftar Efek Suara (SFX) Umum")]
    [Tooltip("Masukkan semua file SFX umum di sini")]
    public AudioClip[] daftarSFX;

    [Header("Klip Suara Efek Spesifik")]
    public AudioClip suaraGeserKartu;

    [Header("Bank Suara Karakter (Mumbling)")]
    [Tooltip("Masukkan semua file suara (Pejabat, Petani, Nelayan, dll) ke dalam list ini")]
    public AudioClip[] semuaSuaraKarakter;

    private AudioClip suaraDisiapkanSaatIni;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Opsional: Buat AudioManager tidak hancur saat pindah scene
             DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ==========================================
    // BAGIAN BGM (BACKGROUND MUSIC)
    // ==========================================

    // Memainkan BGM berdasarkan Indeks dari daftarBGM
    public void MainkanBGM(int indeksBGM)
    {
        if (daftarBGM == null || daftarBGM.Length == 0) return;

        // Cek apakah indeks valid
        if (indeksBGM >= 0 && indeksBGM < daftarBGM.Length)
        {
            MemutarKlipBGM(daftarBGM[indeksBGM]);
        }
        else
        {
            Debug.LogWarning("Indeks BGM tidak valid: " + indeksBGM);
        }
    }

    // Memainkan BGM berdasarkan Nama File
    public void MainkanBGM(string namaBGM)
    {
        if (daftarBGM == null || daftarBGM.Length == 0) return;

        foreach (AudioClip klip in daftarBGM)
        {
            if (klip.name == namaBGM)
            {
                MemutarKlipBGM(klip);
                return; // Berhenti mencari setelah ketemu
            }
        }
        Debug.LogWarning("BGM dengan nama " + namaBGM + " tidak ditemukan!");
    }

    // Fungsi internal untuk memutar BGM
    private void MemutarKlipBGM(AudioClip klipBGM)
    {
        if (audioSourceBGM == null || klipBGM == null) return;

        // Cegah lagu yang sama di-restart jika sudah bermain
        if (audioSourceBGM.clip == klipBGM && audioSourceBGM.isPlaying) return;

        audioSourceBGM.clip = klipBGM;
        audioSourceBGM.loop = true; // BGM biasanya diputar berulang
        audioSourceBGM.Play();
    }

    // Menghentikan BGM
    public void HentikanBGM()
    {
        if (audioSourceBGM != null && audioSourceBGM.isPlaying)
        {
            audioSourceBGM.Stop();
        }
    }

    // ==========================================
    // BAGIAN SFX (SOUND EFFECTS) UMUM
    // ==========================================

    // Memainkan SFX berdasarkan Indeks dari daftarSFX
    public void MainkanSFX(int indeksSFX)
    {
        if (daftarSFX == null || daftarSFX.Length == 0) return;

        if (indeksSFX >= 0 && indeksSFX < daftarSFX.Length)
        {
            MemutarKlipSFX(daftarSFX[indeksSFX]);
        }
        else
        {
            Debug.LogWarning("Indeks SFX tidak valid: " + indeksSFX);
        }
    }

    // Memainkan SFX berdasarkan Nama File
    public void MainkanSFX(string namaSFX)
    {
        if (daftarSFX == null || daftarSFX.Length == 0) return;

        foreach (AudioClip klip in daftarSFX)
        {
            if (klip.name == namaSFX)
            {
                MemutarKlipSFX(klip);
                return;
            }
        }
        Debug.LogWarning("SFX dengan nama " + namaSFX + " tidak ditemukan!");
    }

    // Memainkan Klip SFX secara langsung (jika Anda punya referensi AudioClip-nya)
    public void MemutarKlipSFX(AudioClip klipSFX)
    {
        if (audioSourceSFX != null && klipSFX != null)
        {
            audioSourceSFX.PlayOneShot(klipSFX);
        }
    }

    // Fungsi spesifik lama
    public void MainkanSuaraKertas()
    {
        if (audioSourceSFX != null && suaraGeserKartu != null)
        {
            audioSourceSFX.PlayOneShot(suaraGeserKartu);
        }
    }

    // ==========================================
    // BAGIAN SUARA KARAKTER (MUMBLING)
    // ==========================================

    public void SiapkanSuaraKarakterBaru()
    {
        if (semuaSuaraKarakter != null && semuaSuaraKarakter.Length > 0)
        {
            int randomIndex = Random.Range(0, semuaSuaraKarakter.Length);
            suaraDisiapkanSaatIni = semuaSuaraKarakter[randomIndex];
        }
    }

    public void MulaiBergumamDenganDelay()
    {
        if (audioSourceVoice == null || suaraDisiapkanSaatIni == null) return;

        audioSourceVoice.Stop();
        audioSourceVoice.clip = suaraDisiapkanSaatIni;
        audioSourceVoice.loop = false;
        audioSourceVoice.pitch = Random.Range(0.95f, 1.05f);

        audioSourceVoice.PlayDelayed(0.5f);
    }

    public void HentikanBergumam()
    {
        if (audioSourceVoice != null && audioSourceVoice.isPlaying)
        {
            audioSourceVoice.Stop();
        }
    }
}