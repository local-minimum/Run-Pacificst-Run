using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beastiary : Singleton<Beastiary>
{
    [SerializeField]
    Enemy[] beastPrefabs;

    public Enemy GetABeast(int level)
    {
        return Instantiate(beastPrefabs[Random.Range(0, beastPrefabs.Length)]);
    }
}
