using Components;
using ECS;
using Tags;
using UnityEngine;

[System(ESystemCategory.Update)]
public class RangedAttackSystem : EcsSystem
{
    private int _filterId;

    public RangedAttackSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<AttackEvent>(),
                                                     Id<AttackCooldown>(),
                                                     Id<RangedWeapon>(),
                                                     Id<Ammo>(),
                                                     Id<Owner>(),
                                                     Id<DamageComponent>()));
    }

    public override void Tick(EcsWorld world)
    {
        foreach (var id in world.Enumerate(_filterId))
        {
            var attack = world.GetComponent<AttackEvent>(id);
            world.Remove<AttackEvent>(id);
            if (!AttackHelper.CheckAndUpdateAttackCooldown(world, id))
                continue;

            ref var ammo = ref world.GetComponentByRef<Ammo>(id).amount;
            if (ammo == 0)
                continue;
#if DEBUG
            if (ammo < 0)
                throw new System.Exception("negative ammo");
#endif

            ammo--;

            var ownerId = world.GetComponent<Owner>(id).entity.GetId();
            if (world.Have<Animator>(ownerId))
            {
                var animator = world.GetComponent<Animator>(ownerId);
                var playTime = world.GetComponent<AttackCooldown>(id).attackCD;
                AttackHelper.PlayAttackAnimationState(animator, playTime, "IsFiring");
            }

            Ray ray = new Ray(attack.position, attack.direction);
#if DEBUG
            Debug.DrawRay(attack.position, attack.direction * 100, Color.red, 5.0f);
#endif
            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit))
                continue;

            var targetView = hit.collider.gameObject.GetComponent<EntityView>();
            if (targetView == null)
                continue;

            var targetEntity = targetView.Entity;
            if (!world.IsEntityValid(targetEntity))
                continue;

            var targetEntityId = targetEntity.GetId();
            if (!world.Have<HealthComponent>(targetEntityId) || world.Have<Pickup>(targetEntityId))
                continue;

            world.Add(targetEntityId, world.GetComponent<DamageComponent>(id));
            world.Add(targetEntityId, new Impact { position = hit.point, normal = hit.normal });
        }
    }
}