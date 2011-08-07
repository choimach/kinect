namespace Kinect.Recognition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class RecognitionConstants
    {
        /// <summary>
        /// Currently supported geometrical tracking dimensionality
        /// </summary>
        /// <remarks>Currently we onsly support 2 dimensions (X, Y) - no depth</remarks>
        public const int TrackingGeometricalDimensionality = 2;

        /// <summary>
        /// Invalid skeleton id
        /// </summary>
        public const int InvalidSkeletonIdx = -1;

        /// <summary>
        /// Invalid joint id
        /// </summary>
        public const int InvalidJointId = -1;

        /// <summary>
        /// Maximum frames for a gesture
        /// </summary>
        public const int GestureMaxFramesCount = 20;

        /// <summary>
        /// Minimum frames for a gesture
        /// </summary>
        public const int GestureMinFramesCount = 5;

        /// <summary>
        /// Maximum idle seconds before gesture recognition stops
        /// </summary>
        public const int MaxIdleSeconds = 5;

        /// <summary>
        /// A number of skeleton frames to be skipped during continuous recognition
        /// </summary>
        public const int SkeletonSkipFrameCount = 3;

        /// <summary>
        /// Seconds to wait before recording actually starts
        /// </summary>
        public const int PreRecordingIdleTime = 5;
    }
}
