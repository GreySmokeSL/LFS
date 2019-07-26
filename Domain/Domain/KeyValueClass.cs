using System;

namespace Domain.Domain
{
    [Serializable]
    public class KeyValueClass<TKey, TValue>
    {
        public KeyValueClass(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public TKey Key { get; set;  }
        public TValue Value { get; set; }

        //public override string ToString()
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.Append('[');
        //    if (Key != null)
        //        sb.Append(Key);
        //    sb.Append(", ");
        //    if (Value != null)
        //        sb.Append(Value);
        //    sb.Append(']');
        //    //sb.Append(Checked ? '+' : '-');
        //    return sb.ToString();
        //}
    }

}
