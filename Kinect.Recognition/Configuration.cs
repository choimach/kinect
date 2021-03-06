﻿namespace Kinect.Recognition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Kinect.Recognition.States;
    using Kinect.Recognition.Gestures;
    using System.Collections;
    using System.IO;
    using System.Xml;
    using System.Runtime.Serialization.Formatters.Binary;
    using Kinect.Recognition.Tracking;
    using Kinect.Recognition.Extensions;
    using System.Reflection;

    public class Configuration : IConfiguration<TrackingContext>
    {
        private TrackingMode? trackingMode;
        private int? gestureMaxFrames;
        private int? gestureMinFrames;
        private int? skipFrames;

        public Dictionary<FSMStateId, IState<TrackingContext>> States { get; private set; }
        public Dictionary<GestureId, ArrayList> Gestures { get; private set; }
        public Dictionary<KeyValuePair<FSMStateId, GestureId>, FSMEventId> GestureTransitions { get; private set; }
        public Dictionary<KeyValuePair<FSMStateId, FSMEventId>, FSMStateId> StateTransitions { get; private set; }
        public Dictionary<GestureId, DTWRecognizer.ThresholdSettings> GestureSettings { get; private set; }

        /// <summary>
        /// Default ctor
        /// </summary>
        private Configuration()
        {
            this.InitLocalState();
        }   

        /// <summary>
        /// The single instance of the repository
        /// </summary>
        private static Configuration instance = new Configuration();
        
        /// <summary>
        /// Gets the instance
        /// </summary>
        public static Configuration Instance
        {
            get
            {
                return Configuration.instance;
            }
        }

        /// <summary>
        /// Gets or sets the path name for the gestures folder
        /// </summary>
        public string GesturesFolder { get; set; }

        /// <summary>
        /// Persists a recorded gesture
        /// </summary>
        /// <param name="args">The recording arguments</param>
        /// <returns>The name of the created file</returns>
        public string SaveGesture(GestureRecordingEventArgs args)
        {
            if (args == null)
                throw new ArgumentNullException("arguments cannot be null");

            if (args.Frames == null ||
                args.Frames.Count == 0)
                throw new ArgumentException("arguments should contain some frames to be serialized");

            if (args.Id == GestureId.Unknown)
                throw new ArgumentException("Invalid gesture id");

            string fileName = string.Format("{0}gesture_{1}.dat", this.GesturesFolder, args.Id.ToString());

            using (FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(file, args.Frames);
            }

            return fileName;
        }

        /// <summary>
        /// Loads a gesture from a file
        /// </summary>
        /// <param name="fileName">name of the file holding the gesture data</param>
        /// <returns>an array list of tracking gesture frames in DTW format</returns>
        public ArrayList LoadGesture(string fileName)
        {
            string fullFileName = this.GesturesFolder + fileName;

            if (!File.Exists(fullFileName))
                throw new ArgumentException(string.Format("File {0} does not exist", fullFileName));

            ArrayList result = new ArrayList();

            using (FileStream file = new FileStream(fullFileName, FileMode.Open, FileAccess.Read))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                result = (ArrayList)formatter.Deserialize(file);
            }

            return result;
        }

        /// <summary>
        /// Initializes the object from the specified config file
        /// </summary>
        /// <param name="cfgFileName">Full name of the xml configuration file</param>
        public void InitializeFrom(string cfgFileName)
        {
            if (cfgFileName == null ||
                !File.Exists(cfgFileName))
                throw new ArgumentException(string.Format("File {0} does not exist", cfgFileName));
            
            XmlDocument config = new XmlDocument();
            config.Load(cfgFileName);

            this.LoadContextSettings(config);
            this.LoadStates(config);
            this.LoadGestures(config);
            this.LoadGestureTransitions(config);
            this.LoadStateTransitions(config);
            this.LoadThresholds(config);
        }

        /// <summary>
        /// Creates and initializes a tracking context object
        /// </summary>
        /// <returns>A context</returns>
        public TrackingContext CreateContext()
        {
            if (this.trackingMode == null ||
                this.gestureMaxFrames == null ||
                this.gestureMinFrames == null ||
                this.skipFrames == null)
                throw new ApplicationException("The configuration has not been initialized properly or tracking settings are missing!");

            TrackingContext ctx = new TrackingContext();
            ctx.TrackingMode = (TrackingMode)this.trackingMode;
            ctx.MaxFrames = (int)this.gestureMaxFrames;
            ctx.MinFrames = (int)this.gestureMinFrames;
            ctx.SkipFrames = (int)this.skipFrames;
            
            return ctx;
        }

        /// <summary>
        /// Initializes the object's state. Used also by unit testing
        /// </summary>
        internal void InitLocalState()
        {
            this.States = new Dictionary<FSMStateId, IState<TrackingContext>>();
            this.Gestures = new Dictionary<GestureId, ArrayList>();
            this.GestureTransitions = new Dictionary<KeyValuePair<FSMStateId, GestureId>, FSMEventId>();
            this.StateTransitions = new Dictionary<KeyValuePair<FSMStateId, FSMEventId>, FSMStateId>();
            this.GestureSettings = new Dictionary<GestureId, DTWRecognizer.ThresholdSettings>();
            this.trackingMode = null;
            this.gestureMaxFrames = null;
            this.gestureMinFrames = null;
            this.skipFrames = null;
        }

        /// <summary>
        /// Loads the tracking context settings
        /// </summary>
        /// <param name="config"></param>
        private void LoadContextSettings(XmlDocument config)
        {
            config.ProcessXmlNodes("//configuration/tracking",
                                    tracking =>
                                    {
                                        this.trackingMode = (TrackingMode)int.Parse(tracking.Attributes["dimensions"].Value);
                                        this.gestureMinFrames = int.Parse(tracking.Attributes["minFrames"].Value);
                                        this.gestureMaxFrames = int.Parse(tracking.Attributes["maxFrames"].Value);
                                        this.skipFrames = int.Parse(tracking.Attributes["skipFrames"].Value);
                                    });
        }

        /// <summary>
        /// Loads the states configuration and creates all necessary state objects
        /// </summary>
        /// <param name="config">The configuration node</param>
        private void LoadThresholds(XmlDocument config)
        {
            DTWRecognizer.ThresholdSettings generalSettings = null;
            config.ProcessXmlNodes("//configuration/gestureSettings",
                                    settings =>
                                    {
                                        generalSettings = DTWRecognizer.ThresholdSettings.CreateFromXML(settings, false);
                                    });

            config.ProcessXmlNodes("//configuration/gestureSettings/gestureSetting",
                                    settings =>
                                    {
                                        GestureId gestureId = (GestureId)int.Parse(settings.Attributes["gesture"].Value);

                                        if (!this.Gestures.ContainsKey(gestureId))
                                            throw new InvalidDataException(string.Format("Gesture {0} is not specified in the gestures section of the config file",
                                                                            gestureId.ToString()));

                                        this.GestureSettings[gestureId] = DTWRecognizer.ThresholdSettings.CreateFromXML(settings);
                                    });

            // make sure all gestures have their settings
            foreach (GestureId gestureId in this.Gestures.Keys)
            {
                if (gestureId != GestureId.Unknown)
                {
                    bool hasOwnSettings = this.GestureSettings.ContainsKey(gestureId);
                    if (!hasOwnSettings)
                    {
                        if (generalSettings != null)
                            this.GestureSettings[gestureId] = generalSettings;
                        else
                            throw new InvalidDataException(string.Format("Gesture {0} has no own settings and no general settings exist!", gestureId.ToString()));
                    }
                }
            }
        }

        /// <summary>
        /// Loads the states transitions
        /// </summary>
        /// <param name="config">The configuration node</param>
        private void LoadStateTransitions(XmlDocument config)
        {
            config.ProcessXmlNodes("//configuration/statetransitions/transition",
                                    transition =>
                                    {
                                        FSMStateId fromStateId = (FSMStateId)int.Parse(transition.Attributes["fromState"].Value);
                                        FSMStateId toStateId = (FSMStateId)int.Parse(transition.Attributes["moveToState"].Value);
                                        FSMEventId eventId = (FSMEventId)int.Parse(transition.Attributes["onEvent"].Value);

                                        if (!this.States.ContainsKey(fromStateId) && fromStateId != FSMStateId.Unknown)
                                            throw new InvalidDataException(string.Format("State {0} is not specified in the states section of the config file",
                                                                            fromStateId.ToString()));

                                        if (!this.States.ContainsKey(toStateId))
                                            throw new InvalidDataException(string.Format("State {0} is not specified in the states section of the config file",
                                                                            toStateId.ToString()));

                                        this.StateTransitions[new KeyValuePair<FSMStateId, FSMEventId>(fromStateId, eventId)] = toStateId;
                                    });
        }

        /// <summary>
        /// Loads the gesture to eventid mapping
        /// </summary>
        /// <param name="config">The configuration node</param>
        private void LoadGestureTransitions(XmlDocument config)
        {
            config.ProcessXmlNodes("//configuration/gesturetransitions/gesturetransition",
                                    transition =>
                                    {
                                        FSMStateId fromStateId = (FSMStateId)int.Parse(transition.Attributes["fromState"].Value);
                                        GestureId gestureId = (GestureId)int.Parse(transition.Attributes["onGesture"].Value);
                                        FSMEventId eventId = (FSMEventId)int.Parse(transition.Attributes["raiseStateEvent"].Value);

                                        if (!this.Gestures.ContainsKey(gestureId))
                                            throw new InvalidDataException(string.Format("Gesture {0} is not specified in the gestures section of the config file",
                                                                            gestureId.ToString()));

                                        if (!this.States.ContainsKey(fromStateId))
                                            throw new InvalidDataException(string.Format("State {0} is not specified in the states section of the config file",
                                                                            fromStateId.ToString()));

                                        this.GestureTransitions[new KeyValuePair<FSMStateId, GestureId>(fromStateId, gestureId)] = eventId;
                                    });
        }

        /// <summary>
        /// Loads the gestures configuration and loads all necessary gesture files
        /// </summary>
        /// <param name="config">The configuration node</param>
        private void LoadGestures(XmlDocument config)
        {
            config.ProcessXmlNodes("//configuration/gestures/gesture",
                                    gesture =>
                                    {
                                        ArrayList frames = this.LoadGesture(gesture.Attributes["filename"].Value);
                                        GestureId gestureId = (GestureId)int.Parse(gesture.Attributes["id"].Value);

                                        this.Gestures[gestureId] = frames;
                                    });
        }

        /// <summary>
        /// Loads the states configuration and creates all necessary state objects
        /// </summary>
        /// <param name="config">The configuration node</param>
        private void LoadStates(XmlDocument config)
        {
            config.ProcessXmlNodes("//configuration/states/state",
                                    state =>
                                    {
                                        string typeName = state.Attributes["typename"].Value;
                                        Type stateType = Type.GetType(typeName);
                                        ConstructorInfo constructor = stateType.GetConstructor(new Type[] { });
                                        IState<TrackingContext> obj = (IState<TrackingContext>)constructor.Invoke(null);

                                        if (!((int)obj.Id).ToString().Equals(state.Attributes["id"].Value))
                                            throw new InvalidDataException(
                                                string.Format("Configuration error. State {0} is specified with id {1} where actual id is {2}",
                                                typeName,
                                                state.Attributes["id"].Value,
                                                obj.Id.ToString()));

                                        this.States[obj.Id] = obj;
                                    });
        }
    }
}
