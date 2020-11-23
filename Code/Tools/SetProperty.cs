using UnityEngine;

public class SetPropertyAttribute : PropertyAttribute
{
    public SetPropertyAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; private set; }
    public bool IsDirty { get; set; }
}

