﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twister.Business.Tests
{
	public class FatigueTest : TorqueTest
	{
		public FatigueTest()
		{
			TestConditions = new List<FatigueTestCondition>();
		}

		/// <summary>
		/// The conditions that make up the duty cycle.
		/// </summary>
		private List<FatigueTestCondition> TestConditions { get; set; }
		/// <summary>
		/// The date the test was originally created.
		/// </summary>
		public DateTime CreatedDate { get; set; }
		/// <summary>
		/// The last time the test was modified.
		/// </summary>
		public DateTime ModifiedDate { get; set; }

		public TimeSpan EstimatedCompletionTime {
			get { return EstimatedCompletionTimeOfAllTestConditions(); }
		}

		private TimeSpan EstimatedCompletionTimeOfAllTestConditions()
		{
			double secondsToCompletion = 0.0;

			foreach (var fatigueTestCondition in TestConditions)
			{
				secondsToCompletion += fatigueTestCondition.TimeRemaining.TotalSeconds;
			}

			return new TimeSpan(0, 0, (int) secondsToCompletion);
		}
	}
}
