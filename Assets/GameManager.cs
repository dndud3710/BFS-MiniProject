using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.GraphView;

public enum Ebuildings
{
    Armchair,
    Carpet,
    Chair
}
public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance => instance;
    [SerializeField] Player player;
    [SerializeField] GameObject BuildingModePanel;
    [SerializeField] Button StartButton;


    Dictionary<Ebuildings, GameObject> BuildingRes_;
    public Dictionary<Ebuildings, GameObject> BuildingRes => BuildingRes_;

    bool BuildOn;
    Vector3 BuildPos_;

    List<Buildings> Built;
    private void Awake()
    {
        if(Instance == null)
        {
            instance = this;

            BuildingRes_ = new Dictionary<Ebuildings, GameObject>();
            Built = new List<Buildings>();

            BuildingRes_.Add(Ebuildings.Armchair, Resources.Load<GameObject>("Buildings/ArmChair_1"));
            BuildingRes_.Add(Ebuildings.Carpet, Resources.Load<GameObject>("Buildings/Carpet_1"));
            BuildingRes_.Add(Ebuildings.Chair, Resources.Load<GameObject>("Buildings/Chair_1"));
        }
        else
        {
            Destroy(this);
        }
    }
    private void Start()
    {
        StartButton.onClick.AddListener(() => player.FindWay());
    }

    private void Update()
    {
        BuildingMode();
    }
    GameObject ModeOn_Building;
    private void BuildingMode()
    {
        if (BuildOn)
        {
            if (Input.GetAxis("Mouse X") != 0 && Input.GetAxis("Mouse Y") != 0)
            {
                var Point = Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));

                if (Physics.Raycast(Point, out RaycastHit a))
                {

                    if (ModeOn_Building != null)
                    {
                        ModeOn_Building.transform.position = new Vector3(a.point.x, 0f, a.point.z);
                    }
                }
            }
            if (Input.GetMouseButtonDown(0))
            {
                Build();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                BuildingModeOff();
            }
            
        }
    }
  

    public void BuildingModeOn(Ebuildings buildingName)
    {
        BuildingModePanel.SetActive(true);
        ModeOn_Building = Instantiate(BuildingRes_[buildingName], Vector3.zero, Quaternion.identity);
        BuildOn = true;
    }
    private void BuildingModeOff()
    {
        BuildingModePanel.SetActive(false);
        Destroy(ModeOn_Building);
        ModeOn_Building = null;
        BuildOn = false;
    }
    private void Build()
    {
        BuildingModePanel.SetActive(false);
        Buildings bu = ModeOn_Building.GetComponent<Buildings>();
        bu.BuildPossible();
        Built.Add(bu);
        ModeOn_Building = null;
        BuildOn = false;
    }

    public IEnumerator BuildTargetStart(Buildings b,Action Callback)
    {
        while(!b.MaxLevelCheck())
        {
            yield return new WaitForSeconds(1f);
            
            b.UpAlpha();
        }
        Callback?.Invoke();
    }
}
