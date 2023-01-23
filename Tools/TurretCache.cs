namespace TurretTweaks.Tools;

internal class TurretCache
{
    internal string NameOG;
    internal float TurnRateOG;
    internal float AngleHorizontalOG;
    internal float AngleVerticalOG;
    internal float ViewDistanceOG;
    internal float AttackCoolDownOG;
    internal float AttackWarmUpOG;
    internal int MaxAmmoOG;

    internal TurretCache(string _name, float _turnRate, float _angleHorizontal, float _angleVertical,
        float _viewDistance, float _attackCoolDown, float _attackWarmUp, int _maxAmmo)

    {
        NameOG = _name;
        TurnRateOG = _turnRate;
        AngleHorizontalOG = _angleHorizontal;
        AngleVerticalOG = _angleVertical;
        ViewDistanceOG = _viewDistance;
        AttackCoolDownOG = _attackCoolDown;
        AttackWarmUpOG = _attackWarmUp;
        MaxAmmoOG = _maxAmmo;
    }

}