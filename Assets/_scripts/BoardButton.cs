using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class BoardButton : MonoBehaviour {

    ActorAudioManager audioManager;
    public bool isLocked = true;
    public string boardID;
    public string boardIdentityID;
    public int stars = 0;
    public Sprite lockedImage;
    public Sprite unlockedImage;
    public Text nameText;
    public Text starsText;
    public Text playsText;
    public Text votesText;
    public Text winsText;
    public Image buttonImage;
    
    void Start()
    {
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponentInChildren<ActorAudioManager>();
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
        buttonImage.sprite = locked ? lockedImage : unlockedImage;
    }

    public void SetBoardID(string ID)
    {
        boardID = ID;
    }

    public void SetBoardIdentityID(string identity)
    {
        boardIdentityID = identity;
    }

    public void SetDisplayName(string name)
    {
        nameText.text = name;
    }

    public void SetStars(int newStars)
    {
        string s = "";
        if (newStars > 0)
        {
            stars = newStars;
            for(int i = 0; i < (newStars-1); i++)
            {
                s = s + "*";
            }
        }
        starsText.text = s;
    }

    public void SetStats(metricType metric, string value)
    {
        switch(metric)
        {
            case metricType.play:
                if (playsText != null)
                    playsText.text = value;
                break;
            case metricType.vote:
                if (votesText != null)
                    votesText.text = value;
                break;
            case metricType.win:
                if (winsText != null)
                    winsText.text = value;
                break;
        }
    }
    
    public void ClickBoardButton()
    {
        if(isLocked)
        {
            Debug.LogWarning("Board " + boardID + " is locked and cannot be played.");
        }
        else
        {
            State.gameBoardName = boardID;
            State.gameBoardIdentityID = boardIdentityID;
            audioManager.playButtonClick();
            StartCoroutine("LoadLevel", "play");
        }
        
    }

    IEnumerator LoadLevel(string sceneName)
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(sceneName);
        
    }
}
