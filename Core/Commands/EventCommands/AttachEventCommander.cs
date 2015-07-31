using System.Windows;

namespace Core.Commands.EventCommands
{
   public static class AttachAttachEventCommander
   {
      // Make it internal so WPF ignores the property and always uses the public getter/setter.  This is per
      // John Gossman blog post - 07/2008.
      internal static readonly DependencyProperty MappingsProperty = DependencyProperty.RegisterAttached("InternalMappings",
                          typeof(CommandEventCollection), typeof(AttachAttachEventCommander),
                          new UIPropertyMetadata(null, OnMappingsChanged));

      /// <summary>
      /// Retrieves the mapping collection
      /// </summary>
      /// <param name="obj"></param>
      /// <returns></returns>
      internal static CommandEventCollection InternalGetMappingCollection(DependencyObject obj)
      {
         var map = obj.GetValue(MappingsProperty) as CommandEventCollection;
         if (map == null)
         {
            map = new CommandEventCollection();
            SetMappings(obj, map);
         }
         return map;
      }

      /// <summary>
      /// This retrieves the mapping collection
      /// </summary>
      /// <param name="obj">Dependency Object</param>
      /// <returns>Mapping collection</returns>
      public static CommandEventCollection GetMappings(DependencyObject obj)
      {
         return InternalGetMappingCollection(obj);
      }

      /// <summary>
      /// This sets the mapping collection.
      /// </summary>
      /// <param name="obj">Dependency Object</param>
      /// <param name="value">Mapping collection</param>
      public static void SetMappings(DependencyObject obj, CommandEventCollection value)
      {
         obj.SetValue(MappingsProperty, value);
      }

      /// <summary>
      /// This changes the event mapping
      /// </summary>
      /// <param name="target"></param>
      /// <param name="e"></param>
      private static void OnMappingsChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
      {
         if (e.OldValue != null)
         {
            CommandEventCollection cec = e.OldValue as CommandEventCollection;
            if (cec != null)
               cec.Unsubscribe(target);
         }
         if (e.NewValue != null)
         {
            CommandEventCollection cec = e.NewValue as CommandEventCollection;
            if (cec != null)
               cec.Subscribe(target);
         }
      }
   }
}
