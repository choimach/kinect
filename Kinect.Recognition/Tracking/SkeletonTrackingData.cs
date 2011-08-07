namespace Kinect.Recognition.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Research.Kinect.Nui;
    using System.Windows;

    public class SkeletonTrackingData
    {
        private Dictionary<JointID, Point> points;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public SkeletonTrackingData(SkeletonData data, Predicate<Joint> jointFilter)
        {
            this.points = null;
            this.JointFilter = jointFilter;
            this.SkeletonData = data;
        }
        
        /// <summary>
        /// Gets or sets a predicate for filtering out the joints to track.
        /// </summary>
        public Predicate<Joint> JointFilter { get; set; }

        /// <summary>
        /// Gets or sets the original skeleton data
        /// </summary>
        public SkeletonData SkeletonData { get; set; }

        /// <summary>
        /// Indexer for obtaining joint data
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>A Point object</returns>
        public Point this[JointID index]
        {
            get
            {
                this.CheckAndInitialize();
                return points.ContainsKey(index) ? points[index] : default(Point);
            }
        }

        /// <summary>
        /// Processes and normalizes the skeleton joint data points
        /// </summary>
        /// <param name="data">The input skeleton data</param>
        /// <returns>Normalized skeleton data, coordinates according to the point between the shoulders</returns>
        private void CheckAndInitialize()
        {
            // check if already initialized
            if (this.points != null)
                return;

            points = new Dictionary<JointID, Point>();
            Point shoulderRight = default(Point), shoulderLeft = default(Point);

            Joint j = this.SkeletonData.Joints[JointID.ShoulderLeft];
            shoulderLeft = new Point(j.Position.X, j.Position.Y);

            j = this.SkeletonData.Joints[JointID.ShoulderRight];
            shoulderRight = new Point(j.Position.X, j.Position.Y);

            // get the center point and adjust the joints data with it as a center of the coord. system
            Point center = new Point((shoulderLeft.X + shoulderRight.X) / 2, (shoulderLeft.Y + shoulderRight.Y) / 2);
            double shoulderDist = Math.Sqrt(Math.Pow((shoulderLeft.X - shoulderRight.X), 2) + Math.Pow((shoulderLeft.Y - shoulderRight.Y), 2));


            foreach (Joint joint in this.SkeletonData.Joints)
            {
                if (this.JointFilter(joint))
                {
                    // transpose and normalize the coordinates with the shoulder distance as a unit 
                    // (to avoid problems when the skeleton has different scale and absolute position)
                    points[joint.ID] = new Point((joint.Position.X - center.X) / shoulderDist, (joint.Position.Y - center.Y) / shoulderDist);
                }
            }
        }

        /// <summary>
        /// Property for obtaining the skeleton data in a DTW-compatible format
        /// </summary>
        public double[] DTWData
        {
            get
            {
                this.CheckAndInitialize();

                double[] tmp = new double[points.Count * 2];
                int i = 0;

                Dictionary<JointID, Point>.Enumerator enumerator = points.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    Point p = enumerator.Current.Value;
                    tmp[2 * i] = p.X;
                    tmp[2 * i + 1] = p.Y;
                    ++i;
                }

                return tmp;
            }
        }
    }
}
