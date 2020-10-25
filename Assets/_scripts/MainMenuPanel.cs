using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Analytics;

public class MainMenuPanel : MonoBehaviour {

    RectTransform rect;
    
    void Awake()
    {
        rect = gameObject.GetComponentInChildren<RectTransform>();
        rect.localScale = Vector3.zero;
    }

    public void ClickExit()
    {
        PlayButtonClick();
        SendAnalytics("StoreMainMenuExit");
        Application.Quit();
        
    }

    public void ClickFlowers()
    {
        PlayButtonClick();
        SendAnalytics("StoreMainMenuLostAndFound");
        StartCoroutine("LoadLevel", "trinkets");
        
        
    }

    public void ClickStory()
    {
        PlayButtonClick();
        SendAnalytics("StoreMainMenuStory");
        StartCoroutine("LoadLevel", "chapters");
       
    }

    public void ClickHelp()
    {
        PlayButtonClick();
        SendAnalytics("StoreMainMenuHelp");
        GameObject help = GameObject.FindGameObjectWithTag("HelpPanel");
        help.GetComponentInChildren<RectTransform>().localScale = Vector3.one;
        help.GetComponentInChildren<HelpPanel>().ScrollToTop();
        
    }

    public void ClickNews()
    {
        PlayButtonClick();
        SendAnalytics("StoreMainMenuNews");
        Application.OpenURL("http://www.lakehomegames.com/");
    }

    public void ClickSettings()
    {
        PlayButtonClick();
        SendAnalytics("StoreMainMenuSettings");
        StartCoroutine("LoadLevel", "settings");
    }

    public void ClickShop()
    {
        if(State.GetAgeFifteenOrOlder())
        {
            PlayButtonClick();
            SendAnalytics("StoreMainMenuStore");
            StartCoroutine("LoadLevel", "store");
        }
    }

    public void ClickSounds(bool enabled)
    {
        PlayButtonClick();
        ActorAudioManager audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponentInChildren<ActorAudioManager>();
        audioManager.SetMusicOn(!enabled);
        audioManager.SetSoundsOn(!enabled);
    }

    public void ClickYards()
    {
        PlayButtonClick();
        SendAnalytics("StoreMainMenuYards");
        State.gameSource = GameSource.remote;
        State.gameState = GameState.boards;
        StartCoroutine("LoadLevel", "boards");
    }

    public void ClickClose()
    {
        PlayButtonClick();
        SendAnalytics("StoreMainMenuCloseMainMenu");
        RectTransform menuButton = GameObject.FindGameObjectWithTag("MainMenuButton").GetComponentInChildren<RectTransform>();
        menuButton.localScale = Vector3.one;
        rect.localScale = Vector3.zero;
    }

    public void ClickCredits()
    {
        PlayButtonClick();
        SendAnalytics("StoreMainMenuCredits");
        StartCoroutine("LoadLevel", "credits");
    }

    private void PlayButtonClick()
    {
        GameObject.FindGameObjectWithTag("AudioManager").GetComponentInChildren<ActorAudioManager>().playButtonClick();
    }

    private void SendAnalytics(string parmName)
    {
        if (SceneManager.GetActiveScene().name == "store")
        {
            // Send Analytics Event
            Analytics.CustomEvent("Funnel", new Dictionary<string, object>
            {
                { State.GetBoardDisplayName(), 1},
                {parmName,1 }
            });
        }
    }

    IEnumerator LoadLevel(string levelName)
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(levelName);
    }


}
