using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twister.Business.Hardware;
using Twister.Business.Tests;

namespace Twister.Tests
{
	[TestFixture]
	public class SimulatorTests
	{
		[Test]
		public void SimulatedTorqueCellCalculatesTorqueAndUpdatesTorqueValue()
		{
			var stopwatch = new System.Diagnostics.Stopwatch();
			var engine = new SimulatorEngine(stopwatch);
			var condition = new FatigueTestCondition()
			{
				CounterclockwiseTorque = -500,
				ClockwiseTorque = 1500,
				CyclesPerSecond = 3,
				CalibrationInterval = 100,
				CyclesRequired = 1000000,
				FatigueTestId = 5,
				Id = 1
			};
			engine.CurrentCondition = condition;
			engine.StartSimulate();

			var torqueCell = new SimulatedTorqueCell(engine);

			List<float> samples = new List<float>();
			for (int i = 0; i < 500; i++)
			{
				torqueCell.RefreshTorque();
				System.Threading.Thread.Sleep(1);
				samples.Add(torqueCell.Torque);
			}

			Assert.AreEqual(samples.Count, 500);
		}
	}
}
