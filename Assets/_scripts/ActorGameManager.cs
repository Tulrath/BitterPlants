using FullSerializer;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Analytics;

public enum ColorID
{
    white,
    blue,
    red,
    yellow,
    purple, // bomb
    length
}

public class ActorGameManager : MonoBehaviour
{

    public const string flowerSelector = "flowerSelector";
    public const string greenPetals = "greenPetals";
    public const string orangePetals = "orangePetals";
    public const string petalCount = "petalCount";
    public const string petalExplosions = "petalExplosions";
    public const string purplePetals = "purplePetals";
    public const string seedsPlanted = "seedsPlanted";
    public const string seedsRemaining = "seedsRemaining";
    public const string blueSelectButton = "blueSelectButton";
    public const string redSelectButton = "redSelectButton";
    public const string yellowSelectButton = "yellowSelectButton";
    public const string dialogPanel = "dialogPanel";
    public const string dialogText = "dialogText";
    public const string boardNameInput = "boardNameInputField";
    public const string saveBoardText = "languageSaveBoardText";
    public const string voteForBoardButton = "voteForBoardButton";
    public const string tryAgainButton = "tryAgainButton";
    public const string nextBoardButton = "nextBoardButton";
    public const string editMoreButton = "editMoreButton";
    public const string resetBoardButton = "resetBoardButton";
    public const string getHintButton = "getHintButton";
    public const string undoPlayButton = "undoPlayButton";
    public const string ratioText = "ratioText";
    public const string gemsRemaining = "gemsRemaining";
    public const string foundTrinketPanel = "foundTrinketPanel";
    public const string foundTrinketText = "foundTrinketText";
    public const string trinketImage = "trinketImage";
    private const string storyUnlockPanel = "storyUnlockPanel";

    public const string gameWonLog = "*** GAME WON ***";
    public const string gameLostLog = "*** GAME LOST ***";
    public const string gameForfeitLog = "*** GAME FORFEIT ***";
    
    public static bool boardBuilt = false;
    
    // cached instance references
    public static ActorAudioManager audioManager;
    public static ActorGridManager gridManager;
    public static ActorTrinketManager trinketManager;
    

    // GUI references to cache
    public static RectTransform flowerSelectorObject;
    public static Image flowerSelectorImage;
    public static Text petalCountText;
    public static Text seedsRemainingText;
    public static Text dialogPanelText;
    public static Text saveBoardTextObject;
    public static RectTransform blueSelectTransform;
    public static RectTransform redSelectTransform;
    public static RectTransform yellowSelectTransform;
    public static InputField boardNameField;
    public static Text ratioTextObject;
    public static Text gemsRemainingTextObject;
    public static Text foundTrinketTextObject;
    public static Image trinketImageObject;
    
    // game data
    public static bool testMode = false;
    public static Queue<GameStep> seedsPlantedHistory = new Queue<GameStep>();
    private static readonly fsSerializer _serializer = new fsSerializer();
    private static Dictionary<string, Score> scoreboard = new Dictionary<string, Score>();
    public static ColorID currentPlayerColorID = ColorID.red;

    // utility
    private float flowerSelectorThrobPercent = 0;
    
    public static void AddScore(string scoreName, int scoreValue)
    {
        if (!scoreboard.ContainsKey(scoreName))
            scoreboard.Add(scoreName, new Score());

        scoreboard[scoreName].scoreCurrent = scoreboard[scoreName].scoreCurrent + scoreValue;

    }
    
    private static void CheckFoundTrinket()
    {
        if (State.gameActivity != GameActivity.play)
            return;

        if (UnityEngine.Random.Range(0, 60) == 0)
        {
            // show found trinket panel
            int foundID = 0;
            foundID = UnityEngine.Random.Range(0, trinketManager.trinkets.Length);
            Trinket t = trinketManager.trinkets[foundID];
            foundTrinketTextObject.text = "You found " + t.displayName;
            trinketImageObject.sprite = t.sprite;
            ShowGroup(foundTrinketPanel, true);
            
            // record find
            Identity.IncrementCollectStars(t.sprite.name);

            // spawn effects
            SpawnRewardExplosions(6);

        }
    }

   
   

    public static void CheckWinLose()
    {
        if (DetectWin())
        {
            if (State.gameState != GameState.win)
            {
                Debug.Log(gameWonLog);
                State.gameState = GameState.win;
                Database.AddBoardMetric(metricType.win);
                if (audioManager)
                    audioManager.playWin();

                ShowGroup(resetBoardButton, false);
                ShowGroup(getHintButton, false);
                ShowGroup(undoPlayButton, false);

                // if I just won the last board in a tier, then show the story unlock message
                // we're re-using the trinket panel for this
                int buttonID;
                int.TryParse(State.gameBoardName.Substring(0,3), out buttonID);
                Debug.LogWarningFormat("WIN board {0} : {1} ",State.gameBoardName, buttonID);
                if ((buttonID + 1) % 4 == 0 && State.gameSource == GameSource.local)
                {
                    ShowGroup(storyUnlockPanel, true);
                    SpawnRewardExplosions(6);
                }
                
                if (State.gameMode == GameMode.artist)
                {
                    // set the stars for the current board
                    Identity.SetBoardStars(State.gameBoardName, Math.Max(scoreboard[seedsPlanted].scoreGoal - scoreboard[seedsPlanted].scoreCurrent, 0) + 1);

                    // and unlock the next board
                    UnlockNextBoard();

                }

                // Send Analytics Event
                Analytics.CustomEvent("YardWin", new Dictionary<string, object>
                    {
                        {State.GetBoardDisplayName(), 1}
                    });
            }
        }
        else
        {
            if (DetectLoss())
            {
                if (State.gameState != GameState.loss)
                {
                    Debug.Log(gameLostLog);
                    State.gameState = GameState.loss;
                    if (audioManager)
                        audioManager.playLoss();

                    ShowGroup(resetBoardButton, false);
                    ShowGroup(getHintButton, false);
                    ShowGroup(undoPlayButton, false);

                    // Send Analytics Event
                    Analytics.CustomEvent("YardLoss", new Dictionary<string, object>
                    {
                        {State.GetBoardDisplayName(), 1}
                    });
                }
            }
        }

        if(State.gameState != GameState.win && State.gameState != GameState.loss)
        {
            // check if a trinket was found but only if we didn't just win or lose the game
            CheckFoundTrinket();
        }
        
        UpdateDisplay();
    }

    public static void ClearBoardData()
    {
        Debug.Log("Clearing board data...");
        State.gameBoard = null;
        State.gameBoard = new Board();
    }

    public static void ClearHistory()
    {
        Debug.Log("Clearing session local seedsPlantedHistory...");
        seedsPlantedHistory.Clear();
    }

    public static void ClearScores()
    {
        Debug.Log("Clearing scores...");
        scoreboard.Clear();
    }
    
    public static bool DetectLoss()
    {
        bool loss = true;
        
        if (State.gameMode == GameMode.artist && (State.gameActivity == GameActivity.play || State.gameActivity == GameActivity.test))
        {
            // in artist/single mode, you only lose if you run out of seeds
            if (scoreboard.ContainsKey(seedsPlanted) && scoreboard[seedsPlanted].scoreCurrent < scoreboard[seedsPlanted].scoreGoal)
            {
                loss = false;
            }
            
        }
        else
        {
            loss = false;
        }
        
        return loss;
    }

    public static bool DetectWin()
    {
        bool win = false;

        // in artist mode, you only win if you meet the goals of all hexagons with goals
        if (State.gameMode == GameMode.artist && (State.gameActivity == GameActivity.play || State.gameActivity == GameActivity.test))
        {
            GameObject[] hexagons = GameObject.FindGameObjectsWithTag("Hexagon");
            int goalsDefined = 0;
            int goalsMet = 0;
            foreach (GameObject h in hexagons)
            {
                ActorHexagon ah = h.GetComponent<ActorHexagon>();
                if (ah != null && ah.goalColor != ColorID.white)
                {
                    goalsDefined++;
                    if (ah.hexagonColor == ah.goalColor)
                        goalsMet++;
                }
            }
            win = (goalsMet != 0 && goalsDefined != 0 && goalsDefined == goalsMet);
        }
   
        return win;
    }
    public static void EnterTestMode()
    {

        State.gameActivity = GameActivity.test;
        State.gameState = GameState.play;

        // copy the history to the solution 
        // so that it can be restored later
        State.gameBoard.seedsPlantedSolution = new Queue<GameStep>(seedsPlantedHistory);

        // set the goal to be the current number of seeds planted
        // this could either be from history of solution depending on
        // whether this is a new board or a previously-saved board that
        // is being edited
        SetScoreGoal(seedsPlanted, Math.Max(State.gameBoard.seedsPlantedSolution.Count, seedsPlantedHistory.Count));

        // set the current score to zero to simulate just starting the game
        SetScore(seedsPlanted, 0);

        // Clear out the seedsPlantedHistory to simulate just starting the game
        // however, leave the correct solution untouched
        ClearHistory();   
    }

    public static void EnterEditMode()
    {

        State.gameActivity = GameActivity.edit;
        State.gameState = GameState.play;

        // however, the current score to be the current
        // number of seeds planted so that this can
        // be used as the goal if the board is saved
        SetScore(seedsPlanted, Math.Max(State.gameBoard.seedsPlantedSolution.Count, seedsPlantedHistory.Count));

        // copy the solution back to the history to
        // overwrite any incorrect history that may have
        // been generated in testing mode
        seedsPlantedHistory = new Queue<GameStep>(State.gameBoard.seedsPlantedSolution);

        // hide the dialog panel just in case the tester
        // made it all the way to the end and either won or lost
        ShowGroup(dialogPanel, false);

        UpdateDisplay();
        
    }

    private static void GetCurrentBoardData()
    {
        
        // get the current hexagon state
        GameObject[] hexagons = GameObject.FindGameObjectsWithTag(ActorGridManager.HexagonTag);
        foreach (GameObject hex in hexagons)
        {
            ActorHexagon h = hex.GetComponent<ActorHexagon>();
            if (h)
            {
                // always give priority to the goal color
                if(h.goalColor != ColorID.white)
                {
                    if(!State.gameBoard.presetBoard.ContainsKey(h.hexagonID))
                        State.gameBoard.presetBoard.Add(h.hexagonID, (int)h.goalColor);
                }
                else
                {
                    if (h.hexagonColor != ColorID.white)
                    {
                        if (!State.gameBoard.presetBoard.ContainsKey(h.hexagonID))
                            State.gameBoard.presetBoard.Add(h.hexagonID, (int)h.hexagonColor);
                    }
                }
            }
        }

        // set the solution to the history
        State.gameBoard.seedsPlantedSolution = new Queue<GameStep>(seedsPlantedHistory);

        // set the goal to the solution
        SetScoreGoal(seedsPlanted, State.gameBoard.seedsPlantedSolution.Count);
        
    }
    
    public static int GetScore(string scoreName)
    {
        return scoreboard.ContainsKey(scoreName) ? scoreboard[scoreName].scoreCurrent : 0;
    }

    public static void GoNextBoard()
    {
        if(State.gameBoardNameNext != "")
        {
            State.gameBoardName = State.gameBoardNameNext;
            State.gameBoardNameNext = "";
            
        }
    }

    private static void ParseBoardData(string boardData)
    {
        Debug.Log("Board data returned: " + boardData);
        fsData data = fsJsonParser.Parse(boardData);
        object deserialized = null;
        _serializer.TryDeserialize(data, typeof(Board), ref deserialized).AssertSuccessWithoutWarnings();
        State.gameBoard = (Board)deserialized;
        State.gameState = GameState.boardData;
        State.TaskDone("GetBoardData");
        boardBuilt = false;
    }

    public static void PrintCurrentBoard()
    {
        // record the current board data
        GetCurrentBoardData();

        // serialize
        string printJSON = SerializeGameBoard(State.gameBoard);

        // copy to the clipboard
        TextEditor te = new TextEditor();
        te.text = printJSON;
        te.SelectAll();
        te.Copy();
        Debug.Log("Board JSON copied to clipboard: " + printJSON);
    }

   
    public static void RecordSeedPlanted(int hexagonID, ColorID seedColorID)
    {
        if(seedsPlantedHistory.Count < State.gameBoard.seedsPlantedSolution.Count)
        {
            GameStep nextSolution = State.gameBoard.seedsPlantedSolution.ToArray()[seedsPlantedHistory.Count];
            if(nextSolution.colorID != (int)seedColorID || nextSolution.hexagonID != hexagonID)
            {
                // you are no longer following the solution, hide hint
                ShowGroup(getHintButton, false);
            }
        }
        else
        {   // you have gone past the end of the solution, hide hint
            ShowGroup(getHintButton, false);
        }
        
        seedsPlantedHistory.Enqueue(new GameStep(hexagonID, (int)seedColorID));
 
    }

    public static void RequestBoardData()
    {
        boardBuilt = false;

        if (State.BoardIsNew())
        {
            State.gameBoard = new Board();
            State.gameState = GameState.play;
            State.gameActivity = GameActivity.edit;
            State.TaskDone("GetBoardData");
            GameObject.Find("ScreenPlay").GetComponent<ScreenPlay>().UpdateSubPanels();
            return;
        }

        if (State.gameSource == GameSource.local)
        {
            Debug.Log("Loading board data locally");
            TextAsset boardText = Resources.Load(State.gameBoardName) as TextAsset; // extensions must be omitted when using Resources.Load
            if (boardText)
                ParseBoardData(boardText.text);
            else
                Debug.LogError("Unable to load board " + State.gameBoardName + " from local resources");
        }
        else
        {
            Database.GetUserBoard(State.gameBoardIdentityID, State.gameBoardName);
            
        }
            
    }

    public static void ResetBoard()
    {
        // Clear history and scores
        ClearHistory();
        ClearScores();

        // build the grid
        gridManager.BuildGrid(7, 8);

        // get the board data
        State.TaskAdd("GetBoardData");
        RequestBoardData();
        
        // reset the control buttons
        ShowGroup(resetBoardButton, false);
        ShowGroup(getHintButton, true);
        ShowGroup(undoPlayButton, false);
        
        // play the welcome audio
        audioManager.playWelcome();
        
    }

    public void SaveBoard(int i)
    {

        if(!saveBoardTextObject || !boardNameField)
        {
            Debug.LogError("Invalid saveboardtextobject or boardNameField in SaveBoard().  Unable to save yard.");
            audioManager.playIncorrect();
            return;
        }

        string saveBoardName = boardNameField.text;
        if(saveBoardName.Length < 6)
        {
            saveBoardTextObject.text = "Yard names must be at least 6 characters long.";
            audioManager.playIncorrect();
            return;
        }
        if(saveBoardName.Length > 16)
        {
            saveBoardTextObject.text = "Yard names cannot be more than 16 characters long.";
            audioManager.playIncorrect();
            return;
        }

        if(Database.HasBadWords(saveBoardName))
        {
            saveBoardTextObject.text = "This yard name may contain vulgar language.  Vulgar language is not allowed.";
            audioManager.playIncorrect();
            return;
        }
        

        // copy the board to the clipboard
        PrintCurrentBoard();

        // save the board to the database
        Database.SaveUserBoard(State.gameBoard, saveBoardName);

    }

    public static string SerializeGameBoard()
    {
        fsData data;
        _serializer.TrySerialize(typeof(Board), State.gameBoard, out data).AssertSuccessWithoutWarnings();
        return fsJsonPrinter.CompressedJson(data);
    }

    public static string SerializeGameBoard(Board board)
    {
        fsData data;
        _serializer.TrySerialize(typeof(Board), board, out data).AssertSuccessWithoutWarnings();
        return fsJsonPrinter.CompressedJson(data);
    }

    public static void SetBoardColor(int hexagonID, ColorID seedColorID)
    {
        if (!State.gameBoard.presetBoard.ContainsKey(hexagonID))
        {
            State.gameBoard.presetBoard.Add(hexagonID, (int)seedColorID);
            ActorGridManager.SetHexagonColor(hexagonID, seedColorID);
        }
    }

    public static void SetBoardColorGoal(int hexagonID, ColorID seedColorID)
    {
        if (!State.gameBoard.presetBoard.ContainsKey(hexagonID))
        {
            State.gameBoard.presetBoard.Add(hexagonID, (int)seedColorID);
            ActorGridManager.SetHexagonGoalColor(hexagonID, seedColorID);
        }
    }

    public static void SetBoardToTurn(int TurnID)
    {
        
        // reset the grid state
        // this resets colors and goals for all hexagons
        ActorGridManager.ResetGrid();

        // clear the scoreboard
        int originalSeedsPlantedGoal = State.gameBoard.gameGoals.ContainsKey(seedsPlanted) ? State.gameBoard.gameGoals[seedsPlanted] : TurnID;
        ClearScores();
        
        // if this is a NEW board, then copy the history
        // to the solution to simulate a game save
        if (State.BoardIsNew())
        {
            State.gameBoard.seedsPlantedSolution = new Queue<GameStep>(seedsPlantedHistory);
        }

        Debug.LogWarning("Setting board " + State.gameBoardName + " state to turn " + TurnID);

        // if we are in play mode, simply repeat the steps from seedsPlanted History up to the turnID
        if (State.gameActivity == GameActivity.play && seedsPlantedHistory.Count >= TurnID && TurnID > 0)
        {
            Queue<GameStep> replay = new Queue<GameStep>(seedsPlantedHistory);
            ClearHistory();
            SetScoreGoal(seedsPlanted, originalSeedsPlantedGoal);

            for (int i = 0; i < TurnID; i++)
            {
                GameStep playStep = replay.Dequeue();
                SetSeedColor(playStep.colorID);
                ActorGridManager.GetHexagonActor(playStep.hexagonID).PlantSeed();
            }

        }
        
        // if we are in the edit or test activity, and we own this board, then
        // reset the local history
        // this doesn't effect the board's solution
        // and gets rebuild by PlantSeed() function
        // then replay the solution up to the turnID
        // while also rebuilding the solution queue
        // also set the currently-selected seed color to match the last turn
        if ((State.gameActivity == GameActivity.edit || State.gameActivity == GameActivity.test) && State.gameBoardMine)
        {
            ClearHistory();

            // set the goal to the turn count
            SetScoreGoal(seedsPlanted, TurnID);


            for (int i = 0; i < TurnID; i++)
            {
                GameStep playStep = State.gameBoard.seedsPlantedSolution.Dequeue();
                SetSeedColor(playStep.colorID);
                ActorGridManager.GetHexagonActor(playStep.hexagonID).PlantSeed();
            }

            
            // clear the solution and copy the current history to the solution making sure to use deep copy
            State.gameBoard.seedsPlantedSolution.Clear();
            State.gameBoard.seedsPlantedSolution = new Queue<GameStep>(seedsPlantedHistory);         
        }


        // if we are in artist mode, or if we do not own this board
        // then set the hexagon goals
        if (State.gameMode == GameMode.artist || !State.gameBoardMine)
        {
            Debug.Log("Setting " + State.gameBoard.presetBoard.Count + " preset hexagon goals for board " + State.gameBoardName);
            foreach (KeyValuePair<int, int> preset in State.gameBoard.presetBoard)
            {
                ActorGridManager.SetHexagonGoalColor(preset.Key, (ColorID)preset.Value);
            }

            // set the goal back to what it was originally
            SetScoreGoal(seedsPlanted, originalSeedsPlantedGoal);
            
        }

        // automatically advance to PLAY state
        State.gameState = GameState.play;
        
        UpdateDisplay();
        
    }

    private static void SetScore(string n, int s)
    {
        if (!scoreboard.ContainsKey(n))
            scoreboard.Add(n, new Score());

        scoreboard[n].scoreCurrent = s;

        UpdateDisplay();
    }

    public static void SetScoreGoal(string n, int sg)
    {
        if (!scoreboard.ContainsKey(n))
            scoreboard.Add(n, new Score());
        
        scoreboard[n].scoreGoal = sg;

        if (!State.gameBoard.gameGoals.ContainsKey(n))
        {
            State.gameBoard.gameGoals.Add(n, sg);
        }
        else
        {
            State.gameBoard.gameGoals[n] = sg;
        }

    }

    public static void SetSeedColor(int id)
    {
        if (State.gameState == GameState.win || State.gameState == GameState.loss)
            return;

        currentPlayerColorID = (ColorID)id;
        UpdateSeedSelectedDisplay();

    }
    
    public static void ShowGroup(string groupName, bool show)
    {
        
        GameObject g = GameObject.Find(groupName);
        if (g)
            g.GetComponent<RectTransform>().localScale = show ? Vector3.one : Vector3.zero;
    }

    public static void ShowHint()
    {
        if (State.gameActivity != GameActivity.play)
            return;

        Debug.Log("Show Hint Requested");

        if (seedsPlantedHistory.Count < State.gameBoard.seedsPlantedSolution.Count)
        {
            Identity.AddGems(-1);
            GameStep nextSolution = State.gameBoard.seedsPlantedSolution.ToArray()[seedsPlantedHistory.Count]; // get the solution 1 after the current step
            SetSeedColor(nextSolution.colorID);
            ActorGridManager.GetHexagonActor(nextSolution.hexagonID).PlantSeed();

            UpdateDisplay();

        }
        else
        {
            Debug.LogError("Hint requested past end of solution queue");
        }

        
        
    }
    
    public static void SpawnRewardExplosions(int numberToSpawn)
    {
        for (int i = 0; i < numberToSpawn; i++)
        {
            Debug.Log("Spawning Explosion");
            CFX_SpawnSystem.GetNextObject(trinketManager.rewardExplosions[UnityEngine.Random.Range(0, trinketManager.rewardExplosions.Length)]);
        }
    }
    
    void Start()
    {
        // clear any input from previous screens
        Input.ResetInputAxes();

        // make sure the progress throbber from the
        // board list is cleared
        State.TaskDone("GetBoardList");

        // cache instance references
        gridManager = GameObject.FindGameObjectWithTag("GridManager").GetComponentInChildren<ActorGridManager>();
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponentInChildren<ActorAudioManager>();
        trinketManager = GameObject.FindGameObjectWithTag("TrinketManager").GetComponentInChildren<ActorTrinketManager>();

        // find the GUI objects
        flowerSelectorObject = GameObject.Find(flowerSelector).GetComponentInChildren<RectTransform>();
        flowerSelectorImage = GameObject.Find(flowerSelector).GetComponentInChildren<Image>();
        dialogPanelText = GameObject.Find(dialogPanel).GetComponentInChildren<Text>();
        blueSelectTransform = GameObject.Find(blueSelectButton).GetComponentInChildren<RectTransform>();
        redSelectTransform = GameObject.Find(redSelectButton).GetComponentInChildren<RectTransform>(); 
        yellowSelectTransform = GameObject.Find(yellowSelectButton).GetComponentInChildren<RectTransform>();
        boardNameField = GameObject.Find(boardNameInput).GetComponentInChildren<InputField>();
        saveBoardTextObject = GameObject.Find(saveBoardText).GetComponentInChildren<Text>();
        ratioTextObject = GameObject.Find(ratioText).GetComponentInChildren<Text>();
        gemsRemainingTextObject = GameObject.Find(gemsRemaining).GetComponentInChildren<Text>();
        foundTrinketTextObject = GameObject.Find(foundTrinketText).GetComponentInChildren<Text>();
        trinketImageObject = GameObject.Find(trinketImage).GetComponentInChildren<Image>();
        
        // reset the board
        ResetBoard();
        
    }
    
    public static void UpdateGemsRemaining()
    {
        int currentGems = Identity.GetGems();
        gemsRemainingTextObject.text = currentGems.ToString();
    }
    
    public static void UndoTurn()
    {
        if (State.gameActivity == GameActivity.play)
            Identity.AddGems(-1);
        
        SetBoardToTurn(seedsPlantedHistory.Count - 1);
    }

    public static void UnlockNextBoard()
    {
        string boardName = State.GetNextBoard();
        State.gameBoardNameNext = boardName;
        if(boardName != "")
            Identity.UnlockBoard(boardName, 1);
    }

    void Update()
    {
        if (!boardBuilt && State.gameState == GameState.boardData)
        {
            // the board data has been returned
            // so the board can be built now
            boardBuilt = true;
            SetBoardToTurn(State.gameBoard.seedsPlantedSolution.Count);
            State.gameState = GameState.play;

            // when starting a new board
            // always set the starting seed to red
            // and make sure the display is updated
            currentPlayerColorID = ColorID.red;
            UpdateSeedSelectedDisplay();
        }

        flowerSelectorThrobPercent = Mathf.PingPong(Time.realtimeSinceStartup, 1f);
        flowerSelectorImage.color = Color.Lerp(Color.green, Color.red, flowerSelectorThrobPercent);
    }
    
    public static void UpdateDisplay()
    {

        if (seedsRemainingText == null)
        {
            GameObject t = GameObject.Find(seedsRemaining);
            if (t)
                seedsRemainingText = t.GetComponentInChildren<Text>() as Text;
        }

        if (seedsRemainingText)
        {
            if (scoreboard.ContainsKey(seedsPlanted))
            {
                // if I am editing, then count up, otherwise count down
                if (State.gameActivity == GameActivity.edit)
                    seedsRemainingText.text = scoreboard[seedsPlanted].scoreCurrent.ToString();
                else
                    seedsRemainingText.text = (scoreboard[seedsPlanted].scoreGoal - scoreboard[seedsPlanted].scoreCurrent).ToString();

            }
            else
            {
                Debug.LogWarning("No seedsPlanted goal found, setting seedsRemaining to 0");
                seedsRemainingText.text = "0";
            }
        }

        // show the voting button ONLY if I do not own this board, I have won or lost, I am in the play activity, and this is not a local board
        ShowGroup(voteForBoardButton, (State.gameSource != GameSource.local && State.gameActivity == GameActivity.play && !State.gameBoardMine && (State.gameState == GameState.win || State.gameState == GameState.loss)));

        // show the next board button ONLY if a next board actually exists, I won the current board, and I am in the play activity
        ShowGroup(nextBoardButton, (State.gameBoardNameNext != "" && State.gameState == GameState.win && State.gameActivity == GameActivity.play));

        // show the edit more button ONLY if I have either won or lost and I am in the test activity
        ShowGroup(editMoreButton, State.gameActivity == GameActivity.test && (State.gameState == GameState.win || State.gameState == GameState.loss));

        // show the try again button ONLY if I have lost, and I am in the play activity
        ShowGroup(tryAgainButton, State.gameActivity == GameActivity.play && State.gameState == GameState.loss);

        // show the resetBoardButton and the UndoButton only if we are in play activity and have not won or lost the board
        if (State.gameActivity == GameActivity.play && State.gameState != GameState.win && State.gameState != GameState.loss)
        {
            ShowGroup(resetBoardButton, seedsPlantedHistory.Count > 0);
            ShowGroup(undoPlayButton, seedsPlantedHistory.Count > 1);
        }

        string textToDisplay = "Game Over!";
        // update the dialog panel visibility
        switch (State.gameState)
        {
            case GameState.loss:
                ShowGroup(dialogPanel, true);
                textToDisplay = "YOU LOSE\nEvery yard has a solution.  Try again, stinger!";
                dialogPanelText.text = textToDisplay;
                break;

            case GameState.play:
                ShowGroup(dialogPanel, false);
                break;

            case GameState.win:
                ShowGroup(dialogPanel, true);
                textToDisplay = "YOU WIN !!!\nNicely done, stinger!";
                dialogPanelText.text = textToDisplay;
                break;
        }

        // update the ratio text if playing in Editor
        if (scoreboard.ContainsKey(petalExplosions) && scoreboard.ContainsKey(seedsPlanted) && scoreboard[seedsPlanted].scoreCurrent > 0)
        {
            ratioTextObject.text = String.Format("{0:0.000}", (scoreboard[petalExplosions].scoreCurrent / scoreboard[seedsPlanted].scoreCurrent));
        }
        else
        {
            ratioTextObject.text = "0.000";
        }

        // update the gems remaining
        UpdateGemsRemaining();
        
    }

    public static void UpdateSeedSelectedDisplay()
    {
        if (flowerSelectorObject)
        {
            switch (currentPlayerColorID)
            {
                case ColorID.blue:
                    flowerSelectorObject.SetParent(blueSelectTransform, false);
                    break;
                case ColorID.red:
                    flowerSelectorObject.SetParent(redSelectTransform, false);
                    break;
                case ColorID.yellow:
                    flowerSelectorObject.SetParent(yellowSelectTransform, false);
                    break;
            }

        }
    }
    
}