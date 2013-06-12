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

        private Exercise Exercise;
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
                }
                _reptitionStarted = value;
                

            }
        }
        
        public int Repetitions { get; internal set; }
        public string Name { get; internal set; }
        public CriteriaRepetition repetition;

        public List<string> RepetitionToFileId { get; internal set; }

        public ExerciseGameComponent(Game game,Exercise exercise)
            : base(game)
        {
            this.RepetitionComplete = false;
            this.RepetitionStarted = false;
            this.Repetitions = 0;
            this.Exercise = exercise;
            this.Name = exercise.Name;
            this.repetition = new CriteriaRepetition(exercise);
            RepetitionToFileId = new List<string>();
        }

        public override void Initialize()
        {
            base.Initialize();
            // initialize the checkpoint to the 0 based checkpoint. 
            repetition.Checkpoint = 0;
        }

        public override void Update(GameTime gameTime)
        {

            // the stamp being processed
            SkeletonStamp skeletonStamp=skeletonPool.GetOldestSkeleton();
            double[] percentBad = new double[20]; 

            // determine whether a rep has been started based on Exercise Start Criteria.
            if (skeletonStamp != null && skeletonStamp.GetTrackedSkeleton() != null)
            {
                if (!RepetitionStarted)
                {
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

                        // see if the rep has been completed
                        if (RepetitionComplete = repetition.isRepComplete(skeletonStamp))
                        {
                            // increment reps completed and reset the flags
                            Repetitions++;
                            RepetitionComplete = RepetitionStarted = false;
                            // remove all the skeletons before this skeleton
                            skeletonPool.Remove(skeletonStamp.TimeStamp);
                        }
                    }
                }
            }
            // remove the skeleton stamp so it can move on
            if (skeletonStamp != null)
            {
                skeletonPool.Remove(skeletonStamp.TimeStamp);
            }

            base.Update(gameTime);
        }

        public bool isExerciseComplete()
        {
            return Repetitions >= Exercise.Repetitions;
        }
    }
}
