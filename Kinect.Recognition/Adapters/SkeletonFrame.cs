namespace Kinect.Recognition.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Research.Kinect.Nui;

    /// <summary>
    /// Skeleton frame interface
    /// </summary>
    public interface ISkeletonFrame
    {
        /// <summary>
        /// Gets an array of skeleton data implemeting objects
        /// </summary>
        IEnumerable<ISkeletonData> Skeletons { get; }

        /// <summary>
        /// Gets the frame #
        /// </summary>
        int FrameNumber { get; }

        /// <summary>
        /// Gets the time stamp of the skeleton frame
        /// </summary>
        long TimeStamp { get; }

        /// <summary>
        /// Gets the floor plane
        /// </summary>
        Vector FloorClipPlane { get; }
    }

    /// <summary>
    /// Duck typed SkeletonFrame object
    /// </summary>
    public class SkeletonFrameAdapter : ISkeletonFrame
    {
        private SkeletonFrame adaptedFrame;

        /// <summary>
        /// default ctor
        /// </summary>
        /// <param name="frame">The frame to be adapted</param>
        public SkeletonFrameAdapter(SkeletonFrame frame)
        {
            this.adaptedFrame = frame;
        }

        /// <summary>
        /// Gets the skeletons
        /// </summary>
        public IEnumerable<ISkeletonData> Skeletons
        {
            get 
            {
                foreach (SkeletonData data in this.adaptedFrame.Skeletons)
                    yield return new SkeletonDataAdapter(data);
            }
        }

        /// <summary>
        /// Gets the frame #
        /// </summary>
        public int FrameNumber
        {
            get { return adaptedFrame.FrameNumber; }
        }

        /// <summary>
        /// Gets the time stamp of the skeleton frame
        /// </summary>
        public long TimeStamp
        {
            get { return adaptedFrame.TimeStamp; }
        }

        /// <summary>
        /// Gets the floor plane
        /// </summary>
        public Vector FloorClipPlane
        {
            get { return adaptedFrame.FloorClipPlane; }
        }
    }
}
