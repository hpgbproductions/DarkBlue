using UnityEngine;

public class GradientTools : MonoBehaviour
{
    public string DebugGradientInformation(Gradient g, string name = "")
    {
        string DebugText = "Information for Gradient " + name;

        GradientColorKey[] ColorKeys = g.colorKeys;
        int NumColorKeys = g.colorKeys.Length;
        DebugText += "\n\n" + NumColorKeys + " color keys detected:";
        for (int i = 0; i < NumColorKeys; i++)
        {
            DebugText += string.Format("\n> {0} @ {1}", ColorKeys[i].color.ToString(), ColorKeys[i].time);
        }

        GradientAlphaKey[] AlphaKeys = g.alphaKeys;
        int NumAlphaKeys = g.alphaKeys.Length;
        DebugText += "\n\n" + NumAlphaKeys + " alpha keys detected:";
        for (int i = 0; i < NumAlphaKeys; i++)
        {
            DebugText += string.Format("\n> {0} @ {1}", AlphaKeys[i].alpha, AlphaKeys[i].time);
        }

        return DebugText;
    }

    public Color LerpColor(Color a, Color b, float t)
    {
        return new Color(
            Mathf.Lerp(a.r, b.r, t),
            Mathf.Lerp(a.g, b.g, t),
            Mathf.Lerp(a.b, b.b, t),
            Mathf.Lerp(a.a, b.a, t));
    }

    public Gradient GlobalAlpha(Gradient g, float a)
    {
        for (int i = 0; i < g.alphaKeys.Length; i++)
        {
            g.alphaKeys[i].alpha = a;
        }
        return g;
    }

    public Gradient GlobalBrightness(Gradient g, float lerpValue)
    {
        for (int i = 0; i < g.colorKeys.Length; i++)
        {
            g.colorKeys[i].color = LerpColor(Color.black, g.colorKeys[i].color, lerpValue);
        }
        return g;
    }
}