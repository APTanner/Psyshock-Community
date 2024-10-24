using Unity.Entities;

namespace Latios.Psyshock.Anna.Systems
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class AnnaSuperSystem : RootSuperSystem
    {
        protected override void CreateSystems()
        {
            GetOrCreateAndAddUnmanagedSystem<BuildEnvironmentCollisionLayerSystem>();
            GetOrCreateAndAddUnmanagedSystem<BuildKinematicCollisionLayerSystem>();
            GetOrCreateAndAddUnmanagedSystem<BuildRigidBodyCollisionLayerSystem>();
            GetOrCreateAndAddUnmanagedSystem<RigidBodyVsRigidBodySystem>();
            GetOrCreateAndAddUnmanagedSystem<RigidBodyVsEnvironmentSystem>();
            GetOrCreateAndAddUnmanagedSystem<RigidBodyVsKinematicSystem>();
            GetOrCreateAndAddUnmanagedSystem<SolveSystem>();
            GetOrCreateAndAddUnmanagedSystem<IntegrateRigidBodiesSystem>();
        }
    }
}

