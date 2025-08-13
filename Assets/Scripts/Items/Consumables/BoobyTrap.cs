using UnityEngine;
using UnityEngine.Localization;

public abstract class BoobyTrap : Consumable
{
    protected Survivor setter;
    public Survivor Setter => setter;
    protected Box ownnerBox;
    bool isTriggered;
    public bool IsTriggered => isTriggered;

    public void SetSetter(Survivor setter, Box ownnerBox)
    {
        this.setter = setter;
        this.ownnerBox = ownnerBox;
        isTriggered = true;
    }

    public abstract void Trigger(Survivor victim);

    public BoobyTrap(ItemManager.Items itemType, LocalizedString itemName, float weight, int amount = 1) : base(itemType, itemName, weight, amount)
    {
    }
}

public class NoiseTrap : BoobyTrap
{
    public NoiseTrap(ItemManager.Items itemType, LocalizedString itemName, float weight, int amount = 1) : base(itemType, itemName, weight, amount)
    {
    }

    public override void Trigger(Survivor victim)
    {
        if (ownnerBox == null) return;
        ownnerBox.PlaySFX("alarm_short,1500");
    }
}

public class ChemicalTrap : BoobyTrap
{
    public ChemicalTrap(ItemManager.Items itemType, LocalizedString itemName, float weight, int amount = 1) : base(itemType, itemName, weight, amount)
    {
    }

    public override void Trigger(Survivor victim)
    {
        if (ownnerBox == null) return;
        PoolManager.Spawn(ResourceEnum.Prefab.GasLeak, ownnerBox.transform.position);
        var hits = Physics2D.OverlapCircleAll(ownnerBox.transform.position, 2f, LayerMask.GetMask("Survivor"));
        foreach (var hit in hits)
        {
            if (!hit.isTrigger && hit.TryGetComponent(out Survivor splashedSurvivor))
            {
                splashedSurvivor.Poisoning(setter);
            }
        }
    }
}

public class ShrapnelTrap : BoobyTrap
{
    public ShrapnelTrap(ItemManager.Items itemType, LocalizedString itemName, float weight, int amount = 1) : base(itemType, itemName, weight, amount)
    {
    }

    public override void Trigger(Survivor victim)
    {
        if (ownnerBox == null) return;
        SoundManager.Play(ResourceEnum.SFX.bang_04, ownnerBox.transform.position);
        for (int i=0; i < 48; i++)
        {
            GameObject prefab = PoolManager.Spawn(ResourceEnum.Prefab.Bullet, ownnerBox.transform.position);
            Bullet bullet = prefab.GetComponent<Bullet>();
            bullet.Initiate(setter, Vector2.up.Rotate(i * 7.5f));
        }
    }
}

public class ExplosiveTrap : BoobyTrap
{
    public ExplosiveTrap(ItemManager.Items itemType, LocalizedString itemName, float weight, int amount = 1) : base(itemType, itemName, weight, amount)
    {
    }

    public override void Trigger(Survivor victim)
    {
        if (ownnerBox == null) return;
        PoolManager.Spawn(ResourceEnum.Prefab.Explosion, ownnerBox.transform.position);
        victim.TakeDamage(this, 100);
        var hits = Physics2D.OverlapCircleAll(ownnerBox.transform.position, 2f, LayerMask.GetMask("Survivor"));
        foreach (var hit in hits)
        {
            if (!hit.isTrigger && hit.TryGetComponent(out Survivor splashedSurvivor))
            {
                if (splashedSurvivor == victim) continue;
                Vector2 closestPoint = hit.ClosestPoint(ownnerBox.transform.position);
                float distance = Mathf.Max(Vector2.Distance(ownnerBox.transform.position, closestPoint), 1);
                splashedSurvivor.TakeDamage(this, 100 / (distance * distance));
            }
        }
    }
}