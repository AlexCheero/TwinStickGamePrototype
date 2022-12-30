
using System.Collections.Generic;

public enum EWeaponType
{
    Melee,
    Ranged,
    Projectile
}

public struct WeaponStashData
{
    public EntityView Prefab;
    public int Ammo;
}

//TODO: rename to PlayerStashHolder
public class EntityStashHolder : Singleton<EntityStashHolder>
{
    public float Health;
    public EWeaponType CurrentWeaponType;
    public Dictionary<EWeaponType, WeaponStashData> WeaponPrefabs;

    public WeaponStashData Melee => GetWeaponViewByType(EWeaponType.Melee);
    public WeaponStashData Ranged => GetWeaponViewByType(EWeaponType.Ranged);
    public WeaponStashData Projectile => GetWeaponViewByType(EWeaponType.Projectile);
    
    private WeaponStashData GetWeaponViewByType(EWeaponType type) => WeaponPrefabs.ContainsKey(type) ? WeaponPrefabs[type] : default;

    protected override void Init()
    {
        DontDestroyOnLoad(this);
        WeaponPrefabs ??= new Dictionary<EWeaponType, WeaponStashData>();
    }
}
