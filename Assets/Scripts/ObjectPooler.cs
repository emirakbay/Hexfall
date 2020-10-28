using UnityEngine;
using System.Collections.Generic;
using RotaryHeart.Lib.SerializableDictionary;
using System.Linq;

public class ObjectPooler : MonoBehaviour
{
    public int initialPoolCount;
    public ObjectTypeToObject objectDictionary;

    public Dictionary<ObjectTypes, List<Object>> poolDictionary;

    public static ObjectPooler Instance;

    private void Awake()
    {
        Instance = this;
        InstantiatePools();
    }

    public void InstantiatePools()
    {
        poolDictionary = new Dictionary<ObjectTypes, List<Object>>();
        foreach (var item in objectDictionary.Keys)
        {
            poolDictionary.Add(item, new List<Object>());
        }

        foreach (var item in poolDictionary.Keys)
        {
            for (int i = 0; i < initialPoolCount; i++)
            {
                var poolList = poolDictionary[item];
                GameObject goToAdd = Instantiate((GameObject)objectDictionary[item]);
                goToAdd.SetActive(false);
                goToAdd.transform.SetParent(transform);
                poolList.Add(goToAdd);
            }
        }
    }

    public GameObject GetObject(ObjectTypes typeToGet)
    {
        GameObject goToReturn;
        if (poolDictionary[typeToGet].Count != 0)
        {
            goToReturn = (GameObject)poolDictionary[typeToGet].First();
            poolDictionary[typeToGet].RemoveAt(0);
            goToReturn.transform.SetParent(null);
        }
        else
        {
            var poolList = poolDictionary[typeToGet];
            goToReturn = Instantiate((GameObject)objectDictionary[typeToGet]);
        }
        
        goToReturn.SetActive(true);
        return goToReturn;
    }

    public void ReturnObject(ObjectTypes typeToReturn, Object obj)
    {
        GameObject goToReturn = (GameObject)obj;
        goToReturn.transform.SetParent(transform);
        goToReturn.SetActive(false);
        poolDictionary[typeToReturn].Add(goToReturn);
    }

}



[System.Serializable]
public class ObjectTypeToObject : SerializableDictionaryBase<ObjectTypes, Object>
{

}


public enum ObjectTypes
{
    Hexagon,
    Particles,
    Bomb
}