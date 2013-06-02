using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SWENG.Service
{
    /// <summary>
    /// Needed in order to allow the ExerciseQueue to be both a GameComponent and a Service
    /// </summary>
    public interface IExerciseQueue
    {
        ExerciseGameComponent[] Exercises { get; }
        Queue<ExerciseGameComponent> PendingExercises { get; }
        Queue<ExerciseGameComponent> CompletedExercises { get; }
    }
}
