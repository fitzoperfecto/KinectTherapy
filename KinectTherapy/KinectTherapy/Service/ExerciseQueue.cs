using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SWENG.Criteria;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Kinect;
using System.Diagnostics;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;

namespace SWENG.Service
{
    /// <summary>
    /// The exercise queue is the workout for a given patient. 
    /// It will contain the list of exercises which need to be done.
    /// It will also keep track of the current exercise/how many reps have been done
    /// Exercises will be loaded from a local xml file. This service then can be accessed by the UI 
    /// components to display the necessary data. 
    /// </summary>
    public class ExerciseQueue : GameComponent
    {
        // what we need. 
        // list of exercises.
        public ExerciseGameComponent[] Exercises { get; internal set; }
        public Queue<ExerciseGameComponent> PendingExercises { get; internal set; }
        public Queue<ExerciseGameComponent> CompletedExercises { get; internal set; }
        public int mostRecentComlpleteIndex = 0;
        public bool isInitialized { get; internal set; }

        // an exercies game component for the current exercise.
        public ExerciseGameComponent currentExercise;
        // a list of attributes needed by the UI

        public ExerciseQueue(Game game)
            : base(game)
        {
            PendingExercises = new Queue<ExerciseGameComponent>();
            CompletedExercises = new Queue<ExerciseGameComponent>();
        }

        public override void Initialize()
        {
            if (!isInitialized)
            {
                // load the exercises. Should be done through content loader. will hardcode for now.
                //Exercise[] GeneratedExercises = generateExercises();
                Exercise[] GeneratedExercises = ReadExercises();
                // create a game component for each exercise.
                Exercises = new ExerciseGameComponent[GeneratedExercises.Length];
                int i = 0;
                foreach (Exercise Exercise in GeneratedExercises)
                {
                    ExerciseGameComponent egc = new ExerciseGameComponent(this.Game, Exercise);
                    Exercises[i++] = egc;
                    PendingExercises.Enqueue(egc);
                }
                // add remove the first element and make the current exercise being performed.
                currentExercise = PendingExercises.Dequeue();
                this.Game.Components.Add(currentExercise);
                isInitialized = true;
            }
            base.Initialize();
            
        }

        private Exercise[] ReadExercises()
        {
            Workout workout = null;
            // i know this is a terrible way to do this, but not sure a better way right now so sleepy
            string path = System.AppDomain.CurrentDomain.BaseDirectory +"../../../../KinectTherapyContent/Exercises/ArmExtensions.xml";

            XmlSerializer serializer = new XmlSerializer(typeof(Workout));
            StreamReader reader = new StreamReader(path);
            workout = (Workout)serializer.Deserialize(reader);
            reader.Close();
            return workout.Exercises;
        }

        private Exercise[] generateExercises()
        {
            Exercise[] exercises = new Exercise[2];
            // Hardcoding Right Angle Right Elbow Vertex Criteria
            Exercise armExtensionExerciseRight = new Exercise();
            armExtensionExerciseRight.Name = "Right Arm Extension";
            armExtensionExerciseRight.Repetitions = 3;
            // criteria to start tracking a repetition
            XmlJointType[] otherJoints = new XmlJointType[2] { new XmlJointType("WristRight"), new XmlJointType("ShoulderRight")};
            Criterion rightElbow = new AngleCriterion(90f, new XmlJointType("ElbowRight"), otherJoints, 10f);
            armExtensionExerciseRight.StartingCriteria = new Criterion[] { rightElbow };
            // criteria to track during the progress of a repetition
            // make sure hips stay horizontally aligned
            XmlJointType[] hipJoints = new XmlJointType[2] { new XmlJointType("HipLeft"), new XmlJointType("HipRight") };
            Criterion hips = new AlignmentCriterion(hipJoints, Alignment.Horizontal, 0.1f);
            armExtensionExerciseRight.TrackingCriteria = new Criterion[] { hips };

            // don't forget the left arm
            Exercise armExtensionExerciseLeft = new Exercise();
            armExtensionExerciseLeft.Name = "Left Arm Extension";
            armExtensionExerciseLeft.Repetitions = 3;
            // criteria to start tracking a repetition
            XmlJointType[] leftJoints = new XmlJointType[2] { new XmlJointType("WristLeft"), new XmlJointType("ShoulderLeft") };
            Criterion leftElbow = new AngleCriterion(90f, new XmlJointType("ElbowLeft"), leftJoints, 10f);
            armExtensionExerciseLeft.StartingCriteria = new Criterion[] { leftElbow };
            // criteria to track during the progress of a repetition
            // make sure hips stay horizontally aligned
            armExtensionExerciseLeft.TrackingCriteria = new Criterion[] { hips };

            exercises[0] = armExtensionExerciseRight;
            exercises[1] = armExtensionExerciseLeft;
            return exercises;
        }

        public override void Update(GameTime gameTime)
        {
            // check to see if the reps for the exercise are complete.
            // if they are, move to the next exercise. 
            if (currentExercise.isExerciseComplete())
            {
                // remove component from game components
                this.Game.Components.Remove(currentExercise);

                // complete the exercise
                CompletedExercises.Enqueue(currentExercise);
                // see if we're completely done
                if (PendingExercises.Count > 0)
                {
                    // grab the next one and add to game components
                    currentExercise = PendingExercises.Dequeue();
                    this.Game.Components.Add(currentExercise);
                }
            }
            base.Update(gameTime);
        }

        
    }
}
