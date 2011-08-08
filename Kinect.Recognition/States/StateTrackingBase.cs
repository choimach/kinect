namespace Kinect.Recognition.States
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Collections;
    using Kinect.Recognition.Tracking;
    using Microsoft.Research.Kinect.Nui;
    using System.Diagnostics;

    public class StateTrackingBase : StateBase
    {
        private ArrayList patternBuffer;
        private int frameCounter;

        /// <summary>
        /// Default ctor
        /// </summary>
        /// <param name="id">The state id</param>
        public StateTrackingBase(FSMStateId id)
            : base(id)
        {
            this.ResetState();
        }

        /// <summary>
        /// Returns true if the current frame should be handled
        /// </summary>
        protected virtual bool CanHandleFrame
        {
            get
            {
                frameCounter = (frameCounter + 1) % base.Context.SkipFrames;
                return frameCounter == 0;
            }
        }

        /// <summary>
        /// Gets the frames buffer
        /// </summary>
        protected ArrayList FrameBuffer
        {
            get
            {
                return this.patternBuffer;
            }
        }

        /// <summary>
        /// Handles state initialization
        /// </summary>
        /// <param name="context">The context</param>
        public override void StateEntered(TrackingContext context)
        {
            base.StateEntered(context);

            this.ResetState();
        }

        /// <summary>
        /// Returns the active skeleton for tracking
        /// </summary>
        /// <param name="skeletons">A list of skeletons</param>
        /// <returns>The active skeleton</returns>
        protected override IEnumerable<SkeletonData> ObservableSkeletons(SkeletonData[] skeletons)
        {
            int skeletonIdx = base.Context.ActiveSkeleton;

            foreach (SkeletonData skeleton in skeletons)
                if (skeleton.TrackingState != SkeletonTrackingState.NotTracked && skeleton.TrackingID == skeletonIdx)
                {
                    yield return skeleton;
                    break;
                }
        }

        /// <summary>
        /// Resets the local object state
        /// </summary>
        protected virtual void ResetState()
        {
            this.frameCounter = 0;
            this.patternBuffer = new ArrayList(base.Context.MaxFrames);
        }
    }
}
