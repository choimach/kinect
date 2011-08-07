namespace Kinect.Recognition.States
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Research.Kinect.Nui;
    using Kinect.Recognition.Tracking;

    /// <summary>
    /// Base state implementation
    /// </summary>
    public abstract class StateBase : IState<TrackingContext>
    {
        private TrackingContext currentContext;

        /// <summary>
        /// default ctor
        /// </summary>
        protected StateBase(FSMStateId id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Returns true if the state is functional
        /// </summary>
        public virtual bool Initialized
        {
            get
            {
                return this.currentContext != null && this.Id != FSMStateId.Unknown;
            }
        }

        /// <summary>
        /// Gets or sets a controlling interface to the FSM
        /// </summary>
        public IFSMController Controller { get; set; }

        /// <summary>
        /// Gets or sets the state id
        /// </summary>
        public FSMStateId Id { get; protected set; }

        /// <summary>
        /// Gets or sets the current context
        /// </summary>
        protected TrackingContext Context
        {
            get
            {
                return this.currentContext;
            }
            set
            {
                this.currentContext = value;
            }
        }

        /// <summary>
        /// Last depth image
        /// </summary>
        private ImageFrame LastDepthImage { get; set; }

        /// <summary>
        /// Processes a depth frame
        /// </summary>
        /// <param name="data">The depth data</param>
        public void ProcessDepth(ImageFrame data)
        {
            if (data == null || !this.Initialized)
                return;

            this.LastDepthImage = data;
            
            // TODO: figure out currently active skeleton and update context
        }

        /// <summary>
        /// Implements basic skeleton tracking processing event
        /// </summary>
        /// <param name="data"></param>
        public void ProcessSkeletons(SkeletonFrame data)
        {
            if (data == null || !this.Initialized)
                return;
            
            foreach (SkeletonData skeleton in this.ObservableSkeletons(data.Skeletons))
            {
                bool canContinue = this.ProcessSkeleton(skeleton);

                if (!canContinue)
                    break;
            }
        }

        /// <summary>
        /// Returns an enumerable of observable skeletons.
        /// </summary>
        /// <param name="skeletons">All skeletons</param>
        /// <returns>The one to be used</returns>
        protected abstract IEnumerable<SkeletonData> ObservableSkeletons(SkeletonData[] skeletons);

        /// <summary>
        /// Allows individual skeleton tracking.
        /// </summary>
        /// <param name="origData">the original skeleton event data</param>
        /// <returns>true if traversing should continue with next skeleton, otherwise false</returns>
        protected virtual bool ProcessSkeleton(SkeletonData origData)
        {
            return false;
        }
        
        public virtual void StateEntered(TrackingContext context)
        {
            this.Context = context;
        }

        public virtual void StateExited()
        {
        }
    }
}
