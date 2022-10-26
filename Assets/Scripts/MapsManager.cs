using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapsManager
{
    private enum Map { DoublePortals, NoPortals, NotOriginal };

    public static int[,] originalMap = Maps.original;

    private static List<int[,]> theMaps = new List<int[,]>() {Maps.DoublePortals, Maps.NoPortals, Maps.NotOriginal};

    public static int[,] GetNewMap(bool reset)
    {
        if (reset) { return originalMap; }

        int rand = Random.Range(0, 3);
        return theMaps[rand];
    }

}