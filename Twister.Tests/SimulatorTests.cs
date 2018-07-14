using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twister.Business.Data;
using Twister.Business.Hardware;
using Twister.Business.Tests;

namespace Twister.Tests
{
	[TestFixture]
	public class SimulatorTests
	{
		private SimulatorEngine _engine;

		[SetUp]
		public void Init()
		{
			var stopwatch = new System.Diagnostics.Stopwatch();
			_engine = new SimulatorEngine(stopwatch);
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
			_engine.CurrentCondition = condition;
			_engine.StartSimulate();
		}

		[TearDown]
		public void Cleanup()
		{
			_engine.StopSimulate();
			_engine = null;
		}

		[Test]
		public void SimulatedTorqueCellCalculatesTorqueAndUpdatesTorqueValue()
		{
			var torqueCell = new SimulatedTorqueCell(_engine);

			List<object> objSamples = new List<object>();
			List<float> samples = new List<float>();
			for (int i = 0; i < 500; i++)
			{
				torqueCell.RefreshTorque();
				System.Threading.Thread.Sleep(1);
				objSamples.Add(new {torqueCell.Torque, torqueCell.LastTime});
				samples.Add(torqueCell.Torque);
			}

			Assert.AreEqual(objSamples.Count, 500);
			Assert.AreEqual(samples.Count, 500);
		}

		[Test]
		public void CheckTestInProcessSetAndRetrievedProperly()
		{
			int newValue = 1;
			var enumName = ServoDriveEnums.RegisterAddress.TestInProcess;

			var servoDrive = new SimulatedServoDrive(_engine);
			int actual = servoDrive.RetrieveParameter(enumName);
			Assert.AreEqual(0, actual);

			servoDrive.StoreParameter(enumName, newValue);
			actual = servoDrive.RetrieveParameter(enumName);
			Assert.AreEqual(newValue, actual);
		}

		[Test]
		public void CheckSoftwareInitializedSetAndRetrievedProperly()
		{
			int newValue = 1;
			var enumName = ServoDriveEnums.RegisterAddress.SoftwareInitialized;

			var servoDrive = new SimulatedServoDrive(_engine);
			int actual = servoDrive.RetrieveParameter(enumName);
			Assert.AreEqual(0, actual);

			servoDrive.StoreParameter(enumName, newValue);
			actual = servoDrive.RetrieveParameter(enumName);
			Assert.AreEqual(newValue, actual);
		}

		[Test]
		public void CheckTorqueValueSetAndRetrievedProperly()
		{
			int newValue = 500;
			var enumName = ServoDriveEnums.RegisterAddress.TorqueValue;

			var servoDrive = new SimulatedServoDrive(_engine);
			int actual = servoDrive.RetrieveParameter(enumName);
			Assert.AreEqual(0, actual);

			servoDrive.StoreParameter(enumName, newValue);
			actual = servoDrive.RetrieveParameter(enumName);
			Assert.AreEqual(newValue, actual);
		}
	}
}
