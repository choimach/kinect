namespace Kinect.Recognition.UnitTests
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Kinect.Recognition.Tracking;
    using Kinect.Recognition.States;
    using System.Diagnostics;

    /// <summary>
    /// Unit tests for the GesturesFSM<T> class
    /// </summary>
    [TestClass]
    public class GesturesFSMTests
    {
        /// <summary>
        /// Positive test for adding a transition
        /// </summary>
        [TestMethod]
        public void TestAddTransition()
        {
            // arrange
            var mockedContext = new Mock<TrackingContext>();
            var mockedState = new Mock<IState<TrackingContext>>();
            var fsm = new GesturesFSM<TrackingContext>(mockedContext.Object);

            // act
            fsm.AddTransition(FSMStateId.Unknown, FSMEventId.Unknown, mockedState.Object);
            fsm.PerformTransition(FSMEventId.Unknown);

            // assert
            mockedState.VerifySet(x => x.Controller = fsm, "controller successfully set to state");

            Assert.IsTrue(object.ReferenceEquals(mockedState.Object, fsm.Current), "state successfully added");
        }

        /// <summary>
        /// Negative test when transition not added and hence
        /// a transition isn't possible
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void TestAddTransitionNotCalled()
        {
            // arrange
            var fsm = new GesturesFSM<TrackingContext>(new Mock<TrackingContext>().Object);

            // act
            fsm.PerformTransition(FSMEventId.Unknown);

            // assert
            Assert.Fail();
        }

        /// <summary>
        /// Tests the transition to the same state - no reentrance
        /// </summary>
        [TestMethod]
        public void TestPerformTransitionToSameState()
        {
            // arrange
            var mockedContext = new Mock<TrackingContext>();
            var mockedState = new Mock<StateBase>(FSMStateId.Idle);
            var fsm = new GesturesFSM<TrackingContext>(mockedContext.Object);

            int stateEntranceCount = 0;
            int stateChangedRaisedCount = 0;

            fsm.AddTransition(FSMStateId.Unknown, FSMEventId.Unknown, mockedState.Object);
            fsm.AddTransition(FSMStateId.Idle, FSMEventId.GoIdle, mockedState.Object);

            fsm.StateChanged += new StateChangedEventHandler((obj, args) => ++stateChangedRaisedCount );
            mockedState.Setup(x => x.StateEntered(mockedContext.Object)).Callback(() => ++stateEntranceCount);
            mockedState.Setup(x => x.StateExited()).Callback(() => Assert.Fail("Should not exit state"));

            // act
            fsm.PerformTransition(FSMEventId.Unknown);
            fsm.PerformTransition(FSMEventId.GoIdle);

            // assert
            Assert.AreEqual(1, stateEntranceCount, "state entered only once");
            Assert.AreEqual(1, stateChangedRaisedCount, "state changed event raised only once");
        }

        /// <summary>
        /// Tests the transition to different state
        /// </summary>
        [TestMethod]
        public void TestPerformTransitionToAnotherState()
        {
            // arrange
            var mockedContext = new Mock<TrackingContext>();
            var mockedState = new Mock<StateBase>(FSMStateId.Idle);
            var mockedState2 = new Mock<StateBase>(FSMStateId.Recording);
            var fsm = new GesturesFSM<TrackingContext>(mockedContext.Object);

            int stateChangedRaisedCount = 0;

            fsm.AddTransition(FSMStateId.Unknown, FSMEventId.Unknown, mockedState.Object);
            fsm.AddTransition(FSMStateId.Idle, FSMEventId.Record, mockedState2.Object);

            fsm.StateChanged += new StateChangedEventHandler((obj, args) => ++stateChangedRaisedCount);
            mockedState.Setup(x => x.StateEntered(mockedContext.Object));
            mockedState.Setup(x => x.StateExited());
            mockedState2.Setup(x => x.StateEntered(mockedContext.Object));

            // act
            fsm.PerformTransition(FSMEventId.Unknown);
            fsm.PerformTransition(FSMEventId.Record);

            // assert
            Assert.AreEqual(2, stateChangedRaisedCount, "state changed event raised for all states");
            mockedState.VerifyAll();
            mockedState2.VerifyAll();
        }

        /// <summary>
        /// Positive test of raising a gesture event
        /// </summary>
        [TestMethod]
        public void TestRaiseGestureRecognizedEvent()
        {
            // arrange
            var args = new GestureRecognizedEventArgs(null, FSMEventId.GoIdle);
            var fsm = new Mock<GesturesFSM<TrackingContext>>(new Mock<TrackingContext>().Object) { CallBase = true };
            bool eventRaised = false;

            fsm.Object.GestureRecognized += new GestureRecognizedEventHandler((obj, a) => eventRaised = true);
            fsm.Setup(x => x.PerformTransition(args.Event));

            // act
            fsm.Object.RaiseGestureRecognizedEvent(args);

            // assert
            fsm.VerifyAll(); // state transition called
            Assert.IsTrue(eventRaised, "gesture event raised");
        }

        /// <summary>
        /// Tests initialization with empty configuration, which in turn
        /// results in raising an exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void TestInitializeFromEmptyConfiguration()
        {
            // arrange
            var fsm = new GesturesFSM<TrackingContext>(new Mock<TrackingContext>().Object);
            var config = new Mock<IConfiguration<TrackingContext>>();

            config.Setup(x=>x.StateTransitions).Returns(new Dictionary<KeyValuePair<FSMStateId, FSMEventId>, FSMStateId>());

            // act
            this.DisableUIAsserts();
            fsm.InitializeFromConfiguration(config.Object);
        }

        /// <summary>
        /// Disables UI asserts which block the testing thread
        /// </summary>
        private void DisableUIAsserts()
        {
            foreach(TraceListener listener in Trace.Listeners)
                if (listener is DefaultTraceListener)
                {
                    DefaultTraceListener defListener = listener as DefaultTraceListener;
                    defListener.AssertUiEnabled = false;
                }
        }

    }
}
