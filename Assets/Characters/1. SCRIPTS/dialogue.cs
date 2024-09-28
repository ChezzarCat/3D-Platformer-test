using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class dialogueEntry
{
	public string textsound;
    public string face;
    
    [TextArea(3, 10)]
    public string sentence;
}

[System.Serializable]
public class dialogue
{
    public dialogueEntry[] entries;
}
