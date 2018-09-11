using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
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
		private SimulatedTorqueCell _torqueCell;
		private SimulatedServoDrive _servoDrive;

		[SetUp]
		public void Init()
		{
			var stopwatch = new System.Diagnostics.Stopwatch();
			_engine = new SimulatorEngine(stopwatch);
			var condition = new FatigueTestCondition()
			{
				CounterclockwiseTorque = -1000,
				ClockwiseTorque = 2500,
				CyclesPerSecond = 3,
				CalibrationInterval = 100,
				CyclesRequired = 1000000,
				FatigueTestId = 5,
				Id = 1
			};
			_engine.CurrentCondition = condition;
			_torqueCell = new SimulatedTorqueCell(_engine);
			_servoDrive = new SimulatedServoDrive(_engine);
			_servoDrive.Stiffness = 525;

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
			List<object> objSamples = new List<object>();
			List<float> samples = new List<float>();
			for (int i = 0; i < 500; i++)
			{
				_torqueCell.RefreshTorque();
				System.Threading.Thread.Sleep(1);
				objSamples.Add(new {_torqueCell.Torque, _torqueCell.LastTime});
				samples.Add(_torqueCell.Torque);
			}

			Assert.AreEqual(objSamples.Count, 500);
			Assert.AreEqual(samples.Count, 500);
		}

		[Test]
		public void SimulatedServoDriveOutputsAngle()
		{
			List<object> objSamples = new List<object>();
			List<float> samples = new List<float>();
			for (int i = 0; i < 500; i++)
			{
				_torqueCell.RefreshTorque();
				_servoDrive.StoreParameter(ServoDriveEnums.RegisterAddress.TorqueValue, (int)_torqueCell.Torque);
				_servoDrive.RefreshPosition();
				System.Threading.Thread.Sleep(1);
				objSamples.Add(new { _servoDrive.GearboxAngle, _torqueCell.LastTime });
				samples.Add(_servoDrive.GearboxAngle);
			}

			Assert.AreEqual(objSamples.Count, 500);
			Assert.AreEqual(samples.Count, 500);
		}

		[Test]
		public void SimulatorCollectsValidDataPoints()
		{
			List<Sample> samples = new List<Sample>();
			for (int i = 0; i < 1000; i++)
			{
				_torqueCell.RefreshTorque();
				_servoDrive.StoreParameter(ServoDriveEnums.RegisterAddress.TorqueValue, (int)_torqueCell.Torque);
				_servoDrive.RefreshPosition();
				samples.Add(new Sample(_torqueCell.Torque, _servoDrive.GearboxAngle, _torqueCell.LastTime));

				System.Threading.Thread.Sleep(1);
			}

			Assert.AreEqual(samples.Count, 1000);

			using (StreamWriter sr = new StreamWriter("C:\\temp\\simulatorSamples.csv"))
			{
				sr.WriteLine("Torque,Angle,Time");

				// create csv output to verify in excel.
				foreach (var sample in samples)
				{
					sr.WriteLine($"{sample.Torque},{sample.Angle},{sample.ElapsedTime}");
				}
			}
			
		}

		[Test]
		public void SimulatedServoDriveCountsCycles()
		{
			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
			sw.Start();

			for (int i = 0; i < 5000; i++)
			{
				_torqueCell.RefreshTorque();
				_servoDrive.StoreParameter(ServoDriveEnums.RegisterAddress.TorqueValue, (int)_torqueCell.Torque);
				_servoDrive.RefreshPosition();
				System.Threading.Thread.Sleep(1);
			}
			
			Assert.Greater(TestBench.Singleton.GetCycleCount(), 1);

			float seconds = sw.ElapsedMilliseconds / 1000f;
			Assert.AreEqual(TestBench.Singleton.GetCycleCount() / seconds, _engine.CurrentCondition.CyclesPerSecond,
				0.1);
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

		[Test]
		public void CheckWatchdogValueSetAndRetrievedProperly()
		{
			int newValue = 500;
			var enumName = ServoDriveEnums.RegisterAddress.WatchdogValue;

			var servoDrive = new SimulatedServoDrive(_engine);
			int actual = servoDrive.RetrieveParameter(enumName);
			Assert.AreEqual(0, actual);

			servoDrive.StoreParameter(enumName, newValue);
			actual = servoDrive.RetrieveParameter(enumName);
			Assert.AreEqual(newValue, actual);
		}

		[Test]
		public void CheckCcwTorqueValueSetAndRetrievedProperly()
		{
			int newValue = 500;
			var enumName = ServoDriveEnums.RegisterAddress.CcwTorqueLimit;

			var servoDrive = new SimulatedServoDrive(_engine);
			int actual = servoDrive.RetrieveParameter(enumName);
			Assert.AreEqual(0, actual);

			servoDrive.StoreParameter(enumName, newValue);
			actual = servoDrive.RetrieveParameter(enumName);
			Assert.AreEqual(newValue, actual);
		}

		[Test]
		public void CheckCwTorqueValueSetAndRetrievedProperly()
		{
			int newValue = 500;
			var enumName = ServoDriveEnums.RegisterAddress.CwTorqueLimit;

			var servoDrive = new SimulatedServoDrive(_engine);
			int actual = servoDrive.RetrieveParameter(enumName);
			Assert.AreEqual(0, actual);

			servoDrive.StoreParameter(enumName, newValue);
			actual = servoDrive.RetrieveParameter(enumName);
			Assert.AreEqual(newValue, actual);
		}

		[Test]
		public void CheckRunSpeedValueSetAndRetrievedProperly()
		{
			int newValue = 500;
			var enumName = ServoDriveEnums.RegisterAddress.Runspeed;

			var servoDrive = new SimulatedServoDrive(_engine);
			int actual = servoDrive.RetrieveParameter(enumName);
			Assert.AreEqual(0, actual);

			servoDrive.StoreParameter(enumName, newValue);
			actual = servoDrive.RetrieveParameter(enumName);
			Assert.AreEqual(newValue, actual);
		}

		[Test]
		public void CheckManualSpeedValueSetAndRetrievedProperly()
		{
			int newValue = 500;
			var enumName = ServoDriveEnums.RegisterAddress.Manualspeed;

			var servoDrive = new SimulatedServoDrive(_engine);
			int actual = servoDrive.RetrieveParameter(enumName);
			Assert.AreEqual(0, actual);

			servoDrive.StoreParameter(enumName, newValue);
			actual = servoDrive.RetrieveParameter(enumName);
			Assert.AreEqual(newValue, actual);
		}

		[Test]
		public void CheckDiffLimitValueSetAndRetrievedProperly()
		{
			float newValue = 500f;
			var enumName = ServoDriveEnums.RegisterAddress.DiffLimit;

			var servoDrive = new SimulatedServoDrive(_engine);
			int actual = servoDrive.RetrieveParameter(enumName);
			Assert.AreEqual(0, actual);

			servoDrive.StoreParameter(enumName, newValue);
			actual = servoDrive.RetrieveParameter(enumName);
			Assert.AreEqual(newValue, actual, .001);
		}

		[Test]
		public void CheckTorqueDirectionValueSetAndRetrievedProperly()
		{
			int newValue = 1;
			var enumName = ServoDriveEnums.RegisterAddress.TorqueDirection;

			var servoDrive = new SimulatedServoDrive(_engine);
			int actual = servoDrive.RetrieveParameter(enumName);
			Assert.AreEqual(0, actual);

			servoDrive.StoreParameter(enumName, newValue);
			actual = servoDrive.RetrieveParameter(enumName);
			Assert.AreEqual(newValue, actual);
		}

		[Test]
		public void CheckTestTypeValueSetAndRetrievedProperly()
		{
			int newValue = 2;
			var enumName = ServoDriveEnums.RegisterAddress.TestType;

			var servoDrive = new SimulatedServoDrive(_engine);
			int actual = servoDrive.RetrieveParameter(enumName);
			Assert.AreEqual(0, actual);

			servoDrive.StoreParameter(enumName, newValue);
			actual = servoDrive.RetrieveParameter(enumName);
			Assert.AreEqual(newValue, actual);
		}

		[Test]
		public void CheckOperatorEndsTestValueSetAndRetrievedProperly()
		{
			int newValue = 1;
			var enumName = ServoDriveEnums.RegisterAddress.OperatorEndsTest;

			var servoDrive = new SimulatedServoDrive(_engine);
			int actual = servoDrive.RetrieveParameter(enumName);
			Assert.AreEqual(0, actual);

			servoDrive.StoreParameter(enumName, newValue);
			actual = servoDrive.RetrieveParameter(enumName);
			Assert.AreEqual(newValue, actual);
		}
	}
}
