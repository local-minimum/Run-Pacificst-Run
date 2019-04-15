using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LevelFeatureValue = System.UInt32;

public static class LevelDesigner
{
    public static LevelFeatureValue[,] Generate(int level)
    {
        if (level == 0)
        {
            return LevelHome();
        }
        throw new System.NotImplementedException($"{level} not implemented");
    }

    static LevelFeatureValue[,] LevelHome()
    {
        int width = 10;
        int height = 9;
        LevelFeatureValue[,] lvl = new LevelFeatureValue[width, height];
        for (int x=0; x<width; x++)
        {
            for (int y=0; y<height; y++)
            {
                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                {
                    lvl[x, y] = LevelFeature.Ground(false, true, 0);
                } else if (x == 6)
                {
                    lvl[x, y] = LevelFeature.Ground(y == 3, y != 7, 0);
                }
                else if (x > 6 && y == 5)
                {
                    lvl[x, y] = LevelFeature.Ground(false, true, 0);
                }
            }
        }
        switch (Random.Range(0, 1))
        {
            case 0:
                lvl[8, 3] |= LevelFeature.Agent(true, false, 0);
                lvl[8, 7] |= LevelFeature.Agent(false, false, 1);
                break;
            case 1:
                lvl[8, 7] |= LevelFeature.Agent(true, false, 0);
                lvl[8, 3] |= LevelFeature.Agent(false, false, 1);
                break;
        }
        return lvl;
    }
}
