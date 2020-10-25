using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Amazon;
using Amazon.CognitoSync;
using Amazon.CognitoIdentity;
using Amazon.CognitoSync.SyncManager;


public static class Identity {

    private const string IdentityPoolID = "us-east-1:3f645f0f-a824-47f9-8e44-17f3ed8668d4";
    private const string gemCountKeyName = "BitterPlants_GemCount";
    private static RegionEndpoint regionEndpoint = RegionEndpoint.USEast1;
    private static AmazonCognitoSyncConfig syncConfig = new AmazonCognitoSyncConfig { RegionEndpoint = regionEndpoint };
    private static Dictionary<string, int> boardStars = new Dictionary<string, int>();
    public static Dictionary<int, string[]> nameWords = new Dictionary<int, string[]>();
    public static string foundIdentityID = "";
    

    // AWS Credentials
    private static CognitoAWSCredentials _credentials;
    public static CognitoAWSCredentials Credentials
    {
        get
        {
            if (_credentials == null)
                _credentials = new CognitoAWSCredentials(IdentityPoolID, regionEndpoint);
            return _credentials;
        }
    }

    // Cognito Objects
    private static Dataset GameProgress;
    private static Dataset PlayerInfo;
    private static Dataset CollectProgress;
    private static CognitoSyncManager _syncManager;
    private static CognitoSyncManager SyncManager
    {
        get
        {
            if (_syncManager == null)
            {
                _syncManager = new CognitoSyncManager(Credentials, syncConfig);
            }
            return _syncManager;
        }
    }


    /// <summary>
    /// Constructor
    /// </summary>
    static Identity()
    {
        // work-around for AWS issue 643
        // https://github.com/aws/aws-sdk-net/issues/643
        AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;
    }

    public static void StartSync()
    {
        // each database sync has an impact of 10 on total loading
        State.loadGoals += 30;
        Debug.LogWarningFormat("Loading Goal Increased to {0}", State.loadGoals);

        State.gameState = GameState.syncGameProgress;
        Debug.Log("*** syncGameProgress starting...");
        GameProgress = InitializeDataset("GameProgress");
    }

    /// <summary>
    /// Cognito public functions for user's game progress and Gems
    /// </summary>
    public static void SyncGameProgress()
    {
        GameProgress.SynchronizeAsync();
    }

    public static void SyncPlayerInfo()
    {
        PlayerInfo.SynchronizeAsync();
    }

    public static void SyncCollectProgress()
    {
        CollectProgress.SynchronizeAsync();
    }


    public static void UnlockBoard(string levelName, int stars)
    {
        if (GameProgress != null && levelName != null)
        {
            if(BoardStars(levelName) < stars)
                GameProgress.Put(levelName, stars.ToString());
        }

        SyncGameProgress();
    }

    
    
    public static int GetGems()
    {
        int currentGems = 0;
        int.TryParse(PlayerInfo.Get(gemCountKeyName), out currentGems);
        return currentGems;
    }

    public static void AddGems(int count)
    {

        Debug.Log("*** Add Gems");   
        if(PlayerInfo != null)
        {
            int currentGems;
            if(int.TryParse(PlayerInfo.Get(gemCountKeyName), out currentGems))
            {

                if ((currentGems + count) < 0)
                {
                    currentGems = 0;
                }
                else
                {
                    if((currentGems + count) > int.MaxValue)
                    {
                        currentGems = int.MaxValue;
                    }
                    else
                    {
                        currentGems = currentGems + count;
                    }
                }

                PlayerInfo.Put(gemCountKeyName, currentGems.ToString());
                SyncPlayerInfo();
            }
            else
            {
                Debug.LogWarning("Could not retrieve current Gem count.  Creating " + gemCountKeyName + " key...");
                PlayerInfo.Put(gemCountKeyName, count.ToString());
                SyncPlayerInfo();
            }
        }
    }


    // records that do not exist are assumed to be locked
    // any record that does exist that has a 0 stars ranking is assumed to be locked

    public static bool BoardLocked(string boardName)
    {    
        int currentStars = 0;
        Record boardStars = null;
        if (GameProgress != null)
        {
            boardStars = GameProgress.GetRecord(boardName);
        }
        if (boardStars != null)
            int.TryParse(boardStars.Value, out currentStars);
        return (boardStars == null || currentStars == 0);
    }

    public static bool CollectLocked(string collectName)
    {
        int currentStars = 0;
        Record collectStars = null;
        if(CollectProgress != null)
        {
            collectStars = CollectProgress.GetRecord(collectName);
        }
        if (collectStars != null)
            int.TryParse(collectStars.Value, out currentStars);
        return (collectStars == null || currentStars == 0);
    }

    public static int BoardStars(string boardName)
    {
        int currentStars = 0;
        Record boardStars = null;
        if(GameProgress != null)
        {
            boardStars = GameProgress.GetRecord(boardName);
        }
        if(boardStars != null)
            int.TryParse(boardStars.Value, out currentStars);
        return currentStars;
    }
    
    public static int CollectStars(string collectName)
    {
        int currentStars = 0;
        Record collectStars = null;
        if(CollectProgress != null)
        {
            collectStars = CollectProgress.GetRecord(collectName);
        }
        if (collectStars != null)
            int.TryParse(collectStars.Value, out currentStars);
        return currentStars;
    }
    
    public static void SetBoardStars(string boardName, int newStars)
    {
        int currentStars = 0;
        Record boardStars = GameProgress.GetRecord(boardName);
        if (boardStars != null)
            int.TryParse(boardStars.Value, out currentStars);
        Debug.LogWarningFormat("Game progress for {0}: currentStars {1}, newStars: {2}", boardName, currentStars, newStars);
        if (newStars > currentStars)
        {  
            GameProgress.Put(boardName, newStars.ToString());
            SyncGameProgress();
        }

    }

    public static void IncrementCollectStars(string collectName)
    {
        int currentStars = 0;
        Record collectStars = CollectProgress.GetRecord(collectName);
        if (collectStars != null)
            int.TryParse(collectStars.Value, out currentStars);
        CollectProgress.Put(collectName, (currentStars + 1).ToString());
        SyncCollectProgress();
    }

    public static void ResetBoardStars()
    {
        foreach (string boardName in State.gameBoardList)
        {
            // only executes for local boards
            if(!boardName.Contains(Database.delimiter))
            {
                GameProgress.Put(boardName, "0");
            }
        }

        SyncGameProgress();    
    }

    public static void AdvanceGameState()
    {
        // each database sync has an impact of 10 on total loading
        State.loadGoalsComplete += 10;

        switch (State.gameState)
        {
            case GameState.syncGameProgress:
                Debug.Log("*** syncGameProgress complete.");
                State.gameState = GameState.syncPlayerInfo;
                Debug.Log("*** syncPlayerInfo starting...");
                PlayerInfo = InitializeDataset("PlayerInfo");
                break;
            case GameState.syncPlayerInfo:
                Debug.Log("*** syncPlayerInfo complete.");
                State.gameState = GameState.syncCollectProgress;
                Debug.Log("*** syncCollectProgress starting...");
                CollectProgress = InitializeDataset("CollectProgress");
                break;
            case GameState.syncCollectProgress:
                Debug.Log("*** syncCollectProgress complete.");
                State.gameState = GameState.syncdone;
                Debug.Log("*** GameState advanced to syncDone.");
                break;
        }
    
    }
    
    /// <summary>
    /// Private Functions and Callbacks
    /// </summary>
    private static Dataset InitializeDataset(string datasetName)
    {

        Debug.LogFormat("Initializing Cognito Sync for dataset {0}", datasetName);
        
        Dataset d = SyncManager.OpenOrCreateDataset(datasetName);

        d.OnSyncSuccess += HandleSyncSuccess;
        d.OnSyncFailure += HandleSyncFailure;
        d.OnSyncConflict = HandleSyncConflict;
        d.OnDatasetMerged = HandleDatasetMerged;
        d.OnDatasetDeleted = HandleDatasetDeleted;

        d.Put("LastAccessTime", DateTime.Now.ToString());

        d.SynchronizeAsync();
        
        return d;
    }

    private static bool HandleDatasetDeleted(Dataset dataset)
    {
        
        if (dataset.Metadata != null)
        {
            Debug.LogWarning("Dataset purge required for dataset: " + dataset.Metadata.DatasetName);
        }
        else
        {
            Debug.LogWarning("Dataset purge required for dataset");
        }

        AdvanceGameState();

        // returning true informs the corresponding dataset can be purged in the local storage and return false retains the local dataset
        return true;
    }

    public static bool HandleDatasetMerged(Dataset dataset, List<string> datasetNames)
    {
        
        if (dataset.Metadata != null)
        {
            Debug.LogWarning("Merged required for dataset: " + dataset.Metadata.DatasetName);
        }
        else
        {
            Debug.LogWarning("Merge required for dataset");
        }

        AdvanceGameState();

        // returning true allows the Synchronize to resume and false cancels it
        return true;
    }

    private static bool HandleSyncConflict(Amazon.CognitoSync.SyncManager.Dataset dataset, List<SyncConflict> conflicts)
    {

        if (dataset.Metadata != null)
        {
            Debug.LogWarning("Sync conflicct resolution required for dataset: " + dataset.Metadata.DatasetName);
        }
        else
        {
            Debug.LogWarning("Sync conflicct resolution required for dataset");
        }

        List<Amazon.CognitoSync.SyncManager.Record> resolvedRecords = new List<Amazon.CognitoSync.SyncManager.Record>();

        foreach (SyncConflict conflictRecord in conflicts)
        {
            // This example resolves all the conflicts using ResolveWithRemoteRecord 
            // SyncManager provides the following default conflict resolution methods:
            //      ResolveWithRemoteRecord - overwrites the local with remote records
            //      ResolveWithLocalRecord - overwrites the remote with local records
            //      ResolveWithValue - for developer logic  
            resolvedRecords.Add(conflictRecord.ResolveWithRemoteRecord());
        }

        // resolves the conflicts in local storage
        dataset.Resolve(resolvedRecords);

        // on return true the synchronize operation continues where it left,
        //      returning false cancels the synchronize operation

        AdvanceGameState();

        return true;
    }

    private static void HandleSyncSuccess(object sender, SyncSuccessEventArgs e)
    {
        
        Dataset dataset = (Dataset)sender;

        if (dataset.Metadata != null)
        {
            Debug.Log("Successfully synced for dataset: " + dataset.Metadata.DatasetName);
        }
        else
        {
            Debug.Log("Successfully synced for dataset");
        }

        AdvanceGameState();

    }

    private static void HandleSyncFailure(object sender, SyncFailureEventArgs e)
    {

        Dataset dataset = (Dataset)sender;
        if (dataset.Metadata != null)
        {
            Debug.LogWarning("Sync failed for dataset : " + dataset.Metadata.DatasetName);
        }
        else
        {
            Debug.LogWarning("Sync failed for dataset");
        }

        Debug.LogException(e.Exception);

        AdvanceGameState();
    }

    public static void ClearLocalIdentity()
    {
        Debug.Log("Calling Clear, ClearCredentials, ClearIdentityCache");
        Credentials.Clear();
        Credentials.ClearCredentials();
        Credentials.ClearIdentityCache();
    }

    public static void SetLocalIdentity(string newIdentityID)
    {
        ClearLocalIdentity();
        Credentials.CacheIdentityId(newIdentityID);

    }

}
