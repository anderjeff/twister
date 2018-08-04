﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twister.Business.Data;

namespace Twister.Business.Tests
{
	public class FatigueTest : TorqueTest
	{
		public FatigueTest()
		{
			TestConditions = new List<FatigueTestCondition>();
			TestData = new List<FatigueTestDataPoint>();
		}

		public List<FatigueTestDataPoint> TestData { get; set; }

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
	}
}
