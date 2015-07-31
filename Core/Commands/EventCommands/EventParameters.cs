using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Core.Commands.EventCommands
{
   public class EventParameters
   {
      /// <summary>
      /// The sender of the handled event
      /// </summary>
      public object Sender { get; set; }
      /// <summary>
      /// The passed EventArgs for the event.
      /// </summary>
      public EventArgs EventArgs { get; set; }

      /// <summary>
      /// The ICommand which has just been executed
      /// </summary>
      public ICommand Command { get; set; }

      /// <summary>
      /// The associated CommandParameter (if any).
      /// </summary>
      public object CommandParameter { get; set; }

      /// <summary>
      /// Constructor for the EventParameters
      /// </summary>
      /// <param name="command">ICommand</param>
      /// <param name="sender">Event sender</param>
      /// <param name="e">Event args</param>
      /// <param name="parameter">CommandParameter</param>
      internal EventParameters(ICommand command, object sender, EventArgs e, object parameter)
      {
         Command = command;
         Sender = sender;
         EventArgs = e;
         CommandParameter = parameter;
      }
   }
}
