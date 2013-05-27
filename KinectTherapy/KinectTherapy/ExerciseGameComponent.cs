﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SWENG;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Kinect;

namespace SWENG
{
    class ExerciseGameComponent : DrawableGameComponent
    {
        // queue of skeletons
        private SkeletonPool skeletonPool;
        private Boolean repComplete;
        private Boolean repStarted;
        private int reps;

        // these i dont really understand yet just testing 
        SpriteFont Font1;
        //Vector2 FontPos;

        private IRepetition repetition;

        public ExerciseGameComponent(Game game, SkeletonPool skeletonPool)
            : base(game)
        {
            //grab the reference to the skeleton pool
            this.skeletonPool = skeletonPool;
            this.repComplete = false;
            this.repStarted = false;
            this.reps = 0;
        }

        public override void Initialize()
        {
            base.Initialize();
            // may need a factory here to determine the repetition to use. for now hardcoding.
            //repetition = new TimerRepetition(); <<<<--- test of the repetition interface

            // Hardcoding Right Angle Right Elbow Vertex Criteria
            Criteria rightElbowRightAngleVertex = new Criteria();
            Microsoft.Kinect.JointType[] otherJoints = new Microsoft.Kinect.JointType[2] {Microsoft.Kinect.JointType.HandRight,Microsoft.Kinect.JointType.ShoulderRight};
            Criterion rightElbow = new AngleCriterion(270f,Microsoft.Kinect.JointType.ElbowRight,otherJoints,10f);
            rightElbowRightAngleVertex.addCriterion(Microsoft.Kinect.JointType.ElbowRight, new Criterion[]{rightElbow});
            // Retrive the exercise definition... for now we'll hardcode this
            repetition = new CriteriaRepetition(rightElbowRightAngleVertex);
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            this.Font1 = this.Game.Content.Load<SpriteFont>("Segoe16");
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
           
            // the stamp being processed
            SkeletonStamp skeletonStamp=skeletonPool.GetOldestSkeleton();

            // determine whether a rep has been started based on Exercise Start Criteria.
            if (skeletonStamp != null && skeletonStamp.getTrackedSkeleton()!=null)
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
                        // see if the rep has been completed
                        repComplete = repetition.isRepComplete(skeletonStamp);
                        if (repComplete)
                        {
                            reps++;
                        }
                    }
                }
            }
            // remove the skeleton stamp so it can move on
            if (skeletonStamp != null)
            {
                skeletonPool.Remove(skeletonStamp.TimeStamp);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            
            // reset repMonitor if a rep is complete and reset
            if (repComplete)
            {
                repComplete = repStarted = false;
            }
            // draw a number on the screen
            SpriteBatch spriteBatch = (SpriteBatch)this.Game.Services.GetService(typeof(SpriteBatch));
            // display the rep counted
            spriteBatch.Begin();
            // this being the line that answers your question
            spriteBatch.DrawString(this.Font1, reps + " strt:" + repStarted + " com:" + repComplete, new Vector2(450, 0), Color.Red);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}