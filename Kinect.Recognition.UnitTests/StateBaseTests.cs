namespace Kinect.Recognition.UnitTests
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Moq.Protected;
    using Kinect.Recognition.States;
    using Microsoft.Research.Kinect.Nui;
    using Kinect.Recognition.Adapters;
    using Kinect.Recognition.Tracking;

    /// <summary>
    /// Unit tests for the StateBase abstract class
    /// </summary>
    [TestClass]
    public class StateBaseTests
    {
        /// <summary>
        /// Test initialization
        /// </summary>
        [TestMethod]
        public void TestConstructor()
        {
            // arrange & act
            FSMStateId testState = FSMStateId.Recording;
            var mockedState = new Mock<StateBase>(testState) { CallBase = true }; 

            // assert
            Assert.AreEqual(testState, mockedState.Object.Id, "identifier set");
        }

        /// <summary>
        /// Positive test for the initialized constraints
        /// </summary>
        [TestMethod]
        public void TestInitialized()
        {
            // arrange
            var mockedState = new Mock<StateBase>(FSMStateId.Recording) { CallBase = true };
            mockedState.Object.StateEntered(new Mock<TrackingContext>().Object);

            // act & assert
            Assert.IsTrue(mockedState.Object.Initialized, "initialized");
        }

        /// <summary>
        /// Negative test for the initialized constraints
        /// </summary>
        [TestMethod]
        public void TestNotInitialized()
        {
            // arrange
            var mockedState = new Mock<StateBase>(FSMStateId.Unknown) { CallBase = true };

            // act & assert
            Assert.IsFalse(mockedState.Object.Initialized, "not initialized - nothing initialized");
        }

        /// <summary>
        /// Negative test for the initialized constraints
        /// </summary>
        [TestMethod]
        public void TestNotInitialized2()
        {
            // arrange
            var mockedState = new Mock<StateBase>(FSMStateId.Idle) { CallBase = true };
            
            // act & assert
            Assert.IsFalse(mockedState.Object.Initialized, "not initialized - missing context");
        }

        /// <summary>
        /// Negative test for the initialized constraints
        /// </summary>
        [TestMethod]
        public void TestNotInitialized3()
        {
            // arrange
            var mockedState = new Mock<StateBase>(FSMStateId.Unknown) { CallBase = true };
            mockedState.Object.StateEntered(new Mock<TrackingContext>().Object);

            // act & assert
            Assert.IsFalse(mockedState.Object.Initialized, "not initialized - unknown state id");
        }

        /// <summary>
        /// Positive test for the process skeletons implementation
        /// </summary>
        [TestMethod]
        public void TestProcessSkeletons()
        {
            // arrange
            var mockedData = new Mock<ISkeletonData>();
            var mockedData2 = new Mock<ISkeletonData>();
            var mockedFrame = new Mock<ISkeletonFrame>();
            var mockedState = new Mock<StateBase>(FSMStateId.Idle) { CallBase = true };
            int invocationCount = 0;

            // set expectation for base method's implementation
            mockedState.Setup(x => x.Initialized).Returns(true);
            mockedState.Protected().Setup<IEnumerable<ISkeletonData>>("ObservableSkeletons", ItExpr.IsAny<IEnumerable<ISkeletonData>>()).Returns(new ISkeletonData[] { mockedData.Object, mockedData2.Object });
            mockedState.Protected().Setup<bool>("ProcessSkeleton", mockedData.Object).Returns(true).Callback(() => ++invocationCount);
            mockedState.Protected().Setup<bool>("ProcessSkeleton", mockedData2.Object).Returns(true).Callback(() => ++invocationCount);

            // act
            mockedState.Object.ProcessSkeletons(mockedFrame.Object);

            // assert
            Assert.AreEqual(2, invocationCount, "ProcessSkeleton invoked for all skeletons");
            mockedState.VerifyAll();
        }

        /// <summary>
        /// Positive test of the skeleton processing when ProcessSkeleton breaks the loop
        /// </summary>
        [TestMethod]
        public void TestProcessSkeletonsWithBreak()
        {
            // arrange
            var mockedData = new Mock<ISkeletonData>();
            var mockedData2 = new Mock<ISkeletonData>();
            var mockedFrame = new Mock<ISkeletonFrame>();
            var mockedState = new Mock<StateBase>(FSMStateId.Idle) { CallBase = true };
            int invocationCount = 0;

            // set expectation for base method's implementation
            mockedState.Setup(x => x.Initialized).Returns(true);
            mockedState.Protected().Setup<IEnumerable<ISkeletonData>>("ObservableSkeletons", ItExpr.IsAny<IEnumerable<ISkeletonData>>()).Returns(new ISkeletonData[] { mockedData.Object, mockedData2.Object });
            mockedState.Protected().Setup<bool>("ProcessSkeleton", mockedData.Object).Returns(false).Callback(() => ++invocationCount);
            mockedState.Protected().Setup<bool>("ProcessSkeleton", mockedData2.Object).Throws(new InvalidOperationException("should not get here"));

            // act
            mockedState.Object.ProcessSkeletons(mockedFrame.Object);

            // assert
            Assert.AreEqual(1, invocationCount, "ProcessSkeleton invoked for first skeleton only");
        }

        /// <summary>
        /// Negative test for skeleton processing (bad input param)
        /// </summary>
        [TestMethod]
        public void TestProcessSkeletonsInvalidArguments1()
        {
            // arrange
            var mockedState = new Mock<StateBase>(FSMStateId.Idle) { CallBase = true };
            // setup a failure on ObservableSkeletons being called (i.e. not exiting on bad input param)
            mockedState.Setup(x => x.Initialized).Throws(new InvalidOperationException("should not get here"));

            // act & assert (setup during arrangement)
            mockedState.Object.ProcessSkeletons(null);
        }

        /// <summary>
        /// Negative test for skeleton processing (not initialized)
        /// </summary>
        [TestMethod]
        public void TestProcessSkeletonsInvalidArguments2()
        {
            // arrange
            var mockedState = new Mock<StateBase>(FSMStateId.Idle) { CallBase = true };
            mockedState.Setup(x => x.Initialized).Returns(false);
            
            // setup a failure on ObservableSkeletons being called (i.e. not exiting on not initialized)
            mockedState.Protected().Setup<IEnumerable<ISkeletonData>>("ObservableSkeletons", ItExpr.IsAny<IEnumerable<ISkeletonData>>()).Throws(new InvalidOperationException("should not get here"));

            // act & assert (setup during arrangement)
            mockedState.Object.ProcessSkeletons(new Mock<ISkeletonFrame>().Object);
        }

        /// <summary>
        /// Positive test of entering the state
        /// </summary>
        [TestMethod]
        public void TestStateEntered()
        {
            // arrange
            var mockedState = new Mock<StateBase>(FSMStateId.Idle) { CallBase = true };
            var mockedContext = new Mock<TrackingContext>();
            bool contextSet = false;

            mockedState.Protected().SetupSet<TrackingContext>("Context", mockedContext.Object).Callback(x => contextSet = true);

            // act
            mockedState.Object.StateEntered(mockedContext.Object);

            // assert
            Assert.IsTrue(contextSet, "context not set");
        }

        /// <summary>
        /// Tests the controller property
        /// </summary>
        [TestMethod]
        public void TestControllerProperty()
        {
            // arrange
            var mockedState = new Mock<StateBase>(FSMStateId.Idle) { CallBase = true };
            var mockedController = new Mock<IFSMController>();

            // check initial state
            Assert.IsNull(mockedState.Object.Controller, "initial state is null");

            // act
            mockedState.Object.Controller = mockedController.Object;

            // assert
            Assert.AreEqual(mockedController.Object, mockedState.Object.Controller, "value set");
        }
    }
}
