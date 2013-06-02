﻿using System;
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
    public class ExerciseGameComponent : GameComponent
    {
        // queue of skeletons
        // reference to the skeleton pool service
        private SkeletonPool skeletonPool
        {
            get
            {
                return (SkeletonPool)Game.Services.GetService(typeof(SkeletonPool));
            }
        }

        private SpriteBatch spriteBatch
        {
            get
            {
                return (SpriteBatch)this.Game.Services.GetService(typeof(SpriteBatch));
            }
        }

        private Exercise Exercise;
        public Boolean repComplete { get; internal set; }
        public Boolean repStarted { get; internal set; }
        public int reps { get; internal set; }
        public string Name { get; internal set; }
        private IRepetition repetition;

        public ExerciseGameComponent(Game game,Exercise exercise)
            : base(game)
        {
            this.repComplete = false;
            this.repStarted = false;
            this.reps = 0;
            this.Exercise = exercise;
            this.Name = exercise.Name;
            this.repetition = new CriteriaRepetition(exercise);
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {

            // the stamp being processed
            SkeletonStamp skeletonStamp=skeletonPool.GetOldestSkeleton();
            double[] percentBad = new double[20]; 

            // determine whether a rep has been started based on Exercise Start Criteria.
            if (skeletonStamp != null && skeletonStamp.GetTrackedSkeleton() != null)
            {
                if (!repStarted)
                {
                    repStarted = repetition.isRepStarted(skeletonStamp);
                }
                else
                {
                    // this needs to be here so we do not count a rep twice by accident before the next draw occurs. 
                    // A couple updates could occur before the next draw. 
                    if (!repComplete)
                    {
                        // if the rep has been started we need to check the form of the repetition
                        // just a stub of what needs to be done... we'll need to determine how a FormResponse should look. 
                        percentBad = repetition.checkForm(skeletonStamp);

                        // see if the rep has been completed
                        if (repComplete = repetition.isRepComplete(skeletonStamp))
                        {
                            // increment reps completed and reset the flags
                            reps++;
                            repComplete = repStarted = false;
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
            return reps >= Exercise.Repetitions;
        }
    }
}
