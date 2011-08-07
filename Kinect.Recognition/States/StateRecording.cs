namespace Kinect.Recognition.States
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Research.Kinect.Nui;
    using System.Diagnostics;
    using Kinect.Recognition.Tracking;
    using System.Collections;
    using Kinect.Recognition.Gestures;


    /// <summary>
    /// Handler for gesture recording-related events
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void GestureRecordingHandler(object sender, GestureRecordingEventArgs args);

    /// <summary>
    /// Event arguments for gesture recording events
    /// </summary>
    public class GestureRecordingEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the ID of the gesture to be recorded
        /// </summary>
        public GestureId Id { get; set; }
        
        /// <summary>
        /// Gets or sets the array holding the frames for the gesture
        /// </summary>
        public ArrayList Frames { get; set; }
    }

    public class StateRecording : StateTrackingBase
    {
        public event GestureRecordingHandler RecordingStarting;
        public event GestureRecordingHandler RecordingStarted;
        public event GestureRecordingHandler RecordingStopping;
        public event GestureRecordingHandler RecordingStopped;
        public event GestureRecordingHandler FrameRecorded;

        private long startTime;
        private bool canRecord;

        /// <summary>
        /// Default ctor
        /// </summary>
        public StateRecording()
            : base(FSMStateId.Recording)
        {
        }

        /// <summary>
        /// Gets or sets the id of the gesture to be recorded
        /// </summary>
        public GestureId RecordingId { get; set; }

        /// <summary>
        /// Returns true if the current frame should be handled
        /// </summary>
        protected override bool CanHandleFrame
        {
            get
            {
                return base.CanHandleFrame && this.CanRecord;
            }
        }

        /// <summary>
        /// Returns true if the pre-recording idle time has passed
        /// </summary>
        private bool CanRecord
        {
            get
            {
                if (!this.canRecord)
                {
                    this.canRecord = (DateTime.Now.Ticks - this.startTime) > new TimeSpan(0, 0, RecognitionConstants.PreRecordingIdleTime).Ticks;

                    if (this.canRecord)
                        this.RaiseEvent(this.RecordingStarted);
                }

                return this.canRecord;
            }
        }

        /// <summary>
        /// Handles entering the recording state
        /// </summary>
        /// <param name="context"></param>
        public override void StateEntered(TrackingContext context)
        {
            base.StateEntered(context);

            this.RaiseEvent(this.RecordingStarting);
            this.startTime = DateTime.Now.Ticks;
            this.canRecord = false;
        }

        /// <summary>
        /// Handles exiting the recording state
        /// </summary>
        public override void StateExited()
        {
            this.RaiseEvent(this.RecordingStopping);
            base.StateExited();
            this.RaiseEvent(this.RecordingStopped);
        }

        /// <summary>
        /// Allows individual skeleton tracking.
        /// </summary>
        /// <param name="origData">the original skeleton event data</param>
        /// <returns>true if traversing should continue with next skeleton, otherwise false</returns>
        protected override bool ProcessSkeleton(SkeletonData origData)
        {
            if (this.CanHandleFrame)
            {
                ArrayList frameBuffer = base.FrameBuffer;

                // if we record more than max frames, this state should change
                if (frameBuffer.Count >= RecognitionConstants.GestureMaxFramesCount)
                {
                    base.Controller.PerformTransition(FSMEventId.GoIdle);
                }
                else
                {
                    // add current skeleton data to recognition buffer
                    SkeletonTrackingData trackingData = new SkeletonTrackingData(origData, base.Context.IsJointTracked);
                    frameBuffer.Add(trackingData.DTWData);
                    this.RaiseEvent(this.FrameRecorded);
                }
            }

            // result doesn't matter as this state only handles 1 skeleton at a time
            return false;
        }

        /// <summary>
        /// Raises a recording event
        /// </summary>
        /// <param name="handler">The particular event to be raised</param>
        /// <param name="args">event arguments, optional</param>
        private void RaiseEvent(GestureRecordingHandler handler, GestureRecordingEventArgs args = null)
        {
            if (handler != null)
            {
                if (args == null)
                    args = new GestureRecordingEventArgs() { Id = this.RecordingId, Frames = this.FrameBuffer };

                handler(this, args);
            }
        }
    }
}
