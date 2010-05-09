/*************************************************************************
 *
 *   file		: CodeTimers.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-29 07:04:15 +0200 (sø, 29 mar 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 832 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WCell.Tools.Ralek.Utility
{
    /// <summary>
    /// Stats represents a list of samples (floating point values)
    /// This class can calculate the standard statistics on this list
    /// (Mean, Median, StandardDeviation ...)
    /// </summary>
    public class Stats : IEnumerable<float>
    {
        public Stats()
        {
            data = new List<float>();
        }

        public void Add(float dataItem)
        {
            statsComputed = false;
            data.Add(dataItem);
        }

        public int Count
        {
            get { return data.Count; }
        }

        public float this[int idx]
        {
            get { return data[idx]; }
        }

        public IEnumerator<float> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return data.GetEnumerator();
        }

        public float Minimum
        {
            get
            {
                if (!statsComputed) ComputeStats();
                return minimum;
            }
        }

        public float Maximum
        {
            get
            {
                if (!statsComputed) ComputeStats();
                return maximum;
            }
        }

        public float Median
        {
            get
            {
                if (!statsComputed) ComputeStats();
                return median;
            }
        }

        public float Mean
        {
            get
            {
                if (!statsComputed) ComputeStats();
                return mean;
            }
        }

        public float StandardDeviation
        {
            get
            {
                if (!statsComputed) ComputeStats();
                return standardDeviation;
            }
        }

        public override string ToString()
        {
            if (!statsComputed)
                ComputeStats();
            return "mean=" + mean.ToString("f3") + " median=" + median.ToString("f3") +
                   " min=" + minimum.ToString("f3") + " max=" + maximum.ToString("f3") +
                   " sdtdev=" + standardDeviation.ToString("f3") + " samples=" + Count;
        }

        #region privates

        public void ComputeStats()
        {
            minimum = float.MaxValue;
            maximum = float.MinValue;
            median = 0.0F;
            mean = 0.0F;
            standardDeviation = 0.0F;

            double total = 0;
            foreach (float dataPoint in this)
            {
                if (dataPoint < minimum)
                    minimum = dataPoint;
                if (dataPoint > maximum)
                    maximum = dataPoint;
                total += dataPoint;
            }

            if (Count > 0)
            {
                data.Sort();
                if (Count%2 == 1)
                    median = this[Count/2];
                else
                    median = (this[(Count/2) - 1] + this[Count/2])/2;
                mean = (float) (total/Count);

                double squares = 0.0;
                foreach (float dataPoint in this)
                {
                    double diffFromMean = dataPoint - mean;
                    squares += diffFromMean*diffFromMean;
                }
                standardDeviation = (float) Math.Sqrt(squares/Count);
            }

            statsComputed = true;
        }

        private List<float> data;
        private float minimum;
        private float maximum;
        private float median;
        private float mean;
        private float standardDeviation;
        private bool statsComputed;

        #endregion
    } ;

    /// <summary>
    /// The CodeTimer class only times one invocation of the code.
    /// Often, you want to collect many samples so that you can determine
    /// how noisy the resulting data is.  This is what MultiSampleCodeTimer does. 
    /// </summary>
    public class MultiSampleCodeTimer
    {
        public MultiSampleCodeTimer() : this(1)
        {
        }

        public MultiSampleCodeTimer(int sampleCount) : this(sampleCount, 1)
        {
        }

        public MultiSampleCodeTimer(int sampleCount, int iterationCount)
        {
            SampleCount = sampleCount;
            timer = new CodeTimer
                        {
                            OnMeasure = null,
                            IterationCount = iterationCount
                        };
            OnMeasure = Print;
        }

        public int IterationCount
        {
            get { return timer.IterationCount; }
            set { timer.IterationCount = value; }
        }

        public int SampleCount;

        public static float ResolutionUsec
        {
            get { return 1000000.0F/Stopwatch.Frequency; }
        }

        public delegate void MeasureCallback(string name, int iterationCount, Stats sample);

        public MeasureCallback OnMeasure;

        public Stats Measure(string name, Action action)
        {
            Stats stats = new Stats();
            for (int i = 0; i < SampleCount; i++)
                stats.Add(timer.Measure(name, action));

            if (OnMeasure != null)
                OnMeasure(name, IterationCount, stats);
            return stats;
        }

        /// <summary>
        /// Prints the mean, median, min, max, and stdDev and count of the samples to the Console
        /// </summary>
        public static MeasureCallback PrintStats =
            (name, iterationCount, sample) => Console.WriteLine(name + ": " + sample.ToString());

        /// <summary>
        /// Prints the mean with a error bound (2 standard deviations, which imply a you have
        /// 95% confidence that a sample will be with the bounds (for a normal distribution). 
        /// This is a good default way of displaying the data.  
        /// </summary>
        public static MeasureCallback Print = delegate(string name, int iterationCount, Stats sample){
                                                                                                         // +- two standard deviations covers 95% of all samples in a normal distribution 
                                                                                                         float errorPercent = (sample.StandardDeviation*2*100)/Math.Abs((float) ((sbyte) sample.Mean));
                                                                                                         string errorString = ">400%";
                                                                                                         if (errorPercent < 400)
                                                                                                             errorString = (errorPercent.ToString("f0") + "%").PadRight(5);
                                                                                                         string countString = "";
                                                                                                         if (iterationCount != 1)
                                                                                                             countString = "count: " + iterationCount.ToString() + " ";
                                                                                                         Console.WriteLine(name + ": " + countString +
                                                                                                                           sample.Mean.ToString("f3").PadLeft(8) + " +- " + errorString +
                                                                                                                           " msec");
        };

        #region privates

        private CodeTimer timer;

        #endregion
    } ;

    /// <summary>
    /// CodeTimer is a simple wrapper that uses System.Diagnostics.StopWatch
    /// to time the body of some code (given by a delegate), to high precision. 
    /// 
    /// The 
    /// </summary>
    public class CodeTimer
    {
        public CodeTimer() : this(1)
        {
        }

        public CodeTimer(int iterationCount)
        {
            this.iterationCount = iterationCount;
            OnMeasure = Print;
        }

        public int IterationCount
        {
            get { return iterationCount; }
            set
            {
                iterationCount = value;
                overheadValid = false;
            }
        }

        public delegate void MeasureCallback(string name, int iterationCount, float sample);

        public MeasureCallback OnMeasure;

        public static float ResolutionUsec
        {
            get { return 1000000.0F/Stopwatch.Frequency; }
        }

        /// <summary>
        /// Returns the number of millisecond it took to run 'action', 'count' times.  
        /// </summary>
        public float Measure(string name, Action action)
        {
            Stopwatch sw = new Stopwatch();

            // Run the action once to do any JITTing that might happen. 
            action();
            float overhead = GetOverhead();

            sw.Reset();
            sw.Start();
            for (int j = 0; j < iterationCount; j++)
                action();
            sw.Stop();

            float sample = (float) (sw.Elapsed.TotalMilliseconds - overhead);

            if (!computingOverhead && OnMeasure != null)
                OnMeasure(name, iterationCount, sample);
            return sample;
        }

        public static MeasureCallback Print =
            (name, iterationCount, sample) =>
            Console.WriteLine("{0}: count={1} time={2:f3} msec ", name, iterationCount, sample);

        #region privates

        /// <summary>
        /// Time the overhead of the harness that does nothing so we can subtract it out. 
        /// </summary>
        /// <returns></returns>
        private float GetOverhead()
        {
            if (!overheadValid)
            {
                if (computingOverhead)
                    return 0.0F;
                computingOverhead = true;

                // Compute the average over 5 runs; 
                overhead = 0.0F;
                for (int i = 0; i < 5; i++)
                    overhead += Measure(null, delegate { });
                overhead = overhead/5;


                computingOverhead = false;
                overheadValid = true;
            }
            return overhead;
        }

        private bool overheadValid;
        private bool computingOverhead;
        private int iterationCount;
        private float overhead;

        #endregion
    } ;
}