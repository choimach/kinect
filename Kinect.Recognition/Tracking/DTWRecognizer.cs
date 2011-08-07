﻿namespace Kinect.Recognition.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Collections;
    using Kinect.Recognition.Gestures;
    using Kinect.Recognition.States;
    using System.Xml;
    using System.IO;

    public class DTWRecognizer
    {
        public class TreshholdSettings
        {
            public const int AttributesCount = 3;

            public double FirstTreshhold { get; set; }
            public double MatchTreshhold { get; set; }
            public double MaxSlope { get; set; }

            public static TreshholdSettings CreateFromXML(XmlNode node)
            {
                if (node.Attributes.Count != TreshholdSettings.AttributesCount)
                    throw new InvalidDataException("Invalid number of attributes");

                return new DTWRecognizer.TreshholdSettings()
                {
                    FirstTreshhold = double.Parse(node.Attributes["firstTreshhold"].Value),
                    MatchTreshhold = double.Parse(node.Attributes["matchTreshhold"].Value),
                    MaxSlope = double.Parse(node.Attributes["maxSlope"].Value)
                };
            }
        }

        // Size of obeservations vectors.
        private int dim;

        // Known sequences
        private ArrayList sequences;

        // Labels of those known sequences
        private ArrayList sequenceIds;

        // Maximum treshhold settings per gesture sequence
        private ArrayList recognitionSettings;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dim"></param>
        /// <param name="threshold"></param>
        /// <param name="firstThreshold"></param>
        /// <param name="ms"></param>
        public DTWRecognizer()
        {
            sequences = new ArrayList();
            sequenceIds = new ArrayList();
            recognitionSettings = new ArrayList();
        }

        /// <summary>
        /// Gets or sets the dimensionality of the sequences to match
        /// </summary>
        public int SequenceDimensionSize 
        {
            get
            {
                return dim;
            }
            set
            {
                dim = value;
            }
        }

        /// <summary>
        /// Add a seqence with a label to the known sequences library. 
        /// The gesture MUST start on the first observation of the sequence and end on the last one.
        /// Sequences may have different lengths.
        /// </summary>
        /// <param name="gestureId">The gesture id</param>
        /// <param name="seq">The gesture sequence</param>
        /// <param name="settings">The treshhold settings</param>
        public void AddPatterns(GestureId gestureId, ArrayList seq, TreshholdSettings settings)
        {
            sequences.Add(seq);
            sequenceIds.Add(gestureId);
            recognitionSettings.Add(settings);
        }

        /// <summary>
        /// Clears the patterns to match.
        /// </summary>
        public void Clear()
        {
            sequences.Clear();
            sequenceIds.Clear();
            recognitionSettings.Clear();
        }

        /// <summary>
        /// Recognizes a gesture sequence. It will always assume that the gesture ends on the last observation of that sequence.
        /// If the distance between the last observations of each sequence is too great, or if the overall DTW distance between 
        /// the two sequence is too great, no gesture will be recognized
        /// </summary>
        /// <param name="seq"></param>
        /// <returns></returns>
        public IGesture Recognize(ArrayList seq)
        {
            double minDist = double.PositiveInfinity;
            GestureId gestureId = GestureId.Unknown;
            TreshholdSettings minSettings = null;

            for (int i = 0; i < sequences.Count; i++)
            {
                TreshholdSettings settings = (TreshholdSettings)recognitionSettings[i];
                ArrayList example = (ArrayList)sequences[i];

                if (this.Distance((double[])seq[seq.Count - 1], (double[])example[example.Count - 1]) < settings.FirstTreshhold)
                {
                    double d = dtw(seq, example, settings) / (example.Count);
                    if (d < minDist)
                    {
                        minDist = d;
                        gestureId = (GestureId)(sequenceIds[i]);
                        minSettings = settings;
                    }
                }
            }

            return new Gesture() 
            {
                Id = (minSettings != null && minDist < minSettings.MatchTreshhold ? gestureId : GestureId.Unknown), 
                MinDistance = minDist 
            };
        }

        /// <summary>
        /// Computes a 2-distance between two observations. (aka Euclidian distance).
        /// </summary>
        /// <param name="a">First observation</param>
        /// <param name="b">Second observation</param>
        /// <returns>The distance</returns>
        private double Distance(double[] a, double[] b)
        {
            double d = 0;
            for (int i = 0; i < dim; i++)
            {
                d += Math.Pow(a[i] - b[i], 2);
            }
            return Math.Sqrt(d);
        }

        /// <summary>
        /// Compute the min DTW distance between seq2 and all possible endings of seq1.
        /// </summary>
        /// <param name="seq1">Sequence 1</param>
        /// <param name="seq2">Sequence 2</param>
        /// <param name="settings">The settings object</param>
        /// <returns>DTW distance</returns>
        public double dtw(ArrayList seq1, ArrayList seq2, TreshholdSettings settings)
        {
            // Init
            ArrayList seq1r = new ArrayList(seq1); seq1r.Reverse();
            ArrayList seq2r = new ArrayList(seq2); seq2r.Reverse();
            double[,] tab = new double[seq1r.Count + 1, seq2r.Count + 1];
            int[,] slopeI = new int[seq1r.Count + 1, seq2r.Count + 1];
            int[,] slopeJ = new int[seq1r.Count + 1, seq2r.Count + 1];

            for (int i = 0; i < seq1r.Count + 1; i++)
            {
                for (int j = 0; j < seq2r.Count + 1; j++)
                {
                    tab[i, j] = double.PositiveInfinity;
                    slopeI[i, j] = 0;
                    slopeJ[i, j] = 0;
                }
            }
            tab[0, 0] = 0;

            // Dynamic computation of the DTW matrix.
            for (int i = 1; i < seq1r.Count + 1; i++)
            {
                for (int j = 1; j < seq2r.Count + 1; j++)
                {
                    if (tab[i, j - 1] < tab[i - 1, j - 1] && tab[i, j - 1] < tab[i - 1, j] && slopeI[i, j - 1] < settings.MaxSlope)
                    {
                        tab[i, j] = this.Distance((double[])seq1r[i - 1], (double[])seq2r[j - 1]) + tab[i, j - 1];
                        slopeI[i, j] = slopeJ[i, j - 1] + 1; ;
                        slopeJ[i, j] = 0;
                    }
                    else if (tab[i - 1, j] < tab[i - 1, j - 1] && tab[i - 1, j] < tab[i, j - 1] && slopeJ[i - 1, j] < settings.MaxSlope)
                    {
                        tab[i, j] = this.Distance((double[])seq1r[i - 1], (double[])seq2r[j - 1]) + tab[i - 1, j];
                        slopeI[i, j] = 0;
                        slopeJ[i, j] = slopeJ[i - 1, j] + 1;
                    }
                    else
                    {
                        tab[i, j] = this.Distance((double[])seq1r[i - 1], (double[])seq2r[j - 1]) + tab[i - 1, j - 1];
                        slopeI[i, j] = 0;
                        slopeJ[i, j] = 0;
                    }
                }
            }

            // Find best between seq2 and an ending (postfix) of seq1.
            double bestMatch = double.PositiveInfinity;
            for (int i = 1; i < seq1r.Count + 1; i++)
            {
                if (tab[i, seq2r.Count] < bestMatch)
                    bestMatch = tab[i, seq2r.Count];
            }

            return bestMatch;
        }
    }
}