using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages all save data throughout scenes and between sessions
/// </summary>
public class GameManager : MonoBehaviour
{
    // constants
    private const float GRAVITY_FORCE = 19.62f;

    // instance
    public static GameManager instance;

    // save data
    [System.Serializable]
    private class SaveData
    {
        public string sceneName;
        public Vector2 spawnPoint;
        public int health;
    }
    private SaveData data;

    // canvas
    [SerializeField] private GameObject platformerCanvas;

    // UNITY METHODS ----------------------------------------------------------------------------

    private void Awake() // called each time a scene is loaded/reloaded
    {
        // setup SavePointManager as a singleton class
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // initialize and load data
            data = new SaveData();
            string path = Application.persistentDataPath + "/savedata.json";
            if (File.Exists(path))
            {
                // read json file into data object
                string json = File.ReadAllText(path);
                data = JsonUtility.FromJson<SaveData>(json);
            }
            else // default save file configuration
            {
                InitializeDefaultSaveData();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start() // only called once (at program boot-up)
    {
        // load correct scene at start if applicable
        if (data.sceneName != SceneManager.GetActiveScene().name)
        {
            SceneManager.LoadScene(data.sceneName, LoadSceneMode.Single);
        }

        // physics instantiation
        Physics2D.gravity = new Vector2(0, -1 * GRAVITY_FORCE);
    }

    private void OnEnable() // called when script instance is made
    {
        // set OnSceneLoaded as the method that is called when the sceneLoaded event occurs
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable() // called when script instance is disabled
    {
        // removes OnSceneLoaded from event listener
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// called when a new scene is loaded
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // load UI elements (WHERE DOES THIS GO?!?!?!?)
        Instantiate(platformerCanvas);
    }

    private void OnApplicationQuit()
    {
        // save SavePointData to json file
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(Application.persistentDataPath + "/savedata.json", json);
    }

    // GETTERS -----------------------------------------------------------------------------------

    /// <summary>
    /// converts Vector2 spawnpoint to a Vector3
    /// </summary>
    /// <returns></returns>
    public Vector3 GetSpawnPoint()
    {
        return new Vector3(data.spawnPoint.x, data.spawnPoint.y, 0);
    }

    public string GetSceneName()
    {
        return data.sceneName;
    }

    public int GetHealth()
    {
        return data.health;
    }

    // SETTERS ----------------------------------------------------------------------------------

    public void SetSpawnPoint(Vector3 newSpawnPoint)
    {
        data.spawnPoint.x = newSpawnPoint.x;
        data.spawnPoint.y = newSpawnPoint.y;
    }

    // MODIFIERS ----------------------------------------------------------------------------------

    /// <summary>
    /// decrements health and reloads scene
    /// </summary>
    public void HazardCollision()
    {
        // decrement health
        data.health--;

        // game over state
        if (data.health <= 0)
        {
            InitializeDefaultSaveData(); // resets to initial save data default (until more involved save system is established)
        }

        // reload scene
        SceneManager.LoadScene(data.sceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// updates scene index and spawnpoint data and loads new scene
    /// </summary>
    /// <param name="transitionData"></param>
    public void TransitionScene(TransitionData transitionData)
    {
        // set new scene and spawnPoint in saveManager
        data.sceneName = transitionData.sceneName;
        data.spawnPoint = transitionData.position;
        // load new scene
        SceneManager.LoadScene(data.sceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// resets health to maximum
    /// </summary>
    public void RestoreHealth()
    {
        data.health = 9;
    }

    /// <summary>
    /// initializes save data to a default state (useful for if save file is missing or game state is reset after a game over)
    /// </summary>
    public void InitializeDefaultSaveData()
    {
        data.sceneName = "Scene0";
        data.spawnPoint = new Vector2(-14, 4);
        data.health = 9;
    }

}
