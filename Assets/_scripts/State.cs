using System.Collections.Generic;

public enum GameMode
{
    artist,
    baskets,
    construction,
    demolition,
    length
}

public enum GameState
{
    sync,
    syncGameProgress,
    syncPlayerInfo,
    syncCollectProgress,
    syncdone,
    settings,
    boards,
    boardList,
    boardData,
    play,
    win,
    loss,
    credits,
    chapters,
    length
}

public enum GameSource
{
    local,
    remote,
    length
}

public enum GameActivity
{
    play,
    edit,
    test,
    length
}

public enum GameBoardCategory
{
    campaign,
    mine,
    popular,
    newest,
    length
}

public static class State
{
    // game state
    public static GameMode gameMode;
    public static GameState gameState { get { return _gameState; } set { _gameState = value; UnityEngine.Debug.LogWarningFormat("GAMESTATE SET: {0}", value); } }
    public static GameSource gameSource;
    public static GameActivity gameActivity;
    public static GameBoardCategory gameBoardCategory;
    public static bool gameBots;
    public static string gameBoardName;
    public static string gameBoardNameNext;
    public static string[] gameBoardList;
    public static Board gameBoard;
    public static string gameBoardID;
    public static string gameBoardIdentityID;
    public static bool gameBoardMine = true;
    public static bool gameConnected { get { return UnityEngine.Application.internetReachability != UnityEngine.NetworkReachability.NotReachable; } }

    // utility
    private static GameState _gameState;
    public static float loadGoals;
    public static float loadGoalsComplete;
    private static List<string> gameLoadingMessages;
    private const string AgeVerificationKey = "BitterPlants_117_AgeFifteenOrOlder";
    private const string LanguageKey = "BitterPlants_117_Language";
    private const string MusicVolumeKey = "BitterPlants_117_MusicVolume";
    private const string SoundEffectsVolumeKey = "BitterPlants_117_SoundEffectsVolume";
    public static UnityEngine.Vector2 referenceScreenSize = new UnityEngine.Vector2(640, 960);
    private static List<string> TaskList = new List<string>();

    static State()
    {
        gameMode = GameMode.artist;
        gameState = GameState.sync;
        gameBoardName = "000tutorial";
        gameBoard = new Board();
        gameBoardCategory = GameBoardCategory.campaign;

        

        gameLoadingMessages = new List<string>();

        gameLoadingMessages.Add("Wrangling Bees");
        gameLoadingMessages.Add("Sorting Seeds");
        gameLoadingMessages.Add("Picking Flowers");
        gameLoadingMessages.Add("Tasting Honey");
        gameLoadingMessages.Add("Collecting Pollen");
        gameLoadingMessages.Add("Reviewing Golden Ratio");
        gameLoadingMessages.Add("Counting Bee Ancestors");
        gameLoadingMessages.Add("Counting Hexagon Sides");
        gameLoadingMessages.Add("Mapping Backyard");
        gameLoadingMessages.Add("Surviving Bee Bootcamp");
        gameLoadingMessages.Add("Counting Days Until Solstice");
        gameLoadingMessages.Add("Building Superorganism");
        gameLoadingMessages.Add("Recruiting Sweat Bees");
        gameLoadingMessages.Add("Recruiting Bee Keepers");
        gameLoadingMessages.Add("Making Beeswax");
        gameLoadingMessages.Add("Avoiding Dragonflies");
        gameLoadingMessages.Add("Hiding from Beewolf");
        gameLoadingMessages.Add("Helping Apiculture");
        gameLoadingMessages.Add("Making Propolis");
        gameLoadingMessages.Add("Inducing Apoptosis");
        gameLoadingMessages.Add("Making Royal Jelly");
        gameLoadingMessages.Add("Sipping Nectar");
        gameLoadingMessages.Add("Evolving Longer Tongues");
        gameLoadingMessages.Add("Reviewing Inclusive Fitness");
        gameLoadingMessages.Add("Gathering Carpenters");
        gameLoadingMessages.Add("Gathering Leafcutters");
        gameLoadingMessages.Add("Gathering Masons");
        gameLoadingMessages.Add("Waggling");
    }

    public static string GetBoardDisplayName()
    {
        return GetBoardDisplayName(gameBoardName);
    }

    public static string GetBoardDisplayName(string boardName)
    {
        string display = "";
        if(boardName.Contains(Database.delimiter))
        {
            string[] temp = boardName.Split(Database.delimiter[0]);
            display = temp.Length > 2 ? temp[3] : temp[1];
        }
        else
        {
            display = boardName.Substring(3);
        }
        
        display = display.Substring(0, 1).ToUpper() + display.Substring(1).ToLower();

        return display;
    }

    public static int GetBoardNumber()
    {
        int retval = 0;
        if(!gameBoardName.Contains(Database.delimiter))
            int.TryParse(gameBoardName.Substring(0, 3), out retval);
        return retval;
    }

    public static string GetNextBoard()
    {
        string retval = "";
        for(int i = 0; i < gameBoardList.Length; i++)
        {
            if(gameBoardList[i] == gameBoardName && i < (gameBoardList.Length - 1))
            {
                retval = gameBoardList[i + 1];
            }
        }
        return retval;
    }

    public static string GetRandomLoadingMessage()
    {
        return gameLoadingMessages[UnityEngine.Random.Range(0, gameLoadingMessages.Count - 1)] + "...";
    }
    public static bool AgeKeySet()
    {
        return (UnityEngine.PlayerPrefs.HasKey(AgeVerificationKey));
    }

    public static bool LanguageKeySet()
    {
        return (UnityEngine.PlayerPrefs.HasKey(LanguageKey));
    }

    public static string GetLanguageName()
    {
        // set to english if not defined
        if (UnityEngine.PlayerPrefs.HasKey(LanguageKey))
        {
            return (UnityEngine.PlayerPrefs.GetString(LanguageKey));
        } 
        else
        {
            SetLanguageKey("english");
            return "english";
        }
    }

    public static bool GetAgeFifteenOrOlder()
    {
        // default to false if not defined or not set to YES
        if(UnityEngine.PlayerPrefs.HasKey(AgeVerificationKey))
        {
            if(UnityEngine.PlayerPrefs.GetString(AgeVerificationKey) == "YES")
            {
                return true;
            }
        }
        return false;
    }

    public static void SetAgeFifteenOrOlder(bool AgeFifteenOrOlder)
    {
        UnityEngine.PlayerPrefs.SetString(AgeVerificationKey, AgeFifteenOrOlder == true ? "YES" : "NO");
    }

    public static void SetLanguageKey(string languageName)
    {
        UnityEngine.PlayerPrefs.SetString(LanguageKey, languageName);
        Language.LoadLanguage(languageName);
        Language.TranslateScreen();
    }

    public static float GetMusicVolume()
    {
        float volume = 0.5f;
        if (UnityEngine.PlayerPrefs.HasKey(MusicVolumeKey))
        {
            volume = UnityEngine.PlayerPrefs.GetFloat(MusicVolumeKey);
        }
        return volume;
    }

    public static float GetSoundEffectsVolume()
    {
        float volume = 0.5f;
        if (UnityEngine.PlayerPrefs.HasKey(SoundEffectsVolumeKey))
        {
            volume = UnityEngine.PlayerPrefs.GetFloat(SoundEffectsVolumeKey);
        }
        return volume;
    }

    public static void SetMusicVolume(float volume)
    {
        UnityEngine.PlayerPrefs.SetFloat(MusicVolumeKey, volume);
    }

    public static void SetSoundEffectsVolume(float volume)
    {
        UnityEngine.PlayerPrefs.SetFloat(SoundEffectsVolumeKey, volume);
    }
 
    public static bool BoardIsNew()
    {
        return gameBoardName == "999new";
    }

    public static bool TasksWaiting()
    {
        return TaskList.Count > 0;
    }

    public static void TaskAdd(string taskName)
    {
        TaskList.Add(taskName);
    }
    
    public static void TaskDone(string taskName)
    {
        TaskList.Remove(taskName);
    }
}