
using System.Collections.Generic;
using ECS;

public class EntityStashHolder : Singleton<EntityStashHolder>
{
    public Dictionary<int, IComponentStash> PlayerStash;
    public Dictionary<int, IComponentStash> MeleeStash;

    void Start()
    {
        DontDestroyOnLoad(this);
        PlayerStash ??= new Dictionary<int, IComponentStash>();
        MeleeStash ??= new Dictionary<int, IComponentStash>();
    }
}
