using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct SkibidiTrainerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        ////SkibidiTrainerJob skibidiTrainerJob = new SkibidiTrainerJob { 
        //                                                // set the job arguments here

        //                                                //};

        //// schedule parallel uses job scheduler and if there is not enough jobs to split it will run on main thread
        ////skibidiTrainerJob.ScheduleParallel();


        
        //// Single threaded approach
        //foreach ((
        //    RefRO<LocalTransform> localTransform,
        //    RefRO<SkibidiTrainer> skibidiTrainer
        //    )
        //    in SystemAPI.Query<
        //        RefRO<LocalTransform>,
        //        RefRO<SkibidiTrainer>
        //    >()
        //    )
        //{
            
        //}
    }
}

//[BurstCompile]
//public partial struct SkibidiTrainerJob : IJobEntity
//{
//    //in for RO ref for RW
//    public void Execute(in LocalTransform localTransform, in SkibidiTrainer skibidiTrainer)
//    {
        
//    }
//}
