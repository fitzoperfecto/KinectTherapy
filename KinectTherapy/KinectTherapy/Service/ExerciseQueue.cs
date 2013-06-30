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

        public void RepetitionIncreased(object sender, EventArgs args)
        {
            Update();
        }

        public void Initialize()
        {
            // since it got moved to LoadExercises we probably don't need this anymore. 
            // Leaving stubbed out for now.
        }

        /// <summary>
        /// When the catalog notifies that the Exercises have been loaded and the patient is ready to exercise,
        /// The exercise queue will load up the exercises pushed into the CurrentCatalog. 
        /// 
        /// </summary>
        /// <returns></returns>
        public void LoadExercises(object sender, CatalogCompleteEventArg e)
        {
            OnLoadStarted(EventArgs.Empty);
            Exercise[] workout = new Exercise[e.Exercises.Length];
            string path = System.AppDomain.CurrentDomain.BaseDirectory + "../../../../KinectTherapyContent/Exercises/";
             //loop through the exercises in the CurrentCatalog and turn them into Exercise objects. 
            for (int i = 0; i < workout.Length; i++)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Exercise));
                StreamReader reader = new StreamReader(path + e.Exercises[i].Id + ".xml");
                // deserialize the xml and create an Exercise
                ExerciseGameComponent egc = new ExerciseGameComponent(_game, (Exercise)serializer.Deserialize(reader));
                reader.Close();
                Exercises[i] = egc;
                //Queue up for a workout
                PendingExercises.Enqueue(egc);
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
