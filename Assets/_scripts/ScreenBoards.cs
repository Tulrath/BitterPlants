using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

public class ScreenBoards : MonoBehaviour {

    private ActorAudioManager audioManager;
    public ToggleGroup boardToggleGroup;
    public Toggle beginnerToggle;
    public Toggle myToggle;
    public Toggle newToggle;
    public Toggle popularToggle;

    private const string pageUpButton = "pageUpButton";
    private const string pageDownButton = "pageDownButton";

    public void PlayButtonClick()
    {
        if(audioManager != null)
            audioManager.playButtonClick();
    }

    void Start()
    {
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponentInChildren<ActorAudioManager>();
        audioManager.playScreenLoaded();
        State.gameBoardMine = true;
        Language.TranslateScreen();
        Debug.LogFormat("Current State Gameboard Category:{0}", State.gameBoardCategory);
        switch (State.gameBoardCategory)
        {
            case GameBoardCategory.campaign:
                beginnerToggle.isOn = true;
                break;
            case GameBoardCategory.mine:
                myToggle.isOn = true;
                break;
            case GameBoardCategory.newest:
                newToggle.isOn = true;
                break;
            case GameBoardCategory.popular:
                popularToggle.isOn = true;
                break;
        }
        YardListUpdate(true);

    }

    IEnumerator LoadLevel(string sceneName)
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(sceneName);
    }
    

    public void YardListUpdate(bool toggleOn)
    {
        if(toggleOn)
        {
            if(boardToggleGroup.AnyTogglesOn())
            {
                State.TaskAdd("GetBoardList");
                switch(boardToggleGroup.ActiveToggles().First().gameObject.name)
                {
                    case "MyYardsButton":
                        StartCoroutine("ShowMyBoards");
                        break;
                    case "NewYardsButton":
                        StartCoroutine("ShowNewBoards");
                        break;
                    case "PopularButton":
                        StartCoroutine("ShowPopularBoards");
                        break;
                    case "BeginnerButton":
                        StartCoroutine("ShowBeginnerBoards");
                        break;
                    default:
                        StartCoroutine("ShowBeginnerBoards");
                        break;

                }
            }
        }
    }

    public void PageUp()
    {
        if(!Database.AtFirstPage())
        {
            Database.PageUp();
            PageDown();
        } 
    }

    public void PageDown()
    {
        if(!Database.AtLastPage())
        {
            switch (State.gameBoardCategory)
            {
                case GameBoardCategory.newest:
                    Database.GetNewBoards(false);
                    break;
                case GameBoardCategory.popular:
                    Database.GetPopularBoards(false);
                    break;
            }
        }
    }

   

    IEnumerator ShowMyBoards()
    {
        yield return null;
        State.gameBoardCategory = GameBoardCategory.mine;
        State.gameBoardMine = true;
        State.gameSource = GameSource.remote;
        State.gameActivity = GameActivity.play;
        ActorGameManager.ShowGroup(pageUpButton, false);
        ActorGameManager.ShowGroup(pageDownButton, false);
        Database.GetMyBoards();
    }

    IEnumerator ShowNewBoards()
    {
        yield return null;
        State.gameBoardCategory = GameBoardCategory.newest;
        State.gameBoardMine = false;
        State.gameSource = GameSource.remote;
        State.gameActivity = GameActivity.play;
        ActorGameManager.ShowGroup(pageUpButton, true);
        ActorGameManager.ShowGroup(pageDownButton, true);
        Database.GetNewBoards(true);
    }

    IEnumerator ShowPopularBoards()
    {
        yield return null;
        State.gameBoardCategory = GameBoardCategory.popular;
        State.gameBoardMine = false;
        State.gameSource = GameSource.remote;
        State.gameActivity = GameActivity.play;
        ActorGameManager.ShowGroup(pageUpButton, true);
        ActorGameManager.ShowGroup(pageDownButton, true);
        Database.GetPopularBoards(true);
    }

    IEnumerator ShowBeginnerBoards()
    {
        yield return null;
        Debug.LogWarning("Showing Story Boards for BoardList");
        State.gameBoardCategory = GameBoardCategory.campaign;
        State.gameBoardMine = false;
        State.gameSource = GameSource.local;
        State.gameActivity = GameActivity.play;
        ActorGameManager.ShowGroup(pageUpButton, false);
        ActorGameManager.ShowGroup(pageDownButton, false);
        Database.GetLocalBoards();
    }

    public void ResetLocalStars()
    {
        Identity.ResetBoardStars();
    }
    
}
