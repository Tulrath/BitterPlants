using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InputFieldFix : UIBehaviour {

    private InputField inputField = null;

    new IEnumerator Start()
    {
        // wait a couple frames so that the caret GO gets created.

        yield return null;
        yield return null;
        if (inputField == null)
            inputField = GetComponent<InputField>();

        if (inputField != null)
        {
            Transform caretGO = inputField.transform.Find(inputField.transform.name + " Input Caret");
            caretGO.GetComponent<CanvasRenderer>().SetMaterial(Graphic.defaultGraphicMaterial, Texture2D.whiteTexture);
        }
    }
}
