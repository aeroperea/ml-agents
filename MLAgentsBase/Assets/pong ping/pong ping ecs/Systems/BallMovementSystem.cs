using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct BallMovementSystem : ISystem
{
    //[BurstCompile]
    //public void OnUpdate(ref SystemState state)
    //{
    //    //BallMovementJob ballMovementJob = new BallMovementJob { 
    //                                                    // set the job arguments here

    //                                                    //};

    //    // schedule parallel uses job scheduler and if there is not enough jobs to split it will run on main thread
    //    //ballMovementJob.ScheduleParallel();


        
    //    // Single threaded approach
    //    foreach ((
    //        RefRO<LocalTransform> localTransform,
    //        RefRO<BallMovement> ballMovement
    //        )
    //        in SystemAPI.Query<
    //            RefRO<LocalTransform>,
    //            RefRO<BallMovement>
    //        >()
    //        )
    //    {
            
    //    }
    //}
}

//[BurstCompile]
//public partial struct BallMovementJob : IJobEntity
//{
//    //in for RO ref for RW
//    public void Execute(in LocalTransform localTransform, in BallMovement ballMovement)
//    {
        
//    }
//}
