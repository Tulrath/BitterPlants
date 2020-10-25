using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Analytics;
using System.Collections.Generic;


public class MainMenuButton : MonoBehaviour {

    RectTransform rect;
    
    void Awake()
    {
        rect = gameObject.GetComponentInChildren<RectTransform>();
        rect.localScale = Vector3.one;
    }

    public void OnClick()
    {
        GameObject.FindGameObjectWithTag("AudioManager").GetComponentInChildren<ActorAudioManager>().playButtonClick();

        RectTransform menu = GameObject.FindGameObjectWithTag("MainMenuPanel").GetComponentInChildren<RectTransform>();
        menu.localScale = Vector3.one;
        rect.localScale = Vector3.zero;

        if(SceneManager.GetActiveScene().name == "store")
        {
            // Send Analytics Event
            Analytics.CustomEvent("Funnel", new Dictionary<string, object>
            {
                { State.GetBoardDisplayName(), 1},
                {"StoreMainMenu",1 }
            });
        }
        
    }
}
