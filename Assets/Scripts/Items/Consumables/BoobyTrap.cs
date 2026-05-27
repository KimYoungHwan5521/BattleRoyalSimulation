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

    public BoobyTrap(ItemManager.Items itemType, LocalizedString itemName, float weight, CraftingQuality quality = CraftingQuality.NotCrafted, int amount = 1) 
        : base(itemType, itemName, weight, quality, amount)
    {
    }
}

public class NoiseTrap : BoobyTrap
{
    float noiseVolume;
    public NoiseTrap(ItemManager.Items itemType, LocalizedString itemName, float weight, float noiseVolume, CraftingQuality quality = CraftingQuality.NotCrafted, int amount = 1) 
        : base(itemType, itemName, weight, quality, amount)
    {
        this.noiseVolume = noiseVolume;
    }

    public override void Trigger(Survivor victim)
    {
        if (ownnerBox == null) return;
        ownnerBox.PlaySFX($"alarm_short,{noiseVolume}");
    }
}

public class ChemicalTrap : BoobyTrap
{
    public ChemicalTrap(ItemManager.Items itemType, LocalizedString itemName, float weight, CraftingQuality quality = CraftingQuality.NotCrafted, int amount = 1) 
        : base(itemType, itemName, weight, quality, amount)
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
    float damage;
    public ShrapnelTrap(ItemManager.Items itemType, LocalizedString itemName, float weight, float damage, CraftingQuality quality = CraftingQuality.NotCrafted, int amount = 1) 
        : base(itemType, itemName, weight, quality, amount)
    {
        this.damage = damage;
    }

    public override void Trigger(Survivor victim)
    {
        if (ownnerBox == null) return;
        SoundManager.Play(ResourceEnum.SFX.bang_04, ownnerBox.transform.position);
        for (int i=0; i < 48; i++)
        {
            GameObject prefab = PoolManager.Spawn(ResourceEnum.Prefab.Bullet, ownnerBox.transform.position);
            Bullet bullet = prefab.GetComponent<Bullet>();
            bullet.Initiate(setter, Vector2.up.Rotate(i * 7.5f), damage);
        }
    }
}

public class ExplosiveTrap : BoobyTrap
{
    float damage;
    float explosionRange;
    public ExplosiveTrap(ItemManager.Items itemType, LocalizedString itemName, float weight, float damage, float explosionRange, CraftingQuality quality = CraftingQuality.NotCrafted, int amount = 1) 
        : base(itemType, itemName, weight, quality, amount)
    {
        this.damage = damage;
        this.explosionRange = explosionRange;
    }

    public override void Trigger(Survivor victim)
    {
        if (ownnerBox == null) return;
        GameObject explosion = PoolManager.Spawn(ResourceEnum.Prefab.Explosion, ownnerBox.transform.position);
        explosion.transform.localScale = new(explosionRange / 2f, explosionRange / 2f);
        victim.TakeDamage(this, damage);
        var hits = Physics2D.OverlapCircleAll(ownnerBox.transform.position, explosionRange, LayerMask.GetMask("Survivor"));
        foreach (var hit in hits)
        {
            if (!hit.isTrigger && hit.TryGetComponent(out Survivor splashedSurvivor))
            {
                if (splashedSurvivor == victim) continue;
                Vector2 closestPoint = hit.ClosestPoint(ownnerBox.transform.position);
                float distance = Mathf.Max(Vector2.Distance(ownnerBox.transform.position, closestPoint), 1);
                splashedSurvivor.TakeDamage(this, damage / (distance * distance));
            }
        }
    }
}