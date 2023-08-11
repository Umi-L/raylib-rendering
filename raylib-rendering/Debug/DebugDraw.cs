namespace raylib_rendering.Debug;

public class DebugDraw
{
    public delegate void DebugDrawCallback();
    public static List<DebugDrawCallback> callbacks = new List<DebugDrawCallback>();

    public static void AddImGuiCallback(DebugDrawCallback callback)
    {
        callbacks.Add(callback);
    }

    public static void Draw()
    {
        foreach (var callback in callbacks)
        {
            callback();
        }
        
        callbacks.Clear();
    }
}