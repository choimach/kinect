namespace Kinect.Recognition.Gestures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Research.Kinect.Nui;
    using Kinect.Recognition.States;

    public enum GestureId
    {
        Unknown = 0,
        RightHandSwipeLeft = 1,
        RightHandSwipeRight = 2,
        RightHandClick = 3,
        LeftHandSwipeLeft = 4,
        LeftHandSwipeRight = 5,
        LeftHandClick = 6,
    }

    /// <summary>
    /// Interface to a gesture
    /// </summary>
    public interface IGesture
    {
        GestureId Id { get; }
        double MinDistance { get; set; }
    }

    /// <summary>
    /// Base gesture class
    /// </summary>
    public class Gesture : IGesture
    {
        public GestureId Id { get; set; }

        public double MinDistance { get; set; }
    }
}
