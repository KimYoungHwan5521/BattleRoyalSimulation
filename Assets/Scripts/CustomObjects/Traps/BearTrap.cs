public class BearTrap : Trap
{
    protected override bool Trigger(bool rightLeg)
    {
        if (!base.Trigger(rightLeg)) return false;
        victim.TakeDamage(this, rightLeg ? InjurySite.RightFoot : InjurySite.LeftFoot);
        if (IsEnchanted) victim.Poisoning(setter);
        return true;
    }
}
