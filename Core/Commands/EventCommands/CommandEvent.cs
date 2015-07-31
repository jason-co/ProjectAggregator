using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Core.Commands.EventCommands
{
   public class CommandEvent : Freezable
   {
      /// <summary>
      /// Command Property Dependency Property
      /// </summary>
      public static readonly DependencyProperty CommandProperty =
         DependencyProperty.Register("Command", typeof(ICommand), typeof(CommandEvent),
            new UIPropertyMetadata(null));
      public ICommand Command
      {
         get { return (ICommand)GetValue(CommandProperty); }
         set { SetValue(CommandProperty, value); }
      }

      /// <summary>
      /// Parameter for the ICommand
      /// </summary>
      public static readonly DependencyProperty CommandParameterProperty =
         DependencyProperty.Register("CommandParameter", typeof(object), typeof(CommandEvent),
            new UIPropertyMetadata(null));
      public object CommandParameter
      {
         get { return GetValue(CommandParameterProperty); }
         set { SetValue(CommandParameterProperty, value); }
      }

      /// <summary>
      /// Event Dependency Property
      /// </summary>
      public static readonly DependencyProperty EventProperty =
         DependencyProperty.Register("Event", typeof(string), typeof(CommandEvent),
            new UIPropertyMetadata(string.Empty));
      public string Event
      {
         get { return (string)GetValue(EventProperty); }
         set { SetValue(EventProperty, value); }
      }

      /// <summary>
      /// DataContext for any bindings applied to this CommandEvent
      /// </summary>
      public static readonly DependencyProperty DataContextProperty =
         FrameworkElement.DataContextProperty.AddOwner(typeof(CommandEvent),
            new FrameworkPropertyMetadata(null));

      /// <summary>
      /// Establishes whether the parameter passed is wrapped in an EventParameter so that one can access the EventArgs and Source of the event.
      /// </summary>
      public static readonly DependencyProperty UseEventParameterWrapperProperty =
         DependencyProperty.Register("UseEventParameterWrapper", typeof(bool), typeof(CommandEvent),
            new PropertyMetadata(false));
      public bool UseEventParameterWrapper
      {
         get { return (bool)GetValue(UseEventParameterWrapperProperty); }
         set { SetValue(UseEventParameterWrapperProperty, value); }
      }

      /// <summary>
      /// Wires up an event to the target
      /// </summary>
      /// <param name="target"></param>
      internal void Subscribe(object target)
      {
         if (target != null)
         {
            if (target is FrameworkElement)
            {
               BindingOperations.SetBinding(this, FrameworkElement.DataContextProperty, new Binding("DataContext") { Source = target });
            }

            EventInfo ei = target.GetType().GetEvent(Event, BindingFlags.Public | BindingFlags.Instance);
            if (ei != null)
            {
               ei.RemoveEventHandler(target, GetEventMethod(ei));
               ei.AddEventHandler(target, GetEventMethod(ei));
            }
         }
      }

      /// <summary>
      /// Unwires target event
      /// </summary>
      /// <param name="target"></param>
      internal void Unsubscribe(object target)
      {
         if (target != null)
         {
            EventInfo ei = target.GetType().GetEvent(Event, BindingFlags.Public | BindingFlags.Instance);
            if (ei != null)
               ei.RemoveEventHandler(target, GetEventMethod(ei));
         }
      }

      private Delegate _method;
      private Delegate GetEventMethod(EventInfo ei)
      {
         if (ei == null)
            throw new ArgumentNullException("ei");
         if (ei.EventHandlerType == null)
            throw new ArgumentException("EventHandlerType is null");
         if (_method == null)
            _method = Delegate.CreateDelegate(ei.EventHandlerType, this, GetType().GetMethod("OnEventRaised", BindingFlags.NonPublic | BindingFlags.Instance));

         return _method;
      }

      /// <summary>
      /// This is invoked by the event - it invokes the command.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnEventRaised(object sender, EventArgs e)
      {
         if (Command != null)
         {
            if (Command.CanExecute(CommandParameter))
            {
               object parameter = UseEventParameterWrapper ? new EventParameters(Command, sender, e, CommandParameter) : CommandParameter;
               Command.Execute(parameter);
            }
         }
#if DEBUG
         else
         {
            Debug.WriteLine(string.Format("Missing Command on event handler, {0}: Sender={1}, EventArgs={2}", Event, sender, e));
         }
#endif
      }

      /// <summary>
      /// When implemented in a derived class, creates a new instance of the <see cref="T:System.Windows.Freezable"/> derived class. 
      /// </summary>
      /// <returns>
      /// The new instance.
      /// </returns>
      protected override Freezable CreateInstanceCore()
      {
         throw new NotImplementedException();
      }
   }
}
