using System;

[Serializable]
public struct SerializableInput
{
    public float Horizontal;
    public float Vertical;

    public SerializableVector3 Vector;

    public SerializableInput(float Horizontal, float Vertical, SerializableVector3 Vector)
    {
        this.Horizontal = Horizontal;
        this.Vertical = Vertical;
        this.Vector = Vector;
    }
}
