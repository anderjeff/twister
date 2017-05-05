using System;
using System.Xml;
using Twister.Business.Hardware;

namespace Twister.Business.Tests
{
    internal class BenchLimits
    {
        public int MinTorque { get; private set; }
        public int MaxTorque { get; private set; }
        public int RunSpeed { get; private set; }
        public int ManualSpeed { get; private set; }

        /// <summary>
        ///     Prepares the test to be able to be started by setting the
        ///     clockwise torque, counterclockwise torque, run speed and
        ///     manual mode speed.
        /// </summary>
        /// <param name="path">The path to the params file.</param>
        internal void Load(string path)
        {
            XmlDocument dom = new XmlDocument();
            dom.Load(path);

            // set test parameters on AKD Basic drive.
            LoadTorqueLimitFromXml(dom);
            LoadRunSpeedFromXml(dom);
            LoadManualSpeedFromXml(dom);
        }

        private void LoadManualSpeedFromXml(XmlDocument dom)
        {
            try
            {
                /* get the desired speed for manual mode, this is how fast the shaft will rotate when the 
                 * manual mode switch is set and the joystick is positioned in either the up or down position.
                 */
                XmlNode manualSpeedNode =
                    dom.DocumentElement.SelectSingleNode("//parameter[@name='manualSpeed']/@value");
                var manualSpeedValue = manualSpeedNode.Value;
                int manualSpeed;
                if (int.TryParse(manualSpeedValue, out manualSpeed))
                {
                    // set manual speed
                    TestBench.Singleton.LoadTestParameter(ServoDrive.RegisterAddress.Manualspeed, manualSpeed);
                    ManualSpeed = manualSpeed;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void LoadRunSpeedFromXml(XmlDocument dom)
        {
            try
            {
                // get the desired run speed, this is how fast the shaft will turn during the test.
                XmlNode runSpeedNode = dom.DocumentElement.SelectSingleNode("//parameter[@name='runSpeed']/@value");
                var runSpeedValue = runSpeedNode.Value;
                int runSpeed;
                if (int.TryParse(runSpeedValue, out runSpeed))
                {
                    // set run speed
                    TestBench.Singleton.LoadTestParameter(ServoDrive.RegisterAddress.Runspeed, runSpeed);
                    RunSpeed = runSpeed;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void LoadTorqueLimitFromXml(XmlDocument dom)
        {
            try
            {
                /* because this is a FULLY REVERSED TEST, the CW and CCW directions will both 
                 * have the same value, so no need to get the parameter for each.
                 */
                XmlNode cwTorqueLimitNode =
                    dom.DocumentElement.SelectSingleNode("//parameter[@name='cwTorque']/@value");
                var cwTorqueValue = cwTorqueLimitNode.Value;
                int torqueValue;
                if (int.TryParse(cwTorqueValue, out torqueValue))
                {
                    // set clockwise torque
                    TestBench.Singleton.LoadTestParameter(ServoDrive.RegisterAddress.CwTorqueLimit, torqueValue);

                    /* set counterclockwise torque to be the same value, this is, after all,
                     * a fully reversed torque test, so by definition the values need to be 
                     * equal and opposite (thus the -1 multiplier). 
                     */
                    TestBench.Singleton.LoadTestParameter(ServoDrive.RegisterAddress.CcwTorqueLimit, torqueValue * -1);

                    MaxTorque = torqueValue;
                    MinTorque = torqueValue * -1;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}