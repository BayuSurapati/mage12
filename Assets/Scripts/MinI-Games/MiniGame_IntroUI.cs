using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class MiniGame_IntroUI : MonoBehaviour
{
    public MiniGameManager miniGameManager;

    [Header("Referensi UI Teks")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;
    public GameObject panelContainer;

    private CardData currentCardData;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowIntro(CardData cardData)
    {
        currentCardData = cardData;
        titleText.text = cardData.introMiniGame;
        descText.text = cardData.deskripsiMiniGame;
        gameObject.SetActive(true);

        StartCoroutine(AnimatePopUp(Vector3.zero, Vector3.one));
    }

    public void OnStartMiniGameButtonClicked()
    {
        gameObject.SetActive(false);
        miniGameManager.StartMiniGame();
    }

    private IEnumerator AnimatePopUp(Vector3 startScale, Vector3 endScale)
    {
        float time = 0;
        float duration = 0.5f; // Durasi animasi dalam detik
        panelContainer.transform.localScale = startScale;

        while(time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            panelContainer.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }
        panelContainer.transform.localScale = endScale;
    }
}
