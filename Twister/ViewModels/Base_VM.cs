using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Twister.ViewModels
{
    public class Base_VM : INotifyPropertyChanged
    {
        private string _message;

        /// <summary>
        ///     A message from the main view model.
        /// </summary>
        public string MainVmMessage
        {
            get { return _message; }
            set
            {
                if (_message != null)
                {
                    _message = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}