namespace Kinect.Recognition.States
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Research.Kinect.Nui;
    using Kinect.Recognition.Tracking;
    using Kinect.Recognition.Gestures;
    using Kinect.Recognition.Extensions;
    using System.Collections;

    public class StateIdle : StateBase
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public StateIdle()
            : base(FSMStateId.Idle)
        {
        }

        public override void StateEntered(TrackingContext context)
        {
            base.StateEntered(context);
            context.CurrentCue = null;
        }

        /// <summary>
        /// Observe all skeletons
        /// </summary>
        /// <param name="skeletons">all skeletons</param>
        /// <returns>An iterator over all received skeletons</returns>
        protected override IEnumerable<SkeletonData> ObservableSkeletons(SkeletonData[] skeletons)
        {
            foreach (SkeletonData skeleton in skeletons)
                if (skeleton.TrackingState != SkeletonTrackingState.NotTracked)
                    yield return skeleton;
        }

        /// <summary>
        /// Allows individual skeleton tracking.
        /// </summary>
        /// <param name="origData">the original skeleton event data</param>
        /// <returns>true if traversing should continue with next skeleton, otherwise false</returns>
        protected override bool ProcessSkeleton(SkeletonData origData)
        {
            bool foundSkeleton = false;
            int cueJointId = this.SkeletonNeedsAttention(origData);

            if (RecognitionConstants.InvalidJointId != cueJointId)
            {
                foundSkeleton = true;
                base.Context.ActiveSkeleton = origData.TrackingID;
                base.Context.CurrentCue = (JointID)cueJointId;
                base.Controller.PerformTransition(FSMEventId.WaitForCommand);
            }

            return !foundSkeleton;
        }

        /// <summary>
        /// Checks if this skeleton is trying to get attention
        /// </summary>
        /// <param name="origData">the skeleton data</param>
        /// <returns>The cue joint id if such is identified, otherwise -1</returns>
        /// <remarks>
        /// Joint is considered requiring attention if it's raised at the same 
        /// height as the skeletons head or higher
        /// </remarks>
        private int SkeletonNeedsAttention(SkeletonData origData)
        {
            // get the head and the limb raised the highest (as this one has the highest chance 
            // of being raised to draw attention)
            Joint head = origData.Joints[JointID.Head];
            Joint cueJoint = this.GetCandidateCueJoints(origData.Joints).Max<Joint, float>(j => j.Position.Y, (f1, f2) => { return f1 - f2; });

            return (this.IsItCueJoint(cueJoint) && head.Position.Y <= cueJoint.Position.Y) ? (int)cueJoint.ID : RecognitionConstants.InvalidJointId;
        }

        /// <summary>
        /// Returns the possible cue joints
        /// </summary>
        /// <param name="joints">A collection of joints</param>
        /// <returns>An enumerable of candidate cue joints</returns>
        private IEnumerable<Joint> GetCandidateCueJoints(IEnumerable joints)
        {
            foreach (Joint j in joints)
            {
                if (this.IsItCueJoint(j))
                    yield return j;
            }
        }

        /// <summary>
        /// Checks if a given joint can be used as a cue joint.
        /// </summary>
        /// <param name="j">The joint to be checked</param>
        /// <returns>true if it can be used as cue, otherwise false.</returns>
        private bool IsItCueJoint(Joint j)
        {
            bool result = false;

            switch (j.ID)
            {
                case JointID.HandRight:
                case JointID.HandLeft:
                    result = true;
                    break;
            }

            return result;
        }
    }
}
