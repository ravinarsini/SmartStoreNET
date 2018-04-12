﻿using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Services.Localization
{
	public class LocalizedValue
	{
		// Regex for all types of brackets which need to be "swapped": ({[]})
		private readonly static Regex _rgBrackets = new Regex(@"\(|\{|\[|\]|\}|\)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

		public static string FixBrackets(string str, bool rtl = false)
		{
			var controlChar = rtl ? "&rlm;" : "&lrm;";
			return _rgBrackets.Replace(str, m =>
			{
				return controlChar + m.Value + controlChar;
			});
		}
	}

	[Serializable]
	public class LocalizedValue<T> : IHtmlString, IComparable //, IComparable<T>
	{
		private readonly T _value;
		private readonly Language _requestLanguage;
		private readonly Language _currentLanguage;

		//private string _bidiStr;

		public LocalizedValue(T value, Language requestLanguage, Language currentLanguage)
		{
			_value = value;
			_requestLanguage = requestLanguage;
			_currentLanguage = currentLanguage;
		}

		public T Value
		{
			get { return _value; }
		}

		[JsonIgnore]
		public Language RequestLanguage
		{
			get { return _requestLanguage; }
		}

		[JsonIgnore]
		public Language CurrentLanguage
		{
			get { return _currentLanguage; }
		}

		public bool IsFallback
		{
			get { return _requestLanguage != _currentLanguage; }
		}

		public bool BidiOverride
		{
			get { return _requestLanguage != _currentLanguage && _requestLanguage.Rtl != _currentLanguage.Rtl; }
		}

		public static implicit operator T(LocalizedValue<T> obj)
		{
			if (obj == null)
			{
				return default(T);
			}

			return obj.Value;
		}

		public override string ToString()
		{
			if (_value == null)
			{
				return null;
			}

			if (typeof(T) == typeof(string))
			{
				return _value as string;
			}

			return _value.Convert<string>(CultureInfo.GetCultureInfo(_currentLanguage.LanguageCulture));
		}

		public string ToHtmlString()
		{
			return ToString();

			//var str = ToString();

			//if (BidiOverride)
			//{
			//	if (_bidiStr == null)
			//	{
			//		_bidiStr = LocalizedValue.FixBrackets(str, _currentLanguage.Rtl);
			//	}
				
			//	return _bidiStr;
			//}

			//return str;
		}

		public override int GetHashCode()
		{
			var hashCode = 0;
			if (_value != null)
				hashCode ^= _value.GetHashCode();
			return hashCode;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || obj.GetType() != GetType())
				return false;

			var that = (LocalizedValue<T>)obj;
			return string.Equals(_value, that._value);
		}

		public int CompareTo(object other)
		{
			if (Value is IComparable c && other is LocalizedValue<T> l)
			{
				return c.CompareTo(l.Value);
			}

			return 0;
		}

		//public int CompareTo(T other)
		//{
		//	if (Value is IComparable<T> c)
		//	{
		//		return c.CompareTo(other);
		//	}

		//	return 0;
		//}
	}
}
