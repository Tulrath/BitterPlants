using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Analytics;
using UnityEngine.Networking;

public class ScreenPlay : MonoBehaviour
{
    private ActorAudioManager audioManager;
    public ScrollRect helpScrollView;
    private const string editModePanelName = "editModePanel";
    private const string testModePanelName = "testModePanel";
    private const string playModeControlsPanelName = "playModeControlsPanel";
    private const string helpPanelName = "helpPanel";
    private const string dialogPanel = "dialogPanel";
    private const string nextBoardButton = "nextBoardButton";
    private const string saveBoardPanelName = "saveBoardPanel";
    private const string voteForBoardButton = "voteForBoardButton";
    private const string twitterButton = "twitterButton";
    private const string facebookButton = "facebookButton";
    private const string getHintPanel = "getHintPanel";
    private const string storyUnlockPanel = "storyUnlockPanel";
    private const string boardNameText = "boardNameText";
    private long boardNumber = 0;

    public void CancelHint()
    {
        ActorGameManager.ShowGroup(getHintPanel, false);

        // Send Analytics Event
        Analytics.CustomEvent("Funnel", new Dictionary<string, object>
            {
                { State.GetBoardDisplayName(), 1},
                {"HintCancelStore",1 }
            });
    }

    public void BuyGems()
    {
        ActorGameManager.ShowGroup(getHintPanel, false);
        StartCoroutine("LoadLevel", "store");

        // Send Analytics Event
        Analytics.CustomEvent("Funnel", new Dictionary<string, object>
            {
                { State.GetBoardDisplayName(), 1},
                {"HintOpenStore",1 }
            });
    }
    
    public void EnterEditMode()
    {
        ActorGameManager.ShowGroup(testModePanelName, false);
        ActorGameManager.ShowGroup(playModeControlsPanelName, false);
        ActorGameManager.ShowGroup(editModePanelName, true);
        ActorGridManager.EnterEditMode();
        ActorGameManager.EnterEditMode();
        ActorGameManager.ShowGroup(helpPanelName, false);
    }

    public void EnterTestMode()
    {
        ActorGameManager.ShowGroup(testModePanelName, true);
        ActorGameManager.ShowGroup(playModeControlsPanelName, false);
        ActorGameManager.ShowGroup(editModePanelName, false);
        ActorGridManager.EnterTestMode();
        ActorGameManager.EnterTestMode();
        ActorGameManager.ShowGroup(helpPanelName, false);
    }

    public void GetHint()
    {
        if(Identity.GetGems() < 1)
        {
            ActorGameManager.ShowGroup(getHintPanel, true);

            // Send Analytics Event
            Analytics.CustomEvent("Funnel", new Dictionary<string, object>
            {
                { State.GetBoardDisplayName(), 1},
                {"HintOutOfGems",1 }
            });
        }
        else
        {
            ActorGameManager.ShowHint();
        }
    }

    public void GoBoardSelect()
    {
        // revert to play mode
        Debug.LogWarning("ScreenPlay.GoBoardSelect setting gameActivity to play");
        State.gameActivity = GameActivity.play;
        State.gameState = GameState.boards;
        StartCoroutine("LoadLevel", "boards");
    }

    public void GoNextBoard()
    {
        ActorGameManager.ShowGroup(nextBoardButton, false);
        ActorGameManager.GoNextBoard();
        StartCoroutine("LoadLevel", "play");
    }

    public void HideHelp()
    {
        ActorGameManager.ShowGroup(helpPanelName, false);
    }

    public void HideSaveBoard()
    {
        ActorGameManager.ShowGroup(saveBoardPanelName, false);
        ActorGameManager.ShowGroup(helpPanelName, false);
    }

    public void HideFoundTrinketPanel()
    {
        ActorGameManager.ShowGroup(ActorGameManager.foundTrinketPanel, false);
    }

    private IEnumerator LoadLevel(string sceneName)
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(sceneName);
    }

    public void PlayButtonClick()
    {
        audioManager.playButtonClick();
    }

    public void PlayShake()
    {
        audioManager.playShake();
    }


    public void PrintCurrentBoard()
    {
        // defaults to current game mode
        ActorGameManager.PrintCurrentBoard();
    }

    public void ResetBoard()
    {
        Database.AddBoardMetric(metricType.play);
        ActorGameManager.ShowGroup(helpPanelName, false);
        ActorGameManager.ShowGroup(dialogPanel, false);
        ActorGameManager.ResetBoard();
    }

    public void ScrollToTop(ScrollRect scroller)
    {
        scroller.verticalNormalizedPosition = 1f;
    }

    public void SetSeedColor(int id)
    {
        ActorGameManager.SetSeedColor(id);
    }

    public void ShowHelp()
    {
        ActorGameManager.ShowGroup(helpPanelName, true);
        ScrollToTop(helpScrollView);
    }

    public void ShowSaveBoard()
    {
        ActorGameManager.ShowGroup(saveBoardPanelName, true);
        ActorGameManager.ShowGroup(helpPanelName, false);
    }

    public void SocialTwitter()
    {
        
        string url = "https://twitter.com/intent/tweet?text=" + UnityWebRequest.EscapeURL("I solved " + State.GetBoardDisplayName() + " using just " + 
            ActorGameManager.GetScore(ActorGameManager.seedsPlanted) + " seeds - can you beat my Bitter Plants score?") +
            "&url=" + UnityWebRequest.EscapeURL("https://www.lakehomegames.com/games/bitterplants");
        Application.OpenURL(url);
    }

    public void SocialFacebook()
    {
        string url = "http://www.facebook.com/sharer.php?&p[url]=http://www.bitterplants.com";
        Application.OpenURL(url);
    }

    private void Start()
    {

        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponentInChildren<ActorAudioManager>();
        audioManager.playScreenLoaded();
        
        // only show voting button when the board is not mine
        ActorGameManager.ShowGroup(voteForBoardButton, !State.gameBoardMine);

        // update sub-panels based on activity
        UpdateSubPanels();

        // only show the social buttons when the player is age 15 or older
        ActorGameManager.ShowGroup(twitterButton, State.GetAgeFifteenOrOlder());
        ActorGameManager.ShowGroup(facebookButton, State.GetAgeFifteenOrOlder());

        Language.TranslateScreen();

        // set board name
        GameObject.Find(boardNameText).GetComponent<Text>().text = State.GetBoardDisplayName();

        // cache board number
        boardNumber = State.GetBoardNumber();

    }

    public void StoryLater()
    {
        ActorGameManager.ShowGroup(storyUnlockPanel, false);
    }


    public void StoryNow()
    {
        ActorGameManager.ShowGroup(storyUnlockPanel, false);
        StartCoroutine("LoadLevel", "chapters");
    }

    
    
    public void UpdateSubPanels()
    {
        // show sub-panels based on activity
        ActorGameManager.ShowGroup(playModeControlsPanelName, State.gameActivity == GameActivity.play);
        ActorGameManager.ShowGroup(editModePanelName, State.gameActivity == GameActivity.edit);
        ActorGameManager.ShowGroup(testModePanelName, State.gameActivity == GameActivity.test);
    }
    
    public void UndoTurn()
    {
        // only charge gems when in normal play mode (not when in edit or test)
        if (Identity.GetGems() < 1 && State.gameActivity == GameActivity.play)
        {
            ActorGameManager.ShowGroup(getHintPanel, true);
        }
        else
        {
            ActorGameManager.UndoTurn();
        }
    }

    public void VoteForBoard()
    {
        ActorGameManager.ShowGroup(voteForBoardButton, false);
        Database.AddBoardMetric(metricType.vote);
    }
 
}