using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleGameButtonBehaviour : MonoBehaviour
{
    public GameObject GameToToggle;

    public void OnToggleGamePressed()
    {

        GameToToggle.SetActive(!GameToToggle.activeInHierarchy);
    }
}
