using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace quick_mouse_recorder
{
	public static class Extension
	{
		private const string SEPARATOR = ","; // 区切り記号として使用する文字列
		private const string FORMAT = "{0}:{1}"; // 複合書式指定文字列

		/// <summary>
		/// すべての公開フィールドの情報を文字列にして返します
		/// </summary>
		public static string ToStringFields<T>(this T obj)
		{
			return String.Join(SEPARATOR, obj
				.GetType()
				.GetFields(BindingFlags.Instance | BindingFlags.Public)
				.Select(c => {
					var o = c.GetValue(obj);
				// if (o is IDictionary) return ToStringKVPEnumerable(o);
				// else if (o is IList) return ToStringEnumerable(o);
				return String.Format(FORMAT, c.Name, o);
				})
				.ToArray());
		}

		/// <summary>
		/// すべての公開プロパティの情報を文字列にして返します
		/// </summary>
		public static string ToStringProperties<T>(this T obj)
		{
			return String.Join(SEPARATOR, obj
				.GetType()
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(c => c.CanRead)
				.Select(c => String.Format(FORMAT, c.Name, c.GetValue(obj, null)))
				.ToArray());
		}

		/// <summary>
		/// すべての公開フィールドと公開プロパティの情報を文字列にして返します
		/// </summary>
		public static string ToStringMembers<T>(this T obj)
		{
			return String.Join(SEPARATOR, new[] { obj.ToStringFields(), obj.ToStringProperties() });
		}

		public static string ToStringEnumerable<T>(this IEnumerable<T> source, string separator = SEPARATOR)
		{
			return source + ":\n" + String.Join(separator, source
					   .Select((e, i) => "\t" + String.Format(FORMAT, "[" + i + "]", e.ToString()) + "\n")
					   .ToArray());
		}

		public static string ToStringKVPEnumerable<T, U>(this IEnumerable<KeyValuePair<T, U>> source, string separator = SEPARATOR)
		{
			return source + ":\n" + String.Join(separator, source
					   .Select((e) => "\t" + String.Format(FORMAT, "[" + e.Key + "]", e.Value.ToString()) + "\n")
					   .ToArray());
		}
	}
}
