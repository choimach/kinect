using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kinect.Recognition;
using Kinect.Recognition.States;
using System.Collections;
using Kinect.Recognition.Gestures;
using System.IO;
using System.Runtime.Serialization;
using System.Reflection;
using Moq;
using Kinect.Recognition.Tracking;

namespace Kinect.Recognition.UnitTests
{
    /// <summary>
    /// Unit tests for the Configuration class
    /// </summary>
    [TestClass]
    public class ConfigurationTests
    {
        private Configuration cfg = Configuration.Instance;
        private string fileToDelete = string.Empty;

        [TestInitialize]
        public void TestInitialize()
        {
            if (cfg != null)
                cfg.InitLocalState();
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            if (!fileToDelete.Equals(string.Empty)
                && File.Exists(fileToDelete))
            {
                File.Delete(fileToDelete);
                fileToDelete = string.Empty;
            }
        }

        /// <summary>
        /// Validating the constructor logic
        /// </summary>
        [TestMethod]
        public void TestConstructor()
        {
            // assert
            Assert.IsNotNull(cfg.States, "states are null");
            Assert.IsNotNull(cfg.Gestures, "gestures are null");
            Assert.IsNotNull(cfg.GestureTransitions, " gesture transitions are null");
            Assert.IsNotNull(cfg.StateTransitions, "state transitions are null");
            Assert.IsNotNull(cfg.GestureSettings, "gesture settings are null");
        }

        /// <summary>
        /// Tests the property
        /// </summary>
        [TestMethod]
        public void TestGesturesFolder()
        {
            string tstFolder = "test";
            cfg.GesturesFolder = tstFolder;
            Assert.AreEqual(tstFolder, cfg.GesturesFolder, "new value set");
        }

        /// <summary>
        /// Tests the save of a gesture to a file
        /// </summary>
        [TestMethod]
        public void TestSaveGesture()
        {
            // act
            fileToDelete = cfg.SaveGesture(this.CreateRecordingArgs());

            // assert
            Assert.AreEqual(string.Format("{0}gesture_{1}.dat", cfg.GesturesFolder, GestureId.RightHandClick.ToString()), fileToDelete, "correct file name");
            Assert.IsTrue(File.Exists(fileToDelete), "file exists");
        }

        /// <summary>
        /// Invalid arguments
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSaveGestureMissingArgs()
        {
            // act
            cfg.SaveGesture(null);
            
            // assert
            Assert.Fail();
        }

        /// <summary>
        /// Invalid arguments
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestSaveGestureInvalidId()
        {
            // arrange
            var args = this.CreateRecordingArgs();
            args.Id = GestureId.Unknown;

            // act
            cfg.SaveGesture(args);

            // assert
            Assert.Fail();
        }

        /// <summary>
        /// Invalid arguments
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestSaveGestureMissingFrames()
        {
            // arrange
            var args = this.CreateRecordingArgs();
            args.Frames = null;

            // act
            cfg.SaveGesture(args);

            // assert
            Assert.Fail();
        }

        /// <summary>
        /// Invalid arguments
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestSaveGestureEmptyFrames()
        {
            // arrange
            var args = this.CreateRecordingArgs();
            args.Frames = new ArrayList();

            // act
            cfg.SaveGesture(args);

            // assert
            Assert.Fail();
        }

        /// <summary>
        /// Positive test for loading a gesture
        /// </summary>
        [TestMethod]
        public void TestLoadGesture()
        {
            // arrange
            var args = this.CreateRecordingArgs();
            fileToDelete = cfg.SaveGesture(args);

            // act
            ArrayList list = cfg.LoadGesture(fileToDelete);

            // assert
            Assert.AreEqual(args.Frames.Count, list.Count, "same elements");

            for (int i = 0; i < args.Frames.Count; ++i)
                Assert.IsTrue(Enumerable.SequenceEqual<double>((double[])args.Frames[i], (double[])list[i]), string.Format("same element at {0}", i));
        }

        /// <summary>
        /// Negative test for loading a gesture from a corrupt file
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SerializationException))]
        public void TestLoadGestureInvalid()
        {
            fileToDelete = "testFile.dat";
            
            // arrange
            using (FileStream file = new FileStream(fileToDelete, FileMode.Create))
            {
                file.Write(new byte[] { 1, 1, 1, 1 }, 0, 4);
            }

            // act
            ArrayList list = cfg.LoadGesture(fileToDelete);
            
            // assert
            Assert.Fail();
        }

        /// <summary>
        /// Testing invalid argument behavior
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestInitializeFromInvalid()
        {
            // act
            cfg.InitializeFrom("nonexistentfile");
        }

        /// <summary>
        /// Positive test for loading a config file
        /// </summary>
        [TestMethod]
        public void TestInitializeFrom()
        {
            // arrange
            string path = Path.GetDirectoryName(new Uri(Assembly.GetAssembly(typeof(ConfigurationTests)).CodeBase).LocalPath);
            cfg.GesturesFolder = path + @"\..\..\..\..\Kinect.Recognition.UnitTests\resources\";

            // act
            cfg.InitializeFrom(cfg.GesturesFolder + "testConfig.xml");

            // asserts

            // check gestures
            Assert.AreEqual(2, cfg.Gestures.Count, "gestures count");
            Assert.IsTrue(cfg.Gestures.ContainsKey(GestureId.RightHandSwipeLeft), "left swipe loaded");
            Assert.IsTrue(cfg.Gestures.ContainsKey(GestureId.RightHandSwipeRight), "right swipe loaded");
            
            // check states
            Assert.AreEqual(2, cfg.States.Count, "states count");
            Assert.AreEqual(typeof(StateIdle).Name, cfg.States[FSMStateId.Idle].GetType().Name, "idle type loaded");
            Assert.AreEqual(typeof(StateWaitingCommand).Name, cfg.States[FSMStateId.WaitingForCommand].GetType().Name, "waiting type loaded");

            // check gesture transitions
            Assert.AreEqual(2, cfg.GestureTransitions.Count, "gesture transitions count");
            Assert.AreEqual(FSMEventId.GoIdle, cfg.GestureTransitions[new KeyValuePair<FSMStateId, GestureId>(FSMStateId.WaitingForCommand, GestureId.RightHandSwipeLeft)], "first transition loaded");
            Assert.AreEqual(FSMEventId.GoIdle, cfg.GestureTransitions[new KeyValuePair<FSMStateId, GestureId>(FSMStateId.WaitingForCommand, GestureId.RightHandSwipeRight)], "second transition loaded");

            // check state transitions
            Assert.AreEqual(2, cfg.StateTransitions.Count, "state transitions count");
            Assert.AreEqual(FSMStateId.Idle, cfg.StateTransitions[new KeyValuePair<FSMStateId, FSMEventId>(FSMStateId.Unknown, FSMEventId.GoIdle)], "first state transition loaded");
            Assert.AreEqual(FSMStateId.WaitingForCommand, cfg.StateTransitions[new KeyValuePair<FSMStateId, FSMEventId>(FSMStateId.Idle, FSMEventId.WaitForCommand)], "second state transition loaded");

            // check gesture settings
            Assert.AreEqual(2, cfg.GestureSettings.Count, "gesture settings count");
            Assert.AreEqual(new DTWRecognizer.ThresholdSettings() { FirstThreshold = 1, MatchThreshold = 1, MaxSlope = 3 },
                                cfg.GestureSettings[GestureId.RightHandSwipeRight],
                                "settings for right swipe");
            Assert.AreEqual(new DTWRecognizer.ThresholdSettings() { FirstThreshold = 0.5, MatchThreshold = 0.5, MaxSlope = 3 },
                                cfg.GestureSettings[GestureId.RightHandSwipeLeft],
                                "specialized settings for left swipe");
        }

        private GestureRecordingEventArgs CreateRecordingArgs()
        {
            ArrayList frames = new ArrayList();
            frames.Add(new double[] { 1, 2, 3});
            frames.Add(new double[] { 4, 5, 6 });
            return new GestureRecordingEventArgs() { Id = GestureId.RightHandClick, Frames = frames };
        }
    }
}
