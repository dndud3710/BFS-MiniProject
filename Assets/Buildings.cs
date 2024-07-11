using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buildings : MonoBehaviour
{
    [SerializeField] int MaxLevel;

    Material mat;


    [HideInInspector]
    private bool buildPossible_; // 건축 가능한지 여부 혹은 건축 완료 (빌드모드인 상태인지 아닌지) false = 건축 불가능 true = 건축 가능
    
    int level;
    public int Level => level;
    private void Awake()
    {
        level = 0;
        MeshRenderer mesh = GetComponent<MeshRenderer>();
        mat = mesh.materials[0];
    }

    private void Start()
    {
        Color c = mat.color;
        c.a = 0.2f;
        mat.color = c;
    }
    public bool MaxLevelCheck()
    {
        if (level >= MaxLevel)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void UpAlpha() 
    {
        if (level >= MaxLevel)
        {
            buildPossible_ = false;
            return;
        }

        Color c = mat.color;
        c.a = (0.25f * (float)++level);
        mat.color = c;
    }
    public void BuildPossible()
    {
        buildPossible_ = true;
    }
}
