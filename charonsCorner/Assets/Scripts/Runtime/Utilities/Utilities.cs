using System;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public static class Utilities 
    {
        /// <summary>
        /// Null propogation check for Unity object methods.
        /// Equivalent to 'obj?.Method()' but Unity safe.
        /// </summary>
        public static T IfNotNullThenCall<T>(this T obj, Action<T> action) where T : UnityEngine.Object
        {
            if (obj != null)
                action(obj);
            return obj;
        }

        /// <summary>
        /// Null propogation check for Unity object methods/properties that return a value.
        /// Equivalent to 'obj?.Method()' or 'obj?.Property' but Unity safe.
        /// </summary>
        public static TResult IfNotNullThenGet<T, TResult>(this T obj, Func<T, TResult> func) where T : UnityEngine.Object
        {
            return obj != null ? func(obj) : default;
        }

        /// <summary>
        /// Helper method to set the alpha value of a Unity UI Image component.
        /// Needed because Image.color.a = alpha does not work.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="alpha"></param>
        public static void SetImageAlpha(this UnityEngine.UI.Image image, float alpha)
        {
            if (image == null)
                return;

            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }

        /// <summary>
        /// Returns a new Vector3 with one component (x, y, or z) set to a new value.
        /// Vector3.x = x does not work as expected in Unity, so this is a workaround.
        /// </summary>
        public static Vector3 WithX(this Vector3 vector, float x) => new Vector3(x, vector.y, vector.z);
        /// <summary>
        /// Returns a new Vector3 with one component (x, y, or z) set to a new value.
        /// Vector3.z = z does not work as expected in Unity, so this is a workaround.
        /// </summary>
        public static Vector3 WithY(this Vector3 vector, float y) => new Vector3(vector.x, y, vector.z);
        /// <summary>
        /// Returns a new Vector3 with one component (x, y, or z) set to a new value.
        /// Vector3.z = z does not work as expected in Unity, so this is a workaround.
        /// </summary>
        public static Vector3 WithZ(this Vector3 vector, float z) => new Vector3(vector.x, vector.y, z);

        /// <summary>
        /// Sets the x component of a Vector3 to a new value.
        /// Vector3.x = x does not work as expected in Unity, so this is a workaround.
        /// </summary>
        public static void SetX(this ref Vector3 vector, float x) => vector = vector.WithX(x);
        /// <summary>
        /// Sets the y component of a Vector3 to a new value.
        /// Vector3.y = y does not work as expected in Unity, so this is a workaround.
        /// </summary>
        public static void SetY(this ref Vector3 vector, float y) => vector = vector.WithY(y);
        /// <summary>
        /// Sets the z component of a Vector3 to a new value.
        /// Vector3.z = z does not work as expected in Unity, so this is a workaround.
        /// </summary>
        public static void SetZ(this ref Vector3 vector, float z) => vector = vector.WithZ(z);
    }
}
