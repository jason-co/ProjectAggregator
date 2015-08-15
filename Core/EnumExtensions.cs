using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Core
{
   public static class EnumExtensions
   {
      private static readonly ConcurrentDictionary<Enum, string> descriptions = new ConcurrentDictionary<Enum, string>();


      /// <summary>
      /// Returns the Description of an enum member, as specified by the DescriptionAttribute.  If no DescriptionAttribute is found, the result of ToString is returned.
      /// </summary>
      /// <param name="en"></param>
      /// <returns></returns>
      public static string ToDescription(this Enum en)
      {
         return descriptions.GetOrAdd(en, ToDescriptionNoCache);
      }

      /// <summary>
      /// Returns the Description of an enum member, as specified by the DescriptionAttribute.  If no DescriptionAttribute is found, the result of ToString is returned.
      /// </summary>
      /// <param name="en"></param>
      /// <returns></returns>
      public static string ToDescriptionNoCache(this Enum en)
      {
         if (en == null) { return string.Empty; }
         Type type = en.GetType();
         MemberInfo[] memberInfos = type.GetMember(en.ToString());
         if (memberInfos.Length > 0)
         {
            object[] attributes = memberInfos[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
            {
               return ((DescriptionAttribute)attributes[0]).Description;
            }
         }
         return en.ToString();
      }
   }
}
