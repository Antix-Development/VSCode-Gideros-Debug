using System;
using System.Collections.Generic;
using System.IO;

namespace GiderosPlayerRemote
{
    static class Util
    {
        public static V? Find<K, V>(this Dictionary<K, V> dict, K key)
            where V: struct
        {
            V value;
            if (dict.TryGetValue(key, out value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        public static V FindR<K, V>(this Dictionary<K, V> dict, K key)
            where V : class
        {
            V value;
            if (dict.TryGetValue(key, out value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        // 마지막 변경 시각으로부터 지금까지 흐른 시간을 초 단위로.
        public static int FileAge(string path)
        {
            var now = DateTime.Now;
            var wtime = File.GetLastWriteTime(path);
            return (int)(now - wtime).TotalSeconds;
        }
    }
}
