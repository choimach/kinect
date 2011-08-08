using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Kinect.Recognition.Tracking;
using Kinect.Recognition.States;
using Kinect.Recognition;
using Kinect.Recognition.Gestures;
using System.Windows.Controls;
using Kinect;
using System.Reflection;
using System.IO;

namespace TestProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int msgCount = 13;
        private ObservableCollection<string> messages;
        private GesturesFSM<TrackingContext> fsm;
        private TrackingContext context;
        private StateRecording recordingState;
        private int frmCount;

        private void InitFSM()
        {
            messages = new ObservableCollection<string>();
            lstMessages.DataContext = messages;
            lstMessages.ItemsSource = messages;

            string file = Path.GetDirectoryName(new Uri(Assembly.GetAssembly(typeof(MainWindow)).CodeBase).LocalPath);

            Configuration cfg = Configuration.Instance;
            cfg.GesturesFolder = file + @"\..\..\Gestures\";
            cfg.InitializeFrom(file + @"\..\..\gesturesConfig.xml");
                        
            context = cfg.CreateContext();
            fsm = new GesturesFSM<TrackingContext>(context);
            fsm.InitializeFromConfiguration(cfg);

            // bind to recording state events
            recordingState = (StateRecording)cfg.States[FSMStateId.Recording];
            recordingState.RecordingStarted += new GestureRecordingHandler(recordingState_RecordingStarted);
            recordingState.RecordingStarting += new GestureRecordingHandler(recordingState_RecordingStarting);
            recordingState.RecordingStopped += new GestureRecordingHandler(recordingState_RecordingStopped);
            recordingState.FrameRecorded += new GestureRecordingHandler(recordingState_FrameRecorded);    

            // bind to fsm events
            fsm.GestureRecognized += new GestureRecognizedEventHandler(fsm_GestureRecognized);
            fsm.StateChanged += new StateChangedEventHandler(fsm_StateChanged);
        }

        private void fsm_StateChanged(object sender, StateChangedEventArgs args)
        {
            //AddMessage(string.Format("State changed from {0} to {1}", args.OldState.ToString(), args.NewState.ToString()));
        }

        private void fsm_GestureRecognized(object sender, GestureRecognizedEventArgs args)
        {
            AddMessage(string.Format("Gesture {0} recognized, distance {1}", args.Gesture.Id, args.Gesture.MinDistance.ToString("F2")));
        }

        void recordingState_RecordingStopped(object sender, GestureRecordingEventArgs args)
        {
            AddMessage("Gesture recorded");
            Configuration.Instance.SaveGesture(args);
        }

        void recordingState_RecordingStarting(object sender, GestureRecordingEventArgs args)
        {
            AddMessage(string.Format("Recording will start in {0} seconds", RecognitionConstants.PreRecordingIdleTime));
            recordingState.RecordingId = (GestureId)int.Parse(((ComboBoxItem)cmbGestureId.SelectedValue).Content.ToString());
        }

        private void recordingState_RecordingStarted(object sender, GestureRecordingEventArgs args)
        {
            AddMessage("Perform gesture NOW");
            frmCount = 0;
        }

        private void recordingState_FrameRecorded(object sender, GestureRecordingEventArgs args)
        {
            AddMessage(string.Format("Recording gesture. Frames: {0}", ++frmCount));
        }

        private void btnRecord_Click(object sender, RoutedEventArgs e)
        {
            if (cmbGestureId.SelectedIndex == -1)
                this.AddMessage("Select a gesture id to be recorded");
            else if (this.context.CurrentCue == null)
                this.AddMessage("Raise one of your hands to select a gesture with it");
            else
                fsm.PerformTransition(FSMEventId.Record);
        }

        private void cmbGestureId_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lblGestureName.Content = ((GestureId)int.Parse(((ComboBoxItem)e.AddedItems[0]).Content.ToString())).ToString();
        }


        private void AddMessage(string msg)
        {
            if (messages.Count > msgCount)
            {
                messages.RemoveAt(0);
            }

            messages.Add(msg);
            lblStatus.Content = msg;
        }
    }
}
