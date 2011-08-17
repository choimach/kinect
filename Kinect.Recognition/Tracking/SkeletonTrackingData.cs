namespace Kinect.Recognition.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Research.Kinect.Nui;
    using Kinect.Recognition.Adapters;

    public class SkeletonTrackingData
    {
        private TrackingContext Context { get; set; }
        private Dictionary<JointID, Vector> points;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public SkeletonTrackingData(ISkeletonData data, Predicate<Joint> jointFilter, TrackingContext context)
        {
            this.points = null;
            this.JointFilter = jointFilter;
            this.SkeletonData = data;
            this.Context = context;
        }
        
        /// <summary>
        /// Gets or sets a predicate for filtering out the joints to track.
        /// </summary>
        public Predicate<Joint> JointFilter { get; set; }

        /// <summary>
        /// Gets or sets the original skeleton data
        /// </summary>
        public ISkeletonData SkeletonData { get; set; }

        /// <summary>
        /// Indexer for obtaining joint data
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>A Point object</returns>
        public Vector this[JointID index]
        {
            get
            {
                this.CheckAndInitialize();
                return points.ContainsKey(index) ? points[index] : default(Vector);
            }
        }

        /// <summary>
        /// Processes and normalizes the skeleton joint data points
        /// </summary>
        /// <param name="data">The input skeleton data</param>
        /// <returns>Normalized skeleton data, coordinates according to the point between the shoulders</returns>
        protected virtual void CheckAndInitialize()
        {
            // check if already initialized
            if (this.points != null)
                return;

            points = new Dictionary<JointID, Vector>();
            Vector shoulderRight = this.SkeletonData.JointAt(JointID.ShoulderRight).Position; 
            Vector shoulderLeft = this.SkeletonData.JointAt(JointID.ShoulderLeft).Position;

            // get the center point and adjust the joints data with it as a center of the coord. system
            Vector center = new Vector()
            {
                X = (shoulderLeft.X + shoulderRight.X) / 2,
                Y = (shoulderLeft.Y + shoulderRight.Y) / 2,
                Z = (shoulderLeft.Z + shoulderRight.Z) / 2
            };
                
            float shoulderDist = (float)Math.Sqrt(Math.Pow((shoulderLeft.X - shoulderRight.X), 2) 
                                            + Math.Pow((shoulderLeft.Y - shoulderRight.Y), 2)
                                            + Math.Pow((shoulderLeft.Z - shoulderRight.Z), 2));

            foreach (Joint joint in this.SkeletonData.Joints)
            {
                if (this.JointFilter(joint))
                {
                    // transpose and normalize the coordinates with the shoulder distance as a unit 
                    // (to avoid problems when the skeleton has different scale and absolute position)
                    points[joint.ID] = new Vector()
                    {
                        X = (joint.Position.X - center.X) / shoulderDist, 
                        Y = (joint.Position.Y - center.Y) / shoulderDist,
                        Z = (joint.Position.Z - center.Z) / shoulderDist
                    };
                }
            }
        }

        /// <summary>
        /// Property for obtaining the skeleton data in a DTW-compatible format
        /// </summary>
        public virtual double[] DTWData
        {
            get
            {
                this.CheckAndInitialize();

                double[] tmp = new double[this.Context.TrackingDimensionality];
                int i = 0;
                int offset = (int)this.Context.TrackingMode;

                Dictionary<JointID, Vector>.Enumerator enumerator = points.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    Vector p = enumerator.Current.Value;
                    tmp[offset * i] = p.X;
                    tmp[offset * i + 1] = p.Y;

                    if (this.Context.TrackingMode == TrackingMode.Mode3D)
                        tmp[offset * i + 2] = p.Z;

                    ++i;
                }

                return tmp;
            }
        }
    }
}
