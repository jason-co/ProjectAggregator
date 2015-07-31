using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;

namespace Core.Commands.EventCommands
{
   public class CommandEventCollection : FreezableCollection<CommandEvent>
   {
      private object _target;
      private readonly List<CommandEvent> _currentList = new List<CommandEvent>();

      /// <summary>
      /// Constructor
      /// </summary>
      public CommandEventCollection()
      {
         ((INotifyCollectionChanged)this).CollectionChanged += OnCollectionChanged;
      }

      /// <summary>
      /// Wire up events to the target
      /// </summary>
      /// <param name="target"></param>
      internal void Subscribe(object target)
      {
         _target = target;
         foreach (var item in this)
            item.Subscribe(target);
      }

      /// <summary>
      /// Unwire all target events
      /// </summary>
      /// <param name="target"></param>
      internal void Unsubscribe(object target)
      {
         foreach (var item in this)
            item.Unsubscribe(target);
         _target = null;
      }

      /// <summary>
      /// This handles the collection change event - it then subscribes and unsubscribes events.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
      {
         switch (e.Action)
         {
            case NotifyCollectionChangedAction.Add:
               foreach (var item in e.NewItems)
                  OnItemAdded((CommandEvent)item);
               break;

            case NotifyCollectionChangedAction.Remove:
               foreach (var item in e.OldItems)
                  OnItemRemoved((CommandEvent)item);
               break;

            case NotifyCollectionChangedAction.Replace:
               foreach (var item in e.OldItems)
                  OnItemRemoved((CommandEvent)item);
               foreach (var item in e.NewItems)
                  OnItemAdded((CommandEvent)item);
               break;

            case NotifyCollectionChangedAction.Move:
               break;

            case NotifyCollectionChangedAction.Reset:
               _currentList.ForEach(i => i.Unsubscribe(_target));
               _currentList.Clear();
               foreach (var item in this)
                  OnItemAdded(item);
               break;

            default:
               return;
         }
      }

      /// <summary>
      /// A new item has been added to the event list
      /// </summary>
      /// <param name="item"></param>
      private void OnItemAdded(CommandEvent item)
      {
         if (item != null && _target != null)
         {
            _currentList.Add(item);
            item.Subscribe(_target);
         }
      }

      /// <summary>
      /// An item has been removed from the event list.
      /// </summary>
      /// <param name="item"></param>
      private void OnItemRemoved(CommandEvent item)
      {
         if (item != null && _target != null)
         {
            _currentList.Remove(item);
            item.Unsubscribe(_target);
         }
      }
   }
}
