using UnityEngine;

public class TransitionData : MonoBehaviour
{
    [field: SerializeField] public string sceneName { get; private set; }
    [field: SerializeField] public Vector2 position { get; private set; }
}
