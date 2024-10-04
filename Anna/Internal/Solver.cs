using Latios.Transforms;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Latios.Psyshock.Anna
{
    internal struct SolveBodiesProcessor : IForEachPairProcessor
    {
        [NativeDisableParallelForRestriction] public NativeArray<CapturedRigidBodyState> states;
        [ReadOnly] public NativeArray<UnitySim.Velocity>                                 kinematicVelocities;
        public float                                                                     invNumSolverIterations;
        public float                                                                     deltaTime;
        public bool                                                                      firstIteration;
        public bool                                                                      lastIteration;

        public InstantiateCommandBuffer<WorldTransform>.ParallelWriter icb;

        public void Execute(ref PairStream.Pair pair)
        {
            var statesSpan = states.AsSpan();

            if (pair.userByte == SolveByteCodes.contactEnvironment)
            {
                ref var           streamData          = ref pair.GetRef<ContactStreamData>();
                ref var           rigidBodyA          = ref statesSpan[streamData.indexA];
                UnitySim.Velocity environmentVelocity = default;
                UnitySim.SolveJacobian(ref rigidBodyA.velocity,
                                       in rigidBodyA.mass,
                                       in rigidBodyA.motionStabilizer,
                                       ref environmentVelocity,
                                       default,
                                       UnitySim.MotionStabilizer.kDefault,
                                       streamData.contactParameters.AsSpan(),
                                       streamData.contactImpulses.AsSpan(),
                                       in streamData.bodyParameters,
                                       false,
                                       invNumSolverIterations,
                                       out _);

                if (firstIteration)
                {
                    if (UnitySim.IsStabilizerSignificantBody(rigidBodyA.mass.inverseMass, 0f))
                        rigidBodyA.numOtherSignificantBodiesInContact++;
                }
            }
            else if (pair.userByte == SolveByteCodes.contactKinematic)
            {
                ref var streamData          = ref pair.GetRef<ContactStreamData>();
                ref var rigidBodyA          = ref statesSpan[streamData.indexA];
                var     environmentVelocity = kinematicVelocities[streamData.indexB];
                UnitySim.SolveJacobian(ref rigidBodyA.velocity,
                                       in rigidBodyA.mass,
                                       in rigidBodyA.motionStabilizer,
                                       ref environmentVelocity,
                                       default,
                                       UnitySim.MotionStabilizer.kDefault,
                                       streamData.contactParameters.AsSpan(),
                                       streamData.contactImpulses.AsSpan(),
                                       in streamData.bodyParameters,
                                       false,
                                       invNumSolverIterations,
                                       out _);

                if (firstIteration)
                {
                    if (UnitySim.IsStabilizerSignificantBody(rigidBodyA.mass.inverseMass, 0f))
                        rigidBodyA.numOtherSignificantBodiesInContact++;
                }
            }
            else if (pair.userByte == SolveByteCodes.contactBody)
            {
                ref var streamData = ref pair.GetRef<ContactStreamData>();
                ref var rigidBodyA = ref statesSpan[streamData.indexA];
                ref var rigidBodyB = ref statesSpan[streamData.indexB];
                UnitySim.SolveJacobian(ref rigidBodyA.velocity,
                                       in rigidBodyA.mass,
                                       in rigidBodyA.motionStabilizer,
                                       ref rigidBodyB.velocity,
                                       in rigidBodyB.mass,
                                       in rigidBodyB.motionStabilizer,
                                       streamData.contactParameters.AsSpan(),
                                       streamData.contactImpulses.AsSpan(),
                                       in streamData.bodyParameters,
                                       false,
                                       invNumSolverIterations,
                                       out _);
                if (firstIteration)
                {
                    if (UnitySim.IsStabilizerSignificantBody(rigidBodyA.mass.inverseMass, rigidBodyB.mass.inverseMass))
                        rigidBodyA.numOtherSignificantBodiesInContact++;
                    if (UnitySim.IsStabilizerSignificantBody(rigidBodyB.mass.inverseMass, rigidBodyA.mass.inverseMass))
                        rigidBodyB.numOtherSignificantBodiesInContact++;
                }
            }
        }
    }
}
