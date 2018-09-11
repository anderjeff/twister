using System;
using System.Collections.Generic;
using System.Linq;
using Twister.Business.Data;
using Twister.Business.Database;

namespace Twister.Business.Tests
{
    /// <summary>
    ///     Represents a calibration of a particular part number.
    /// </summary>
    public class Calibration
    {
        public Calibration() : this(null, null)
        {
        }

        public Calibration(string partNumber, string revision)
        {
            PartNumber = partNumber;
            Revision = revision;
        }

        public string PartNumber { get; set; }
        public string Revision { get; set; }
        public DateTime? DateCalibrated { get; set; }
        public string CalibratedByClockId { get; set; }
        public decimal? NominalCwDeflection { get; set; }
        public decimal? NominalCcwDeflection { get; set; }
        public int? CwTestTorque { get; set; }
        public int? CcwTestTorque { get; set; }
        public string Username { get; internal set; }

        /// <summary>
        ///     Persist this calibration to storage.
        /// </summary>
        /// <returns></returns>
        public int Save()
        {
            return CalibratedPartsDb.Save(this);
        }

        /// <summary>
        ///     Load a new object with the values saved in storage.
        ///     Precondition: Part number and revision have already been specified.
        /// </summary>
        public void Load()
        {
            CalibratedPartsDb.Load(this);
        }

        public int Delete()
        {
            return CalibratedPartsDb.Delete(this);
        }

	    /// <summary>
	    ///     Takes the data from a test and runs it through an algorithm to determine the
	    ///     values that will be either saved to disk, or compared against other tests to
	    ///     determine pass and fail criteria.
	    /// </summary>
	    /// <param name="testSamples"></param>
	    public void CalculateCalibrationValues(List<Sample> testSamples, float targetTorqueMax, float targetTorqueMin)
	    {
		    // get two points in the positive quadrant (Quadrant II) to calculate 
		    // slope and interpolate an angle at the targetTorqueMax.
		    var positiveValues = testSamples.Where(t => t.Torque > 0).OrderBy(t => t.SampleTime).ToList();

		    Sample maxTorqueSample = positiveValues.OrderByDescending(s => s.Torque).First();
		    var maxSampleIdx = positiveValues.IndexOf(maxTorqueSample);
		    Sample oneBeforeMaxTorqueSample = positiveValues[maxSampleIdx - 1];

		    // get two points in the positive quadrant (Quadrant IV) to calculate 
		    // slope and interpolate an angle at the targetTorqueMin.
		    var negativeValues = testSamples.Where(t => t.Torque < 0).OrderBy(t => t.SampleTime).ToList();

		    // get minimum torque value, then get the value just prior to that in time.
		    Sample minTorqueSample = negativeValues.OrderBy(s => s.Torque).First();
		    var minSampleIdx = negativeValues.IndexOf(minTorqueSample);
		    Sample oneBeforeMinTorqueSample = negativeValues[minSampleIdx - 1];

		    // get out before null refernce exception
		    if (maxTorqueSample == null || oneBeforeMaxTorqueSample == null) return;
		    if (minTorqueSample == null || oneBeforeMinTorqueSample == null) return;

		    // now calculate the slopes
		    var slopePosVals = // negative
			    (maxTorqueSample.Torque - oneBeforeMaxTorqueSample.Torque) / // positive
			    (maxTorqueSample.Angle - oneBeforeMaxTorqueSample.Angle); // negative

		    var torqueDiffPositive = targetTorqueMax - oneBeforeMaxTorqueSample.Torque; // positive
		    var angleDiffNegative = torqueDiffPositive / slopePosVals; // negative
		    var expectedAnglePosTorque = oneBeforeMaxTorqueSample.Angle + angleDiffNegative; // negative

		    var slopeNegVals = // negative
			    (minTorqueSample.Torque - oneBeforeMinTorqueSample.Torque) / // negative
			    (minTorqueSample.Angle - oneBeforeMinTorqueSample.Angle); // positive

		    var torqueDiffNegative = targetTorqueMin - oneBeforeMinTorqueSample.Torque; // negative
		    var angleDiffPositive = torqueDiffNegative / slopeNegVals; // positive
		    var expectedAngleNegTorque = oneBeforeMinTorqueSample.Angle + angleDiffPositive; // positive

		    // now set the values on this object.
		    CwTestTorque = (int) targetTorqueMax;
		    CcwTestTorque = (int) targetTorqueMin;
		    NominalCwDeflection = (decimal) expectedAnglePosTorque;
		    NominalCcwDeflection = (decimal) expectedAngleNegTorque;
	    }
    }
}