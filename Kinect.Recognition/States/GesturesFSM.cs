namespace Kinect.Recognition.States
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Kinect.Recognition.Gestures;
    using Kinect.Recognition.Tracking;
    using System.Diagnostics;

    /// <summary>
    /// Gesture identification finite state machine
    /// </summary>
    /// <typeparam name="TContext">Type of the context object, shared amongst states</typeparam>
    public class GesturesFSM<TContext> : IFSMController where TContext : class
    {
        private Dictionary<KeyValuePair<FSMStateId, FSMEventId>, IState<TContext>> transitions;
        private IState<TContext> currentState;
        private TContext sharedContext;

        /// <summary>
        /// State changed event
        /// </summary>
        public event StateChangedEventHandler StateChanged;

        /// <summary>
        /// Gesture recognized event
        /// </summary>
        public event GestureRecognizedEventHandler GestureRecognized;

        /// <summary>
        /// Default constructor
        /// </summary>
        public GesturesFSM(TContext context)
        {
            Debug.Assert(context != null, "context cannot be null");

            this.transitions = new Dictionary<KeyValuePair<FSMStateId, FSMEventId>, IState<TContext>>();
            this.currentState = null;
            this.sharedContext = context;
        }

        /// <summary>
        /// Initializes the FSM by triggering an idle
        /// </summary>
        public virtual void Initialize()
        {
            Debug.Assert(this.transitions.Count > 0, "no transitions added");

            this.PerformTransition(FSMEventId.GoIdle);
        }

        /// <summary>
        /// Initializes the FSM from the configuration object
        /// </summary>
        /// <param name="config">The configuration object</param>
        public virtual void InitializeFromConfiguration(IConfiguration<TContext> config)
        {
            foreach (var pair in config.StateTransitions)
                this.AddTransition(pair.Key.Key, pair.Key.Value, config.States[pair.Value]);

            this.Initialize();
        }

        /// <summary>
        /// Handles state transition
        /// </summary>
        /// <param name="args">The gesture details</param>
        public void RaiseGestureRecognizedEvent(GestureRecognizedEventArgs args)
        {
            this.PerformTransition(args.Event);

            if (this.GestureRecognized != null)
                this.GestureRecognized(this, args);
        }

        /// <summary>
        /// Performs a state transition for a gesture
        /// </summary>
        /// <param name="eventId">The gesture for which to change state</param>
        public virtual void PerformTransition(FSMEventId eventId)
        {
            FSMStateId oldStateId = this.currentState != null ? this.currentState.Id : FSMStateId.Unknown;
            var transitionKey = this.GetTransitionKey(eventId);
            IState<TContext> oldState = this.currentState;

            if (!transitions.ContainsKey(transitionKey))
                throw new ApplicationException(string.Format("No transition for event {0} found", eventId.ToString()));

            currentState = transitions[transitionKey];

            if (currentState == null)
                throw new ApplicationException(string.Format("Invalid state for event {0}", eventId.ToString()));

            // we only enter/exit states if anything really changed
            if (currentState.Id != transitionKey.Key)
            {
                if (oldState != null)
                    oldState.StateExited();

                currentState.StateEntered(this.sharedContext);

                if (this.StateChanged != null)
                    this.StateChanged(this, new StateChangedEventArgs() { OldState = oldStateId, NewState = currentState.Id });
            }
        }

        /// <summary>
        /// Current state
        /// </summary>
        public IState<TContext> Current
        {
            get
            {
                return currentState;
            }
        }

        /// <summary>
        /// Add a state transition to the FSM
        /// </summary>
        /// <param name="state">The state</param>
        /// <param name="eventId">The transition event</param>
        public void AddTransition(FSMStateId fromState, FSMEventId eventId, IState<TContext> state)
        {
            var transitionKey = new KeyValuePair<FSMStateId, FSMEventId>(fromState, eventId);
            Debug.Assert(!transitions.ContainsKey(transitionKey), string.Format("Overwriting gesture {0}", eventId.ToString()));
            
            state.Controller = this;
            this.transitions[transitionKey] = state;
        }

        /// <summary>
        /// Returns a key that can be used for identifying transitions in the FSM
        /// </summary>
        /// <param name="eventId">The event id</param>
        /// <returns>A transition key</returns>
        private KeyValuePair<FSMStateId, FSMEventId> GetTransitionKey(FSMEventId eventId)
        {
            return new KeyValuePair<FSMStateId, FSMEventId>(this.currentState == null ? FSMStateId.Unknown : this.currentState.Id, eventId); 
        }
    }
}