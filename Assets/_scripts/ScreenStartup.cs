using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class ScreenStartup : MonoBehaviour {

    private Text LoadingText;
    private Text VersionText;
    private float nextUpdateTime;
    private ActorAudioManager audioManager;
    private bool ageSet = false;
    private bool languageSet = false;
    public GameObject LoadingImage;
    public GameObject AgeVerificationPanel;
    public GameObject LanguageSelectionPanel;
    public Slider ProgressSlider;
    private float maxTimeOnEachSync = 6f;
    private float timeNextForceAdvanceState = 0;
    
    // Use this for initialization
    void Start () {

        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponentInChildren<ActorAudioManager>();

        LoadingText = LoadingImage.GetComponent<Text>();
        VersionText = GameObject.FindGameObjectWithTag("VersionText").GetComponentInChildren<Text>();
        TrackedBundleVersionInfo currentRunningVersion = new TrackedBundleVersion().current;
        Debug.LogWarningFormat("***CURRENT VERSION: {0}",currentRunningVersion.ToString());
        VersionText.text = "Version " + currentRunningVersion.ToString();

        
        // show the age verification if age not set yet
        if (!State.AgeKeySet())
        {
            ShowAgeVerification();
        }
        else
        {
            ageSet = true;
            languageSet = true;
        }

    }
	
	// Update is called once per frame
	void Update () {

        if (Time.realtimeSinceStartup > nextUpdateTime)
        {
            nextUpdateTime = Time.realtimeSinceStartup + 1.25f;
            LoadingText.text = State.GetRandomLoadingMessage();

            // send out a periodic update on status to the log
            Debug.LogWarningFormat("Status AgeSet={0}, AudioLoaded={1}, GameState={2}", ageSet, audioManager.allAudioLoaded, State.gameState);

            // artificially advance GameState if database sync is taking too long
            if(State.gameState == GameState.syncCollectProgress || State.gameState == GameState.syncGameProgress || State.gameState == GameState.syncPlayerInfo)
            {
                if (timeNextForceAdvanceState == 0)
                    timeNextForceAdvanceState = Time.realtimeSinceStartup + maxTimeOnEachSync;

                if(Time.realtimeSinceStartup > timeNextForceAdvanceState)
                {
                    Debug.LogErrorFormat("** Sync taking too long, GameState advance forced.");
                    Identity.AdvanceGameState();
                    timeNextForceAdvanceState = Time.realtimeSinceStartup + maxTimeOnEachSync;

                }
            }
        }

        if(State.loadGoals > 0)
        {
            ProgressSlider.value = (State.loadGoalsComplete / State.loadGoals);
        }

        // we want to force the gamestate to syncdone if Cognito is taking too long
        /*if((Time.realtimeSinceStartup - startTimeOnStartup) > maxTimeOnStartup)
        {
            Debug.LogWarningFormat("!!! SYNCDONE Forced: Wait time exceeded.");
            if (State.gameState == GameState.sync)
                State.gameState = GameState.syncdone;
        }*/


        // also check to make sure languageSet is true when localization is added
        if(ageSet && audioManager.allAudioLoaded && State.gameState == GameState.syncdone)
        {
            Debug.Log("GAME IS READY!");
            State.gameState = GameState.boards;
            SceneManager.LoadScene("boards");
        }

        
    }

    private void ShowAgeVerification()
    {
        // if age panel is shown, language panel cannot be shown
        LanguageSelectionPanel.GetComponent<RectTransform>().localScale = Vector3.zero;
        AgeVerificationPanel.GetComponent<RectTransform>().localScale = Vector3.one;
    }

    private void ShowLanguageSelection()
    {
        // if language panel is shown, age panel cannot be shown
        LanguageSelectionPanel.GetComponent<RectTransform>().localScale = Vector3.one;
        AgeVerificationPanel.GetComponent<RectTransform>().localScale = Vector3.zero;
    }
   
    public void SelectLanguage(string languageName)
    {
        // set the language, this immediately translates all text on the screen
        State.SetLanguageKey(languageName);
        languageSet = true;

        // if the language has been changed, then always show the age verification
        ageSet = false;
        ShowAgeVerification();
        
    }

    public void AgeVerificationYes()
    {
        State.SetAgeFifteenOrOlder(true);
        AgeVerificationPanel.GetComponent<RectTransform>().localScale = Vector3.zero;
        ageSet = true;
    }

    public void AgeVerificationNo()
    {
        State.SetAgeFifteenOrOlder(false);
        AgeVerificationPanel.GetComponent<RectTransform>().localScale = Vector3.zero;
        ageSet = true;

    }
}
