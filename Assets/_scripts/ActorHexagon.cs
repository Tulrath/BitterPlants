using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ActorHexagon : MonoBehaviour
{

    public ColorID hexagonColor;
    public ColorID goalColor = ColorID.white;
    public ColorID hoverColor = ColorID.white;
    public Dictionary<int, ActorHexagon> petals;
    
    public int hexagonID;

    public Image ColorImage;
    public Image GoalImage;
    public Image HoverImage;
    
    public Sprite[] flowerCenters;
    public Sprite[] flowerPetals;
    public Sprite[] goalRings;
    public Sprite[] hoverCenters;

    public GameObject petalExplosion;
    
    public ActorHexagon()
    {
        hexagonColor = ColorID.white;
        petals = new Dictionary<int, ActorHexagon>();
    }
    
    public void PlantSeed()
    {

        if (State.gameState == GameState.win || State.gameState == GameState.loss)
            return;

        ColorID seedColorID = ActorGameManager.currentPlayerColorID;

        if(seedColorID == ColorID.white)
        {
            // erasing
            if (hexagonColor == ColorID.white)
                return;

            SetColor(6, seedColorID);
            if (State.gameState == GameState.play)
                GameObject.FindGameObjectWithTag("AudioManager").GetComponentInChildren<ActorAudioManager>().playPlanted();

        }
        else
        {
            // planting
            if (hexagonColor != ColorID.white)
                return;
            
            SetColor(6, seedColorID);
            
            foreach (KeyValuePair<int, ActorHexagon> p in petals)
            {
                p.Value.GrowPetal(p.Key, seedColorID);
            }
            ActorGameManager.AddScore(ActorGameManager.seedsPlanted, 1);
            ActorGameManager.RecordSeedPlanted(hexagonID, seedColorID);
            ActorGameManager.CheckWinLose();
            if (State.gameState == GameState.play)
                GameObject.FindGameObjectWithTag("AudioManager").GetComponentInChildren<ActorAudioManager>().playPlanted();

        }
        
        return;
    }

  
    public void AddPetal(int rotation, ActorHexagon petal)
    {
        if(petal != null)
        {
            petals.Add(rotation, petal);
           
        } else
        {
            Debug.LogError("Attempt to add null petal not allowed");
        }
            
    }

    

    public void SetColor(int rotation, ColorID seedColorID)
    {
        hexagonColor = seedColorID;
        if(rotation >= 0 && rotation < 7 && ColorImage != null)
        {
            ColorImage.sprite = rotation < 6 ? flowerPetals[(int)seedColorID] : flowerCenters[(int)seedColorID];
            gameObject.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, rotation * -60f);
        }
        else
        {
            Debug.LogErrorFormat("Improper rotation requested: {0} for {1} or no ColorImage define", rotation, gameObject.name);
        }
        
    }

    public void SetGoalColor(ColorID goalColorID)
    {
        goalColor = goalColorID;
        GoalImage.GetComponent<RectTransform>().localScale = goalColorID == ColorID.white ? Vector3.zero : Vector3.one;
        GoalImage.sprite = goalRings[(int)goalColorID];  
    }

    public void SetHoverColor(ColorID hoverColorID)
    {
        hoverColor = hoverColorID;
        HoverImage.GetComponent<RectTransform>().localScale = hoverColorID == ColorID.white ? Vector3.zero : Vector3.one;

        //if ((int)hoverColorID > hoverCenters.Length - 1 || (int)hoverColorID < 0)
        //    Debug.LogErrorFormat("Invalid hoverCenters requested {0} for {1}", (int)hoverColorID, gameObject.name);
        //else
        //    HoverImage.sprite = hoverCenters[(int)hoverColorID];
    }
    
    public void GrowPetal(int rotation, ColorID seedColorID)
    {
        
        // early exit if hexagon color is already this color so no points are scored
        if (hexagonColor == seedColorID)
            return;
        
        if (hexagonColor == ColorID.white)
        {
            SetColor(rotation, seedColorID);
            ActorGameManager.AddScore(ActorGameManager.petalCount, 1);
        } 
        else
        {
            SetColor(0, ColorID.white);
            ActorGameManager.AddScore(ActorGameManager.petalCount, -1);
            ActorGameManager.AddScore(ActorGameManager.petalExplosions, 1);
            SpawnPetalExplosion();
        }
        
        return;
    }

    public void SpawnPetalExplosion()
    {
        GameObject instance = CFX_SpawnSystem.GetNextObject(petalExplosion);
        Vector3 rectPosition = GetComponent<RectTransform>().position;
        rectPosition.z = -1f;
        instance.GetComponent<Transform>().position = rectPosition;
    }

}
