﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using SWENG.Criteria;

namespace SWENG.Service
{
    public delegate void LoadIsStartedEventHandler(object sender, EventArgs e);
    public delegate void LoadIsDoneEventHandler(object sender, EventArgs e);
    public delegate void QueueIsDoneEventHandler(object sender, EventArgs e);

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

        // Called whenever the exercise queue starts loading exercises.
        public event LoadIsStartedEventHandler LoadIsStarted;
        protected virtual void OnLoadStarted(EventArgs e)
        {
            if (LoadIsStarted != null)
                LoadIsStarted(this, e);
        }

        // called whenever the exercise queue is finished loading exercises.
        public event LoadIsDoneEventHandler LoadIsDone;
        protected virtual void OnLoadComplete(EventArgs e)
        {
            if (LoadIsDone != null)
                LoadIsDone(this, e);
        }
        #endregion

        public ExerciseGameComponent[] Exercises { get; internal set; }
        public Queue<ExerciseGameComponent> PendingExercises { get; internal set; }
        public Queue<ExerciseGameComponent> CompletedExercises { get; internal set; }
        public List<StartedRepetitionEventHandler> RepetitionStartedListener { get; internal set; }
        public int MostRecentComlpleteIndex = 0;
        public bool IsInitialized { get; internal set; }

        private Game _game;

        // an exercise game component for the current exercise.
        public ExerciseGameComponent CurrentExercise;

        public ExerciseQueue(Game game)
        {
            _game = game;
            ReInitialize();
            RepetitionStartedListener = new List<StartedRepetitionEventHandler>();

            // The queue will require fewer updates if it responds to the event
            // and will not 
            RepetitionStartedListener.Add(RepetitionIncreased);
        }

        public void AssociateFiles(object sender, RecordingStatusChangedEventArg args)
        {
            CurrentExercise.RepetitionToFileId.Add(args.FileId);
        }

        public void RepetitionIncreased(object sender, EventArgs args)
        {
            Update();
        }

        public void Initialize()
        {
            // since it got moved to LoadExercises we probably don't need this anymore. 
            // Leaving stubbed out for now.
        }

        private void ReInitialize()
        {
            PendingExercises = new Queue<ExerciseGameComponent>();
            CompletedExercises = new Queue<ExerciseGameComponent>();
        }

        /// <summary>
        /// When the catalog notifies that the Exercises have been loaded and the patient is ready to exercise,
        /// The exercise queue will load up the exercises pushed into the CurrentCatalog. 
        /// 
        /// </summary>
        /// <returns></returns>
        public void LoadExercises(object sender, CatalogCompleteEventArg e)
        {
            ReInitialize();
            OnLoadStarted(EventArgs.Empty);
            string path = System.AppDomain.CurrentDomain.BaseDirectory + "../../../../KinectTherapyContent/Exercises/";

            Exercises = new ExerciseGameComponent[e.Exercises.Length];

             //loop through the exercises in the CurrentCatalog and turn them into Exercise objects. 
            for (int i = 0; i < e.Exercises.Length; i++)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Exercise));
                StreamReader reader = new StreamReader(path + e.Exercises[i].Id + ".xml");
                // deserialize the xml and create an Exercise
                Exercise temp = (Exercise)serializer.Deserialize(reader);
                if (null != e.Exercises[i].Repetitions)
                {
                    temp.Repetitions = e.Exercises[i].Repetitions;
                }
                if (null != e.Exercises[i].Variance)
                {
                    temp.Variance = e.Exercises[i].Variance;
                }
                Exercises[i] = new ExerciseGameComponent(_game, temp);
                reader.Close();

                //Queue up for a workout
                PendingExercises.Enqueue(Exercises[i]);
            }
            // once they're all queued start the first exercise. 
            NextExercise();
            OnLoadComplete(EventArgs.Empty);
        }

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

        // moves the shifts the workout to the next exercise
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
