public class BearTrap : Trap
{
    protected override void Trigger(bool rightLeg)
    {
        base.Trigger(rightLeg);
        victim.TakeDamage(this, rightLeg ? InjurySite.RightAncle : InjurySite.LeftAncle);
        if (IsEnchanted) victim.Poisoning(setter);
    }
}
