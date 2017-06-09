using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Twister.Utilities;
using Twister.Views;

namespace Twister.ViewModels
{
    public class RunSpeedSettings_VM : Base_VM
    {
        public RunSpeedSettings_VM()
        {
            UpdateSpeedSettingsCommand = new RelayCommand(UpdateSpeedSettings);
            CancelSpeedSettingsUpdateCommand = new RelayCommand(CancelSpeedSettingsUpdate);
        }
        
        private int _moveSpeed;
        private int _runSpeed;
        private RunSpeedSettingsWindow _window;

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

        public RelayCommand UpdateSpeedSettingsCommand { get; private set; }
        public RelayCommand CancelSpeedSettingsUpdateCommand { get; private set; }

        private void CancelSpeedSettingsUpdate()
        {
            _window.DialogResult = false;
        }

        private void UpdateSpeedSettings()
        {
            _window.DialogResult = true;
        }

        public bool ShowDialog()
        {
            _window = new RunSpeedSettingsWindow();
            _window.DataContext = this;

            bool? result = _window.ShowDialog();

            if (result == true)
            {
                return true;
            }
            
            return false;
        }

        public void UpdateSpeedParameters()
        {

        }
    }
}