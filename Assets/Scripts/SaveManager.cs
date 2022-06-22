using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    // instance
    public static SaveManager instance;

    // save data
    private SavePointData data;

    private void Awake()
    {
        // setup SavePointManager as a singleton class
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // initialize and load data
            data = new SavePointData();
            string path = Application.persistentDataPath + "/savepoint.json";
            if (File.Exists(path))
            {
                // read json file into data object
                string json = File.ReadAllText(path);
                data = JsonUtility.FromJson<SavePointData>(json);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        // save SavePointData to json file
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(Application.persistentDataPath + "/savepoint.json", json);
    }

    // serializable class for storing all save data
    [System.Serializable]
    private class SavePointData
    {
        public int sceneIndex;
        public float spawnPointX;
        public float spawnPointY;
    }

    public void SetSpawnPoint(Vector3 spawnPoint)
    {
        data.spawnPointX = spawnPoint.x;
        data.spawnPointY = spawnPoint.y;
    }

    public void SetSceneIndex(int index)
    {
        data.sceneIndex = index;
    }

    public Vector3 GetSpawnPoint()
    {
        return new Vector3(data.spawnPointX, data.spawnPointY, 0);
    }

    public int GetSceneIndex()
    {
        return data.sceneIndex;
    }

}
