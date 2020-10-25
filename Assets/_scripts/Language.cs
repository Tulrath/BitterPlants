using FullSerializer;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;


public static class Language {

    private static readonly fsSerializer _serializer = new fsSerializer();
    private static Dictionary<string, string> localization = new Dictionary<string, string>();
    private const string languageFileExtension = ".txt"; // only needed for creating a sample language file
    private const string languageGUIPrefix = "language";
    private static Font ArialFont = null;
    private static string languageName = "";

    public static void LoadLanguage(string languageName)
    {
        languageName = languageName == "" ? "english" : languageName;
        localization.Clear();

        Debug.Log("Loading language data locally: " + languageName);
        TextAsset languageText = Resources.Load(languageName) as TextAsset; // extensions must be omitted when using Resources.Load
        Debug.LogWarning("Language Text: " + languageText);
        if (languageText && languageText.text != "")
        {
            fsData data = fsJsonParser.Parse(languageText.text);
            object deserialized = null;
            _serializer.TryDeserialize(data, typeof(Dictionary<string, string>), ref deserialized).AssertSuccessWithoutWarnings();
            localization = (Dictionary<string, string>)deserialized;
            Debug.Log(localization.Count + " language keys loaded.");
        }
            
        else
        {
            Debug.LogError("Unable to load language: " + languageName + " from local resources");
        }
        
    }

    public static string Translate(string key)
    {
        if (localization.ContainsKey(key))
            return localization[key];
        else
            return "language key error: " + key;
    }
    
    public static void LocalizeText(Text[] screenText)
    {
        for(int i = 0; i < screenText.Length; i ++)
        {
            screenText[i].text = Translate(screenText[i].gameObject.name);
        }
    }

    public static void TranslateScreen()
    {

        // if our arialfont is undefined, then load a reference to it
        if(ArialFont == null)
        {
            ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        }


        // get our language name
        languageName = State.GetLanguageName();

        // if our langauge is defined, but not loaded, then load the language file
        if (State.LanguageKeySet() && localization.Count == 0)
        {
            LoadLanguage(languageName);
        }

        // if our english is not english, then translate the page
        if(languageName != "english")
        {
            GameObject[] objs = GameObject.FindObjectsOfType<GameObject>();

            for (int i = 0; i < objs.Length; i++)
            {
                Text[] texts = objs[i].GetComponentsInChildren<Text>();

                for (int k = 0; k < texts.Length; k++)
                {
                    // only translate TEXT elements that use the languagePrefix
                    // this will leave symbol elements alone
                    if (texts[k].gameObject.name.StartsWith(languageGUIPrefix))
                    {
                        texts[k].text = Translate(texts[k].gameObject.name);
                        Debug.LogWarning("Translation: " + Translate(texts[k].gameObject.name));

                        // so far, this is not necessary

                        // also change the font to Arial, so that all special
                        // characters will print correctly
                        // texts[k].font = ArialFont;

                    }
                }

            }
        }
        
    }



}
