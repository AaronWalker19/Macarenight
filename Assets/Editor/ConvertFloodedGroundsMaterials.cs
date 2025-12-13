using UnityEngine;
using UnityEditor;
using System.IO;

public class ConvertFloodedGroundsMaterials : EditorWindow
{
    [MenuItem("Tools/Convert Flooded Grounds Materials to URP")]
    static void ConvertMaterials()
    {
        string[] materialGuids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/Flooded_Grounds" });
        int converted = 0;
        
        foreach (string guid in materialGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            
            if (mat != null && mat.shader != null)
            {
                string shaderName = mat.shader.name;
                
                // Convertir les shaders Standard, Flooded_Grounds custom, ou shaders cassés
                if (shaderName.Contains("Standard") || 
                    shaderName.Contains("Flooded_Grounds") || 
                    shaderName.Contains("FG_") || 
                    shaderName.Contains("Hidden") ||
                    shaderName.Contains("Legacy"))
                {
                    // Sauvegarder les propriétés existantes
                    Texture mainTex = mat.mainTexture;
                    Texture normalMap = mat.HasProperty("_BumpMap") ? mat.GetTexture("_BumpMap") : null;
                    Texture metallicMap = mat.HasProperty("_MetallicGlossMap") ? mat.GetTexture("_MetallicGlossMap") : null;
                    Texture occlusionMap = mat.HasProperty("_OcclusionMap") ? mat.GetTexture("_OcclusionMap") : null;
                    Color color = mat.HasProperty("_Color") ? mat.color : Color.white;
                    float smoothness = mat.HasProperty("_Glossiness") ? mat.GetFloat("_Glossiness") : 0.5f;
                    float metallic = mat.HasProperty("_Metallic") ? mat.GetFloat("_Metallic") : 0f;
                    
                    // Remplacer par le shader URP/Lit
                    Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
                    if (urpShader != null)
                    {
                        mat.shader = urpShader;
                        
                        // Restaurer la couleur de base
                        mat.SetColor("_BaseColor", color);
                        
                        // Restaurer la texture principale
                        if (mainTex != null)
                        {
                            mat.SetTexture("_BaseMap", mainTex);
                        }
                        
                        // Restaurer la normal map
                        if (normalMap != null)
                        {
                            mat.SetTexture("_BumpMap", normalMap);
                            mat.EnableKeyword("_NORMALMAP");
                        }
                        
                        // Restaurer metallic et smoothness
                        mat.SetFloat("_Metallic", metallic);
                        mat.SetFloat("_Smoothness", smoothness);
                        
                        if (metallicMap != null)
                        {
                            mat.SetTexture("_MetallicGlossMap", metallicMap);
                        }
                        
                        // Restaurer occlusion map
                        if (occlusionMap != null)
                        {
                            mat.SetTexture("_OcclusionMap", occlusionMap);
                        }
                        
                        // Désactiver l'émission par défaut
                        mat.DisableKeyword("_EMISSION");
                        
                        EditorUtility.SetDirty(mat);
                        converted++;
                        Debug.Log($"Converti: {path} (shader: {shaderName})");
                    }
                }
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Conversion terminée ! {converted} matériaux convertis vers URP/Lit");
    }
}
