namespace Kinect.Recognition.States
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Kinect.Recognition.Gestures;
    using System.Collections;
    using Kinect.Recognition.Tracking;

    public interface IConfiguration<TContext> where TContext : class
    {
        /// <summary>
        /// Instantiated state objects by id
        /// </summary>
        Dictionary<FSMStateId, IState<TContext>> States { get; }
        
        /// <summary>
        /// Gesture sequences by id
        /// </summary>
        Dictionary<GestureId, ArrayList> Gestures { get; }
        
        /// <summary>
        /// Gesture triggerred transitions (from state + gesture = state event to trigger transition)
        /// </summary>
        Dictionary<KeyValuePair<FSMStateId, GestureId>, FSMEventId> GestureTransitions { get; }
        
        /// <summary>
        /// State transitions (FSM stuff - from state + state event -> to state)
        /// </summary>
        Dictionary<KeyValuePair<FSMStateId, FSMEventId>, FSMStateId> StateTransitions { get; }

        /// <summary>
        /// Gesture recognition settings by id
        /// </summary>
        Dictionary<GestureId, DTWRecognizer.ThresholdSettings> GestureSettings { get; }
    }
}
