using UnityEngine;

public static class ObjectManager
{
    public static GameObject GetObjectAtPath(string path, bool createIfMissing = false)
    {
        string[] pathParts = path.Split('/');
        GameObject current = null;

        foreach (string part in pathParts)
        {
            if (string.IsNullOrEmpty(part)) continue;

            GameObject next = current == null 
                ? GameObject.Find(part) 
                : current.transform.Find(part)?.gameObject;

            if (next == null)
            {
                if (!createIfMissing)
                    return null;

                next = new GameObject(part);
                if (current != null)
                    next.transform.SetParent(current.transform);
            }

            current = next;
        }

        return current;
    }
}