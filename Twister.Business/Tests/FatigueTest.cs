using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twister.Business.Data;

namespace Twister.Business.Tests
{
	public class FatigueTest : TorqueTest
	{
		private object _objLock = new object();

		public FatigueTest()
		{
			TestConditions = new List<FatigueTestCondition>();
			TestData = new List<FatigueTestDataPoint>();
			IncompleteData = new Dictionary<int, List<FatigueTestDataPoint>>();
		}

		private List<FatigueTestDataPoint> TestData { get; }
		private Dictionary<int, List<FatigueTestDataPoint>> IncompleteData { get; }

		/// <summary>
		/// The conditions that make up the duty cycle.
		/// </summary>
		public List<FatigueTestCondition> TestConditions { get; set; }

		/// <summary>
		/// The date the test was originally created.
		/// </summary>
		public DateTime CreatedDate { get; set; }

		/// <summary>
		/// The last time the test was modified.
		/// </summary>
		public DateTime ModifiedDate { get; set; }

		/// <summary>
		/// The estimated time when all test conditions should be completed.
		/// </summary>
		public TimeSpan EstimatedCompletionTime => EstimatedCompletionTimeOfAllTestConditions();

		private TimeSpan EstimatedCompletionTimeOfAllTestConditions()
		{
			double secondsToCompletion = 0.0;

			foreach (var fatigueTestCondition in TestConditions)
			{
				var secondsForThisCondition = fatigueTestCondition.TimeRemaining.TotalSeconds;
				secondsToCompletion += secondsForThisCondition;
			}

			var timeToCompletion = new TimeSpan(0, 0, (int) secondsToCompletion);
			return timeToCompletion;
		}

		/// <summary>
		/// Start running the test.
		/// </summary>
		internal override void Start()
		{
			InformInitializationComplete();
			base.Start();
		}

		/// <summary>
		/// Adds a piece of test data to an internal collection.
		/// </summary>
		/// <param name="dataPoint"></param>
		public void AddTestData(FatigueTestDataPoint dataPoint)
		{
			lock(_objLock)
			{
				TestData.Add(dataPoint);
			}
		}

		public IEnumerable<FatigueTestDataPoint> ProcessedData()
		{
			var temp = new List<FatigueTestDataPoint>();
			lock (_objLock)
			{
				temp.AddRange(TestData);
				// clear out the TestData
				TestData.Clear();
			}

            if (temp.Count == 0) return new List<FatigueTestDataPoint>();

            // the last cycle may not yet be complete and we need to add it to overflow data.
            int maxCycle = temp.Max(t => t.CycleNumber);
			// make one list for each cycle
			var cycleData = new List<FatigueTestDataPoint>();
			var dataPointsInCycles = temp.GroupBy(p => p.CycleNumber, h => h);
			foreach (var dptsInCycle in dataPointsInCycles)
			{
				var dptList = dptsInCycle.ToList<FatigueTestDataPoint>();
				// use overflow data
				if (IncompleteData.ContainsKey(dptsInCycle.Key))
				{
					dptList.AddRange(IncompleteData[dptsInCycle.Key]);
					IncompleteData.Remove(dptsInCycle.Key);
				}
				// add overflow data
				if (dptsInCycle.Key == maxCycle)
				{
					IncompleteData.Add(dptsInCycle.Key, dptList);
					continue; // don't want to add a data point because this is incomplete data.
				}

				var maxAngle = dptList.Max(p => p.MaxAngle);
				var maxTorque = dptList.Max(p => p.MaxTorque);
				var minAngle = dptList.Min(p => p.MinAngle);
				var minTorque = dptList.Min(p => p.MinTorque);

				cycleData.Add(new FatigueTestDataPoint(dptsInCycle.Key)
				{
					MaxTorque = maxTorque,
					MinTorque = minTorque,
					MaxAngle = maxAngle,
					MinAngle = minAngle
				});
			}

			return cycleData;
		}
	}
}
