using System;
using Twister.Business.Data;
using Twister.Business.Shared;

namespace Twister.Business.Tests
{
    /// <summary>
    ///     Responsible for analyzing the results of a particular test, to ensure the test has passed.
    /// </summary>
    public class TestResultAnalyzer
    {
        // this is a business rule, the amount of difference between the calibrated and 
        // measured torsional deflection.  This determines PASS/FAIL
        private const float ALLOWABLE_PCT_DIFF = 0.2f;

        private Calibration _savedCalibration;

        public bool CannotAcceptResults(TorqueTest test, string workId, out string result)
        {
            // look up part number and revision
            WorkOrderInfo partInWorkOrder = new WorkOrderInfo(workId);
            partInWorkOrder.Load();

            // load a previous calibration
            _savedCalibration = new Calibration(partInWorkOrder.PartNumber, partInWorkOrder.Revision);
            _savedCalibration.Load();
            if (_savedCalibration.NominalCwDeflection == null)
            {
                result = Messages.MissingCalibrationDataError();
                return false;
            }

            // now create a comparison calibration based off the test results
            Calibration comparison = new Calibration(partInWorkOrder.PartNumber, partInWorkOrder.Revision);
            comparison.CalculateCalibrationValues(test.CopyOfData, test.MaxTorque, test.MinTorque);

            // clockwise
            var expectedAngleCw = (decimal) _savedCalibration.NominalCwDeflection;
            var actualAngleCw = (decimal) comparison.NominalCwDeflection;
            var percentDiffCw = PercentDifference(expectedAngleCw, actualAngleCw);

            // counterclockwise
            var expectedAngleCcw = (decimal) _savedCalibration.NominalCcwDeflection;
            var actualAngleCcw = (decimal) comparison.NominalCcwDeflection;
            var percentDiffCcw = PercentDifference(expectedAngleCcw, actualAngleCcw);

            // compare the calibration results.
            if (percentDiffCw < ALLOWABLE_PCT_DIFF && percentDiffCcw < ALLOWABLE_PCT_DIFF)
            {
                result = Messages.TestPassedMessage(percentDiffCw, percentDiffCcw, ALLOWABLE_PCT_DIFF);
                return false; // false means pass (CAN accept results)
            }
            result = Messages.TestFailureMessage(percentDiffCw, percentDiffCcw, ALLOWABLE_PCT_DIFF);
            return true; // true means fail (Cannot Accept Results)
        }

        public static float PercentDifference(decimal expected, decimal actual)
        {
            // avoid divide by zero
            if (expected + actual == 0)
                return 0;

            return (float) Math.Abs((expected - actual) / ((expected + actual) / 2));
        }
    }
}