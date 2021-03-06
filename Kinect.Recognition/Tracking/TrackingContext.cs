﻿namespace Kinect.Recognition.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Research.Kinect.Nui;
    using System.Diagnostics;

    /// <summary>
    /// Number of gemoetrical dimensions used for tracking
    /// </summary>
    public enum TrackingMode
    {
        Mode2D = 2,
        Mode3D = 3
    }

    public class TrackingContext
    {
        private int activeSkeletonId;

        /// <summary>
        /// Default ctor
        /// </summary>
        public TrackingContext()
        {
            this.activeSkeletonId = RecognitionConstants.InvalidSkeletonIdx;
        }

        /// <summary>
        /// Current cue joint
        /// </summary>
        public virtual JointID? CurrentCue { get; set; }

        /// <summary>
        /// Currently active skeleton
        /// </summary>
        public virtual int ActiveSkeleton 
        {
            get
            {
                Debug.Assert(this.activeSkeletonId != RecognitionConstants.InvalidSkeletonIdx, "index can't be -1");
                return this.activeSkeletonId;
            }
            set
            {
                if (value == RecognitionConstants.InvalidSkeletonIdx)
                    throw new InvalidOperationException("can't set invalid skeleton idx");
                
                activeSkeletonId = value;
            }
        }

        /// <summary>
        /// Returns the tracking mode as specified by the user
        /// </summary>
        public TrackingMode TrackingMode { get; set; }

        /// <summary>
        /// Returns the maximum number of frames tracked per gesture
        /// </summary>
        public int MaxFrames { get; set; }

        /// <summary>
        /// Returns the minimum number of frames tracked per gesture
        /// </summary>
        public int MinFrames { get; set; }

        /// <summary>
        /// Returns the number of frames skipped during continuous recognition
        /// </summary>
        public int SkipFrames { get; set; }

        /// <summary>
        /// Determines if a joint is being tracked for gesture recognition purposes,
        /// with respect to the currently selected cue joint
        /// </summary>
        /// <param name="joint">The joint to be checked</param>
        /// <returns>True if the joint is tracked, otherwise false.</returns>
        public virtual bool IsJointTracked(Joint joint)
        {
            bool result = false;

            if (this.CurrentCue != null)
            {
                JointID id = joint.ID;

                switch (this.CurrentCue)
                {
                    case JointID.HandLeft:
                        result = id == JointID.HandLeft ||
                                 id == JointID.ElbowLeft ||
                                 id == JointID.WristLeft;
                        break;
                    case JointID.HandRight:
                        result = id == JointID.HandRight ||
                                 id == JointID.ElbowRight ||
                                 id == JointID.WristRight;
                        break;
                    default:
                        result = false;
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Dimensionality of the tracking sequence
        /// </summary>
        /// <remarks>This is the product of the supported geometrical dimensionality
        /// and the number of tracked joints</remarks>
        public virtual int TrackingDimensionality
        {
            get
            {
                int result = 0;

                switch (this.CurrentCue)
                {
                    case JointID.HandLeft:
                    case JointID.HandRight:
                        result = 3;
                        break;
                    default:
                        break;
                }

                return result * (int)this.TrackingMode;
            }
        }
    }
}
