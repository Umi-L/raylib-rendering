namespace raylib_rendering.Rendering;

public static class ResizeManager
{
    public delegate void ResizeCallback(int width, int height);

    public static List<ResizeCallback> listeners = new List<ResizeCallback>();
    
    public static void AddListener(ResizeCallback callback)
    {
        listeners.Add(callback);
    }
    
    public static void RemoveListener(ResizeCallback callback)
    {
        listeners.Remove(callback);
    }
    
    public static void Invoke(int width, int height)
    {
        foreach (var listener in listeners)
        {
            listener(width, height);
        }
    }
}