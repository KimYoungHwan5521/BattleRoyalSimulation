using UnityEngine;

public class Locked : MonoBehaviour
{
    public UnlockManager.UnlockCondition unlockCondition;
    Animator anim => GetComponent<Animator>();
    bool readyUnlockAnim;
    bool isUnlocked;
    [SerializeField] bool notNeedRelock;
    public bool NotNeedRelock => notNeedRelock;

    public bool IsUnlocked
    {
        get { return isUnlocked; }
    }

    private void Awake()
    {
        //anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (readyUnlockAnim && gameObject.activeInHierarchy)
        {
            anim.SetBool("Unlock", true);
        }
    }

    public void Unlock()
    {
        if (isUnlocked == true) return;
        readyUnlockAnim = true;
        isUnlocked = true;
    }

    public void Lock()
    {
        gameObject.SetActive(true);
        anim.SetBool("Unlock", false);
        readyUnlockAnim = false;
        isUnlocked = false;
    }

    public void AE_UnlockCompletely()
    {
        readyUnlockAnim = false;
        gameObject.SetActive(false);
    }
}
