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
			engine.CurrentCondition = SingleFatigueTestCondition();
			fatigueTest.TestConditions.Add(engine.CurrentCondition);
			
			// load the test.
			TestBench.Singleton.LoadTest(fatigueTest);
			TestBench.Singleton.BeginCurrentTest();

			TimeSpan start = new TimeSpan(0, 0, 0);
			for (int i = 0; i < 1000; i++)
			{
				if (i == 0)
				{
					start = fatigueTest.EstimatedCompletionTime;
				}
				torqueCell.RefreshTorque();
				servoDrive.StoreParameter(ServoDriveEnums.RegisterAddress.TorqueValue, (int) torqueCell.Torque);
				servoDrive.RefreshPosition();
				System.Threading.Thread.Sleep(10);
			}
			TimeSpan finish = fatigueTest.EstimatedCompletionTime;
			Console.WriteLine($"The estimated completion for the single condition is {finish.Days}:{finish.Hours}:{finish.Minutes}:{finish.Seconds}");

			var totalSeconds = start.Subtract(finish).TotalSeconds;
			Assert.GreaterOrEqual(totalSeconds, 10);
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

		[Test]
		public void ElapsedTimeCountsDownForMultipleTestConditions()
		{
			// initialize the simulated test bench
			var engine = new SimulatorEngine(new Stopwatch());
			var torqueCell = new SimulatedTorqueCell(engine);
			var servoDrive = new SimulatedServoDrive(engine);
			TestBench.Initialize(torqueCell, servoDrive);

			// create a fatigue test and add a condition.
			var fatigueTest = new FatigueTest();
			engine.CurrentCondition = SingleFatigueTestCondition();
			fatigueTest.TestConditions.Add(engine.CurrentCondition);
			fatigueTest.TestConditions.AddRange(MultipleFatigueTestConditions());
			
			// load the test.
			TestBench.Singleton.LoadTest(fatigueTest);
			TestBench.Singleton.BeginCurrentTest();

			TimeSpan start = new TimeSpan(0, 0, 0);
			for (int i = 0; i < 1000; i++)
			{
				if (i == 0)
				{
					start = fatigueTest.EstimatedCompletionTime;
				}
				torqueCell.RefreshTorque();
				servoDrive.StoreParameter(ServoDriveEnums.RegisterAddress.TorqueValue, (int) torqueCell.Torque);
				servoDrive.RefreshPosition();
				System.Threading.Thread.Sleep(10);
			}
			TimeSpan finish = fatigueTest.EstimatedCompletionTime;
			Console.WriteLine($"The estimated completion for the duty cycle is {finish.Days}:{finish.Hours}:{finish.Minutes}:{finish.Seconds}");
			var totalSeconds = start.Subtract(finish).TotalSeconds;
			Assert.GreaterOrEqual(totalSeconds, 10);
		}

		private IEnumerable<FatigueTestCondition> MultipleFatigueTestConditions()
		{
			var conditions = new List<FatigueTestCondition>()
			{
				new FatigueTestCondition()
				{
					CalibrationInterval = 0, // never calibrate, so I can just rely on CyclesRequired / CyclesPerSecond
					CyclesPerSecond = 2,
					CounterclockwiseTorque = -500,
					ClockwiseTorque = 500,
					CyclesRequired = 20, // 10 seconds
					CyclesCompleted = 0,
					FatigueTestId = 1,
					Id = 2
				},
				new FatigueTestCondition()
				{
					CalibrationInterval = 0,
					CyclesPerSecond = 3,
					CounterclockwiseTorque = -1500,
					ClockwiseTorque = 1500,
					CyclesRequired = 300, // 100 seconds
					CyclesCompleted = 0,
					FatigueTestId = 1,
					Id = 3
				}
			};
			return conditions;
		}
	}
}
