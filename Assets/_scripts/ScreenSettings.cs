using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ScreenSettings : MonoBehaviour {

    private ActorAudioManager audioManager;
    
    
    public void Start()
    {
    
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponentInChildren<ActorAudioManager>();
        audioManager.playScreenLoaded();

        GameObject.Find("MusicVolumeSlider").GetComponent<Slider>().value = State.GetMusicVolume();
        GameObject.Find("EffectsVolumeSlider").GetComponent<Slider>().value = State.GetSoundEffectsVolume(); 

        Language.TranslateScreen();
        State.gameState = GameState.settings;
        
    }

    public void ClickBack()
    {
        State.gameState = GameState.boards;
        StartCoroutine("LoadLevel", "boards");
    }
    
    public void SelectLanguage(string languageName)
    {
        State.SetLanguageKey(languageName);   
    }
    
    public void ScrollToTop(ScrollRect scroller)
    {
        scroller.verticalNormalizedPosition = 1f;
    }

    public void MusicOnValueChanged(float volume)
    {
        audioManager.SetMusicVolume(volume);
    }

    public void EffectsOnValueChanged(float volume)
    {
        audioManager.SetSoundEffectsVolume(volume);
    }

    public void ResetGame()
    {
        
        Identity.ClearLocalIdentity();
        Application.Quit();

    }

    public void PlayButtonClick()
    {
        audioManager.playButtonClick();
    }

    IEnumerator LoadLevel(string sceneName)
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(sceneName);
    }

   
}
