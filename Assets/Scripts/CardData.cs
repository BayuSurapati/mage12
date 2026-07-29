using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "KartuBaru", menuName = "Data Kartu")]
public class CardData : ScriptableObject
{
    [Header("Identitas Kartu")]
    public string cardID;
    [TextArea(3, 5)]
    public string narasiCerita;

    [Tooltip("Centang jika ini adalah kartu Mini-Games")]
    public bool isMiniGameCard;

    [Header("Efek Swipe Kiri (Tolak)")]
    public int efekKiriBencana;
    public int efekKiriEkosistem;
    public int efekKiriKeuangan;
    public int efekKiriTeknologi;

    [Header("Efek Swipe Kanan (Setuju)")]
    public int efekKananBencana;
    public int efekKananEkosistem;
    public int efekKananKeuangan;
    public int efekKananTeknologi;
}
