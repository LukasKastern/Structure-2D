using System.Collections.Generic;

namespace Structure2D.Utility
{
    /// <summary>
    /// Generic Pool which you can use to reuse Lists.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class ListPool<T> {

        private static Stack<List<T>> stack = new Stack<List<T>>();

        /// <summary>
        /// Returns a list of the pool.
        /// If there is none one will be created.
        /// </summary>
        /// <returns></returns>
        public static List<T> Get () {
            if (stack.Count > 0) {
                return stack.Pop();
            }
            return new List<T>();
        }

        /// <summary>
        /// Push a list back into the pool.
        /// The list will be cleared before being added.
        /// </summary>
        public static void Add (List<T> list) {
            list.Clear();
            stack.Push(list);
        }
    }
}