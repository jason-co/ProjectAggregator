using System;

namespace Core.Commands
{
   public class RelayCommandEventArgs : EventArgs
   {
      public object Parameter { get; private set; }

      public RelayCommandEventArgs(object parameter)
      {
         Parameter = parameter;
      }
   }
}
