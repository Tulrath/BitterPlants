using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenStore : MonoBehaviour {

    private ActorAudioManager audioManager;
    private const string StoreResultPanelName = "storeResultPanel";
    public Text GemCountText;
    
    void Start ()
    {
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponentInChildren<ActorAudioManager>();
        UpdateGemCountDisplay();
    }

    public void PlayButtonClick()
    {
        audioManager.playButtonClick();
    }

    public void UpdateGemCountDisplay()
    {
        GemCountText.text = Identity.GetGems().ToString();
    }

    public void SetGemCountDisplay(int count)
    {
        GemCountText.text = count.ToString();
    }

    public void ResultPanelOk()
    {
        ActorGameManager.ShowGroup(StoreResultPanelName, false);

    }

}
