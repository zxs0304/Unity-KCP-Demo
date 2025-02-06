using UnityEngine;

public class Effect : MonoBehaviour
{
    public float deadTime;
    void Start()
    {
        Destroy(gameObject,deadTime);
    }

}
