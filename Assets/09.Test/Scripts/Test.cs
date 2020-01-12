using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private void Awake()
    {
        string[] tNames = GetAnimatorTriggers();
        foreach (string tName in tNames)
        {
            Debug.Log(tName);
        }
    }

    public string[] GetAnimatorTriggers()
    {
        string[] t = new string[]
        {
            "123",
            "223",
            "323",
            "423"
        };
        return t;
    }
}
