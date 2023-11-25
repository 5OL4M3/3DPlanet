using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class Description : MonoBehaviour
{
    //This is a describer for the inspector
    [TextArea]
    [SerializeField]
    public string description = "This is a description for the inspector";

}
