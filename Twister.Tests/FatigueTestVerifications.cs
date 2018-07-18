using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Twister.Business.Hardware;
using Twister.Business.Tests;

namespace Twister.Tests
{
	[TestFixture]
	public class FatigueTestVerifications
	{
		[Test]
		public void ElapsedTimeCountsDownForSingleTestCondition()
		{
			// initialize the simulated test bench
			var engine = new SimulatorEngine(new Stopwatch());
			var torqueCell = new SimulatedTorqueCell(engine);
			var servoDrive = new SimulatedServoDrive(engine);
			TestBench.Initialize(torqueCell, servoDrive);

			// create a fatigue test and add a condition.
			var fatigueTest = new FatigueTest();
			fatigueTest.TestConditions.Add(SingleFatigueTestCondition());

			// load the test.
			TestBench.Singleton.LoadTest(fatigueTest);
			TestBench.Singleton.BeginCurrentTest();
		}

		private FatigueTestCondition SingleFatigueTestCondition()
		{
			var condition = new FatigueTestCondition()
			{
				CalibrationInterval = 100,
				CyclesPerSecond = 3,
				CounterclockwiseTorque = -500,
				ClockwiseTorque = 500,
				CyclesRequired = 1000000,
				CyclesCompleted = 0,
				FatigueTestId = 1,
				Id = 1
			};
			return condition;
		}
	}
}
