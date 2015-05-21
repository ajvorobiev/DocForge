using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.ArrayExtensions;
using ClassForge.Model;
using DocForge.Helpers;
using Newtonsoft.Json;

namespace System
{
    public static class ObjectExtensions
    {
        public static string HtmlPage<T>(this T cl) where T : Class
        {
            var result = cl.Name + ".html";

            if (cl.ContainmentParent != null)
            {
               result = ComputePageName(cl, result);
            }

            return result;
        }

        public static string HtmlLinkToPage<T>(this T cl) where T : Class
        {
            return string.Format("<a href=\"{0}\">{1}</a>", cl.HtmlPage(), cl.Name);
        }

        public static Class TopContainer<T>(this T cl) where T : Class
        {
            var containArray = new List<Class>();

            containArray.Add(cl);

            ResolveContainmentBreadcrumb(cl, ref containArray);

            return containArray.Last();
        }

        public static List<Class> Containers<T>(this T cl) where T : Class
        {
            var containArray = new List<Class>();

            containArray.Add(cl);

            ResolveContainmentBreadcrumb(cl, ref containArray);

            return containArray;
        }

        public static string ClassDiagramJSON<T>(this T cl) where T : Class
        {
            PlainClass root;

            if (cl.InheritanceClass != null)
            {
                root = new PlainClass()
                {
                    name = cl.InheritanceClass.Name
                };

                var thisClass = new PlainClass()
                {
                    name = cl.Name
                };

                if (cl.InheritanceChildren.Count != 0)
                {
                    thisClass.children = new List<PlainClass>();
                    foreach (var child in cl.InheritanceChildren)
                    {
                        thisClass.children.Add(new PlainClass() { name = child.Name });
                    } 
                }

                root.children = new List<PlainClass>();
                root.children.Add(thisClass);
            }
            else
            {
                root = new PlainClass()
                {
                    name = cl.Name
                };

                if (cl.InheritanceChildren.Count != 0)
                {
                    root.children = new List<PlainClass>();
                    foreach (var child in cl.InheritanceChildren)
                    {
                        root.children.Add(new PlainClass() {name = child.Name});
                    }
                }
            }

            return JsonConvert.SerializeObject(root, new JsonSerializerSettings(){ NullValueHandling = NullValueHandling.Ignore});
        }

        public static string ContainmentBreadcrumb<T>(this T cl) where T : Class
        {
            var containArray = new List<Class>();

            ResolveContainmentBreadcrumb(cl, ref containArray);

            containArray.Reverse();
            return string.Join(" > ", containArray.Select(s => s.HtmlLinkToPage()));
        }

        private static void ResolveContainmentBreadcrumb(Class cl, ref List<Class> containArray)
        {
            if (cl.ContainmentParent != null)
            {
                containArray.Add(cl.ContainmentParent);
                ResolveContainmentBreadcrumb(cl.ContainmentParent, ref containArray);
            }
        }

        public static string ComputePageName(Class cl, string computed)
        {
            var parent = cl.ContainmentParent;
            var result = string.Format("{0}_{1}", parent.Name, computed);

            if (parent.ContainmentParent != null)
            {
                result = ComputePageName(parent, result);
            }

            return result;
        }
        
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        private static readonly MethodInfo CloneMethod = typeof(Object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool IsPrimitive(this Type type)
        {
            if (type == typeof(String)) return true;
            return (type.IsValueType & type.IsPrimitive);
        }

        public static Object Copy(this Object originalObject)
        {
            return InternalCopy(originalObject, new Dictionary<Object, Object>(new ReferenceEqualityComparer()));
        }
        private static Object InternalCopy(Object originalObject, IDictionary<Object, Object> visited)
        {
            if (originalObject == null) return null;
            var typeToReflect = originalObject.GetType();
            if (IsPrimitive(typeToReflect)) return originalObject;
            if (visited.ContainsKey(originalObject)) return visited[originalObject];
            if (typeof(Delegate).IsAssignableFrom(typeToReflect)) return null;
            var cloneObject = CloneMethod.Invoke(originalObject, null);
            if (typeToReflect.IsArray)
            {
                var arrayType = typeToReflect.GetElementType();
                if (IsPrimitive(arrayType) == false)
                {
                    Array clonedArray = (Array)cloneObject;
                    clonedArray.ForEach((array, indices) => array.SetValue(InternalCopy(clonedArray.GetValue(indices), visited), indices));
                }

            }
            visited.Add(originalObject, cloneObject);
            CopyFields(originalObject, visited, cloneObject, typeToReflect);
            RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect);
            return cloneObject;
        }

        private static void RecursiveCopyBaseTypePrivateFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect)
        {
            if (typeToReflect.BaseType != null)
            {
                RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
                CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
            }
        }

        private static void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
        {
            foreach (FieldInfo fieldInfo in typeToReflect.GetFields(bindingFlags))
            {
                if (filter != null && filter(fieldInfo) == false) continue;
                if (IsPrimitive(fieldInfo.FieldType)) continue;
                var originalFieldValue = fieldInfo.GetValue(originalObject);
                var clonedFieldValue = InternalCopy(originalFieldValue, visited);
                fieldInfo.SetValue(cloneObject, clonedFieldValue);
            }
        }
        public static T Copy<T>(this T original)
        {
            return (T)Copy((Object)original);
        }
    }

    public class ReferenceEqualityComparer : EqualityComparer<Object>
    {
        public override bool Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }
        public override int GetHashCode(object obj)
        {
            if (obj == null) return 0;
            return obj.GetHashCode();
        }
    }

    namespace ArrayExtensions
    {
        public static class ArrayExtensions
        {
            public static void ForEach(this Array array, Action<Array, int[]> action)
            {
                if (array.LongLength == 0) return;
                ArrayTraverse walker = new ArrayTraverse(array);
                do action(array, walker.Position);
                while (walker.Step());
            }
        }

        internal class ArrayTraverse
        {
            public int[] Position;
            private int[] maxLengths;

            public ArrayTraverse(Array array)
            {
                maxLengths = new int[array.Rank];
                for (int i = 0; i < array.Rank; ++i)
                {
                    maxLengths[i] = array.GetLength(i) - 1;
                }
                Position = new int[array.Rank];
            }

            public bool Step()
            {
                for (int i = 0; i < Position.Length; ++i)
                {
                    if (Position[i] < maxLengths[i])
                    {
                        Position[i]++;
                        for (int j = 0; j < i; j++)
                        {
                            Position[j] = 0;
                        }
                        return true;
                    }
                }
                return false;
            }
        }
    }

}