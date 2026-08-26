#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TurboGaraj.Vehicle;
using TurboGaraj.UI;
using TurboGaraj.Track;
using TurboGaraj.Economy;

/// <summary>
/// Editor utility to set up the M1 prototype scene in the active scene.
/// Menu item: TurboGaraj/Setup M1 Scene
/// </summary>
public static class TurboGarajSetup
{
    private const string MENU_PATH = "TurboGaraj/Setup M1 Scene";
    private const string SCENE_PATH = "Assets/Scenes/M1_Prototype.unity";

    // Names of objects we will create/manage as part of the M1 setup.
    // We will destroy any existing objects with these names before creating new ones.
    private static readonly string[] M1ObjectNames = {
        "Main Camera",
        "Directional Light",
        "GroundPlane",
        "EconomyManager",
        "TrackManager",
        "PlayerVehicle",
        "StaminaCanvas",
        "EventSystem"
    };

    /// <summary>
    /// Returns a shader with fallbacks to avoid null references.
    /// </summary>
    private static Shader GetSafeShader(params string[] shaderNames)
    {
        foreach (var name in shaderNames)
        {
            var shader = Shader.Find(name);
            if (shader != null)
                return shader;
        }
        // Ultimate fallback: use the default Diffuse shader (always available in Unity)
        return Shader.Find("Diffuse");
    }

    [MenuItem(MENU_PATH)]
    public static void SetupM1Scene()
    {
        // Work on the active scene
        Scene scene = EditorSceneManager.GetActiveScene();
        if (!scene.isLoaded)
        {
            EditorUtility.DisplayDialog("Error", "No active scene loaded.", "OK");
            return;
        }

        // --- Step 1: Remove any existing M1 setup objects (by name) ---
        // We destroy all GameObjects (active and inactive) with the names we are going to use.
        // First collect objects to destroy to avoid modifying the collection during iteration
        var objectsToDestroy = new System.Collections.Generic.List<GameObject>();
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>(true);
        foreach (var obj in allObjects)
        {
            // Check if the object's name is in our list
            foreach (string name in M1ObjectNames)
            {
                if (obj.name == name)
                {
                    objectsToDestroy.Add(obj);
                    break; // No need to check other names for this object
                }
            }
        }

        // Now destroy all collected objects
        foreach (var obj in objectsToDestroy)
        {
            Object.DestroyImmediate(obj);
        }

        // --- Step 2: Set up the M1 prototype ---

        // Environment
        // Main Camera
        GameObject cameraGO = new GameObject("Main Camera");
        cameraGO.AddComponent<Camera>();
        cameraGO.tag = "MainCamera";

        // Directional Light
        GameObject lightGO = new GameObject("Directional Light");
        lightGO.AddComponent<Light>();
        lightGO.GetComponent<Light>().type = LightType.Directional;
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Ground Plane
        GameObject planeGO = GameObject.CreatePrimitive(PrimitiveType.Plane);
        planeGO.name = "GroundPlane";
        planeGO.transform.position = Vector3.zero;
        planeGO.transform.localScale = new Vector3(10f, 1f, 10f);
        // Optional: give it a simple material
        var planeMat = new Material(GetSafeShader("Universal Render Pipeline/Lit", "Standard", "Diffuse"));
        planeMat.color = new Color(0.2f, 0.6f, 0.2f);
        planeGO.GetComponent<Renderer>().material = planeMat;

        // Managers
        // EconomyManager
        GameObject economyGO = new GameObject("EconomyManager");
        economyGO.AddComponent<EconomyManager>();

        // TrackManager
        GameObject trackGO = new GameObject("TrackManager");
        trackGO.AddComponent<TrackManager>();

        // Vehicle
        GameObject vehicleGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        vehicleGO.name = "PlayerVehicle";
        vehicleGO.transform.position = new Vector3(0f, 0.5f, 0f); // slightly above ground
        vehicleGO.AddComponent<Rigidbody>();
        // Configure Rigidbody for realistic mass and center of mass (as in VehicleController Awake)
        Rigidbody rb = vehicleGO.GetComponent<Rigidbody>();
        rb.mass = 1200f;
        rb.centerOfMass = new Vector3(0f, -0.3f, 0f);

        VehicleController vc = vehicleGO.AddComponent<VehicleController>();
        EngineController ec = vehicleGO.AddComponent<EngineController>();
        ec.peakTorque = 1500f; // Set engine peak torque

        // Set up wheel colliders
        WheelCollider CreateWheel(string name, Vector3 localPos)
        {
            GameObject wheelGO = new GameObject(name);
            wheelGO.transform.SetParent(vehicleGO.transform, false);
            wheelGO.transform.localPosition = localPos;
            var wc = wheelGO.AddComponent<WheelCollider>();
            wc.radius = 0.3f;
            wc.suspensionDistance = 0.15f;
            return wc;
        }

        vc.frontLeftWheel = CreateWheel("FrontLeft", new Vector3(-0.8f, 0f, 0.9f));
        vc.frontRightWheel = CreateWheel("FrontRight", new Vector3(0.8f, 0f, 0.9f));
        vc.rearLeftWheel = CreateWheel("RearLeft", new Vector3(-0.8f, 0f, -0.9f));
        vc.rearRightWheel = CreateWheel("RearRight", new Vector3(0.8f, 0f, -0.9f));

        // Set sensible defaults
        vc.baseMaxSpeed = 15f;
        vc.staminaSpeedInfluence = 0.9f;
        vc.initialStamina = 100f;
        vc.staminaDrainRate = 15f;
        vc.throttleInput = 0.8f;

        // Visual placeholder (the cube already exists)
        var visualMat = new Material(GetSafeShader("Universal Render Pipeline/Lit", "Standard", "Diffuse"));
        visualMat.color = Color.red;
        vehicleGO.GetComponent<Renderer>().material = visualMat;

        // Engine Audio Controller
        // Ensure the vehicle has an AudioSource
        var audioSource = vehicleGO.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = vehicleGO.AddComponent<AudioSource>();
        }
        var engineAudio = vehicleGO.AddComponent<EngineAudioController>();
        // Note: audio clips are left null; the script will warn and not play sound.
        // In a real project, you would assign appropriate audio clips.

        // UI (Stamina Slider)
        GameObject canvasGO = new GameObject("StaminaCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Ensure we have an EventSystem in the scene (create if not present)
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSysGO = new GameObject("EventSystem");
            eventSysGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSysGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        GameObject sliderGO = new GameObject("StaminaSlider");
        sliderGO.transform.SetParent(canvasGO.transform, false);
        var slider = sliderGO.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.wholeNumbers = false;

        // Make slider look like a thin horizontal bar
        RectTransform sliderRect = slider.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 0.05f); // centered horizontally, near bottom
        sliderRect.anchorMax = new Vector2(0.5f, 0.05f);
        sliderRect.sizeDelta = new Vector3(300f, 20f);
        sliderRect.anchoredPosition = Vector2.zero;

        // Setup fill area (simple approach: use Image components)
        // Background
        GameObject bgGO = new GameObject("Fill Area");
        bgGO.transform.SetParent(sliderGO.transform, false);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        bgImg.rectTransform.anchorMin = Vector2.zero;
        bgImg.rectTransform.anchorMax = Vector2.one;
        bgImg.rectTransform.sizeDelta = Vector2.zero;

        // Fill
        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(bgGO.transform, false);
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = Color.green;
        fillImg.rectTransform.anchorMin = Vector2.zero;
        fillImg.rectTransform.anchorMax = Vector2.one;
        fillImg.rectTransform.sizeDelta = Vector2.zero;
        slider.fillRect = fillImg.rectTransform;

        // Handle (optional tiny square)
        GameObject handleGO = new GameObject("Handle");
        handleGO.transform.SetParent(sliderGO.transform, false);
        var handleImg = handleGO.AddComponent<Image>();
        handleImg.color = Color.white;
        var handleRect = handleGO.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20f, 20f);
        slider.handleRect = handleRect;

        // Attach StaminaUI script to the slider (or its parent)
        var staminaUI = sliderGO.AddComponent<StaminaUI>();
        staminaUI.vehicle = vc; // assign the vehicle controller

        // --- HUD (Vehicle Stats) ---
        GameObject hudGO = new GameObject("VehicleHUD");
        hudGO.transform.SetParent(canvasGO.transform, false);
        var hud = hudGO.AddComponent<VehicleHUD>();

        // Helper to create a UI Text element
        Text CreateUIText(string name, Vector2 anchoredPosition, int fontSize = 24, Color? color = null)
        {
            GameObject textGO = new GameObject(name);
            textGO.transform.SetParent(hudGO.transform, false);
            var text = textGO.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleLeft;
            text.color = color ?? Color.white;
            var rectTransform = text.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1); // top-left
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = new Vector2(300, 30); // width, height
            return text;
        }

        // Create six text elements for the HUD
        float startY = 10f; // starting Y offset from top
        float lineHeight = 30f;
        hud.speedText = CreateUIText("SpeedText", new Vector2(10f, -startY));
        hud.rpmText = CreateUIText("RPMText", new Vector2(10f, -startY - lineHeight));
        hud.gearText = CreateUIText("GearText", new Vector2(10f, -startY - 2 * lineHeight));
        hud.staminaText = CreateUIText("StaminaText", new Vector2(10f, -startY - 3 * lineHeight));
        hud.softCurrencyText = CreateUIText("SoftCurrencyText", new Vector2(10f, -startY - 4 * lineHeight));
        hud.hardCurrencyText = CreateUIText("HardCurrencyText", new Vector2(10f, -startY - 5 * lineHeight));

        // --- Camera Follow ---
        var follow = cameraGO.AddComponent<CameraFollow>();
        follow.target = vehicleGO.transform;
        // offset and smoothTime already have defaults in the script (0,3,-7 and 0.3)

        // --- Step 3: Focus and Select ---
        // Focus the scene view on the vehicle
        SceneView.lastActiveSceneView.LookAt(vehicleGO.transform.position, Quaternion.identity, 10f, false);
        // Select the vehicle root for convenience
        Selection.activeGameObject = vehicleGO;

        // --- Step 4: Save the scene to the specified path ---
        // Ensure the directory exists
        string sceneDir = Path.GetDirectoryName(SCENE_PATH);
        if (!Directory.Exists(sceneDir))
        {
            Directory.CreateDirectory(sceneDir);
        }

        // Save the active scene to the M1 prototype path
        EditorSceneManager.SaveScene(scene, SCENE_PATH);

        EditorUtility.DisplayDialog(
            "M1 Scene Setup Complete",
            "The active scene has been set up as the M1 prototype and saved to:\n" + SCENE_PATH +
            "\n\nYou can now press Play to see the vehicle drive and the stamina bar drain.\n" +
            "Adjust values in the Inspector if needed.",
            "OK"
        );
    }
}
#endif