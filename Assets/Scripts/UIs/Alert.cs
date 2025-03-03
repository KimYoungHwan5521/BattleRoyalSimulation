using UnityEngine;

public class Alert : MonoBehaviour
{
    public void CloseAlert()
    {
        PoolManager.Despawn(gameObject);
    }
}
