using System;
using System.Diagnostics;
using System.Windows.Input;

namespace Core.Commands
{
   public class RelayCommand<T> : ICommand
   {
      #region fields

      private readonly Action<T> execute;
      private readonly Predicate<T> canExecute;

      #endregion

      #region constructors

      public RelayCommand(Action<T> execute)
         : this(execute, null)
      {
      }

      public RelayCommand(Action<T> execute, Predicate<T> canExecute)
      {
         if (execute == null)
            throw new ArgumentException("execute");

         this.execute = execute;
         this.canExecute = canExecute;
      }

      #endregion

      #region events

      public event EventHandler<RelayCommandEventArgs> PreExecution;
      public event EventHandler<RelayCommandEventArgs> PostExecution;

      #endregion

      #region ICommand members

      [DebuggerStepThrough]
      public bool CanExecute(object parameter)
      {
         T convertedParameter;
         try
         {
            if (parameter != null && typeof(T).IsAssignableFrom(parameter.GetType()))
            {
               convertedParameter = (T)parameter;
            }
            else
            {
               IConvertible convertible = parameter as IConvertible;
               convertedParameter = (parameter == null || convertible != null) ? (T)Convert.ChangeType(parameter, typeof(T)) : default(T);
            }
         }
         catch
         {
            convertedParameter = default(T);
         }
         return canExecute == null ? true : (parameter == null ? canExecute(default(T)) : canExecute(convertedParameter));
      }

      public event EventHandler CanExecuteChanged
      {
         add
         {
            if (canExecute != null)
               CommandManager.RequerySuggested += value;
         }
         remove
         {
            if (canExecute != null)
               CommandManager.RequerySuggested -= value;
         }
      }

      public void Execute(object parameter)
      {
         if (PreExecution != null)
            PreExecution(this, new RelayCommandEventArgs(parameter));

         T p = parameter is T ? (T)parameter : (T)Convert.ChangeType(parameter, typeof(T));
         execute(p);

         if (PostExecution != null)
            PostExecution(this, new RelayCommandEventArgs(parameter));
      }

      #endregion

      #region static method

      public static RelayCommand<T> CreateCommand(ref RelayCommand<T> command, Action<T> execute)
      {
         return CreateCommand(ref command, execute, null);
      }

      public static RelayCommand<T> CreateCommand(ref RelayCommand<T> command, Action<T> execute, Predicate<T> canExecute)
      {
         if (command == null)
         {
            if (canExecute != null)
            {
               command = new RelayCommand<T>(execute, canExecute);
            }
            else
            {
               command = new RelayCommand<T>(execute);
            }
         }

         return command;
      }

      #endregion
   }

   public class RelayCommand : ICommand
   {
      #region fields

      private readonly Action<object> execute;
      private readonly Predicate<object> canExecute;

      #endregion

      #region constructors

      public RelayCommand(Action execute)
         : this(o => execute())
      {
      }

      public RelayCommand(Action<object> execute)
         : this(execute, null)
      {
      }

      public RelayCommand(Action execute, Func<bool> canExecute)
         : this(o => execute(), o => canExecute())
      {
      }

      public RelayCommand(Action<object> execute, Predicate<object> canExecute)
      {
         if (execute == null)
            throw new ArgumentException("execute");

         this.execute = execute;
         this.canExecute = canExecute;
      }

      #endregion

      #region events

      public event EventHandler<RelayCommandEventArgs> PreExecution;
      public event EventHandler<RelayCommandEventArgs> PostExecution;

      #endregion

      #region ICommand members

      [DebuggerStepThrough]
      public bool CanExecute(object parameter)
      {
         return canExecute == null ? true : canExecute(parameter);
      }

      public event EventHandler CanExecuteChanged
      {
         add
         {
            if (canExecute != null)
               CommandManager.RequerySuggested += value;
         }
         remove
         {
            if (canExecute != null)
               CommandManager.RequerySuggested -= value;
         }
      }

      public void Execute(object parameter)
      {
         if (PreExecution != null)
            PreExecution(this, new RelayCommandEventArgs(parameter));

         execute(parameter);

         if (PostExecution != null)
            PostExecution(this, new RelayCommandEventArgs(parameter));
      }

      #endregion

      #region static method

      public static RelayCommand<T> CreateCommand<T>(ref RelayCommand<T> command, Action<T> execute)
      {
         return CreateCommand(ref command, execute, null);
      }

      public static RelayCommand<T> CreateCommand<T>(ref RelayCommand<T> command, Action<T> execute, Predicate<T> canExecute)
      {
         return command ?? (command = new RelayCommand<T>(execute, canExecute));
      }

      public static RelayCommand CreateCommand(ref RelayCommand command, Action execute)
      {
         return CreateCommand(ref command, execute, null);
      }

      public static RelayCommand CreateCommand(ref RelayCommand command, Action execute, Func<bool> canExecute)
      {
         if (command == null)
         {
            if (canExecute != null)
            {
               command = new RelayCommand(execute, canExecute);
            }
            else
            {
               command = new RelayCommand(execute);
            }
         }

         return command;
      }

      #endregion
   }
}
