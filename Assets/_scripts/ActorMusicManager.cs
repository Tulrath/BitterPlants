using UnityEngine;
using System.Collections.Generic;


public class ActorMusicManager : MonoBehaviour {

    public AudioClip[] audioEffects;
    public AudioSource effectSource;

    public static ActorMusicManager singleton;

    private AudioSource[] sources;
    private float[] maxVolumes;
    private List<int> RemixTarget = new List<int>();
    private float changeProbability = 0.25f;
    private float remixIntervalSeconds = 16f;
    private float effectDelayMinimum = 32f;
    private float effectDelayMaximum = 128f;
    private float nextRemixTime;
    private float nextEffectTime;
    private bool started = false;
    private bool muted = false;
    
    void Awake()
    {
        if(singleton == null)
        {
            DontDestroyOnLoad(gameObject);
            singleton = this;
        } 
        else if(singleton != this)
        {
            Destroy(gameObject);
        }
    }
    
    
    void Start ()
    {
        if (started)
            return;

        started = true;
        nextEffectTime = Time.time + Random.Range(effectDelayMinimum, effectDelayMaximum);

        sources = gameObject.GetComponents<AudioSource>();
        maxVolumes = new float[sources.Length];
        for(int i=0; i<sources.Length; i++)
        {
            maxVolumes[i] = sources[i].volume;
        }

        Remix();
        
	}
	
	
	void Update ()
    {
        if (muted)
            return;

        if(Time.time > nextRemixTime)
        {
            Remix();
        }

        if(Time.time > nextEffectTime)
        {
            AmbientOnShot();
        }
        for(int i = 0; i< sources.Length; i++)
        {
            if(sources[i].isPlaying && !RemixTarget.Contains(i))
            {
                // this source should fade out
                sources[i].volume = Mathf.Clamp(sources[i].volume - (Time.deltaTime * 0.25f * maxVolumes[i]), 0,maxVolumes[i]);
                if(sources[i].volume == 0)
                {
                    sources[i].Stop();
                }
            }

            if(!sources[i].isPlaying && RemixTarget.Contains(i))
            {
                sources[i].volume = 0;
                sources[i].time = Random.Range(0, sources[i].clip.length -1 );
                sources[i].Play();
            }

            if(sources[i].isPlaying && RemixTarget.Contains(i))
            {
                // this source should fade in
                sources[i].volume = Mathf.Clamp(sources[i].volume + (Time.deltaTime * 0.125f * maxVolumes[i]), 0, maxVolumes[i]);
            }
        }
	}

    void AmbientOnShot()
    {
        nextEffectTime = Time.time + Random.Range(effectDelayMinimum, effectDelayMaximum);
        effectSource.PlayOneShot(audioEffects[Random.Range(0, audioEffects.Length)]);
    }

    void Remix()
    {
        nextRemixTime = Time.time + remixIntervalSeconds;

        if(RemixTarget.Count == 0)
        {
            // set initial 3
            for(int i=0;i < 3; i++)
            {
                RemixTarget.Add(Mathf.FloorToInt(Random.Range(0, sources.Length)));
            }
        }
        else
        {
            // randomly change some parts of the mix
            for (int i = 0; i < 3; i++)
            {
                if(Random.Range(0,1f) <= changeProbability)
                {
                    RemixTarget[i] = (Mathf.FloorToInt(Random.Range(0, sources.Length)));
                }
            }
        }
        
    }

    public void Mute()
    {
        muted = true;
        foreach (AudioSource a in sources)
        {
            a.Stop();
            a.mute = true;
        }
    }

    public void UnMute()
    {
        muted = false;
        // this will force a new music selection
        RemixTarget.Clear();
        foreach (AudioSource a in sources)
        {
            a.mute = false;
        }
    }

    public void ToggleMusic()
    {
        if (muted)
            UnMute();
        else
            Mute();
    }
}
