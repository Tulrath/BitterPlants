using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FullSerializer;

// Needed for AWS
using Amazon;

// Needed for DynamoDB
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;


public enum metricType
{
    vote = 0,
    play = 1,
    win = 2
}


public static class Database {

    // JSON serializer
    private static readonly fsSerializer _serializer = new fsSerializer();
    private static bool atLastPage = false;

    // DynamoDB Objects
    public const string delimiter = ":";
    private static IAmazonDynamoDB _ddbClient;
    private static IAmazonDynamoDB Client
    {
        get
        {
            if (_ddbClient == null)
            {
                _ddbClient = new AmazonDynamoDBClient(Identity.Credentials, RegionEndpoint.USEast1);
            }

            return _ddbClient;
        }
    }
    private static DynamoDBContext _context;
    private static DynamoDBContext Context
    {
        get
        {
            if (_context == null)
                _context = new DynamoDBContext(Client);

            return _context;
        }
    }

    // database bookmark stack
    // for use in paging through any table
    private static Stack<Dictionary<string, AttributeValue>> lastKeysEvaluated = new Stack<Dictionary<string, AttributeValue>>();
    

    // badword file resource
    private static TextAsset badWordText = null;
    private static string[] patternArray;


    public static void AddBoardMetric(metricType metric)
    {
        if (State.gameBoardID == "" || State.gameBoardIdentityID == "")
            return;

        // Retrieve the ddbBoard
        BoardDDB ddbBoard = new BoardDDB();
        Context.LoadAsync<BoardDDB>(State.gameBoardIdentityID, State.gameBoardID, (result) =>
        {
            if (result.Exception == null)
            {
                ddbBoard = result.Result as BoardDDB;
                
                switch (metric)
                {
                    case metricType.vote:
                        // votes will be negative so we can sort-descending with most-voted first
                        ddbBoard.Votes_D = ddbBoard.Votes_D - 1;
                        ddbBoard.Votes_A = ddbBoard.Votes_A + 1;
                        ddbBoard.VotesPlaysRatio = Math.Abs(ddbBoard.Votes_A) / (float)Math.Abs(ddbBoard.Plays_A);
                        break;
                    case metricType.play:
                        // plays will be positive so we can sort-ascending with least-played first
                        ddbBoard.Plays_D = ddbBoard.Plays_D - 1;
                        ddbBoard.Plays_A = ddbBoard.Plays_A + 1;
                        ddbBoard.VotesPlaysRatio = Math.Abs(ddbBoard.Votes_A) / (float)Math.Abs(ddbBoard.Plays_A);
                        ddbBoard.WinsPlaysRatio = Math.Abs(ddbBoard.Wins_A) / (float)Math.Abs(ddbBoard.Plays_A);
                        break;
                    case metricType.win:
                        // wins will be positive so we can sort-ascending with least-won first
                        ddbBoard.Wins_D = ddbBoard.Wins_D - 1;
                        ddbBoard.Wins_A = ddbBoard.Wins_A - 1;
                        ddbBoard.WinsPlaysRatio = Math.Abs(ddbBoard.Wins_A) / (float)Math.Abs(ddbBoard.Plays_A);
                        break;
                }

                Context.SaveAsync<BoardDDB>(ddbBoard, (res) =>
                {
                    if (res.Exception == null)
                        Debug.Log("User board metric " + metric + " incremented.");
                });
            }
        });
    }

    public static bool AtFirstPage()
    {
        return lastKeysEvaluated.Count < 2;
    }

    public static bool AtLastPage()
    {
        return atLastPage;
    }

    public static bool HasBadWords(string input)
    {
        bool retval = false;

        if(badWordText == null)
        {
            badWordText = (TextAsset)Resources.Load("badword_patterns", typeof(TextAsset));
            patternArray = badWordText.text.Split("\n"[0]);
            Debug.Log("Loaded " + patternArray.Length + " bad word patterns");
        }

        Debug.Log("Testing " + input);
        for(int i = 0; i < patternArray.Length; i++)
        {
            if (retval == false)
            {
                retval = Regex.IsMatch(input, patternArray[i], RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            }
                
            if (retval)
            {
                Debug.LogWarningFormat("BAD WORD MATCH on {0}", patternArray[i]);
                i = patternArray.Length;
            }
                
        }
            
        return retval;
    }

   

    public static void GetMyBoards()
    {
        // get boards I own
        GetUserBoards(Identity.Credentials.GetIdentityId(), true);
    }
    
    public static void GetUserBoard(string identityID, string boardID)
    {
        State.gameBoardID = "";
        State.gameBoardIdentityID = "";
        
        Debug.LogFormat("Retrieving board {0} for {1}", boardID, identityID);
        BoardDDB ddbBoard = new BoardDDB();
        Board retrievedBoard = new Board();
        
        Context.LoadAsync<BoardDDB>(identityID, boardID, (result) =>
        {
            if (result.Exception == null)
            {
                ddbBoard = result.Result;
                State.gameBoardID = boardID;
                State.gameBoardIdentityID = identityID;
                retrievedBoard = ddbBoard.UserBoard;
                State.gameBoard = retrievedBoard;
                AddBoardMetric(metricType.play);
                State.gameState = GameState.boardData;
                State.TaskDone("GetBoardData");
                Debug.LogWarning("Set gamestate to boardData");
            }
            else
            {
                Debug.LogError("Unable to LoadAsync board " + boardID);
                State.TaskDone("GetBoardData");
                Debug.LogError(result.Exception);
            }
        });
    }
    
    public static void GetUserBoards(string ownerIdentityID, bool firstPage)
    {
        // get boards someone else owns
        QueryRequest request = new QueryRequest
        {
            TableName = "BitterPlants_UserBoards",
            Limit = 20,
            ExpressionAttributeValues = new Dictionary<string, AttributeValue> { { ":val", new AttributeValue { S = ownerIdentityID } } },
            KeyConditionExpression = "IdentityID = :val",
            ProjectionExpression = "IdentityID, BoardID, GameModeID, Votes_A, Plays_A, Wins_A"
        };
        
        RunQuery(firstPage, request);
    }

    public static void GetPopularBoards(bool firstPage)
    {
        ScanRequest request = new ScanRequest
        {
            TableName = "BitterPlants_UserBoards",
            IndexName = "GameModeID-Votes_D-index",
            Limit = 10,
            ExclusiveStartKey = (firstPage || lastKeysEvaluated.Count == 0) ? null : lastKeysEvaluated.Peek(),
            ProjectionExpression = "IdentityID, BoardID, GameModeID, Votes_D, Plays_A, Wins_A"
        };
        
        RunScan(firstPage, request);
    }

    public static void GetNewBoards(bool firstPage)
    {
        ScanRequest request = new ScanRequest
        {
            TableName = "BitterPlants_UserBoards",
            IndexName = "GameModeID-Plays_A-index",
            Limit = 10,
            ExclusiveStartKey = (firstPage || lastKeysEvaluated.Count == 0) ? null : lastKeysEvaluated.Peek(),
            ProjectionExpression = "IdentityID, BoardID, GameModeID, Plays_A, Votes_A, Wins_A"
        };

        Debug.LogWarningFormat("Database.GetNewBoards lastKeysEvaluatedCount: {0}", lastKeysEvaluated.Count);
        RunScan(firstPage, request);
    }

    public static void GetLocalBoards()
    {
        // manually set the board list - fastest method

        State.gameBoardList = new string[]
        {
            "001alcove", "002beads", "003bench", "004candle", "005dance", "006flame", "007incense", "008nautilus", "009voyage", "010island", "011dolphin", "012leaf", "013owl", "014zepplin", "015nimbus", "016moon", "017zenith", "018lightning", "019kite", "020village", "021farm", "022academy", "023factory", "024flute", "025kaval", "026piccolo", "027zither", "028orca", "029narwhal", "030beluga", "031omura", "032pan", "033dish", "034spoon", "035vat", "036newt", "037iguana", "038viper", "039dragon", "040yeti", "041bigfoot", "042zombie", "043alicorn", "044zenith", "045zodiac", "046ylem", "047xylograph", "048vampire", "049satyr", "050naga", "051fairy", "052slate", "053xenolith", "054pillow", "055marble", "056badger", "057nightingale", "058ocelot", "059bison", "060playa", "061quarry", "062fjord", "063valley", "064anthology", "065verse", "066vellum", "067quill", "068love", "069believe", "070mediate", "071nurture"
        };
        Debug.LogFormat("{0} boards parsed", State.gameBoardList.Length);
        State.gameState = GameState.boardList;
        State.TaskDone("GetBoardList");
    }

    public static string GetChapterText(string chapterFileName)
    {
        // get the chapter text from local storage

        string chapterText = "Requested chapter could not be read from local storage.  This probably means the chapter isn't available yet.  New chapters are released on a regular basis.";
        Debug.LogWarning("Loading chapter text locally for " + chapterFileName);
        TextAsset chapterTextAsset = Resources.Load(chapterFileName) as TextAsset; // extensions must be omitted when using Resources.Load
        if(chapterTextAsset)
        {
            chapterText = "\n\n\n" + chapterTextAsset.text + "\n\n\n";
        }
        return chapterText;
    }

   public static void PageUp()
    {
        atLastPage = false;
        if (lastKeysEvaluated.Count > 0)
            lastKeysEvaluated.Pop();
        if (lastKeysEvaluated.Count > 0)
            lastKeysEvaluated.Pop();
        Debug.LogFormat("Database.PageUp lastKeysEvaluatedCount: {0}", lastKeysEvaluated.Count);
    }
  
    public static void RunScan(bool firstPage, ScanRequest request)
    {
        
        if (firstPage)
        {
            lastKeysEvaluated.Clear();
            request.ExclusiveStartKey = null;
            State.gameBoardList = null;
        }
        
        Queue<string> boardsRetrieved = new Queue<string>();

        Client.ScanAsync(request, (result) => {

            if(result.Exception == null && result.Response != null)
            {
                

                foreach (Dictionary<string, AttributeValue> item in result.Response.Items)
                {
                    string boardOwnerID = "";
                    string boardID = "";
                    string boardName = "";
                    string boardVotes = "";
                    string boardPlays = "";
                    string boardWins = "";

                    foreach (KeyValuePair<string, AttributeValue> kvp in item)
                    {
                        switch (kvp.Key)
                        {
                            case "BoardID":
                                boardID = kvp.Value.S;
                                break;
                            case "BoardName":
                                boardName = kvp.Value.S;
                                break;
                            case "IdentityID":
                                boardOwnerID = kvp.Value.S;
                                break;
                            case "Plays_A":
                            case "Plays_D":
                                int p;
                                if (int.TryParse(kvp.Value.N, out p))
                                    boardPlays = Math.Abs(p).ToString();
                                break;
                            case "Votes_A":
                            case "Votes_D":
                                int v;
                                if(int.TryParse(kvp.Value.N, out v))
                                    boardVotes = Math.Abs(v).ToString();
                                break;
                            case "Wins_A":
                            case "Wins_D":
                                int w;
                                if (int.TryParse(kvp.Value.N, out w))
                                    boardWins = Math.Abs(w).ToString();
                                break;
                        }
                    }

                    boardsRetrieved.Enqueue(boardOwnerID + delimiter + boardID + delimiter + boardVotes + delimiter + boardPlays + delimiter + boardWins);
                    
                }

                atLastPage = result.Response.Items.Count == 0;

                if (result.Response.LastEvaluatedKey != null)
                    lastKeysEvaluated.Push(result.Response.LastEvaluatedKey);
                
                // setting the state to boardList will notify
                // the ActorBoardListManager that new board list
                // data has arrived and needs to be built
                State.gameBoardList = boardsRetrieved.ToArray();
                State.gameState = GameState.boardList;
                State.TaskDone("GetBoardList");

            } 
        });
    }

    public static void RunQuery(bool firstPage, QueryRequest request)
    {
        
        if (firstPage)
        {
            lastKeysEvaluated.Clear();
            request.ExclusiveStartKey = null;
            State.gameBoardList = null;
        }

        Queue<string> boardsRetrieved = new Queue<string>();

        Client.QueryAsync(request, (result) => {

            if (result.Exception == null && result.Response != null)
            {


                foreach (Dictionary<string, AttributeValue> item in result.Response.Items)
                {
                    string boardOwnerID = "";
                    string boardID = "";
                    string boardName = "";
                    string boardVotes = "";
                    string boardPlays = "";
                    string boardWins = "";

                    foreach (KeyValuePair<string, AttributeValue> kvp in item)
                    {
                        switch (kvp.Key)
                        {
                            case "BoardID":
                                boardID = kvp.Value.S;
                                break;
                            case "BoardName":
                                boardName = kvp.Value.S;
                                break;
                            case "IdentityID":
                                boardOwnerID = kvp.Value.S;
                                break;
                            case "Votes_A":
                            case "Votes_D":
                                int v;
                                if (int.TryParse(kvp.Value.N, out v))
                                    boardVotes = Math.Abs(v).ToString();
                                break;
                            case "Plays_A":
                            case "Plays_D":
                                int p;
                                if (int.TryParse(kvp.Value.N, out p))
                                    boardPlays = Math.Abs(p).ToString();
                                break;
                            case "Wins_A":
                            case "Wins_D":
                                int w;
                                if (int.TryParse(kvp.Value.N, out w))
                                    boardWins = Math.Abs(w).ToString();
                                break;
                        }
                    }

                    boardsRetrieved.Enqueue(boardOwnerID + delimiter + boardID + delimiter + boardVotes + delimiter + boardPlays + delimiter + boardWins);

                }

                atLastPage = result.Response.Items.Count == 0;

                if (result.Response.LastEvaluatedKey != null)
                    lastKeysEvaluated.Push(result.Response.LastEvaluatedKey);

                // setting the state to boardList will notify
                // the ActorBoardListManager that new board list
                // data has arrived and needs to be built
                State.gameBoardList = boardsRetrieved.ToArray();
                State.gameState = GameState.boardList;
                State.TaskDone("GetBoardList");
            }
        });
    }

    public static string SanitizeString(string input)
    {
        return new string(input.Where(c => !char.IsControl(c)).ToArray());
    }
    
    public static void SaveUserBoard(Board myBoard, string userBoardName)
    {

        State.TaskAdd("SaveUserBoard");
        userBoardName = userBoardName.ToLower();
        
        // Create DynamoDB wrapper
        BoardDDB ddbBoard = new BoardDDB();
        string identityID = Identity.Credentials.GetIdentityId();
        ddbBoard.IdentityID = identityID;
        ddbBoard.BoardID = System.Guid.NewGuid() + delimiter + userBoardName;
        ddbBoard.BoardName = userBoardName;
        ddbBoard.UserBoard = myBoard;
        ddbBoard.Votes_A = 0;
        ddbBoard.Votes_D = 0;
        // the plays are set initially to 1 because
        // the board is always played once during creation
        // this also prevents divide-by-zero errors
        ddbBoard.Plays_A = 1;
        ddbBoard.Plays_D = -1;
        ddbBoard.Wins_A = 0;
        ddbBoard.Wins_D = 0;
        ddbBoard.VotesPlaysRatio = 0f;
        ddbBoard.WinsPlaysRatio = 0f;

        string printJSON = ActorGameManager.SerializeGameBoard(myBoard);
        Debug.Log("Saving Board: " + printJSON);

        //  Save the board 
        Context.SaveAsync(ddbBoard, (result) =>
        {
            if (result.Exception == null)
            {
                Debug.Log("Board saved to UserBoards table");
                State.gameState = GameState.boards;
                SceneManager.LoadScene("boards");
                State.TaskDone("SaveUserBoard");
            }
            else
            {
                Debug.LogWarning("Board save to UserBoards table failed.");
                State.TaskDone("SaveUserBoard");
            }
        });
    }


}
