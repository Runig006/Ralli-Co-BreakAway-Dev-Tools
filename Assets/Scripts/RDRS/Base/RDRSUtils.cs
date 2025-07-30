using System.Collections;
using UnityEngine;

public static class RDRSUtils
{
    public static bool toBoolean(object value)
    {
        if(value == null)
        {
            return false;
        }
        switch (value)
        {
            case bool b:
                return b;
                
            case Vector3 v3:
                return v3 != Vector3.zero;

            case Vector2 v2:
                return v2 != Vector2.zero;

            case ICollection collection:
                return collection.Count > 0;

            case IEnumerable enumerable:
                foreach (var _ in enumerable)
                {
                    return true;
                }
                return false;

            default:
                try
                {
                    return System.Convert.ToSingle(value) > 0f;
                }
                catch
                {
                    return true; //Is not a number, but it is something
                }
        }
    }
}
