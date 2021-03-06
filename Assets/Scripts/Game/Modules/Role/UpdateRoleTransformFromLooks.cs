using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

//TODO:this solution is temporary
[DisableAutoCreation]
public class UpdateRoleTransformFromLooks : BaseComponentSystem<RoleState>
{
    // private ComponentGroup Group;
    
    public UpdateRoleTransformFromLooks(GameWorld world) : base(world) {}

    protected override void Update(Entity entity, RoleState roleState)
    {
        if (roleState.looksEntity == Entity.Null || !EntityManager.HasComponent<UnityMMO.MainRoleTag>(entity))
            return;

        var looksTrans = EntityManager.GetComponentObject<Transform>(roleState.looksEntity);
        roleState.transform.position = looksTrans.position;
        roleState.transform.rotation = looksTrans.rotation;
        var pos = new Position();
        pos.Value.x = looksTrans.position.x;
        pos.Value.y = looksTrans.position.y;
        pos.Value.z = looksTrans.position.z;
        EntityManager.SetComponentData<Position>(entity, pos);
    }
}