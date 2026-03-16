using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/*
 *  ============================================
  __                 .__     .__.__  .__
_/  |_  ____   _____ |__|    |__|  | |  |   ____   ______
\   __\/  _ \ /     \|  |    |  |  | |  | _/ __ \ /  ___/
 |  | (  <_> )  Y Y  \  |    |  |  |_|  |_\  ___/ \___ \
 |__|  \____/|__|_|  /__|    |__|____/____/\___  >____  >
                   \/                          \/     \/
 *                   
 *  Unity Icon Generator
 *  Created by Tomi Illes
 *  https://tamas-illes.com
 * 
 *  ============================================
 *
 *  Simple tool for capturing clean PNG icons
 *  from a dedicated camera in Unity.
 */

public class IconGenerator : MonoBehaviour
{
    // =========================================================
    // REFERENCES
    // =========================================================
    [Header("References")]
    [SerializeField] private Camera iconCamera;

    // =========================================================
    // OUTPUT SETTINGS
    // =========================================================
    [Header("Output")]
    [SerializeField] private string folderPath = "Assets/Project/Icons";
    [SerializeField] private string fileName = "icon";
    [SerializeField] private int resolution = 512;

    // =========================================================
    // OPTIONS
    // =========================================================
    [Header("Options")]
    [SerializeField] private bool useGameObjectNameIfEmpty = true;

    /// <summary>
    /// Captures the current camera view and saves it as a PNG icon.
    /// Can be triggered from the component context menu in the Inspector.
    /// </summary>
    [ContextMenu("Capture Icon")]
    public void CaptureIcon()
    {
        // Make sure a camera is assigned
        if (iconCamera == null)
        {
            Debug.LogError("IconGenerator: No camera assigned.", this);
            return;
        }

        // Decide what filename should be used
        string finalFileName = fileName;

        if (string.IsNullOrWhiteSpace(finalFileName) && useGameObjectNameIfEmpty)
        {
            finalFileName = gameObject.name;
        }

        if (string.IsNullOrWhiteSpace(finalFileName))
        {
            Debug.LogError("IconGenerator: No file name set.", this);
            return;
        }

        // Create target folder if it does not exist yet
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string fullPath = Path.Combine(folderPath, finalFileName + ".png");

        // Create temporary render target and texture
        RenderTexture renderTexture = new RenderTexture(resolution, resolution, 24, RenderTextureFormat.ARGB32);
        Texture2D outputTexture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);

        // Backup previous render states
        RenderTexture previousActiveRT = RenderTexture.active;
        RenderTexture previousCameraTarget = iconCamera.targetTexture;

        try
        {
            // Render camera into the temporary RenderTexture
            iconCamera.targetTexture = renderTexture;
            RenderTexture.active = renderTexture;

            iconCamera.Render();

            // Copy rendered image into a Texture2D
            outputTexture.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
            outputTexture.Apply();

            // Encode to PNG and save to disk
            byte[] pngBytes = outputTexture.EncodeToPNG();
            File.WriteAllBytes(fullPath, pngBytes);

            Debug.Log($"Icon saved to: {fullPath}", this);

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }
        finally
        {
            // Restore previous render states
            iconCamera.targetTexture = previousCameraTarget;
            RenderTexture.active = previousActiveRT;

            // Clean up temporary textures safely in edit mode and play mode
            if (Application.isPlaying)
            {
                Destroy(renderTexture);
                Destroy(outputTexture);
            }
            else
            {
                DestroyImmediate(renderTexture);
                DestroyImmediate(outputTexture);
            }
        }
    }
}