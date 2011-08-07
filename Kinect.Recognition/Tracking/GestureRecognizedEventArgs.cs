using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Recognition.States;
using Kinect.Recognition.Gestures;

namespace Kinect.Recognition.Tracking
{
    public delegate void GestureRecognizedEventHandler(object sender, GestureRecognizedEventArgs args);

    public class GestureRecognizedEventArgs : EventArgs
    {
        public IGesture Gesture { get; set; }

        public FSMEventId Event { get; set; }

        public GestureRecognizedEventArgs(IGesture gesture, FSMEventId eventId)
        {
            this.Gesture = gesture;
            this.Event = eventId;
        }
    }
}
