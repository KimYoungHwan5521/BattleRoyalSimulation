using UnityEngine;

public class Locked : MonoBehaviour
{
    public UnlockManager.UnlockCondition unlockCondition;
    Animator anim;
    bool isUnlocked;
    public bool IsUnlocked
    {
        get { return isUnlocked; }
    }

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void Unlock()
    {
        anim.SetBool("Unlock", true);
    }

    public void AE_UnlockCompletely()
    {
        isUnlocked = true;
        gameObject.SetActive(false);
    }
}
