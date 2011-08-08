namespace Kinect.Recognition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class RecognitionConstants
    {
        /// <summary>
        /// Invalid skeleton id
        /// </summary>
        public const int InvalidSkeletonIdx = -1;

        /// <summary>
        /// Invalid joint id
        /// </summary>
        public const int InvalidJointId = -1;

        /// <summary>
        /// Maximum idle seconds before gesture recognition stops
        /// </summary>
        public const int MaxIdleSeconds = 5;

        /// <summary>
        /// Seconds to wait before recording actually starts
        /// </summary>
        public const int PreRecordingIdleTime = 5;
    }
}
