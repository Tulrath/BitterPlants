using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ActorBoardListManager : MonoBehaviour {

    
    public GameObject boardButtonWide;
    public GameObject unlockTierButton;
    public RectTransform viewportContent;
    private float boardButtonSpace = 150f;
    private const string boardButtonTag = "BoardButton";
    private const string tierButtonTag = "UnlockTierButton";
    private const string yardButtonsPanelTag = "YardButtonsPanel";
    private int tierButtonCount;
    private const float boardButtonTopPadding = 64f;
    private const float boardButtonLeftPadding = 8f;
    public ScrollRect scrollViewRect;
    public const string boardListScrollViewTag = "BoardListScrollView";
    private const string firstBoardName = "001alcove";

    private bool Boardlocked(string boardName)
    {
        return boardName == firstBoardName ? false : Identity.BoardLocked(boardName);
    }

    private int BoardStars(string boardName)
    {
        return boardName == firstBoardName ? 6 : Identity.BoardStars(boardName);
    }

    private void BuildBoardButton(string boardName, int buttonID)
    {
        bool userBoard = boardName.Contains(Database.delimiter);

        Debug.Log("Building board button " + buttonID + " with name " + boardName);

        // set GameObject properties
        GameObject newButton = Instantiate(boardButtonWide);
        newButton.name = boardName;
        BoardButton button = newButton.GetComponentInChildren<BoardButton>();
        RectTransform trans = newButton.GetComponentInChildren<RectTransform>();

        // set position, scale
        trans.SetParent(viewportContent, false);
        trans.anchoredPosition = new Vector2(boardButtonLeftPadding, -boardButtonTopPadding + (buttonID + tierButtonCount) * -boardButtonSpace);
        trans.localScale = Vector3.one;

        // set the boardname (data) and the display name
        string ownerUserID = "";
        string boardID = "";
        string displayName = "";

        // get the board's display name and update it for the button
        displayName = State.GetBoardDisplayName(boardName);
        button.SetDisplayName(displayName);

        if (userBoard)
        {
            // this is a userboard
            string[] temp = boardName.Split(Database.delimiter[0]);
            ownerUserID = temp[0] + Database.delimiter + temp[1]; // 0 is region, 1 is identityID
            boardID = temp[2];
            displayName = temp[3];
            
            if (temp.Length > 4)
            {
                if (temp[4] != "")
                    button.SetStats(metricType.vote, temp[4]);
                if (temp[5] != "")
                    button.SetStats(metricType.play, temp[5]);
                if (temp[6] != "")
                    button.SetStats(metricType.win, temp[6]);
            }


            boardName = boardID + Database.delimiter + displayName;
        }

        
        // update the button's board values
        button.SetBoardID(boardName);
        button.SetBoardIdentityID(ownerUserID);
        
        // set lock state and boardname for button
        // remote boards are never locked
        button.SetLocked(State.gameSource == GameSource.remote ? false : Boardlocked(boardName));

        // removed stars (for now)

        // set the boards star count
        //if(State.gameSource == GameSource.local)
        //    button.SetStars(BoardStars(boardName));

        // removed this to remove ads

        // build the unlock tier button only when we are getting local boards (story boards)
        // also, do not build the tier button for players who have self-identified
        // as being under 15 years of age (don't want to show ads to kids)
        //if ((buttonID+2) % 4 == 0 && State.gameSource == GameSource.local && State.gameConnected && State.GetAgeFifteenOrOlder())
        //{
        //    Debug.Log("Building Tier Button");
        //    tierButtonCount++;
        //    GameObject tierButton = Instantiate(unlockTierButton);
        //    tierButton.name = "UnlockTier" + buttonID.ToString() + "Button";
        //    RectTransform tierTrans = tierButton.GetComponentInChildren<RectTransform>();
        //    tierTrans.SetParent(viewportContent, false);
        //    tierTrans.anchoredPosition = new Vector2(boardButtonLeftPadding, -boardButtonTopPadding + (buttonID + tierButtonCount) * -boardButtonSpace);
        //    trans.localScale = Vector3.one;

        //    UnlockTierButton unlock = tierButton.GetComponentInChildren<UnlockTierButton>();
        //    unlock.SetAdTierUnlocks(buttonID);
        //}
    }

    private IEnumerator BuildBoardButtons(string[] boardNames)
    {

        State.TaskDone("GetBoardList");
        State.TaskAdd("GetBoardList");
        ClearBoardButtons();

        int i = 0;
        // if we are getting the list of boards that the player owns, then build a NEW button
        if (State.gameBoardCategory == GameBoardCategory.mine)
        {
            Debug.Log("Building NEW board button");
            BuildBoardButton("999new", i);
            yield return new WaitForSeconds(0.1f);
            i++;

        }

        // adjust the viewport size
        viewportContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (boardButtonSpace * (2f + (boardNames.Length * 1.25f))));

        ScrollToTop();

        // build a button for each board
        if (boardNames.Length > 0)
        {
            foreach (string b in boardNames)
            {
                BuildBoardButton(b, i);
                yield return new WaitForSeconds(0.1f);
                i++;
            }
        }

        State.TaskDone("GetBoardList");

    }

    private void ClearBoardButtons()
    {
        GameObject[] buttons = GameObject.FindGameObjectsWithTag(boardButtonTag);
        foreach(GameObject b in buttons)
        {
            Destroy(b);
        }

        buttons = GameObject.FindGameObjectsWithTag(tierButtonTag);
        foreach (GameObject b in buttons)
        {
            Destroy(b);
        }

        tierButtonCount = 0;
    }

    private void FindScrollViewRect()
    {
        if (scrollViewRect == null)
        {
            GameObject go;
            go = GameObject.FindGameObjectWithTag(boardListScrollViewTag);
            if (go == null)
                go = GameObject.Find(boardListScrollViewTag);
            if (go)
                scrollViewRect = go.GetComponentInChildren<ScrollRect>() as ScrollRect;
            else
                Debug.LogError("BoardListScrollViewTag could not be found");

            if (scrollViewRect == null)
                Debug.LogError("ScrollRect for BoardListScrollViewTag not found");
        }
    }

    private void ScrollToTop()
    {
        FindScrollViewRect();
        scrollViewRect.verticalNormalizedPosition = 1f;
    }

    void Start()
    {
        State.gameBoardID = "";
        State.gameBoardIdentityID = "";
        FindScrollViewRect();
    }

    public void UnlockBoard(string boardName, int stars)
    {

        Identity.UnlockBoard(boardName, stars);
    }

    void Update()
    {
        
        if(State.gameState == GameState.boardList)
        {
            // the board list has been returned
            // so the board buttons can be built now
            // set the state back to boards to prevent re-building
            State.gameState = GameState.boards;
            Debug.LogWarning("Boardlist returned, building board buttons...");
            ActorGameManager.ShowGroup(yardButtonsPanelTag, State.gameSource == GameSource.remote);
            StartCoroutine("BuildBoardButtons", State.gameBoardList);
        }
    }
}
