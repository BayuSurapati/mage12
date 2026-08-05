using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Referensi Modul")]
    public StatsManager statsManager;

    [Header("Referensi UI")]
    public Slider sliderBencana;
    public Slider sliderKeuangan;
    public Slider sliderEkosistem;
    public Slider sliderTeknologi;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateProgressBars()
    {
        if ((statsManager == null))
        {
            return;
        }

        sliderBencana.value = statsManager.statBencana;
        sliderKeuangan.value = statsManager.statKeuangan;
        sliderEkosistem.value = statsManager.statEkosistem;
        sliderTeknologi.value = statsManager.statTeknologi;
    }
}
