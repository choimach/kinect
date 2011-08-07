namespace Kinect.Recognition.States
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Kinect.Recognition.Tracking;

    public enum FSMEventId
    {
        Unknown = 0,
        GoIdle = 1,
        WaitForCommand = 2,
        Record = 3
    }

    public interface IFSMController
    {
        /// <summary>
        /// Handles state transition
        /// </summary>
        /// <param name="args">The gesture details</param>
        void RaiseGestureRecognizedEvent(GestureRecognizedEventArgs args);

        /// <summary>
        /// Performs a state transition for a gesture
        /// </summary>
        /// <param name="gesture">The gesture for which to change state</param>
        void PerformTransition(FSMEventId gesture);
    }
}
