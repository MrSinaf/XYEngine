namespace XYEngine;

public struct Vector3(float x, float y, float z = 0)
{
    public static Vector3 zero => new ();
    public static Vector3 one => new (1);

    public Vector3(float xyz) : this(xyz, xyz, xyz) { }

    public float x = x;
    public float y = y;
    public float z = z;
}