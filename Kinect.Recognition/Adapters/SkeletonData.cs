namespace Kinect.Recognition.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Collections;
    using Microsoft.Research.Kinect.Nui;

    /// <summary>
    /// Represnts skeleton data in the component
    /// </summary>
    public interface ISkeletonData
    {
        /// <summary>
        /// Gets a collection of joints
        /// </summary>
        IEnumerable Joints { get; }

        /// <summary>
        /// Gets the position of the skeleton
        /// </summary>
        Vector Position { get; }

        /// <summary>
        /// Gets the tracking id for the skeleton
        /// </summary>
        int TrackingID { get; }

        /// <summary>
        /// Gets the tracking state of the skeleton
        /// </summary>
        SkeletonTrackingState TrackingState { get; }

        /// <summary>
        /// Gets the user index of the skeleton
        /// </summary>
        int UserIndex { get; }

        /// <summary>
        /// Obtains a specific joint
        /// </summary>
        /// <param name="idx">The joint id</param>
        /// <returns>A joint</returns>
        Joint JointAt(JointID jointId);
    }

    /// <summary>
    /// Duck typed SkeletonData object
    /// </summary>
    public class SkeletonDataAdapter : ISkeletonData
    {
        private SkeletonData adaptedData;

        /// <summary>
        /// default ctor
        /// </summary>
        /// <param name="data">the adapted object</param>
        public SkeletonDataAdapter(SkeletonData data)
        {
            this.adaptedData = data;
        }

        /// <summary>
        /// Gets a collection of joints
        /// </summary>
        public IEnumerable Joints { get { return this.adaptedData.Joints; } }

        /// <summary>
        /// Gets the position of the skeleton
        /// </summary>
        public Vector Position { get { return this.adaptedData.Position; } }

        /// <summary>
        /// Gets the tracking id for the skeleton
        /// </summary>
        public int TrackingID { get { return this.adaptedData.TrackingID; } }

        /// <summary>
        /// Gets the tracking state of the skeleton
        /// </summary>
        public SkeletonTrackingState TrackingState { get { return this.adaptedData.TrackingState; } }

        /// <summary>
        /// Gets the user index of the skeleton
        /// </summary>
        public int UserIndex { get { return this.adaptedData.UserIndex; } }

        /// <summary>
        /// Obtains a specific joint
        /// </summary>
        /// <param name="idx">The joint id</param>
        /// <returns>A joint</returns>
        public Joint JointAt(JointID jointId)
        {
            return this.adaptedData.Joints[jointId];
        }
    }

}
