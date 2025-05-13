namespace ResourceEnum
{
    public enum Prefab
    {
        Test,
        Survivor,
        Bullet,
        Rocket,
        Blood,
        BearTrap,
        LandMine,

        // Effects
        Explosion,
        GasLeak,

        // Map
        Map_2x2_01,
        Map_2x2_02,
        Map_2x2_03,
        Map_2x2_04,
        Map_2x2_05,
        Map_2x2_06,
        Map_2x2_07,
        Map_2x2_08,
        Map_2x2_09,
        Map_2x2_10,
        Map_3x3_01,
        Map_3x3_02,
        Map_3x3_03,
        Map_3x3_04,
        Map_3x3_05,
        Map_4x4_01,
        Map_4x4_02,
        Map_5x5_01,

        // UI
        Alert,
        SurvivorSchedule,
        CraftableAllow,

        // In Game UI
        KillLog,
        Headshot,
    }

    public enum NavMeshData
    {
        Map_2x2_01,
        Map_2x2_02,
        Map_2x2_03,
        Map_2x2_04,
        Map_2x2_05,
        Map_2x2_06,
        Map_2x2_07,
        Map_2x2_08,
        Map_2x2_09,
        Map_2x2_10,
        Map_3x3_01,
        Map_3x3_02,
        Map_3x3_03,
        Map_3x3_04,
        Map_3x3_05,
        Map_4x4_01,
        Map_4x4_02,
        Map_5x5_01,
    }

    public enum Sprite
    {
        Survivor,
        Box,
        // Melee Weapons
        Knife,
        Dagger,
        Bat,
        LongSword,
        Shovel,
        // RangedWeapon
        Revolver,
        Pistol,
        AssaultRifle,
        SubMachineGun,
        ShotGun,
        SniperRifle,
        Bazooka,
        // Bullets
        Bullet_Revolver,
        Bullet_Pistol,
        Bullet_AssaultRifle,
        Bullet_SubMachineGun,
        Bullet_ShotGun,
        Bullet_SniperRifle,
        Rocket_Bazooka,
        // BulletproofHats
        LowLevelBulletproofHelmet,
        MiddleLevelBulletproofHelmet,
        HighLevelBulletproofHelmet,
        // BulletproofVests
        LowLevelBulletproofVest,
        MiddleLevelBulletproofVest,
        HighLevelBulletproofVest,
        // Consumables
        BandageRoll,
        HemostaticBandageRoll,
        Poison,
        Antidote,
        Potion,
        // Traps
        BearTrap,
        LandMine,
        NoiseTrap,
        ChemicalTrap,
        ShrapnelTrap,
        ExplosiveTrap,
        // Crafting Materials
        AdvancedComponent,
        Components,
        Chemicals,
        Salvages,
        Gunpowder,
        // ETC
        WalkingAid,
        TrapDetectionDevice,
        BiometricRader,

        // Leagues
        BronzeLeague,
        SilverLeague,
        GoldLeague,
        SeasonChampionship,
        WorldChampionship,

        // Tiers
        Bronze,
        Silver,
        Gold,
    }

    public enum Material
    {
        Sight_Normal,
        Sight_Suspicious,
        Sight_Alert,
    }

    public enum BGM
    {

    }

    public enum SFX
    {
        assaultriflereload1,
        Handgun_reload,
        revolver_reload,
        Shotgun_singlebullet,
        Rack,
        metal_hit,
        bang_01, // Revolver
        bang_02, // Pistol
        bang_03, // Submuchinegun
        bang_04, // Shotgun
        bang_05, // Sniper riple
        bang_06, // Assult riple
        rocket_launch,
        ricochet,
        ricochet2,
        hit01,
        hit02,
        guard,
        avoid,
        farmingNoise01,
        farmingNoise02,
        farmingNoise03,
        farmingNoise04,
        piep,
        taping,
        hammering,
        hammering_once,
        bubble,
        bear_trap,
        explosion,
        short_gas_leak,
        alarm_short,
        water,
        drink,
    }
}