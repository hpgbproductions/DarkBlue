using System;
using System.Reflection;
using UnityEngine;

public class DarkBlue : MonoBehaviour
{
    // Whether altitude effects should run automatically, using the aircraft's altitude
    private bool AutoAltitude = false;

    // SkyDome GameObject
    private GameObject sky;

    // Atmosphere
    private Type skyComponentAtmosphereType;
    private object skyComponentAtmosphere;
    private FieldInfo Directionality;
    private FieldInfo Brightness;

    // Cloud
    private Type skyComponentCloudType;
    private object skyComponentCloud;
    private FieldInfo CloudOpacity;

    private void Start()
    {
        ServiceProvider.Instance.DevConsole.RegisterCommand("DarkBlueFindSkyDome", FindSkyDome);
        ServiceProvider.Instance.DevConsole.RegisterCommand<float>("DarkBlueSetAltitude", SetAltitude);
        ServiceProvider.Instance.DevConsole.RegisterCommand("DarkBlueAutoAltitude", ActivateAutoAltitude);
    }

    private void Update()
    {
        if (AutoAltitude)
        {
            if (ServiceProvider.Instance.GameState.IsInLevel && !ServiceProvider.Instance.GameState.IsInDesigner)
            {
                if (!ServiceProvider.Instance.GameState.IsPaused)
                    SetAltitude(ServiceProvider.Instance.PlayerAircraft.Altitude);
            }
            else
            {
                AutoAltitude = false;
            }
        }
    }

    // Returns a SkyDome GameObject, or null if it could not be found.
    private GameObject GetSkyDome()
    {
        // Notice: As the name of the sky dome GameObject may be changed in the future, do not use its name to find it
        // e.g.: GameObject.Find("SkyDome_Low(Clone)");
        // Instead, look for a TOD component, getting its class via reflection.

        Component[] AllComponents = FindObjectsOfType<Component>();
        foreach (Component c in AllComponents)
        {
            if (c.GetType().Name == "TOD_Sky")
            {
                return c.gameObject;
            }
        }
        Debug.LogError("No SkyDome found");
        return null;
    }

    private void ActivateAutoAltitude()
    {
        if (ServiceProvider.Instance.GameState.IsInLevel && !ServiceProvider.Instance.GameState.IsInDesigner)
        {
            AutoAltitude = true;
        }
        else
        {
            Debug.LogError("Cannot activate automatic system outside of level!");
            AutoAltitude = false;
        }
    }

    // Manually sets scattering coefficients
    private void SetAltitude(float s)
    {
        if (sky == null)
        {
            Debug.LogError("No SkyDome found! Run DarkBlueFindSkyDome first!");
            return;
        }

        // Returns 1 at low altitude, decreases as altitude increases
        float LerpAmount = Mathf.Min(1, Mathf.Exp(-0.00007f * s));
        float CloudLerpAmount = Mathf.Clamp01(Mathf.InverseLerp(11000f, 4000f, s));

        Directionality.SetValue(skyComponentAtmosphere, Mathf.Lerp(0.2f, 0.7f, LerpAmount));
        Brightness.SetValue(skyComponentAtmosphere, Mathf.Lerp(0.1f, 1.5f, LerpAmount));

        CloudOpacity.SetValue(skyComponentCloud, CloudLerpAmount);
    }

    // Looks for a SkyDome GameObject and displays member information. This is the setup command.
    private void FindSkyDome()
    {
        sky = GetSkyDome();

        // Return if no SkyDome GameObject is loaded.
        if (sky == null)
        {
            return;
        }

        // List all children of the SkyDome GameObject
        string skyc_debug = string.Empty;
        int skyChildCount = sky.transform.childCount;
        for (int i = 0; i < skyChildCount; i++)
        {
            skyc_debug += "\n> " + sky.transform.GetChild(i).name;
        }
        Debug.Log(string.Format("SkyDome GameObject found: {0}\n\n{1} child GameObjects found:{2}",
            sky.name,
            skyChildCount,
            skyc_debug));
        
        Component[] components = sky.GetComponents<Component>();

        for (int c = 0; c < components.Length; c++)
        {
            Type componentType = components[c].GetType();
            string DebugString = "SkyDome component found: " + componentType;

            // List fields of the component type. Only show public fields.
            FieldInfo[] fields = componentType.GetFields();
            string f_debug = string.Empty;
            int f_count = 0;
            foreach (FieldInfo field in fields)
            {
                if (field.IsPublic)
                {
                    f_debug += string.Format("\n> {0} {1} = {2}",
                        field.FieldType,
                        field.Name,
                        field.GetValue(components[c]));
                    f_count++;

                    if (componentType.Name == "TOD_Sky")
                    {
                        // Find specific fields and store them for later
                        if (field.FieldType.Name == "TOD_AtmosphereParameters")
                        {
                            skyComponentAtmosphereType = field.FieldType;
                            skyComponentAtmosphere = field.GetValue(components[c]);

                            FieldInfo[] aFields = skyComponentAtmosphereType.GetFields();
                            foreach (FieldInfo aField in aFields)
                            {
                                if (aField.Name == "Directionality")
                                    Directionality = aField;
                                else if (aField.Name == "Brightness")
                                    Brightness = aField;
                            }
                        }
                        else if (field.FieldType.Name == "TOD_CloudParameters")
                        {
                            skyComponentCloudType = field.FieldType;
                            skyComponentCloud = field.GetValue(components[c]);

                            FieldInfo[] cFields = skyComponentCloudType.GetFields();
                            foreach (FieldInfo cField in cFields)
                            {
                                if (cField.Name == "Opacity")
                                    CloudOpacity = cField;
                            }
                        }
                    }
                }
            }
            DebugString += "\n\n" + f_count + " public fields found:" + f_debug;

            // List properties of the component type
            PropertyInfo[] properties = componentType.GetProperties();
            DebugString += "\n\n" + properties.Length + " properties found:";
            for (int p = 0; p < properties.Length; p++)
            {
                DebugString += string.Format("\n> {0} {1} = {2}", 
                    properties[p].PropertyType,
                    properties[p].Name,
                    properties[p].GetValue(components[c]));
            }

            Debug.Log(DebugString);
        }
    }
}
