using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SelectBuilding : MonoBehaviour
{
    
    Dictionary<Ebuildings, Button> buildings;

    
    private void Awake()
    {
        buildings = new Dictionary<Ebuildings, Button>();

        buildings.Add(Ebuildings.Armchair, transform.GetChild(0).GetComponent<Button>());
        buildings.Add(Ebuildings.Carpet, transform.GetChild(1).GetComponent<Button>());
        buildings.Add(Ebuildings.Chair, transform.GetChild(2).GetComponent<Button>());
    }
    private void Start()
    {
        Ebuildings[] buildingTypes = (Ebuildings[])System.Enum.GetValues(typeof(Ebuildings));
        foreach (Ebuildings type in buildingTypes)
        {
            buildings[type].onClick.AddListener(
                () => GameManager.Instance.BuildingModeOn(type)
                );
        }
    }

}
