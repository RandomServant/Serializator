using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Serialize
{
    public class JsonSerializator
    {
        private string wordClassInJson = "{\"class\":";
        public string Serialize(object obj)
        {
            if (obj is int || obj is long) 
                return SerializeNum(Convert.ToInt64(obj));
            if (obj is string) 
                return SerializeString(obj.ToString());
            if (obj is double) 
                return SerializeDouble(Convert.ToInt64(obj));
            if (obj is bool) 
                return SerializeBool(Convert.ToBoolean(obj));
            if (obj.GetType().GetInterface("IList") != null) 
                return SerializeList((IList)obj);
            if (obj.GetType().GetInterface("") != null) 
                return SerializeDictionary((IDictionary)obj);
            if (obj.GetType().IsClass || obj.GetType().IsValueType) 
                return SerializeClass(obj);
            return "";
        }

        private string SerializeNum(double obj)
        {
            return $"{obj}";
        }
        
        private string SerializeDouble(double obj)
        {
            return $"{obj}";
        }
        
        private string SerializeString(string obj)
        {
            return $"\"{obj}\"";
        }
        
        private string SerializeBool(bool obj)
        {
            return $"{obj.ToString().ToLower()}";
        }
        
        private string SerializeList(IList obj)
        {
            string str = "[";
            foreach (var i in obj)
            {
                str += $"{Serialize(i)},";
            }
            return (str[^1] == ',' ? str.Remove(str.Length - 1) : str) + "]" ;
        }
        
        private string SerializeDictionary(IDictionary obj)
        {
            string str = "{";
            for (int i = 0; i < obj.Keys.Count; i++)
            {
                str += $"\"{obj.Keys.Cast<object>().ToList()[i]}\":" +
                       $"{Serialize(obj.Values.Cast<object>().ToList()[i])},";
            }
            return (str[^1] == ',' ? str.Remove(str.Length - 1) : str) + "}" ;
        }

        private string SerializeClass(object obj)
        {
            string str = $"{wordClassInJson}{obj.GetType().Name}";
            foreach (var i in obj.GetType().GetFields())
            {
                if (i.IsPublic) str += $",\"{i.Name}\":{Serialize(i.GetValue(obj))}";
            }
            return str + "}" ;
        }

        public object Deserialize(string s)
        {
            s = s.Replace(" ", "").Replace("\n", "");
            if (s[..^(s.Length - wordClassInJson.Length)] == wordClassInJson) return DeserializeClass(s);
            if (s[0] == '[') return DeserializeList(s);
            if (s[0] == '{') return DeserializeDictionary(s);
            if (s[0] == '\"') return s[1..^1];
            if (double.TryParse(s, out double d)) return d;
            if (long.TryParse(s, out long i)) return i;
            if (bool.TryParse(s, out bool b)) return b;
            return s;
        }

        private object DeserializeClass(string s)
        {
            string[] str = s[1..^1].Split(',');
            object result = Activator.CreateInstance(Type.GetType(Assembly.GetEntryAssembly()!.GetName().Name + 
                                                                  $".{str[0].Split(':')[1]}")!);
            FieldInfo[] a = result!.GetType().GetFields(BindingFlags.Public);
            for (int i = 1; i <= a.Length; i++)
            {
                a[i - 1].SetValue(result, Deserialize(str[i].Split(':')[1]));
            }
            return result;
        }

        private object DeserializeList(string s)
        {
            s = s[1..^1];
            string[] str = s.Split(',');
            List<object> a = new List<object>();
            foreach (var i in str)
            {
                a.Add(Deserialize(i));
            }
            return a;
        }

        private object DeserializeDictionary(string s)
        {
            s = s[1..^1];
            string[] str = s.Split(',');
            Dictionary<string, object> a = new Dictionary<string, object>();
            foreach (var i in str)
            {
                a.Add(i.Split(':')[0], Deserialize(i.Split(',')[1]));
            }
            return a;
        }
    }
}