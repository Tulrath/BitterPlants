using UnityEngine;
using UnityEngine.UI;

public class ActorMainMenuManager : MonoBehaviour {

    
    public GameObject MainMenuButtonPrefab;
    public GameObject MainMenuPanelPrefab;
    public GameObject HelpPanelPrefab;
    
    private static ActorMainMenuManager singleton;
    private static MainMenuPanel menuPanel;
    private static MainMenuButton menuButton;

    void Awake()
    {
        if (singleton == null)
        {
            DontDestroyOnLoad(gameObject);
            singleton = this;
        }
        else if (singleton != this)
        {
            Destroy(gameObject);
        }
    }

    
    void OnLevelWasLoaded()
    {
        GameObject go = null;
        RectTransform rect = null;
        Button button = null;
        Text text = null;

        RectTransform canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponentInChildren<RectTransform>();

        if (GameObject.FindGameObjectWithTag("MainMenuPanel") == null)
        {
            go = GameObject.Instantiate(MainMenuPanelPrefab) as GameObject;
            rect = go.GetComponentInChildren<RectTransform>();
            rect.SetParent(canvas, false);
        }

        if (GameObject.FindGameObjectWithTag("MainMenuButton") == null)
        {
            go = GameObject.Instantiate(MainMenuButtonPrefab) as GameObject;
            rect = go.GetComponentInChildren<RectTransform>();
            rect.SetParent(canvas, false);

        }

        if (GameObject.FindGameObjectWithTag("HelpPanel") == null)
        {
            go = GameObject.Instantiate(HelpPanelPrefab) as GameObject;
            rect = go.GetComponentInChildren<RectTransform>();
            rect.SetParent(canvas, false);

        }

        // set the shop sprite to disabled and clear the text if the player is under 15 years old
        // or if the platform is NOT Android and NOT WindowsEditor
        go = GameObject.FindGameObjectWithTag("ShopButton");
        if (go != null)
        {
            button = go.GetComponentInChildren<Button>();
            if(button != null)
            {
                text = go.GetComponentInChildren<Text>();
                if (State.GetAgeFifteenOrOlder() == false)
                {
                    button.interactable = false;
                    if (text)
                        text.text = "";
                }
                else
                {
                    button.interactable = true;
                    if (text)
                        text.text = "Shop";
                }
            }
            else
            {
                Debug.LogWarning("Could not find button component in ShopButton");
            }
            
        }
        else 
        {
            Debug.LogWarning("Could not find ShopButton gameobject");
        }
        
    }
    
}
