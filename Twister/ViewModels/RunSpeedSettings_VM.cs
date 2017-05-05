using System.ComponentModel.DataAnnotations;

namespace Twister.ViewModels
{
    public class RunSpeedSettings_VM : Base_VM
    {
        private int _moveSpeed;
        private int _runSpeed;

        [Range(1, 100, ErrorMessage = "Run speed must be between 1 and 100")]
        public int RunSpeed
        {
            get { return _runSpeed; }
            set
            {
                if (_runSpeed != value)
                {
                    _runSpeed = value;
                    OnPropertyChanged();
                }
            }
        }

        [Range(1, 100, ErrorMessage = "Move speed must be between 1 and 100")]
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
    }
}