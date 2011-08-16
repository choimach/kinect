using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Kinect.Recognition.States;
using Microsoft.Research.Kinect.Nui;

namespace Kinect.Recognition.UnitTests
{
    [TestClass]
    public class StateBaseTests
    {
        // TODO - need to duck type all the dependencies on Kinect's SDK classes as they're all sealed
        //        There's simply no other way to proceed with tests..
        /*
        [TestMethod]
        public void TestMethod1()
        {
            var mockedData = new Mock<SkeletonData>();
            var mockedFrame = new Mock<SkeletonFrame>();
            var mockedState = new Mock<StateBase>(new object[] { FSMStateId.Idle }) { CallBase = true };
            
            mockedState.Setup(x => x.Initialized).Returns(true);
            mockedState.Protected().Setup<IEnumerable<SkeletonData>>("ObservableSkeletons", null).Returns(new SkeletonData[] { mockedData.Object, mockedData.Object });
            mockedState.Protected().Setup<bool>("ProcessSkeleton", new object[] { mockedData.Object }).Returns(true);

            mockedState.Object.ProcessSkeletons(mockedFrame.Object);
        }
         * */
    }
}
