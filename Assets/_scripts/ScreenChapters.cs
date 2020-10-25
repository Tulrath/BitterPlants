using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class ScreenChapters : MonoBehaviour {

    private ActorAudioManager audioManager;
    private const string PreferredFontSizeKey = "BitterPlants_PreferredFontSize";
    private Text StoryText;
    private const string StoryTextTag = "StoryText";
    private GameObject go;
    private const int defaultFontSize = 24;

    // Use this for initialization
    void Start () {

        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponentInChildren<ActorAudioManager>();
        audioManager.playScreenLoaded();
        Language.TranslateScreen();
        State.gameState = GameState.chapters;

        StartCoroutine("ShowBeginnerBoards");

        go = GameObject.FindGameObjectWithTag(StoryTextTag);
        if(go)
            StoryText = go.GetComponentInChildren<Text>();
        
        SetFontSize(PlayerPrefs.HasKey(PreferredFontSizeKey) ? PlayerPrefs.GetInt(PreferredFontSizeKey) : defaultFontSize);
    }

    public void ClickBack()
    {
        State.gameState = GameState.boards;
        StartCoroutine("LoadLevel", "boards");
    }

    public void CloseStoryView()
    {
        ActorGameManager.ShowGroup("StoryScrollView", false);
    }

    public void PlayButtonClick()
    {
        audioManager.playButtonClick();
    }

    public void TextSizeChange(bool sizeUp)
    {
        if(StoryText != null)
            SetFontSize(StoryText.fontSize + (sizeUp ? 1 : -1));
    }

    private void SetFontSize(int fontSize)
    {
        if (StoryText != null)
        {
            StoryText.fontSize = fontSize;
            PlayerPrefs.SetInt(PreferredFontSizeKey, fontSize);
        }
    }

    IEnumerator LoadLevel(string sceneName)
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator ShowBeginnerBoards()
    {
        yield return null;
        Debug.LogWarning("Showing Story Boards for Chapters List");
        State.gameBoardCategory = GameBoardCategory.campaign;
        State.gameBoardMine = false;
        State.gameSource = GameSource.local;
        State.gameActivity = GameActivity.play;
        Database.GetLocalBoards();
    }

}
