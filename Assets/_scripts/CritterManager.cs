using UnityEngine;
using System.Collections.Generic;

public class CritterManager : MonoBehaviour {

    
    public Helper[] puzzleHelpers;
    private List<Helper> selectedHelpers = new List<Helper>();

    public void SelectHelpers(int helperCount)
    {
        selectedHelpers.Clear();
        for(int i = 0; i < helperCount; i++)
        {
            selectedHelpers.Add(puzzleHelpers[UnityEngine.Random.Range(0, puzzleHelpers.Length)]);
        }
    }

    public Helper ChoseRandomHelper()
    {
        return puzzleHelpers[UnityEngine.Random.Range(0, puzzleHelpers.Length)];
    }

}
