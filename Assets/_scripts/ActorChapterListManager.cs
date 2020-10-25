using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ActorChapterListManager : MonoBehaviour {

    public GameObject chapterButtonWide;
    private const string chapterButtonTag = "ChapterButton";
    public const string chapterListScrollViewTag = "ChapterListScrollView";
    public const string storyScrollViewTag = "StoryScrollView";
    public ScrollRect chaptersScrollViewRect;
    public ScrollRect storyScrollViewRect;
    public Text storyText;
    public RectTransform viewportContent;
    private const float chapterButtonTopPadding = 64f;
    private const float chapterButtonLeftPadding = 64f;
    private float chapterButtonSpace = 150f;

    private bool BoardLocked(string boardName)
    {
        return Identity.BoardLocked(boardName);
    }

    public void BuildChapterButton(string boardName, int buttonID)
    {
        Debug.Log("Building chapter button " + buttonID + " for board " + boardName);

        GameObject newButton = Instantiate(chapterButtonWide);
        newButton.name = "ChapterButton"+buttonID.ToString();
        ChapterButton button = newButton.GetComponentInChildren<ChapterButton>();
        RectTransform trans = newButton.GetComponentInChildren<RectTransform>();

        // set position, scale
        trans.SetParent(viewportContent, false);
        trans.anchoredPosition = new Vector2(chapterButtonLeftPadding, -chapterButtonTopPadding + (buttonID -1) * -chapterButtonSpace);
        trans.localScale = Vector3.one;

        // update the button's chapter value
        button.SetChapterID("chapter" + buttonID.ToString());
        
        button.SetLocked(boardName == "Epilogue" ? BoardLocked("071nurture") :  BoardLocked(boardName));
        
        button.SetDisplayName(boardName == "Epilogue" ? "Epilogue" : "Chapter " + buttonID.ToString());

    }

    public void BuildChapterButtons(string[] boardNames)
    {
        ClearChapterButtons();

        int i = 0;
        
        // build a button for each board
        if (boardNames.Length > 0)
        {
            foreach (string b in boardNames)
            {
                if(i%4 == 2)
                {
                    BuildChapterButton(b, (i/4)+1);
                }
                i++;
            }
        }

        BuildChapterButton("Epilogue", 19);

        StartCoroutine("ScrollToTop");

    }

    private void ClearChapterButtons()
    {
        GameObject[] buttons = GameObject.FindGameObjectsWithTag(chapterButtonTag);
        foreach (GameObject b in buttons)
        {
            Destroy(b);
        }
    }

    private void FindScrollViewRects()
    {
        if (chaptersScrollViewRect == null)
        {
            GameObject go;
            go = GameObject.FindGameObjectWithTag(chapterListScrollViewTag);
            if (go == null)
                go = GameObject.Find(chapterListScrollViewTag);
            if (go)
                chaptersScrollViewRect = go.GetComponentInChildren<ScrollRect>() as ScrollRect;
            else
                Debug.LogError("ChapterListScrollViewTag could not be found");

            if (chaptersScrollViewRect == null)
                Debug.LogError("ScrollRect for ChapterListScrollViewTag not found");
        }

        if (storyScrollViewRect == null)
        {
            GameObject go;
            go = GameObject.FindGameObjectWithTag(storyScrollViewTag);
            if (go == null)
                go = GameObject.Find(storyScrollViewTag);
            if (go)
                storyScrollViewRect = go.GetComponentInChildren<ScrollRect>() as ScrollRect;
            else
                Debug.LogError("storyScrollViewTag could not be found");

            if (storyScrollViewRect == null)
                Debug.LogError("ScrollRect for storyScrollViewTag not found");
        }
    }
    
   

    public void ShowChapter(string chapterName)
    {
        ActorGameManager.ShowGroup("StoryScrollView", true);
        storyText.text = Database.GetChapterText(chapterName);
        StartCoroutine("ScrollToTop");
    }

    void Start()
    {
        FindScrollViewRects();
    }


    void Update () {

        if (State.gameState == GameState.boardList)
        {
            State.gameState = GameState.boards;
            Debug.LogWarning("Boardlist returned, building chapter buttons...");

            BuildChapterButtons(State.gameBoardList);
        }

    }


    IEnumerator ScrollToTop()
    {
        yield return new WaitForSeconds(0.1f);
        FindScrollViewRects();
        chaptersScrollViewRect.verticalNormalizedPosition = 1f;
        storyScrollViewRect.verticalNormalizedPosition = 1f;
    }

}
