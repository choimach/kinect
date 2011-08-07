namespace Kinect.Recognition.States
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Research.Kinect.Nui;

    public enum FSMStateId
    {
        Unknown = 0,
        Idle = 1,
        WaitingForCommand = 2,
        Recording = 3
    }

    /// <summary>
    /// State's interface
    /// </summary>
    public interface IState<TContext> where TContext : class
    {
        /// <summary>
        /// The state id
        /// </summary>
        FSMStateId Id { get; }

        /// <summary>
        /// An interface to control the FSM
        /// </summary>
        IFSMController Controller { get; set; }

        /// <summary>
        /// Processes a depth frame
        /// </summary>
        /// <param name="args">The depth data</param>
        void ProcessDepth(ImageFrame args);

        /// <summary>
        /// Processes a skeleton frame
        /// </summary>
        /// <param name="args">The </param>
        void ProcessSkeletons(SkeletonFrame args);

        /// <summary>
        /// State notification
        /// </summary>
        /// <param name="context">Context object</param>
        void StateEntered(TContext context);

        /// <summary>
        /// State notification
        /// </summary>
        void StateExited();
    }
}
