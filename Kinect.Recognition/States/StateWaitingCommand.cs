namespace Kinect.Recognition.States
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Kinect.Recognition.Tracking;
    using Microsoft.Research.Kinect.Nui;
    using System.Diagnostics;
    using System.Collections;
    using Kinect.Recognition.Gestures;
    using Kinect.Recognition.Adapters;

    public class StateWaitingCommand : StateTrackingBase
    {
        private DTWRecognizer recognizer;
        private long lastRecognitionTime;

        /// <summary>
        /// default ctor
        /// </summary>
        public StateWaitingCommand() : base(FSMStateId.WaitingForCommand)
        {
            this.recognizer = new DTWRecognizer();
        }

        /// <summary>
        /// Returns true if the state is functional
        /// </summary>
        public override bool Initialized
        {
            get
            {
                return this.recognizer != null && base.Initialized;
            }
        }


        /// <summary>
        /// Performs initialization upon entering the state
        /// </summary>
        /// <param name="context">The shared context</param>
        public override void StateEntered(TrackingContext context)
        {
            base.StateEntered(context);

            recognizer.Clear();
            recognizer.SequenceDimensionSize = context.TrackingDimensionality;

            this.LoadGesturesToMatch();
        }

        /// <summary>
        /// Allows individual skeleton tracking.
        /// </summary>
        /// <param name="origData">the original skeleton event data</param>
        /// <returns>true if traversing should continue with next skeleton, otherwise false</returns>
        protected override bool ProcessSkeleton(ISkeletonData origData)
        {
            if (this.CanHandleFrame)
            {
                IGesture gesture = null;
                ArrayList frameBuffer = base.FrameBuffer;

                // handle recognition if we have some sequence already
                if (frameBuffer.Count > base.Context.MinFrames)
                {
                    gesture = this.recognizer.Recognize(frameBuffer);
                    if (gesture != null && gesture.Id != GestureId.Unknown)
                    {
                        base.Controller.RaiseGestureRecognizedEvent(new GestureRecognizedEventArgs(gesture, 
                            Configuration.Instance.GestureTransitions[new KeyValuePair<FSMStateId,GestureId>(base.Id, gesture.Id)]));

                        this.ResetState();
                    }

                    // check if current state should be exited (idle or unsuccessful for too long)
                    this.ExitStateOnIdle();
                }

                // maintain buffer size
                if (frameBuffer.Count > base.Context.MaxFrames)
                    frameBuffer.RemoveAt(0);

                // add current skeleton data to recognition buffer
                SkeletonTrackingData trackingData = new SkeletonTrackingData(origData, base.Context.IsJointTracked, this.Context);
                frameBuffer.Add(trackingData.DTWData);
            }

            // result doesn't matter as this state only handles 1 skeleton at a time
            return false;
        }

        /// <summary>
        /// Initializes the local state
        /// </summary>
        protected override void ResetState()
        {
            base.ResetState();
            this.lastRecognitionTime = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Checks if the current skeleton is idle and the state should be exited.
        /// </summary>
        private void ExitStateOnIdle()
        {
            long currentTime = DateTime.Now.Ticks;

            if (currentTime - lastRecognitionTime > new TimeSpan(0, 0, RecognitionConstants.MaxIdleSeconds).Ticks)
            {
                // move back to Idle state
                base.Controller.PerformTransition(FSMEventId.GoIdle);
            }
        }

        /// <summary>
        /// Loads the gestures based on the current state
        /// </summary>
        private void LoadGesturesToMatch()
        {
            Configuration cfg = Configuration.Instance;

            foreach (var pair in cfg.GestureTransitions)
            {
                if (pair.Key.Key == base.Id)
                {
                    GestureId gestureId = pair.Key.Value;
                    this.recognizer.AddPatterns(gestureId, cfg.Gestures[gestureId], cfg.GestureSettings[gestureId]);
                }
            }
        }                
    }
}
