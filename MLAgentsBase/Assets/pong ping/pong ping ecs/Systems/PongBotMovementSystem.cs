using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct PongBotMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        ////PongBotMovementJob pongBotMovementJob = new PongBotMovementJob { 
        //                                                // set the job arguments here

        //                                                //};

        //// schedule parallel uses job scheduler and if there is not enough jobs to split it will run on main thread
        ////pongBotMovementJob.ScheduleParallel();


        
        //// Single threaded approach
        //foreach ((
        //    RefRO<LocalTransform> localTransform,
        //    RefRO<PongBotMovement> pongBotMovement
        //    )
        //    in SystemAPI.Query<
        //        RefRO<LocalTransform>,
        //        RefRO<PongBotMovement>
        //    >()
        //    )
        //{
            
        //}
    }
}

//[BurstCompile]
//public partial struct PongBotMovementJob : IJobEntity
//{
//    //in for RO ref for RW
//    public void Execute(in LocalTransform localTransform, in PongBotMovement pongBotMovement)
//    {
        
//    }
//}
