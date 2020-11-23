using System;
using System.Collections;
using System.Collections.Generic;

namespace Model
{
    public abstract class HEnum<T> where T : class
    {
        protected HEnum(string name, int value)
        {
            Name = name;
            Value = value;

            dictNameMembers.Add(name, this);
            dictValueMembers.Add(value, this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">枚举名</param>
        /// <param name="tag">例如伤害属性tag、防御属性tag</param>
        /// <param name="value"></param>
        protected HEnum(string name, int value, string tag, string dTag = "")
        {
            Name = name;
            Value = value;
            Tag = tag;
            DTag = dTag;

            dictNameMembers.Add(name, this);
            dictValueMembers.Add(value, this);
        }

        public static T GetEnum(string name)
        {
            HEnum<T> t;
            dictNameMembers.TryGetValue(name, out t);
            return t as T;
        }

        /// <summary>
        /// 获取某一Tag的枚举列表.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static List<T> GetEnumsByTag(string tag, string dtag = "")
        {
            var members = new List<T>();
            var itor = dictValueMembers.GetEnumerator();
            while (itor.MoveNext())
            {
                var h = itor.Current.Value;
                if (h.Tag.Equals(tag, StringComparison.OrdinalIgnoreCase)
                    && (string.IsNullOrEmpty(dtag) 
                    || h.DTag.Equals(dtag, StringComparison.OrdinalIgnoreCase)))
                {
                    members.Add(h as T);
                }
            }

            return members;
        }

        /// <summary>
        /// 显式强制从int转换
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static explicit operator HEnum<T>(int i)
        {
            if (dictValueMembers.ContainsKey(i))
            {
                return dictValueMembers[i];
            }

            throw new NotSupportedException("[HEnum] 自定义枚举目前不支持跨界转换.");
        }

        /// <summary>
        /// 显式强制向int转换
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static explicit operator int(HEnum<T> e)
        {
            return e.Value;
        }

        public static void ForEach(Action<T> action)
        {
            var itor = dictValueMembers.GetEnumerator();
            while (itor.MoveNext())
            {
                var h = itor.Current.Value;
                action(h as T);
            }
        }

        public static List<T> Enums()
        {
            var list = new List<T>();

            var itor = dictValueMembers.GetEnumerator();
            while (itor.MoveNext())
            {
                var h = itor.Current.Value;
                list.Add(h as T);
            }

            return list;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is HEnum<T>)) return false;
            return Value == ((HEnum<T>)obj).Value;
        }

        public override int GetHashCode()
        {
            HEnum<T> std = dictValueMembers[Value];
            if (std.Name == Name) return base.GetHashCode();
            return std.GetHashCode();
        }

        public override string ToString() { return Name; }
        public bool Equals(HEnum<T> other) { return Value.Equals(other.Value); }
        public int CompareTo(HEnum<T> other) { return Value.CompareTo(other.Value); }
        
        private int Value { get; set; }
        private string Name { get; set; }
        private string Tag { get; set; }
        private string DTag { get; set; }

        private static Dictionary<int, HEnum<T>> dictValueMembers = new Dictionary<int, HEnum<T>>();
        private static Dictionary<string, HEnum<T>> dictNameMembers = new Dictionary<string, HEnum<T>>(StringComparer.OrdinalIgnoreCase);
    }
}