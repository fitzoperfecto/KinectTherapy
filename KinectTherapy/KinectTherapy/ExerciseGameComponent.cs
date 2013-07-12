using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Kinect;
using SWENG.Service;
using SWENG.Criteria;

namespace SWENG
{
    // used for repetition starting
    public delegate void StartedRepetitionEventHandler(object sender, EventArgs e);

    public class ExerciseGameComponent : GameComponent
    {
        #region event stuff
        public event StartedRepetitionEventHandler Changed;

        // Invoke the Changed event; called whenever repetitions changes
        protected virtual void OnChanged(EventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }

        private Boolean _reptitionStarted;
        #endregion
        /// <summary>
        /// Gets the SkeletonPool from the services.
        /// </summary>
        private SkeletonPool skeletonPool
        {
            get
            {
                return (SkeletonPool)Game.Services.GetService(typeof(SkeletonPool));
            }
        }

        private Exercise _exercise;
        public Boolean RepetitionComplete { get; internal set; }
        
        public Boolean RepetitionStarted 
        {
            get 
            { 
                return _reptitionStarted; 
            }
            internal set 
            {
                if (_reptitionStarted != value && value == true)
                {
                    OnChanged(EventArgs.Empty);
                    repetition.Checkpoint = 0;
                }
                _reptitionStarted = value;
                

            }
        }
        
        public int Repetitions { get; internal set; }
        public int RepetitionsToComplete { get { return _exercise.Repetitions; } }
        public string Name { get; internal set; }
        public Repetition repetition;

        public List<string> RepetitionToFileId { get; internal set; }
        public List<string> ExerciseToCatalogId { get; internal set; }

        public ExerciseGameComponent(Game game,Exercise exercise)
            : base(game)
        {
            RepetitionComplete = false;
            RepetitionStarted = false;
            Repetitions = 0;
            _exercise = exercise;
            Name = exercise.Name;
            repetition = new Repetition(exercise);
            RepetitionToFileId = new List<string>();
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {

            // the stamp being processed
            SkeletonStamp skeletonStamp=skeletonPool.GetOldestActiveSkeleton();
            double[] percentBad = new double[20]; 

            // determine whether a rep has been started based on Exercise Start Criteria.
            if (skeletonStamp != null && skeletonStamp.GetTrackedSkeleton() != null)
            {
                if (!RepetitionStarted)
                {
                    // initialize the checkpoint to the 0 based checkpoint. 
                    repetition.Checkpoint = _exercise.Checkpoints.Length;
                    RepetitionStarted = repetition.isRepStarted(skeletonStamp);
                }
                else
                {
                    // this needs to be here so we do not count a rep twice by accident before the next draw occurs. 
                    // A couple updates could occur before the next draw. 
                    if (!RepetitionComplete)
                    {
                        // if the rep has been started we need to check the form of the repetition
                        // just a stub of what needs to be done... we'll need to determine how a FormResponse should look. 
                        percentBad = repetition.checkForm(skeletonStamp);
                        skeletonStamp.PercentBad = percentBad;
                        // see if the rep has been completed
                        if (RepetitionComplete = repetition.isRepComplete(skeletonStamp))
                        {
                            // increment reps completed and reset the flags
                            Repetitions++;
                            RepetitionComplete = RepetitionStarted = false;
                            // remove all the skeletons before this skeleton
                            skeletonPool.FinishedWithSkeleton(skeletonStamp.TimeStamp);
                        }
                    }
                }
            }
            // remove the skeleton stamp so it can move on
            if (skeletonStamp != null)
            {
                skeletonPool.FinishedWithSkeleton(skeletonStamp.TimeStamp);
            }

            base.Update(gameTime);
        }

        public bool isExerciseComplete()
        {
            return Repetitions >= _exercise.Repetitions;
        }
    }
}
