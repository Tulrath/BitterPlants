using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScreenCredits : MonoBehaviour {

    private ActorAudioManager audioManager;

    void Start()
    {

        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponentInChildren<ActorAudioManager>();
        audioManager.playScreenLoaded();
        Language.TranslateScreen();
        State.gameState = GameState.credits;
    }
    
    public void ClickBack()
    {
        State.gameState = GameState.boards;
        StartCoroutine("LoadLevel", "boards");
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
