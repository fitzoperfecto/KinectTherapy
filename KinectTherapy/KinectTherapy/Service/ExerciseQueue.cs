﻿using System;
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
    public delegate void LoadIsStartedEventHandler(object sender, EventArgs e);
    public delegate void LoadIsDoneEventHandler(object sender, EventArgs e);
    public delegate void QueueIsDoneEventHandler(object sender, EventArgs e);

    public delegate void CatalogSelectionIsDoneEventHandler(object sender, EventArgs e);

    /// <summary>
    /// The exercise queue is the workout for a given patient. 
    /// It will contain the list of exercises which need to be done.
    /// It will also keep track of the current exercise/how many reps have been done
    /// Exercises will be loaded from a local xml file. This service then can be accessed by the UI 
    /// components to display the necessary data. 
    /// </summary>
    public class ExerciseQueue : IGameComponent //GameComponent
    {
        #region event stuff
        public event QueueIsDoneEventHandler QueueIsDone;

        // Invoke the Changed event; called whenever repetitions changes
        protected virtual void OnQueueComplete(EventArgs e)
        {
            if (QueueIsDone != null)
                QueueIsDone(this, e);
        }

        public event LoadIsStartedEventHandler LoadIsStarted;
        protected virtual void OnLoadStarted(EventArgs e)
        {
            if (LoadIsStarted != null)
                LoadIsStarted(this, e);
        }

        public event LoadIsDoneEventHandler LoadIsDone;
        protected virtual void OnLoadComplete(EventArgs e)
        {
            if (LoadIsDone != null)
                LoadIsDone(this, e);
        }

        public event CatalogSelectionIsDoneEventHandler SelectionIsDone;

        // ... or when catalog selection is complete
        protected virtual void OnSelectionComplete(EventArgs e)
        {
            if (SelectionIsDone != null)
                SelectionIsDone(this, e);
        }

        #endregion

        // what we need. 
        // list of exercises.
        public ExerciseGameComponent[] Exercises { get; internal set; }
        public Queue<ExerciseGameComponent> PendingExercises { get; internal set; }
        public Queue<ExerciseGameComponent> CompletedExercises { get; internal set; }
        public List<StartedRepetitionEventHandler> RepetitionStartedListener { get; internal set; }
        public int MostRecentComlpleteIndex = 0;
        public bool IsInitialized { get; internal set; }

        private Game _game;

        // an exercies game component for the current exercise.
        public ExerciseGameComponent CurrentExercise;

        // an exercise game component for the current exercise catalog session
        public ExerciseGameComponent CurrentCatalog;

        // a list of attributes needed by the UI

        public ExerciseQueue(Game game)
        {
            _game = game;
            PendingExercises = new Queue<ExerciseGameComponent>();
            CompletedExercises = new Queue<ExerciseGameComponent>();
            RepetitionStartedListener = new List<StartedRepetitionEventHandler>();

            // The queue will require fewer updates if it responds to the event
            // and will not 
            RepetitionStartedListener.Add(RepetitionIncreased);
        }

        public void AssociateFiles(object sender, RecordingStatusChangedEventArg args)
        {
            CurrentExercise.RepetitionToFileId.Add(args.FileId);
        }

        public void AssociateExercises(object sender, CatalogSelectionChangedEventArg args)
        {
            CurrentCatalog.ExerciseToCatalogId.Add(args.ExerciseId);
        }

        public void RepetitionIncreased(object sender, EventArgs args)
        {
            Update();
        }

        public void Initialize()
        {
            if (!IsInitialized)
            {
                // load the exercises. Should be done through content loader. will hardcode for now.
                Exercise[] GeneratedExercises = LoadExercises();
                // create a game component for each exercise.
                Exercises = new ExerciseGameComponent[GeneratedExercises.Length];
                int i = 0;
                foreach (Exercise Exercise in GeneratedExercises)
                {
                    ExerciseGameComponent egc = new ExerciseGameComponent(_game, Exercise);
                    Exercises[i++] = egc;
                    PendingExercises.Enqueue(egc);
                }

                NextExercise();

                IsInitialized = true;
            }
        }

        /// <summary>
        /// Used to pull a workout from the filesystem and insert into the exercise queue. 
        /// </summary>
        /// <returns></returns>
        private Exercise[] LoadExercises()
        {
            OnLoadStarted(EventArgs.Empty);
            Workout workout = null;
            // i know this is a terrible way to do this, but not sure a better way right now so sleepy
            string path = System.AppDomain.CurrentDomain.BaseDirectory + "../../../../KinectTherapyContent/Exercises/ArmExtensions.xml";

            XmlSerializer serializer = new XmlSerializer(typeof(Workout));
            StreamReader reader = new StreamReader(path);
            workout = (Workout)serializer.Deserialize(reader);
            reader.Close();
            OnLoadComplete(EventArgs.Empty);
            return workout.Exercises;
        }

        /// <summary>
        /// Used for hardcoding exercises
        /// </summary>
        /// <returns></returns>
        private Exercise[] generateExercises()
        {
            Exercise[] exercises = new Exercise[2];
            // Hardcoding Right Angle Right Elbow Vertex Criteria
            Exercise armExtensionExerciseRight = new Exercise();
            armExtensionExerciseRight.Name = "Right Arm Extension";
            armExtensionExerciseRight.Repetitions = 3;
            // criteria to start tracking a repetition
            XmlJointType[] otherJoints = new XmlJointType[2] { new XmlJointType("WristRight"), new XmlJointType("ShoulderRight") };
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

        //public override void Update(GameTime gameTime)
        private void Update()
        {
            // check to see if the reps for the exercise are complete.
            // if they are, move to the next exercise. 
            if (CurrentExercise.isExerciseComplete())
            {
                // remove component from game components
                _game.Components.Remove(CurrentExercise);

                // complete the exercise
                CompletedExercises.Enqueue(CurrentExercise);
                // see if we're completely done
                // if so, call people
                if (PendingExercises.Count > 0)
                {
                    NextExercise();
                }
                else
                {
                    OnQueueComplete(EventArgs.Empty);
                }
            }
        }

        private void NextExercise()
        {
            if (null != CurrentExercise)
            {
                foreach (StartedRepetitionEventHandler evt in RepetitionStartedListener)
                {
                    CurrentExercise.Changed -= evt;
                }
            }

            // grab the next (or first) one and add to game components
            CurrentExercise = PendingExercises.Dequeue();

            // be sure to add any listeners
            foreach (StartedRepetitionEventHandler evt in RepetitionStartedListener)
            {
                CurrentExercise.Changed += evt;
            }

            _game.Components.Add(CurrentExercise);
        }

    }
}
