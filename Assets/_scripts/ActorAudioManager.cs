using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public enum MusicState
{
    silent=0,
    start,
    middle,
    end,
    pause
}

[System.Serializable]
public class MusicSet
{
    public AudioClip[] starts;
    public AudioClip[] middles;
    public AudioClip[] ends;

}

public class ActorAudioManager : MonoBehaviour
{
    public static ActorAudioManager singleton;

    private bool musicOn = true;
    private bool soundsOn = true;
    private float maxMusicVolume;
    private float maxSoundEffectVolume;
    private int currentMusicSet = 0;
    private float pauseEndTime;
    private int middleRepeats;
    private int middleRepeatsGoal;

    public string[] ButtonClickSoundName;
    public string[] SceneLoadSoundName;
    public string[] RedPlantedSoundName;
    public string[] BluePlantedSoundName;
    public string[] YellowPlantedSoundName;
    public string[] WinSoundName;
    public string[] LossSoundName;
    public string[] WinMusicName;
    public string[] LossMusicName;
    public string[] CorrectSoundName;
    public string[] IncorrectSoundName;
    public string[] GrowingSoundName;
    public string[] ShakeSoundName;
    public string[] WelcomeSoundName;

    public MusicSet[] MusicSets;
    public AudioSource VoiceSource; // for voice sounds - stream 0
    public AudioSource ButtonSource; // for UI clicks - stream 1
    public AudioSource StingerSource; // for short music stingers - stream 1
    public AudioSource MusicSource; // for background music - stream 2
    public MusicState currentMusicState = MusicState.silent;

    private List<AudioSource> SoundEffectSources = new List<AudioSource>();

    private Queue<string> voiceQueue = new Queue<string>();

    private Dictionary<string, int> dictSounds = new Dictionary<string, int>();
    private Dictionary<string, AudioClip> dictClips = new Dictionary<string, AudioClip>();
    
    private string streamPath;

    public bool allAudioLoaded = false;
    
    void Awake()
    {
        if (singleton == null)
        {
            DontDestroyOnLoad(gameObject);
            singleton = this;

            maxMusicVolume = State.GetMusicVolume();
            maxSoundEffectVolume = State.GetSoundEffectsVolume();

#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidNativeAudio.makePool(3);
#endif
            // cache reference to music source and sound effects sources
            GameObject[] effects = GameObject.FindGameObjectsWithTag("SoundEffectSource");
            foreach (GameObject e in effects)
            {
                SoundEffectSources.Add(e.GetComponentInChildren<AudioSource>());
            }

            // start loading sounds
            StartCoroutine("LoadSounds");

        }
        else if (singleton != this)
        {
            Destroy(gameObject);
        }
        
    }
 
    void Update()
    {
        if(voiceQueue.Count > 0 && !VoiceSource.isPlaying)
        {
            PlaySound(VoiceSource, voiceQueue.Dequeue(), 0);
        }

        if(allAudioLoaded && musicOn && maxMusicVolume > 0 && currentMusicState != MusicState.pause)
        {
            if(!MusicSource.isPlaying)
            {
                playMusic();
            }
        }
    }

    public void SetMusicOn(bool enabled)
    {
        musicOn = enabled;
        if(musicOn)
        {
            currentMusicState = MusicState.silent;
            playMusic();
        }
        else
        {
            MusicSource.volume = 0;
            MusicSource.Stop();
        }
    }

    public void SetMusicVolume(float volume)
    {
        MusicSource.volume = volume;
        maxMusicVolume = volume;
        State.SetMusicVolume(volume);
    }

    public void SetSoundEffectsVolume(float volume)
    {
        foreach (AudioSource a in SoundEffectSources)
        {
            a.volume = volume;
        }
        maxSoundEffectVolume = volume;
        State.SetSoundEffectsVolume(maxSoundEffectVolume);
    }

    public void SetSoundsOn(bool enabled)
    {
        soundsOn = enabled;
    }

    public void queueVoice(string clipName)
    {
        voiceQueue.Enqueue(clipName);
    }

    public void playButtonClick()
    {
        int i = Random.Range(0, ButtonClickSoundName.Length - 1);
        PlaySound(ButtonSource, ButtonClickSoundName[i], 1);
    }

    public void playShake()
    {
        int i = Random.Range(0, ShakeSoundName.Length - 1);
        PlaySound(ButtonSource, ShakeSoundName[i], 1);
    }

    public void playScreenLoaded()
    {
        
    }

    public void playPlanted()
    {
        int i = 0;
        switch (ActorGameManager.currentPlayerColorID)
        {
            case (ColorID.blue):
                i = Random.Range(0, BluePlantedSoundName.Length - 1);
                PlaySound(ButtonSource, BluePlantedSoundName[i], 1);
                break;

            case (ColorID.red):
                i = Random.Range(0, RedPlantedSoundName.Length - 1);
                PlaySound(ButtonSource, RedPlantedSoundName[i],1);
                break;

            case (ColorID.yellow):
                i = Random.Range(0, YellowPlantedSoundName.Length - 1);
                PlaySound(ButtonSource, YellowPlantedSoundName[i],1);
                break;
        }
    }

    public void playWin()
    {
        int i;
        i = Random.Range(0, WinMusicName.Length - 1);
        PlaySound(StingerSource,WinMusicName[i], 1);
        i = Random.Range(0, WinSoundName.Length - 1);
        queueVoice(WinSoundName[i]);
    }

    public void playMusic()
    {
        if (MusicSets.Length < 1)
            return;

        switch(currentMusicState)
        {
            case MusicState.silent:
                // choose a new music set
                currentMusicSet = Random.Range(0,MusicSets.Length);

                // play the start clip
                MusicSource.clip = MusicSets[currentMusicSet].starts[Random.Range(0,MusicSets[currentMusicSet].starts.Length)];
                MusicSource.volume = maxMusicVolume;
                MusicSource.Play();
                Debug.LogWarning("Playing Start Music");

                // reset the middleRepeats, and set the middleRepeatsGoal
                middleRepeats = 0;
                middleRepeatsGoal = (Random.Range(1, 4) * 4) -1; // middle will have 4, 8 or 12 measures
                
                // advance the state
                currentMusicState = MusicState.start;
                
                break;
            case MusicState.start:

                // play a middle clip
                MusicSource.clip = MusicSets[currentMusicSet].middles[Random.Range(0, MusicSets[currentMusicSet].middles.Length)];
                MusicSource.volume = maxMusicVolume;
                MusicSource.Play();
                Debug.LogWarning("Playing Middle Music");

                // advance the state only if we have met our repeat goal
                if (middleRepeats >= middleRepeatsGoal)
                {
                    middleRepeats = 0;
                    currentMusicState = MusicState.middle;
                }
                else
                {
                    middleRepeats++;
                }
                break;
            case MusicState.middle:

                // play the end clip
                MusicSource.clip = MusicSets[currentMusicSet].ends[Random.Range(0, MusicSets[currentMusicSet].ends.Length)];
                MusicSource.volume = maxMusicVolume;
                MusicSource.Play();
                Debug.LogWarning("Playing End Music");

                // advance the state
                currentMusicState = MusicState.end;
                

                break;
            case MusicState.end:

                // end music has ended, start a coroutine for the pause
                MusicSource.Stop(); 
                int pauseSeconds = Random.Range(60, 90);
                StartCoroutine("PauseMusic", (float)pauseSeconds);
                Debug.LogWarning("Music is paused");

                // advance the state
                currentMusicState = MusicState.pause;

                break;
        }
    }

    public void playLoss()
    {
        int i;
        i = Random.Range(0, LossMusicName.Length - 1);
        PlaySound(StingerSource, LossMusicName[i], 1);
        i = Random.Range(0, LossSoundName.Length - 1);
        queueVoice(LossSoundName[i]);
    }

    public void playCorrect()
    {
        int i = Random.Range(0, CorrectSoundName.Length - 1);
        queueVoice(CorrectSoundName[i]);
    }

   

    public void playIncorrect()
    {
        int i = Random.Range(0, IncorrectSoundName.Length - 1);
        queueVoice(IncorrectSoundName[i]);
    }
    

    public void playWelcome()
    {
        int i = Random.Range(0, WelcomeSoundName.Length - 1);
        PlaySound(StingerSource, WelcomeSoundName[i],1);
    }

    public static T[] ConcatArrays<T>(params T[][] list)
    {
        var result = new T[list.Sum(a => a.Length)];
        int offset = 0;
        for (int x = 0; x < list.Length; x++)
        {
            list[x].CopyTo(result, offset);
            offset += list[x].Length;
        }
        return result;
    }

    private IEnumerator LoadSounds()
    {
        yield return new WaitForSeconds(0.1f);
        var allSounds = ConcatArrays(WelcomeSoundName, ButtonClickSoundName, SceneLoadSoundName, RedPlantedSoundName, BluePlantedSoundName, YellowPlantedSoundName, WinSoundName, LossSoundName, WinMusicName, LossMusicName, CorrectSoundName, IncorrectSoundName, GrowingSoundName, ShakeSoundName);
        State.loadGoals += (float)allSounds.Length;

        Debug.LogWarningFormat("Loading Goal Increased to {0}", State.loadGoals);

        // set all volumes to 0
        float tempVoiceVolume = VoiceSource.volume;
        float tempButtonVolume = ButtonSource.volume;
        float tempStingerVolume = StingerSource.volume;

        VoiceSource.volume = 0;
        ButtonSource.volume = 0;
        StingerSource.volume = 0; 


#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidNativeAudio.setVolume(0, 0f);
        AndroidNativeAudio.setVolume(1, 0f);
        AndroidNativeAudio.setVolume(2, 0f);
#endif

        foreach (string c in allSounds)
        {
            string path = "";


#if UNITY_ANDROID && !UNITY_EDITOR
            
            int soundID = AndroidNativeAudio.load(c + ".wav");
            //Debug.LogFormat("Loading Sound:SoundID {0}: {1}", c, soundID);
            if(!dictSounds.ContainsKey(c))
            {
                dictSounds.Add(c, soundID);
                //AndroidNativeAudio.play(dictSounds[c]);
                yield return new WaitForSeconds(0.01f);
            }

#else
            
            path = "file://" + System.IO.Path.Combine(Application.streamingAssetsPath, c + ".wav");
            //Debug.LogFormat("Loading AudioClip {0}::{1}", c, path);
            
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);
                    if (!dictClips.ContainsKey(c))
                    {
                        dictClips.Add(c, myClip);
                        yield return new WaitForSeconds(0.01f);
                    }

                }
            }


           
                

#endif

            State.loadGoalsComplete += 1f;
        }

        // set all volumes back to what they were
        VoiceSource.volume = tempVoiceVolume;
        ButtonSource.volume = tempButtonVolume;
        StingerSource.volume = tempStingerVolume;

#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidNativeAudio.setVolume(0, maxSoundEffectVolume);
        AndroidNativeAudio.setVolume(1, maxSoundEffectVolume);
        AndroidNativeAudio.setVolume(2, maxSoundEffectVolume);
#endif



        allAudioLoaded = true;

    }

   
    private void PlaySound(AudioSource source, string soundName, int streamID)
    {

        
        if (!soundsOn)
            return;
#if UNITY_ANDROID && !UNITY_EDITOR
        if(dictSounds.ContainsKey(soundName))
        {
            AndroidNativeAudio.play(dictSounds[soundName]);
        }
#else
        if (dictClips.ContainsKey(soundName))
        {
            source.clip = dictClips[soundName];
            source.Play();
        }
        
#endif

    }

    private IEnumerator PauseMusic(float pauseSeconds)
    {
        yield return new WaitForSeconds(pauseSeconds);
        currentMusicState = MusicState.silent;

        // setting the state to silent will
        // chose a new music set and start playing
        // the music on the next update
    }
}