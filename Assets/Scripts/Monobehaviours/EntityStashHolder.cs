
using System.Collections.Generic;

public enum EWeaponType
{
    Melee,
    Ranged,
    Projectile
}

//TODO: rename to PlayerStashHolder
public class EntityStashHolder : Singleton<EntityStashHolder>
{
    public float Health;
    public EWeaponType CurrentWeaponType;
    public Dictionary<EWeaponType, EntityView> Weapons;

    public EntityView Melee => GetWeaponViewByType(EWeaponType.Melee);
    public EntityView Ranged => GetWeaponViewByType(EWeaponType.Ranged);
    public EntityView Projectile => GetWeaponViewByType(EWeaponType.Projectile);
    
    private EntityView GetWeaponViewByType(EWeaponType type) => Weapons.ContainsKey(type) ? Weapons[type] : null;

    protected override void Init()
    {
        DontDestroyOnLoad(this);
        Weapons ??= new Dictionary<EWeaponType, EntityView>();
    }
}
