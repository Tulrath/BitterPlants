using UnityEngine;
using UnityEngine.UI;

public class ChapterButton : MonoBehaviour {

    ActorAudioManager audioManager;
    ActorChapterListManager chapterManager;
    public bool isLocked = true;
    public string chapterID;
    public Sprite lockedImage;
    public Sprite unlockedImage;
    public Text nameText;
    public Image buttonImage;

    void Start()
    {
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponentInChildren<ActorAudioManager>();
        chapterManager = GameObject.FindGameObjectWithTag("ChapterListManager").GetComponentInChildren<ActorChapterListManager>();
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
        buttonImage.sprite = locked ? lockedImage : unlockedImage;
    }

    public void SetChapterID(string ID)
    {
        chapterID = ID;
    }

    public void SetDisplayName(string name)
    {
        nameText.text = name;
    }

    public void ClickBoardButton()
    {
        if (isLocked)
        {
            Debug.LogWarning("Chapter " + chapterID + " is locked and cannot be viewed.");
        }
        else
        {
            audioManager.playButtonClick();
            chapterManager.ShowChapter(chapterID);
        }

    }

    
}
