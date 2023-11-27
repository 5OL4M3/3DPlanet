using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helpers
{
    //static script for setting random seed
    public static void SetRandomSeed(int seed)
    {
        Random.InitState(seed);
    }

    //static script for resetting random seed
    public static void ResetRandomSeed()
    {
        Random.InitState(System.Environment.TickCount);
    }
}
