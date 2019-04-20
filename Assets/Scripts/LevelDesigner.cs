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
        int width = 12;
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
                else if (x > 6 && y == 4)
                {
                    lvl[x, y] = LevelFeature.Ground(false, true, 0);
                } else
                {
                    lvl[x, y] = LevelFeature.Ground(false, false, 0);
                }
            }
        }
        switch (Random.Range(0, 1))
        {
            case 0:
                lvl[8, 3] = LevelFeature.SetAgent(true, false, 0, lvl[8, 3]);
                lvl[8, 7] = LevelFeature.SetAgent(false, true, 1, lvl[8, 7]);
                break;
            case 1:
                lvl[8, 7] = LevelFeature.SetAgent(true, false, 0, lvl[8, 7]);
                lvl[8, 3] = LevelFeature.SetAgent(false, true, 1, lvl[8, 3]);
                break;
        }
        return lvl;
    }
}
