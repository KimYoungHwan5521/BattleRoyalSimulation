using UnityEngine;

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

    public BoobyTrap(ItemManager.Items itemType, string itemName, float weight, int amount = 1) : base(itemType, itemName, weight, amount)
    {
    }
}

public class NoiseTrap : BoobyTrap
{
    public NoiseTrap(ItemManager.Items itemType, string itemName, float weight, int amount = 1) : base(itemType, itemName, weight, amount)
    {
    }

    public override void Trigger(Survivor victim)
    {
        ownnerBox.PlaySFX("alarm_short,1500");
    }
}

public class ChemicalTrap : BoobyTrap
{
    public ChemicalTrap(ItemManager.Items itemType, string itemName, float weight, int amount = 1) : base(itemType, itemName, weight, amount)
    {
    }

    public override void Trigger(Survivor victim)
    {
        PoolManager.Spawn(ResourceEnum.Prefab.GasLeak);
        var hits = Physics2D.CircleCastAll(ownnerBox.transform.position, 2f, Vector2.up);
        foreach (var hit in hits)
        {
            if (!hit.collider.isTrigger && hit.collider.TryGetComponent(out Survivor splashedSurvivor))
            {
                splashedSurvivor.Poisoning(setter);
            }
        }
    }
}

public class ShrapnelTrap : BoobyTrap
{
    public ShrapnelTrap(ItemManager.Items itemType, string itemName, float weight, int amount = 1) : base(itemType, itemName, weight, amount)
    {
    }

    public override void Trigger(Survivor victim)
    {
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
    public ExplosiveTrap(ItemManager.Items itemType, string itemName, float weight, int amount = 1) : base(itemType, itemName, weight, amount)
    {
    }

    public override void Trigger(Survivor victim)
    {
        PoolManager.Spawn(ResourceEnum.Prefab.Explosion);
        victim.TakeDamage(this, 100);
        var hits = Physics2D.CircleCastAll(ownnerBox.transform.position, 2f, Vector2.up);
        foreach (var hit in hits)
        {
            if (!hit.collider.isTrigger && hit.collider.TryGetComponent(out Survivor splashedSurvivor))
            {
                if (splashedSurvivor == victim) continue;
                float distance = Mathf.Max(Vector2.Distance(ownnerBox.transform.position, hit.point), 1);
                splashedSurvivor.TakeDamage(this, 100 / (distance * distance));
            }
        }
    }
}