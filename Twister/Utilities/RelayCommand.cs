using System;
using System.Windows.Input;

namespace Twister.Utilities
{
    /// <summary>
    ///     Found at this pluralsight course, this is the no arg version.
    ///     http://www.pluralsight.com/training/player?author=brian-noyes&name=wpf-mvvm-in-depth-m4&mode=live&clip=0&course
    ///     =wpf-mvvm-in-depth
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Func<bool> _canExecute;
        private readonly Action _execute;

        public RelayCommand(Action executeMethod) : this(executeMethod, null)
        {
        }

        public RelayCommand(Action executeMethod, Func<bool> canExecuteMethod)
        {
            _execute = executeMethod;
            _canExecute = canExecuteMethod;
        }

        public void RaiseCanExecuteChanged()
        {
            EventHandler handler = CanExecuteChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            if (_canExecute != null)
                return _canExecute();
            if (_execute != null)
                return true;
            return false;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (_execute != null)
                _execute();
        }

        #endregion
    }

    /// <summary>
    ///     Found at this pluralsight course, this is the generic verison, allowing single
    ///     parameter of type T:
    ///     http://www.pluralsight.com/training/player?author=brian-noyes&name=wpf-mvvm-in-depth-m4&mode=live&clip=0&course
    ///     =wpf-mvvm-in-depth
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Predicate<T> _canExecute;
        private readonly Action<T> _execute;

        public RelayCommand(Action<T> executeMethod)
            : this(executeMethod, null)
        {
        }

        public RelayCommand(Action<T> executeMethod, Predicate<T> canExecuteMethod)
        {
            if (executeMethod == null)
                throw new ArgumentNullException("executeMethod");

            _execute = executeMethod;
            _canExecute = canExecuteMethod;
        }

        public void RaiseCanExecuteChanged()
        {
            EventHandler handler = CanExecuteChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            if (_canExecute != null)
                return _canExecute((T) parameter);
            if (_execute != null)
                return true;
            return false;
        }

        public event EventHandler CanExecuteChanged = delegate { };

        public void Execute(object parameter)
        {
            if (parameter == null)
                return;

            if (_execute != null)
                if (typeof(T) == parameter.GetType())
                    _execute((T) parameter);
        }

        #endregion
    }
}