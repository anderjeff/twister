using System;
using Twister.Business.Database;
using Twister.Business.Shared;

namespace Twister.Business.Tests
{
    /// <summary>
    ///     The currently available tests
    /// </summary>
    public enum TestType
    {
        SteeringShaftTest_4000_inlbs = 1,
        TorsionTestToFailure = 2
    }

    /// <summary>
    ///     Represents the target test settings, pulled from the database.
    /// </summary>
    public class TestTemplate : NotifyPropertyChangedBase
    {
        private int _ccwTorque;
        private int _cwTorque;
        private string _description;

        private int _id;
        private int _moveSpeed;
        private int _runSpeed;
        private bool _templateLoaded;

        public TestTemplate(TestType desiredTestType)
        {
            Id = (int) desiredTestType;
        }

        /// <summary>
        ///     A unique indentifier for this test.
        /// </summary>
        public int Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     A Description of what this test does.
        /// </summary>
        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     The target maximum torque in the clockwise direction.
        /// </summary>
        public int ClockwiseTorque
        {
            get { return _cwTorque; }
            set
            {
                if (_cwTorque != value)
                {
                    _cwTorque = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     The target maximum torque in the counterclockwise direction.
        /// </summary>
        public int CounterclockwiseTorque
        {
            get { return _ccwTorque; }
            set
            {
                if (_ccwTorque != value)
                {
                    _ccwTorque = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     The speed at which the motor rotates while the test is running.
        /// </summary>
        public int RunSpeed
        {
            get => _runSpeed;
            set
            {
                _runSpeed = value;
                OnPropertyChanged();
            }
        }


        /// <summary>
        ///     The rotational speed of the motor when the joystick is up or down, and
        ///     the switch to run in manual mode is enabled.
        /// </summary>
        public int MoveSpeed
        {
            get { return _moveSpeed; }
            set
            {
                if (_moveSpeed != value)
                {
                    _moveSpeed = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Gets a new instance of the test type associated with the template.
        /// </summary>
        /// <returns></returns>
        public TorqueTest TestInstance()
        {
            if (!_templateLoaded)
                throw new Exception("Test template is not loaded.  Must first call Load() method.");

            TorqueTest t = null;
            switch (_id)
            {
                case (int) TestType.SteeringShaftTest_4000_inlbs:
                    Load();
                    t = new FullyReversedTorqueTest();
                    t.TestTemplateId = Id;
                    t.LoadTestParameters(this);
                    break;
                case (int) TestType.TorsionTestToFailure:
                    Load();
                    t = new UnidirectionalTorqueTest();
                    t.TestTemplateId = Id;
                    t.LoadTestParameters(this);
                    break;
                default:
                    throw new Exception("Unsupported test type");
            }

            return t;
        }

        /// <summary>
        ///     Loads the saved test settings from disk.
        /// </summary>
        public void Load()
        {
            try
            {
                TestTemplateDb.LoadTemplateThatHasId(this);
                _templateLoaded = true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}