#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace HierarchyDesigner
{
    internal static class HD_Menu
    {
        #region Windows
        [MenuItem(HD_Constants.AssetLocation, false, HD_Constants.MenuPriorityOne)]
        private static void OpenWindow() => HD_Main.OpenWindow();
        #endregion

        #region Folder
        [MenuItem(HD_Constants.GroupFolders + "/Create All Folders", false, HD_Constants.MenuPriorityOne)]
        public static void CreateAllFolders()
        {
            foreach (KeyValuePair<string, HD_Folders.HD_FolderData> folder in HD_Folders.GetAllFoldersData(false))
            {
                HD_Operations.CreateFolder(folder.Key, false);
            }
        }

        [MenuItem(HD_Constants.GroupFolders + "/Create Default Folder", false, HD_Constants.MenuPriorityOne)]
        public static void CreateDefaultFolder()
        {
            HD_Operations.CreateFolder("New Folder", true);
        }

        [MenuItem(HD_Constants.GroupFolders + "/Create Missing Folders", false, HD_Constants.MenuPriorityOne)]
        public static void CreateMissingFolders()
        {
            foreach (KeyValuePair<string, HD_Folders.HD_FolderData> folder in HD_Folders.GetAllFoldersData(false))
            {
                if (!HD_Operations.FolderExists(folder.Key))
                {
                    HD_Operations.CreateFolder(folder.Key, false);
                }
            }
        }

        [MenuItem(HD_Constants.GroupFolders + "/Transform GameObject into a Folder", false, HD_Constants.MenuPriorityTwo)]
        public static void TransformGameObjectIntoAFolder()
        {
            GameObject selectedObject = Selection.activeGameObject;
            if (selectedObject == null)
            {
                Debug.LogWarning("No GameObject selected.");
                return;
            }

            string folderName = selectedObject.name;
            HD_Folders.HD_FolderData folderData = HD_Folders.GetFolderData(folderName);
            if (folderData == null)
            {
                HD_Folders.SetFolderData(
                    folderName,
                    HD_Settings.FolderDefaultTextColor,
                    HD_Settings.FolderDefaultFontSize,
                    HD_Settings.FolderDefaultFontStyle,
                    HD_Settings.FolderDefaultImageColor,
                    HD_Settings.FolderDefaultImageType
                );
                if (selectedObject.GetComponent<HierarchyDesignerFolder>() == null)
                {
                    selectedObject.AddComponent<HierarchyDesignerFolder>();
                }
                EditorGUIUtility.SetIconForObject(selectedObject, HD_Resources.Textures.FolderScene);
                Debug.Log($"GameObject <color=#73FF7A>'{folderName}'</color> was transformed into a Folder and added to the Folders dictionary.");
            }
            else
            {
                Debug.LogWarning($"GameObject <color=#FF7674>'{folderName}'</color> already exists in the Folders dictionary.");
                return;
            }
        }

        [MenuItem(HD_Constants.GroupFolders + "/Transform Folder into a GameObject", false, HD_Constants.MenuPriorityTwo + 1)]
        public static void TransformFolderIntoAGameObject()
        {
            GameObject selectedObject = Selection.activeGameObject;
            if (selectedObject == null)
            {
                Debug.LogWarning("No GameObject selected.");
                return;
            }

            HierarchyDesignerFolder folderComponent = selectedObject.GetComponent<HierarchyDesignerFolder>();
            if (folderComponent == null)
            {
                Debug.LogWarning($"GameObject <color=#FF7674>'{selectedObject.name}'</color> is not a Folder.");
                return;
            }

            string folderName = selectedObject.name;
            if (HD_Folders.RemoveFolderData(folderName))
            {
                Object.DestroyImmediate(folderComponent);
                EditorGUIUtility.SetIconForObject(selectedObject, null);
                Debug.Log($"GameObject <color=#73FF7A>'{folderName}'</color> was transformed back into a GameObject and removed from the Folders dictionary.");
            }
            else
            {
                Debug.LogWarning($"Folder data for GameObject <color=#FF7674>'{folderName}'</color> does not exist in the Folders dictionary.");
            }
        }
        #endregion

        #region Separator
        [MenuItem(HD_Constants.GroupSeparators + "/Create All Separators", false, HD_Constants.MenuPriorityOne)]
        public static void CreateAllSeparators()
        {
            foreach (KeyValuePair<string, HD_Separators.HD_SeparatorData> separator in HD_Separators.GetAllSeparatorsData(false))
            {
                HD_Operations.CreateSeparator(separator.Key);
            }
        }

        [MenuItem(HD_Constants.GroupSeparators + "/Create Default Separator", false, HD_Constants.MenuPriorityOne)]
        public static void CreateDefaultSeparator()
        {
            HD_Operations.CreateSeparator("Separator");
        }

        [MenuItem(HD_Constants.GroupSeparators + "/Create Missing Separators", false, HD_Constants.MenuPriorityOne)]
        public static void CreateMissingSeparators()
        {
            foreach (KeyValuePair<string, HD_Separators.HD_SeparatorData> separator in HD_Separators.GetAllSeparatorsData(false))
            {
                if (!HD_Operations.SeparatorExists(separator.Key))
                {
                    HD_Operations.CreateSeparator(separator.Key);
                }
            }
        }

        [MenuItem(HD_Constants.GroupSeparators + "/Transform GameObject into a Separator", false, HD_Constants.MenuPriorityTwo)]
        public static void TransformGameObjectIntoASeparator()
        {
            GameObject selectedObject = Selection.activeGameObject;
            if (selectedObject == null)
            {
                Debug.LogWarning("No GameObject selected.");
                return;
            }
            if (selectedObject.GetComponents<Component>().Length > 1)
            {
                Debug.LogWarning("Separators cannot have components because separators are marked as editorOnly, meaning they will not be present in your game's build.");
                return;
            }

            string separatorName = HD_Operations.StripPrefix(selectedObject.name);
            HD_Separators.HD_SeparatorData separatorData = HD_Separators.GetSeparatorData(separatorName);
            if (separatorData == null)
            {
                HD_Separators.SetSeparatorData(
                    separatorName,
                    HD_Settings.SeparatorDefaultTextColor,
                    HD_Settings.SeparatorDefaultIsGradientBackground,
                    HD_Settings.SeparatorDefaultBackgroundColor,
                    HD_Settings.SeparatorDefaultBackgroundGradient,
                    HD_Settings.SeparatorDefaultFontSize,
                    HD_Settings.SeparatorDefaultFontStyle,
                    HD_Settings.SeparatorDefaultTextAnchor,
                    HD_Settings.SeparatorDefaultImageType
                );
                if (!selectedObject.name.StartsWith(HD_Constants.SeparatorPrefix))
                {
                    selectedObject.name = $"{HD_Constants.SeparatorPrefix}{selectedObject.name}";
                }
                selectedObject.tag = HD_Constants.SeparatorTag;
                selectedObject.SetActive(false);
                EditorGUIUtility.SetIconForObject(selectedObject, HD_Resources.Textures.SeparatorInspectorIcon);
                Debug.Log($"GameObject <color=#73FF7A>'{separatorName}'</color> was transformed into a Separator and added to the Separators dictionary.");
            }
            else
            {
                Debug.LogWarning($"GameObject <color=#FF7674>'{separatorName}'</color> already exists in the Separators dictionary.");
                return;
            }
            HD_Operations.SetSeparatorState(selectedObject, false);
        }

        [MenuItem(HD_Constants.GroupSeparators + "/Transform Separator into a GameObject", false, HD_Constants.MenuPriorityTwo + 1)]
        public static void TransformSeparatorIntoAGameObject()
        {
            GameObject selectedObject = Selection.activeGameObject;
            if (selectedObject == null)
            {
                Debug.LogWarning("No GameObject selected.");
                return;
            }

            if (!selectedObject.name.StartsWith(HD_Constants.SeparatorPrefix) && !selectedObject.CompareTag(HD_Constants.SeparatorTag))
            {
                Debug.LogWarning($"GameObject <color=#FF7674>'{selectedObject.name}'</color> is not a Separator.");
                return;
            }

            string separatorName = HD_Operations.StripPrefix(selectedObject.name);
            if (HD_Separators.RemoveSeparatorData(separatorName))
            {
                selectedObject.name = separatorName;
                selectedObject.tag = "Untagged";
                selectedObject.SetActive(true);
                HD_Operations.SetLockState(selectedObject, true);
                EditorGUIUtility.SetIconForObject(selectedObject, null);
                Debug.Log($"GameObject <color=#73FF7A>'{separatorName}'</color> was transformed back into a GameObject and removed from the Separators dictionary.");
            }
            else
            {
                Debug.LogWarning($"Separator data for GameObject <color=#FF7674>'{separatorName}'</color> does not exist in the Separators dictionary.");
            }
        }
        #endregion

        #region Tools
        #region Activate
        #region General
        [MenuItem(HD_Constants.Activate_General + "/Activate Selected GameObjects", false, HD_Constants.MenuPriorityEight)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_SelectedGameObjects() => HD_Operations.Activate_SelectedGameObjects(true);

        [MenuItem(HD_Constants.Activate_General + "/Activate All GameObjects", false, HD_Constants.MenuPriorityNine)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_AllGameObjects() => HD_Operations.Activate_AllGameObjects(true);

        [MenuItem(HD_Constants.Activate_General + "/Activate All Parent GameObjects", false, HD_Constants.MenuPriorityNine)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_ParentGameObjects() => HD_Operations.Activate_AllParentGameObjects(true);

        [MenuItem(HD_Constants.Activate_General + "/Activate All Empty GameObjects", false, HD_Constants.MenuPriorityTen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_EmptyGameObjects() => HD_Operations.Activate_AllEmptyGameObjects(true);

        [MenuItem(HD_Constants.Activate_General + "/Activate All Locked GameObjects", false, HD_Constants.MenuPriorityTen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_LockedGameObjects() => HD_Operations.Activate_AllLockedGameObjects(true);

        [MenuItem(HD_Constants.Activate_General + "/Activate All Folders", false, HD_Constants.MenuPriorityEleven)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Folders() => HD_Operations.Activate_AllFolders(true);
        #endregion

        #region 2D Objects
        [MenuItem(HD_Constants.Activate_2D + "/Activate All Sprites", false, HD_Constants.MenuPriorityNine)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Sprites() => HD_Operations.Activate_AllComponentOfType<SpriteRenderer>(true);

        [MenuItem(HD_Constants.Activate_2D + "/Activate All Sprite Masks", false, HD_Constants.MenuPriorityNine + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_SpriteMasks() => HD_Operations.Activate_AllComponentOfType<SpriteMask>(true);

        [MenuItem(HD_Constants.Activate_2D_Sprites + "/Activate All 9-Sliced Sprites", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_9SlicedSprites() => HD_Operations.Activate_All2DSpritesByType("9-Sliced", true);

        [MenuItem(HD_Constants.Activate_2D_Sprites + "/Activate All Capsule Sprites", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_CapsuleSprites() => HD_Operations.Activate_All2DSpritesByType("Capsule", true);

        [MenuItem(HD_Constants.Activate_2D_Sprites + "/Activate All Circle Sprites", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_CircleSprites() => HD_Operations.Activate_All2DSpritesByType("Circle", true);

        [MenuItem(HD_Constants.Activate_2D_Sprites + "/Activate All Hexagon Flat-Top Sprites", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_HexagonFlatTopSprites() => HD_Operations.Activate_All2DSpritesByType("Hexagon Flat-Top", true);

        [MenuItem(HD_Constants.Activate_2D_Sprites + "/Activate All Hexagon Pointed-Top Sprites", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_HexagonPointedTopSprites() => HD_Operations.Activate_All2DSpritesByType("Hexagon Pointed-Top", true);

        [MenuItem(HD_Constants.Activate_2D_Sprites + "/Activate All Isometric Diamond Sprites", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_IsometricDiamondSprites() => HD_Operations.Activate_All2DSpritesByType("Isometric Diamond", true);

        [MenuItem(HD_Constants.Activate_2D_Sprites + "/Activate All Square Sprites", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_SquareSprites() => HD_Operations.Activate_All2DSpritesByType("Square", true);

        [MenuItem(HD_Constants.Activate_2D_Sprites + "/Activate All Triangle Sprites", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_TriangleSprites() => HD_Operations.Activate_All2DSpritesByType("Triangle", true);

        [MenuItem(HD_Constants.Activate_2D_Physics + "/Activate All Dynamic Sprites", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_PhysicsDynamicSprites() => HD_Operations.Activate_AllPhysicsDynamicSprites(true);

        [MenuItem(HD_Constants.Activate_2D_Physics + "/Activate All Static Sprites", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_PhysicsStaticSprites() => HD_Operations.Activate_AllPhysicsStaticSprites(true);
        #endregion

        #region 3D Objects
        [MenuItem(HD_Constants.Activate_3D + "/Activate All Mesh Filters", false, HD_Constants.MenuPriorityNine)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_MeshFilters() => HD_Operations.Activate_AllComponentOfType<MeshFilter>(true);

        [MenuItem(HD_Constants.Activate_3D + "/Activate All Mesh Renderers", false, HD_Constants.MenuPriorityNine)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_MeshRenderers() => HD_Operations.Activate_AllComponentOfType<MeshRenderer>(true);

        [MenuItem(HD_Constants.Activate_3D + "/Activate All Skinned Mesh Renderer", false, HD_Constants.MenuPriorityNine)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_SkinnedMeshRenderers() => HD_Operations.Activate_AllComponentOfType<SkinnedMeshRenderer>(true);

        [MenuItem(HD_Constants.Activate_3D + "/Activate All Cubes", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_CubesObjects() => HD_Operations.Activate_All3DObjectsByType("Cube", true);

        [MenuItem(HD_Constants.Activate_3D + "/Activate All Spheres", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_SpheresObjects() => HD_Operations.Activate_All3DObjectsByType("Sphere", true);

        [MenuItem(HD_Constants.Activate_3D + "/Activate All Capsules", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_CapsulesObjects() => HD_Operations.Activate_All3DObjectsByType("Capsule", true);

        [MenuItem(HD_Constants.Activate_3D + "/Activate All Cylinders", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_CylindersObjects() => HD_Operations.Activate_All3DObjectsByType("Cylinder", true);

        [MenuItem(HD_Constants.Activate_3D + "/Activate All Planes", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_PlanesObjects() => HD_Operations.Activate_All3DObjectsByType("Plane", true);

        [MenuItem(HD_Constants.Activate_3D + "/Activate All Quads", false, HD_Constants.MenuPriorityTen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_QuadsObjects() => HD_Operations.Activate_All3DObjectsByType("Quad", true);

        [MenuItem(HD_Constants.Activate_3D + "/Activate All Texts - TextMeshPro", false, HD_Constants.MenuPriorityEleven + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_TextMeshProObjects() => HD_Operations.Activate_All3DObjectsByType("TextMeshPro Mesh", true);

        [MenuItem(HD_Constants.Activate_3D_Legacy + "/Activate All Text Meshes", false, HD_Constants.MenuPriorityEleven + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_TextMeshesObjects() => HD_Operations.Activate_AllComponentOfType<TextMesh>(true);

        [MenuItem(HD_Constants.Activate_3D + "/Activate All Terrains", false, HD_Constants.MenuPriorityTwelve + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_TerrainsObjects() => HD_Operations.Activate_AllComponentOfType<Terrain>(true);

        [MenuItem(HD_Constants.Activate_3D + "/Activate All Trees", false, HD_Constants.MenuPriorityTwelve + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_TreesObjects() => HD_Operations.Activate_AllComponentOfType<Tree>(true);

        [MenuItem(HD_Constants.Activate_3D + "/Activate All Wind Zones", false, HD_Constants.MenuPriorityTwelve + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_WindZonesObjects() => HD_Operations.Activate_AllComponentOfType<WindZone>(true);
        #endregion

        #region Audio
        [MenuItem(HD_Constants.Activate_Audio + "/Activate All Audio Sources", false, HD_Constants.MenuPriorityNine + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_AudioSources() => HD_Operations.Activate_AllComponentOfType<AudioSource>(true);

        [MenuItem(HD_Constants.Activate_Audio + "/Activate All Audio Reverb Zones", false, HD_Constants.MenuPriorityNine + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_AudioReverbZones() => HD_Operations.Activate_AllComponentOfType<AudioReverbZone>(true);

        [MenuItem(HD_Constants.Activate_Audio + "/Activate All Audio Chorus Filters", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_AudioChorusFilters() => HD_Operations.Activate_AllComponentOfType<AudioChorusFilter>(true);

        [MenuItem(HD_Constants.Activate_Audio + "/Activate All Audio Distortion Filters", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_AudioDistortionFilters() => HD_Operations.Activate_AllComponentOfType<AudioDistortionFilter>(true);

        [MenuItem(HD_Constants.Activate_Audio + "/Activate All Audio Echo Filters", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_AudioEchoFilters() => HD_Operations.Activate_AllComponentOfType<AudioEchoFilter>(true);

        [MenuItem(HD_Constants.Activate_Audio + "/Activate All Audio High Pass Filters", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_AudioHighPassFilters() => HD_Operations.Activate_AllComponentOfType<AudioHighPassFilter>(true);

        [MenuItem(HD_Constants.Activate_Audio + "/Activate All Audio Listeners", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_AudioListeners() => HD_Operations.Activate_AllComponentOfType<AudioListener>(true);

        [MenuItem(HD_Constants.Activate_Audio + "/Activate All Audio Low Pass Filters", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_AudioLowPassFilters() => HD_Operations.Activate_AllComponentOfType<AudioLowPassFilter>(true);

        [MenuItem(HD_Constants.Activate_Audio + "/Activate All Audio Reverb Filters", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_AudioReverbFilters() => HD_Operations.Activate_AllComponentOfType<AudioReverbFilter>(true);
        #endregion

        #region Effects
        [MenuItem(HD_Constants.Activate_Effects + "/Activate All Particle Systems", false, HD_Constants.MenuPriorityNine + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_ParticleSystems() => HD_Operations.Activate_AllComponentOfType<ParticleSystem>(true);

        [MenuItem(HD_Constants.Activate_Effects + "/Activate All Particle System Force Fields", false, HD_Constants.MenuPriorityNine + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_ParticleSystemForceFields() => HD_Operations.Activate_AllComponentOfType<ParticleSystemForceField>(true);

        [MenuItem(HD_Constants.Activate_Effects + "/Activate All Trail Renderers", false, HD_Constants.MenuPriorityNine + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_TrailRenderers() => HD_Operations.Activate_AllComponentOfType<TrailRenderer>(true);

        [MenuItem(HD_Constants.Activate_Effects + "/Activate All Line Renderers", false, HD_Constants.MenuPriorityNine + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_LineRenderers() => HD_Operations.Activate_AllComponentOfType<LineRenderer>(true);

        [MenuItem(HD_Constants.Activate_Effects + "/Activate All Halos", false, HD_Constants.MenuPriorityTen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Halos() => HD_Operations.Activate_AllHalos(true);

#if UNITY_6000_5_OR_NEWER
#else
        [MenuItem(HD_Constants.Activate_Effects + "/Activate All Lens Flares", false, HD_Constants.MenuPriorityTen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_LensFlares() => HD_Operations.Activate_AllComponentOfType<LensFlare>(true);

        [MenuItem(HD_Constants.Activate_Effects + "/Activate All Projectors", false, HD_Constants.MenuPriorityTen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Projectors() => HD_Operations.Activate_AllComponentOfType<Projector>(true);
#endif

        [MenuItem(HD_Constants.Activate_Effects + "/Activate All Visual Effects", false, HD_Constants.MenuPriorityTen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_VisualEffects() => HD_Operations.Activate_AllComponentOfType<UnityEngine.VFX.VisualEffect>(true);
        #endregion

        #region Lights
        [MenuItem(HD_Constants.Activate_Light + "/Activate All Lights", false, HD_Constants.MenuPriorityNine + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Lights() => HD_Operations.Activate_AllComponentOfType<Light>(true);

        [MenuItem(HD_Constants.Activate_Light + "/Activate All Directional Lights", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_DirectionalLights() => HD_Operations.Activate_AllComponentOfType<Light>(true, light => light.type == LightType.Directional);

        [MenuItem(HD_Constants.Activate_Light + "/Activate All Point Lights", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_PointLights() => HD_Operations.Activate_AllComponentOfType<Light>(true, light => light.type == LightType.Point);

        [MenuItem(HD_Constants.Activate_Light + "/Activate All Spot Lights", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_SpotLights() => HD_Operations.Activate_AllComponentOfType<Light>(true, light => light.type == LightType.Spot);

        [MenuItem(HD_Constants.Activate_Light + "/Activate All Rectangle Area Lights", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_RectangleAreaLights() => HD_Operations.Activate_AllComponentOfType<Light>(true, light => light.type == LightType.Rectangle);

        [MenuItem(HD_Constants.Activate_Light + "/Activate All Disc Area Lights", false, HD_Constants.MenuPriorityTen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_DiscAreaLights() => HD_Operations.Activate_AllComponentOfType<Light>(true, light => light.type == LightType.Disc);

        [MenuItem(HD_Constants.Activate_Light + "/Activate All Reflection Probes", false, HD_Constants.MenuPriorityEleven + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_ReflectionProbes() => HD_Operations.Activate_AllComponentOfType<ReflectionProbe>(true);

        [MenuItem(HD_Constants.Activate_Light + "/Activate All Light Probe Groups", false, HD_Constants.MenuPriorityEleven + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_LightProbeGroups() => HD_Operations.Activate_AllComponentOfType<LightProbeGroup>(true);

#if UNITY_6000_5_OR_NEWER
#else
        [MenuItem(HD_Constants.Activate_Light + "/Activate All Light Probe Proxy Volumes", false, HD_Constants.MenuPriorityEleven + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_LightProbeProxyVolumes() => HD_Operations.Activate_AllComponentOfType<LightProbeProxyVolume>(true);
#endif
        #endregion

        #region Video
        [MenuItem(HD_Constants.Activate_Video + "/Activate All Video Players", false, HD_Constants.MenuPriorityNine + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_VideoPlayers() => HD_Operations.Activate_AllComponentOfType<UnityEngine.Video.VideoPlayer>(true);
        #endregion

        #region UI Toolkit
        [MenuItem(HD_Constants.Activate_UIToolkit + "/Activate All UI Documents", false, HD_Constants.MenuPriorityNine + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_UIDocuments() => HD_Operations.Activate_AllComponentOfType<UnityEngine.UIElements.UIDocument>(true);

        [MenuItem(HD_Constants.Activate_UIToolkit + "/Activate All Panel Event Handlers", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_PanelEventHandlers() => HD_Operations.Activate_AllComponentOfType<UnityEngine.UIElements.PanelEventHandler>(true);

        [MenuItem(HD_Constants.Activate_UIToolkit + "/Activate All Panel Raycasters", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_PanelRaycasters() => HD_Operations.Activate_AllComponentOfType<UnityEngine.UIElements.PanelRaycaster>(true);
        #endregion

        #region Cameras
        [MenuItem(HD_Constants.Tools_Activate + "/Activate All Cameras", false, HD_Constants.MenuPriorityNine + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Cameras() => HD_Operations.Activate_AllComponentOfType<Camera>(true);
        #endregion

        #region UI
        [MenuItem(HD_Constants.Activate_UI + "/Activate All Images", false, HD_Constants.MenuPriorityNine + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Images() => HD_Operations.Activate_AllComponentOfType<Image>(true);

        [MenuItem(HD_Constants.Activate_UI + "/Activate All Texts - TextMeshPro", false, HD_Constants.MenuPriorityNine + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_TextMeshPro() => HD_Operations.Activate_AllTMPComponentIfAvailable<TMPro.TMP_Text>(true);

        [MenuItem(HD_Constants.Activate_UI + "/Activate All Raw Images", false, HD_Constants.MenuPriorityNine + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_RawImages() => HD_Operations.Activate_AllComponentOfType<RawImage>(true);

        [MenuItem(HD_Constants.Activate_UI + "/Activate All Toggles", false, HD_Constants.MenuPriorityTen + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Toggles() => HD_Operations.Activate_AllComponentOfType<Toggle>(true);

        [MenuItem(HD_Constants.Activate_UI + "/Activate All Sliders", false, HD_Constants.MenuPriorityTen + 5)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Sliders() => HD_Operations.Activate_AllComponentOfType<Slider>(true);

        [MenuItem(HD_Constants.Activate_UI + "/Activate All Scrollbars", false, HD_Constants.MenuPriorityTen + 6)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Scrollbars() => HD_Operations.Activate_AllComponentOfType<Scrollbar>(true);

        [MenuItem(HD_Constants.Activate_UI + "/Activate All Scroll Views", false, HD_Constants.MenuPriorityTen + 7)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_ScrollViews() => HD_Operations.Activate_AllComponentOfType<ScrollRect>(true);

        [MenuItem(HD_Constants.Activate_UI + "/Activate All Dropdowns - TextMeshPro", false, HD_Constants.MenuPriorityTen + 8)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_DropdownTextMeshPro() => HD_Operations.Activate_AllTMPComponentIfAvailable<TMPro.TMP_Dropdown>(true);

        [MenuItem(HD_Constants.Activate_UI + "/Activate All Input Fields - TextMeshPro", false, HD_Constants.MenuPriorityTen + 8)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_InputFieldTextMeshPro() => HD_Operations.Activate_AllTMPComponentIfAvailable<TMPro.TMP_InputField>(true);

        [MenuItem(HD_Constants.Activate_UI + "/Activate All Canvases", false, HD_Constants.MenuPriorityEleven + 8)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Canvases() => HD_Operations.Activate_AllComponentOfType<Canvas>(true);

        [MenuItem(HD_Constants.Activate_UI + "/Activate All Event Systems", false, HD_Constants.MenuPriorityEleven + 8)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_EventSystems() => HD_Operations.Activate_AllComponentOfType<UnityEngine.EventSystems.EventSystem>(true);

        [MenuItem(HD_Constants.Activate_UI_Legacy + "/Activate All Texts", false, HD_Constants.MenuPriorityTwelve + 8)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Texts() => HD_Operations.Activate_AllComponentOfType<Text>(true);

        [MenuItem(HD_Constants.Activate_UI_Legacy + "/Activate All Buttons", false, HD_Constants.MenuPriorityTwelve + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Buttons() => HD_Operations.Activate_AllComponentOfType<Button>(true);

        [MenuItem(HD_Constants.Activate_UI_Legacy + "/Activate All Dropdowns", false, HD_Constants.MenuPriorityTwelve + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Dropdowns() => HD_Operations.Activate_AllComponentOfType<Dropdown>(true);

        [MenuItem(HD_Constants.Activate_UI_Legacy + "/Activate All Input Fields", false, HD_Constants.MenuPriorityTwelve + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_InputFields() => HD_Operations.Activate_AllComponentOfType<InputField>(true);

        [MenuItem(HD_Constants.Activate_UI + "/Activate All Masks", false, HD_Constants.MenuPriorityThirteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Masks() => HD_Operations.Activate_AllComponentOfType<Mask>(true);

        [MenuItem(HD_Constants.Activate_UI + "/Activate All Rect Masks 2D", false, HD_Constants.MenuPriorityThirteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_RectMasks2D() => HD_Operations.Activate_AllComponentOfType<RectMask2D>(true);

        [MenuItem(HD_Constants.Activate_UI + "/Activate All Selectables", false, HD_Constants.MenuPriorityThirteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Selectables() => HD_Operations.Activate_AllComponentOfType<Selectable>(true);

        [MenuItem(HD_Constants.Activate_UI + "/Activate All Toggle Groups", false, HD_Constants.MenuPriorityThirteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_ToggleGroups() => HD_Operations.Activate_AllComponentOfType<ToggleGroup>(true);

        [MenuItem(HD_Constants.Activate_UI_Effects + "/Activate All Outlines", false, HD_Constants.MenuPriorityFourteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Outlines() => HD_Operations.Activate_AllComponentOfType<Outline>(true);

        [MenuItem(HD_Constants.Activate_UI_Effects + "/Activate All Positions As UV1", false, HD_Constants.MenuPriorityFourteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_PositionsAsUV1() => HD_Operations.Activate_AllComponentOfType<PositionAsUV1>(true);

        [MenuItem(HD_Constants.Activate_UI_Effects + "/Activate All Shadows", false, HD_Constants.MenuPriorityFourteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Shadows() => HD_Operations.Activate_AllComponentOfType<Shadow>(true);
        #endregion
        #endregion

        #region Deactivate
        #region General
        [MenuItem(HD_Constants.Deactivate_General + "/Deactivate Selected GameObjects", false, HD_Constants.MenuPriorityEight)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_SelectedGameObjects() => HD_Operations.Activate_SelectedGameObjects(false);

        [MenuItem(HD_Constants.Deactivate_General + "/Deactivate All GameObjects", false, HD_Constants.MenuPriorityNine)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_AllGameObjects() => HD_Operations.Activate_AllGameObjects(false);

        [MenuItem(HD_Constants.Deactivate_General + "/Deactivate All Parent GameObjects", false, HD_Constants.MenuPriorityNine)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_ParentGameObjects() => HD_Operations.Activate_AllParentGameObjects(false);

        [MenuItem(HD_Constants.Deactivate_General + "/Deactivate All Empty GameObjects", false, HD_Constants.MenuPriorityTen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_EmptyGameObjects() => HD_Operations.Activate_AllEmptyGameObjects(false);

        [MenuItem(HD_Constants.Deactivate_General + "/Deactivate All Locked GameObjects", false, HD_Constants.MenuPriorityTen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_LockedGameObjects() => HD_Operations.Activate_AllLockedGameObjects(false);

        [MenuItem(HD_Constants.Deactivate_General + "/Deactivate All Folders", false, HD_Constants.MenuPriorityEleven)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Folders() => HD_Operations.Activate_AllFolders(false);
        #endregion

        #region 2D Objects
        [MenuItem(HD_Constants.Deactivate_2D + "/Deactivate All Sprites", false, HD_Constants.MenuPriorityNine)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_AllSprites() => HD_Operations.Activate_AllComponentOfType<SpriteRenderer>(false);

        [MenuItem(HD_Constants.Deactivate_2D + "/Deactivate All Sprite Masks", false, HD_Constants.MenuPriorityNine + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_SpriteMasks() => HD_Operations.Activate_AllComponentOfType<SpriteMask>(false);

        [MenuItem(HD_Constants.Deactivate_2D_Sprites + "/Deactivate All 9-Sliced Sprites", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_9SlicedSprites() => HD_Operations.Activate_All2DSpritesByType("9-Sliced", false);

        [MenuItem(HD_Constants.Deactivate_2D_Sprites + "/Deactivate All Capsule Sprites", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_CapsuleSprites() => HD_Operations.Activate_All2DSpritesByType("Capsule", false);

        [MenuItem(HD_Constants.Deactivate_2D_Sprites + "/Deactivate All Circle Sprites", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_CircleSprites() => HD_Operations.Activate_All2DSpritesByType("Circle", false);

        [MenuItem(HD_Constants.Deactivate_2D_Sprites + "/Deactivate All Hexagon Flat-Top Sprites", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_HexagonFlatTopSprites() => HD_Operations.Activate_All2DSpritesByType("Hexagon Flat-Top", false);

        [MenuItem(HD_Constants.Deactivate_2D_Sprites + "/Deactivate All Hexagon Pointed-Top Sprites", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_HexagonPointedTopSprites() => HD_Operations.Activate_All2DSpritesByType("Hexagon Pointed-Top", false);

        [MenuItem(HD_Constants.Deactivate_2D_Sprites + "/Deactivate All Isometric Diamond Sprites", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_IsometricDiamondSprites() => HD_Operations.Activate_All2DSpritesByType("Isometric Diamond", false);

        [MenuItem(HD_Constants.Deactivate_2D_Sprites + "/Deactivate All Square Sprites", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_SquareSprites() => HD_Operations.Activate_All2DSpritesByType("Square", false);

        [MenuItem(HD_Constants.Deactivate_2D_Sprites + "/Deactivate All Triangle Sprites", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_TriangleSprites() => HD_Operations.Activate_All2DSpritesByType("Triangle", false);

        [MenuItem(HD_Constants.Deactivate_2D_Physics + "/Deactivate All Dynamic Sprites", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_PhysicsDynamicSprites() => HD_Operations.Activate_AllPhysicsDynamicSprites(false);

        [MenuItem(HD_Constants.Deactivate_2D_Physics + "/Deactivate All Static Sprites", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_PhysicsStaticSprites() => HD_Operations.Activate_AllPhysicsStaticSprites(false);
        #endregion

        #region 3D Objects
        [MenuItem(HD_Constants.Deactivate_3D + "/Deactivate All Mesh Filters", false, HD_Constants.MenuPriorityNine)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_MeshFilters() => HD_Operations.Activate_AllComponentOfType<MeshFilter>(false);

        [MenuItem(HD_Constants.Deactivate_3D + "/Deactivate All Mesh Renderers", false, HD_Constants.MenuPriorityNine)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_MeshRenderers() => HD_Operations.Activate_AllComponentOfType<MeshRenderer>(false);

        [MenuItem(HD_Constants.Deactivate_3D + "/Deactivate All Skinned Mesh Renderer", false, HD_Constants.MenuPriorityNine)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_SkinnedMeshRenderers() => HD_Operations.Activate_AllComponentOfType<SkinnedMeshRenderer>(false);

        [MenuItem(HD_Constants.Deactivate_3D + "/Deactivate All Cubes", false, HD_Constants.MenuPriorityTen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_CubesObjects() => HD_Operations.Activate_All3DObjectsByType("Cube", false);

        [MenuItem(HD_Constants.Deactivate_3D + "/Deactivate All Spheres", false, HD_Constants.MenuPriorityTen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_SpheresObjects() => HD_Operations.Activate_All3DObjectsByType("Sphere", false);

        [MenuItem(HD_Constants.Deactivate_3D + "/Deactivate All Capsules", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_CapsulesObjects() => HD_Operations.Activate_All3DObjectsByType("Capsule", false);

        [MenuItem(HD_Constants.Deactivate_3D + "/Deactivate All Cylinders", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_CylindersObjects() => HD_Operations.Activate_All3DObjectsByType("Cylinder", false);

        [MenuItem(HD_Constants.Deactivate_3D + "/Deactivate All Planes", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_PlanesObjects() => HD_Operations.Activate_All3DObjectsByType("Plane", false);

        [MenuItem(HD_Constants.Deactivate_3D + "/Deactivate All Quads", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_QuadsObjects() => HD_Operations.Activate_All3DObjectsByType("Quad", false);

        [MenuItem(HD_Constants.Deactivate_3D + "/Deactivate All Texts - TextMeshPro", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_TextMeshProObjects() => HD_Operations.Activate_All3DObjectsByType("TextMeshPro Mesh", false);

        [MenuItem(HD_Constants.Deactivate_3D_Legacy + "/Deactivate All Text Meshes", false, HD_Constants.MenuPriorityEleven + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_TextMeshesObjects() => HD_Operations.Activate_AllComponentOfType<TextMesh>(false);

        [MenuItem(HD_Constants.Deactivate_3D + "/Deactivate All Terrains", false, HD_Constants.MenuPriorityTwelve + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_TerrainsObjects() => HD_Operations.Activate_AllComponentOfType<Terrain>(false);

        [MenuItem(HD_Constants.Deactivate_3D + "/Deactivate All Trees", false, HD_Constants.MenuPriorityTwelve + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_TreesObjects() => HD_Operations.Activate_AllComponentOfType<Tree>(false);

        [MenuItem(HD_Constants.Deactivate_3D + "/Deactivate All Wind Zones", false, HD_Constants.MenuPriorityTwelve + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_WindZonesObjects() => HD_Operations.Activate_AllComponentOfType<WindZone>(false);
        #endregion

        #region Audio
        [MenuItem(HD_Constants.Deactivate_Audio + "/Deactivate All Audio Sources", false, HD_Constants.MenuPriorityNine + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_AudioSources() => HD_Operations.Activate_AllComponentOfType<AudioSource>(false);

        [MenuItem(HD_Constants.Deactivate_Audio + "/Deactivate All Audio Reverb Zones", false, HD_Constants.MenuPriorityNine + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_AudioReverbZones() => HD_Operations.Activate_AllComponentOfType<AudioReverbZone>(false);

        [MenuItem(HD_Constants.Deactivate_Audio + "/Deactivate All Audio Chorus Filters", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_AudioChorusFilters() => HD_Operations.Activate_AllComponentOfType<AudioChorusFilter>(false);

        [MenuItem(HD_Constants.Deactivate_Audio + "/Deactivate All Audio Distortion Filters", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_AudioDistortionFilters() => HD_Operations.Activate_AllComponentOfType<AudioDistortionFilter>(false);

        [MenuItem(HD_Constants.Deactivate_Audio + "/Deactivate All Audio Echo Filters", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_AudioEchoFilters() => HD_Operations.Activate_AllComponentOfType<AudioEchoFilter>(false);

        [MenuItem(HD_Constants.Deactivate_Audio + "/Deactivate All Audio High Pass Filters", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_AudioHighPassFilters() => HD_Operations.Activate_AllComponentOfType<AudioHighPassFilter>(false);

        [MenuItem(HD_Constants.Deactivate_Audio + "/Deactivate All Audio Listeners", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_AudioListeners() => HD_Operations.Activate_AllComponentOfType<AudioListener>(false);

        [MenuItem(HD_Constants.Deactivate_Audio + "/Deactivate All Audio Low Pass Filters", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_AudioLowPassFilters() => HD_Operations.Activate_AllComponentOfType<AudioLowPassFilter>(false);

        [MenuItem(HD_Constants.Deactivate_Audio + "/Deactivate All Audio Reverb Filters", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_AudioReverbFilters() => HD_Operations.Activate_AllComponentOfType<AudioReverbFilter>(false);
        #endregion

        #region Effects
        [MenuItem(HD_Constants.Deactivate_Effects + "/Deactivate All Particle Systems", false, HD_Constants.MenuPriorityNine + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_ParticleSystems() => HD_Operations.Activate_AllComponentOfType<ParticleSystem>(false);

        [MenuItem(HD_Constants.Deactivate_Effects + "/Deactivate All Particle System Force Fields", false, HD_Constants.MenuPriorityNine + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_ParticleSystemForceFields() => HD_Operations.Activate_AllComponentOfType<ParticleSystemForceField>(false);

        [MenuItem(HD_Constants.Deactivate_Effects + "/Deactivate All Trail Renderers", false, HD_Constants.MenuPriorityNine + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_TrailRenderers() => HD_Operations.Activate_AllComponentOfType<TrailRenderer>(false);

        [MenuItem(HD_Constants.Deactivate_Effects + "/Deactivate All Line Renderers", false, HD_Constants.MenuPriorityNine + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_LineRenderers() => HD_Operations.Activate_AllComponentOfType<LineRenderer>(false);

        [MenuItem(HD_Constants.Deactivate_Effects + "/Deactivate All Halos", false, HD_Constants.MenuPriorityTen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Halos() => HD_Operations.Activate_AllHalos(false);

#if UNITY_6000_5_OR_NEWER
#else
        [MenuItem(HD_Constants.Deactivate_Effects + "/Deactivate All Lens Flares", false, HD_Constants.MenuPriorityTen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_LensFlares() => HD_Operations.Activate_AllComponentOfType<LensFlare>(false);

        [MenuItem(HD_Constants.Deactivate_Effects + "/Deactivate All Projectors", false, HD_Constants.MenuPriorityTen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Projectors() => HD_Operations.Activate_AllComponentOfType<Projector>(false);
#endif

        [MenuItem(HD_Constants.Deactivate_Effects + "/Deactivate All Visual Effects", false, HD_Constants.MenuPriorityTen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_VisualEffects() => HD_Operations.Activate_AllComponentOfType<UnityEngine.VFX.VisualEffect>(false);
        #endregion

        #region Lights
        [MenuItem(HD_Constants.Deactivate_Light + "/Deactivate All Lights", false, HD_Constants.MenuPriorityNine + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Lights() => HD_Operations.Activate_AllComponentOfType<Light>(false);

        [MenuItem(HD_Constants.Deactivate_Light + "/Deactivate All Directional Lights", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_DirectionalLights() => HD_Operations.Activate_AllComponentOfType<Light>(false, light => light.type == LightType.Directional);

        [MenuItem(HD_Constants.Deactivate_Light + "/Deactivate All Point Lights", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_PointLights() => HD_Operations.Activate_AllComponentOfType<Light>(false, light => light.type == LightType.Point);

        [MenuItem(HD_Constants.Deactivate_Light + "/Deactivate All Spot Lights", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_SpotLights() => HD_Operations.Activate_AllComponentOfType<Light>(false, light => light.type == LightType.Spot);

        [MenuItem(HD_Constants.Deactivate_Light + "/Deactivate All Rectangle Area Lights", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_RectangleAreaLights() => HD_Operations.Activate_AllComponentOfType<Light>(false, light => light.type == LightType.Rectangle);

        [MenuItem(HD_Constants.Deactivate_Light + "/Deactivate All Disc Area Lights", false, HD_Constants.MenuPriorityTen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_DiscAreaLights() => HD_Operations.Activate_AllComponentOfType<Light>(false, light => light.type == LightType.Disc);

        [MenuItem(HD_Constants.Deactivate_Light + "/Deactivate All Reflection Probes", false, HD_Constants.MenuPriorityEleven + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_ReflectionProbes() => HD_Operations.Activate_AllComponentOfType<ReflectionProbe>(false);

        [MenuItem(HD_Constants.Deactivate_Light + "/Deactivate All Light Probe Groups", false, HD_Constants.MenuPriorityEleven + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_LightProbeGroups() => HD_Operations.Activate_AllComponentOfType<LightProbeGroup>(false);

#if UNITY_6000_5_OR_NEWER
#else
        [MenuItem(HD_Constants.Deactivate_Light + "/Deactivate All Light Probe Proxy Volumes", false, HD_Constants.MenuPriorityEleven + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_LightProbeProxyVolumes() => HD_Operations.Activate_AllComponentOfType<LightProbeProxyVolume>(false);
#endif
        #endregion

        #region Video
        [MenuItem(HD_Constants.Deactivate_Video + "/Deactivate All Video Players", false, HD_Constants.MenuPriorityNine + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_VideoPlayers() => HD_Operations.Activate_AllComponentOfType<UnityEngine.Video.VideoPlayer>(false);
        #endregion

        #region UI Toolkit
        [MenuItem(HD_Constants.Deactivate_UIToolkit + "/Deactivate All UI Documents", false, HD_Constants.MenuPriorityNine + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_UIDocuments() => HD_Operations.Activate_AllComponentOfType<UnityEngine.UIElements.UIDocument>(false);

        [MenuItem(HD_Constants.Deactivate_UIToolkit + "/Deactivate All Panel Event Handlers", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_PanelEventHandlers() => HD_Operations.Activate_AllComponentOfType<UnityEngine.UIElements.PanelEventHandler>(false);

        [MenuItem(HD_Constants.Deactivate_UIToolkit + "/Deactivate All Panel Raycasters", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_PanelRaycasters() => HD_Operations.Activate_AllComponentOfType<UnityEngine.UIElements.PanelRaycaster>(false);
        #endregion

        #region Cameras
        [MenuItem(HD_Constants.Tools_Deactivate + "/Deactivate All Cameras", false, HD_Constants.MenuPriorityNine + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Cameras() => HD_Operations.Activate_AllComponentOfType<Camera>(false);
        #endregion

        #region UI
        [MenuItem(HD_Constants.Deactivate_UI + "/Deactivate All Images", false, HD_Constants.MenuPriorityNine + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Images() => HD_Operations.Activate_AllComponentOfType<Image>(false);

        [MenuItem(HD_Constants.Deactivate_UI + "/Deactivate All Texts - TextMeshPro", false, HD_Constants.MenuPriorityNine + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_TextMeshPro() => HD_Operations.Activate_AllTMPComponentIfAvailable<TMPro.TMP_Text>(false);

        [MenuItem(HD_Constants.Deactivate_UI + "/Deactivate All Raw Images", false, HD_Constants.MenuPriorityNine + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_RawImages() => HD_Operations.Activate_AllComponentOfType<RawImage>(false);

        [MenuItem(HD_Constants.Deactivate_UI + "/Deactivate All Toggles", false, HD_Constants.MenuPriorityTen + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Toggles() => HD_Operations.Activate_AllComponentOfType<Toggle>(false);

        [MenuItem(HD_Constants.Deactivate_UI + "/Deactivate All Sliders", false, HD_Constants.MenuPriorityTen + 5)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Sliders() => HD_Operations.Activate_AllComponentOfType<Slider>(false);

        [MenuItem(HD_Constants.Deactivate_UI + "/Deactivate All Scrollbars", false, HD_Constants.MenuPriorityTen + 6)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Scrollbars() => HD_Operations.Activate_AllComponentOfType<Scrollbar>(false);

        [MenuItem(HD_Constants.Deactivate_UI + "/Deactivate All Scroll Views", false, HD_Constants.MenuPriorityTen + 7)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_ScrollViews() => HD_Operations.Activate_AllComponentOfType<ScrollRect>(false);

        [MenuItem(HD_Constants.Deactivate_UI + "/Deactivate All Dropdowns - TextMeshPro", false, HD_Constants.MenuPriorityTen + 8)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_DropdownTextMeshPro() => HD_Operations.Activate_AllTMPComponentIfAvailable<TMPro.TMP_Dropdown>(false);

        [MenuItem(HD_Constants.Deactivate_UI + "/Deactivate All Input Fields - TextMeshPro", false, HD_Constants.MenuPriorityTen + 8)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_InputFieldTextMeshPro() => HD_Operations.Activate_AllTMPComponentIfAvailable<TMPro.TMP_InputField>(false);

        [MenuItem(HD_Constants.Deactivate_UI + "/Deactivate All Canvases", false, HD_Constants.MenuPriorityEleven + 8)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Canvases() => HD_Operations.Activate_AllComponentOfType<Canvas>(false);

        [MenuItem(HD_Constants.Deactivate_UI + "/Deactivate All Event Systems", false, HD_Constants.MenuPriorityEleven + 8)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_EventSystems() => HD_Operations.Activate_AllComponentOfType<UnityEngine.EventSystems.EventSystem>(false);

        [MenuItem(HD_Constants.Deactivate_UI_Legacy + "/Deactivate All Texts", false, HD_Constants.MenuPriorityTwelve + 8)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Texts() => HD_Operations.Activate_AllComponentOfType<Text>(false);

        [MenuItem(HD_Constants.Deactivate_UI_Legacy + "/Deactivate All Buttons", false, HD_Constants.MenuPriorityTwelve + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Buttons() => HD_Operations.Activate_AllComponentOfType<Button>(false);

        [MenuItem(HD_Constants.Deactivate_UI_Legacy + "/Deactivate All Dropdowns", false, HD_Constants.MenuPriorityTwelve + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Dropdowns() => HD_Operations.Activate_AllComponentOfType<Dropdown>(false);

        [MenuItem(HD_Constants.Deactivate_UI_Legacy + "/Deactivate All Input Fields", false, HD_Constants.MenuPriorityTwelve + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_InputFields() => HD_Operations.Activate_AllComponentOfType<InputField>(false);

        [MenuItem(HD_Constants.Deactivate_UI + "/Deactivate All Masks", false, HD_Constants.MenuPriorityThirteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Masks() => HD_Operations.Activate_AllComponentOfType<Mask>(false);

        [MenuItem(HD_Constants.Deactivate_UI + "/Deactivate All Rect Masks 2D", false, HD_Constants.MenuPriorityThirteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_RectMasks2D() => HD_Operations.Activate_AllComponentOfType<RectMask2D>(false);

        [MenuItem(HD_Constants.Deactivate_UI + "/Deactivate All Selectables", false, HD_Constants.MenuPriorityThirteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Selectables() => HD_Operations.Activate_AllComponentOfType<Selectable>(false);

        [MenuItem(HD_Constants.Deactivate_UI + "/Deactivate All Toggle Groups", false, HD_Constants.MenuPriorityThirteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_ToggleGroups() => HD_Operations.Activate_AllComponentOfType<ToggleGroup>(false);

        [MenuItem(HD_Constants.Deactivate_UI_Effects + "/Deactivate All Outlines", false, HD_Constants.MenuPriorityFourteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Outlines() => HD_Operations.Activate_AllComponentOfType<Outline>(false);

        [MenuItem(HD_Constants.Deactivate_UI_Effects + "/Deactivate All Positions As UV1", false, HD_Constants.MenuPriorityFourteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_PositionsAsUV1() => HD_Operations.Activate_AllComponentOfType<PositionAsUV1>(false);

        [MenuItem(HD_Constants.Deactivate_UI_Effects + "/Deactivate All Shadows", false, HD_Constants.MenuPriorityFourteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Shadows() => HD_Operations.Activate_AllComponentOfType<Shadow>(false);
        #endregion
        #endregion

        #region Count
        #region General
        [MenuItem(HD_Constants.Count_General + "/Count Selected GameObjects", false, HD_Constants.MenuPriorityNine)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_SelectedGameObjects() => HD_Operations.Count_SelectedGameObjects();

        [MenuItem(HD_Constants.Count_General + "/Count All GameObjects", false, HD_Constants.MenuPriorityTen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_GameObjects() => HD_Operations.Count_AllGameObjects();

        [MenuItem(HD_Constants.Count_General + "/Count All Parent GameObjects", false, HD_Constants.MenuPriorityTen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_ParentGameObjects() => HD_Operations.Count_AllParentGameObjects();

        [MenuItem(HD_Constants.Count_General + "/Count All Empty GameObjects", false, HD_Constants.MenuPriorityEleven)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_EmptyGameObjects() => HD_Operations.Count_AllEmptyGameObjects();

        [MenuItem(HD_Constants.Count_General + "/Count All Locked GameObjects", false, HD_Constants.MenuPriorityEleven)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_LockedGameObjects() => HD_Operations.Count_AllLockedGameObjects();

        [MenuItem(HD_Constants.Count_General + "/Count All Active GameObjects", false, HD_Constants.MenuPriorityTwelve)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_ActiveGameObjects() => HD_Operations.Count_AllActiveGameObjects();

        [MenuItem(HD_Constants.Count_General + "/Count All Inactive GameObjects", false, HD_Constants.MenuPriorityTwelve)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_InactiveGameObjects() => HD_Operations.Count_AllInactiveGameObjects();

        [MenuItem(HD_Constants.Count_General + "/Count All Folders", false, HD_Constants.MenuPriorityThirteen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Folders() => HD_Operations.Count_AllFolders();

        [MenuItem(HD_Constants.Count_General + "/Count All Separators", false, HD_Constants.MenuPriorityThirteen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Separators() => HD_Operations.Count_AllSeparators();
        #endregion

        #region 2D Objects
        [MenuItem(HD_Constants.Count_2D + "/Count All Sprites", false, HD_Constants.MenuPriorityTen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Sprites() => HD_Operations.Count_AllComponentOfType<SpriteRenderer>("Sprites");

        [MenuItem(HD_Constants.Count_2D + "/Count All Sprite Masks", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_SpriteMasks() => HD_Operations.Count_AllComponentOfType<SpriteMask>("Sprite Masks");

        [MenuItem(HD_Constants.Count_2D_Sprites + "/Count All 9-Sliced Sprites", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_9SlicedSprites() => HD_Operations.Count_All2DSpritesByType("9-Sliced");

        [MenuItem(HD_Constants.Count_2D_Sprites + "/Count All Capsule Sprites", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_CapsuleSprites() => HD_Operations.Count_All2DSpritesByType("Capsule");

        [MenuItem(HD_Constants.Count_2D_Sprites + "/Count All Circle Sprites", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_CircleSprites() => HD_Operations.Count_All2DSpritesByType("Circle");

        [MenuItem(HD_Constants.Count_2D_Sprites + "/Count All Hexagon Flat-Top Sprites", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_HexagonFlatTopSprites() => HD_Operations.Count_All2DSpritesByType("Hexagon Flat-Top");

        [MenuItem(HD_Constants.Count_2D_Sprites + "/Count All Hexagon Pointed-Top Sprites", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_HexagonPointedTopSprites() => HD_Operations.Count_All2DSpritesByType("Hexagon Pointed-Top");

        [MenuItem(HD_Constants.Count_2D_Sprites + "/Count All Isometric Diamond Sprites", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_IsometricDiamondSprites() => HD_Operations.Count_All2DSpritesByType("Isometric Diamond");

        [MenuItem(HD_Constants.Count_2D_Sprites + "/Count All Square Sprites", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_SquareSprites() => HD_Operations.Count_All2DSpritesByType("Square");

        [MenuItem(HD_Constants.Count_2D_Sprites + "/Count All Triangle Sprites", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_TriangleSprites() => HD_Operations.Count_All2DSpritesByType("Triangle");

        [MenuItem(HD_Constants.Count_2D_Physics + "/Count All Dynamic Sprites", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_PhysicsDynamicSprites() => HD_Operations.Count_AllPhysicsDynamicSprites();

        [MenuItem(HD_Constants.Count_2D_Physics + "/Count All Static Sprites", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_PhysicsStaticSprites() => HD_Operations.Count_AllPhysicsStaticSprites();
        #endregion

        #region 3D Objects
        [MenuItem(HD_Constants.Count_3D + "/Count All Mesh Filters", false, HD_Constants.MenuPriorityTen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_MeshFilters() => HD_Operations.Count_AllComponentOfType<MeshFilter>("Mesh Filters");

        [MenuItem(HD_Constants.Count_3D + "/Count All Mesh Renderers", false, HD_Constants.MenuPriorityEleven)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_MeshRenderers() => HD_Operations.Count_AllComponentOfType<MeshRenderer>("Mesh Renderers");

        [MenuItem(HD_Constants.Count_3D + "/Count All Skinned Mesh Renderer", false, HD_Constants.MenuPriorityEleven)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_SkinnedMeshRenderers() => HD_Operations.Count_AllComponentOfType<SkinnedMeshRenderer>("Skinned Mesh Renderers");

        [MenuItem(HD_Constants.Count_3D + "/Count All Cubes", false, HD_Constants.MenuPriorityEleven)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_CubesObjects() => HD_Operations.Count_All3DObjectsByType("Cube");

        [MenuItem(HD_Constants.Count_3D + "/Count All Spheres", false, HD_Constants.MenuPriorityEleven)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_SpheresObjects() => HD_Operations.Count_All3DObjectsByType("Sphere");

        [MenuItem(HD_Constants.Count_3D + "/Count All Capsules", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_CapsulesObjects() => HD_Operations.Count_All3DObjectsByType("Capsule");

        [MenuItem(HD_Constants.Count_3D + "/Count All Cylinders", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_CylindersObjects() => HD_Operations.Count_All3DObjectsByType("Cylinder");

        [MenuItem(HD_Constants.Count_3D + "/Count All Planes", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_PlanesObjects() => HD_Operations.Count_All3DObjectsByType("Plane");

        [MenuItem(HD_Constants.Count_3D + "/Count All Quads", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_QuadsObjects() => HD_Operations.Count_All3DObjectsByType("Quad");

        [MenuItem(HD_Constants.Count_3D + "/Count All Texts - TextMeshPro", false, HD_Constants.MenuPriorityTwelve + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_TextMeshProObjects() => HD_Operations.Count_All3DObjectsByType("TextMeshPro Mesh");

        [MenuItem(HD_Constants.Count_3D_Legacy + "/Count All Text Meshes", false, HD_Constants.MenuPriorityTwelve + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_TextMeshesObjects() => HD_Operations.Count_AllComponentOfType<TextMesh>("Text Meshes");

        [MenuItem(HD_Constants.Count_3D + "/Count All Terrains", false, HD_Constants.MenuPriorityThirteen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_TerrainsObjects() => HD_Operations.Count_AllComponentOfType<Terrain>("Terrains");

        [MenuItem(HD_Constants.Count_3D + "/Count All Trees", false, HD_Constants.MenuPriorityThirteen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_TreesObjects() => HD_Operations.Count_AllComponentOfType<Tree>("Trees");

        [MenuItem(HD_Constants.Count_3D + "/Count All Wind Zones", false, HD_Constants.MenuPriorityThirteen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_WindZonesObjects() => HD_Operations.Count_AllComponentOfType<WindZone>("Wind Zones");
        #endregion

        #region Audio
        [MenuItem(HD_Constants.Count_Audio + "/Count All Audio Sources", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_CountAllAudioSources() => HD_Operations.Count_AllComponentOfType<AudioSource>("Audio Sources");

        [MenuItem(HD_Constants.Count_Audio + "/Count All Audio Reverb Zones", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_AudioReverbZones() => HD_Operations.Count_AllComponentOfType<AudioReverbZone>("Audio Reverb Zones");

        [MenuItem(HD_Constants.Count_Audio + "/Count All Audio Chorus Filters", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_AudioChorusFilters() => HD_Operations.Count_AllComponentOfType<AudioChorusFilter>("Audio Chorus Filters");

        [MenuItem(HD_Constants.Count_Audio + "/Count All Audio Distortion Filters", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_AudioDistortionFilters() => HD_Operations.Count_AllComponentOfType<AudioDistortionFilter>("Audio Distortion Filters");

        [MenuItem(HD_Constants.Count_Audio + "/Count All Audio Echo Filters", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_AudioEchoFilters() => HD_Operations.Count_AllComponentOfType<AudioEchoFilter>("Audio Echo Filters");

        [MenuItem(HD_Constants.Count_Audio + "/Count All Audio High Pass Filters", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_AudioHighPassFilters() => HD_Operations.Count_AllComponentOfType<AudioHighPassFilter>("Audio High Pass Filters");

        [MenuItem(HD_Constants.Count_Audio + "/Count All Audio Listeners", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_AudioListeners() => HD_Operations.Count_AllComponentOfType<AudioListener>("Audio Listeners");

        [MenuItem(HD_Constants.Count_Audio + "/Count All Audio Low Pass Filters", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_AudioLowPassFilters() => HD_Operations.Count_AllComponentOfType<AudioLowPassFilter>("Audio Low Pass Filters");

        [MenuItem(HD_Constants.Count_Audio + "/Count All Audio Reverb Filters", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_AudioReverbFilters() => HD_Operations.Count_AllComponentOfType<AudioReverbFilter>("Audio Reverb Filters");
        #endregion

        #region Effects
        [MenuItem(HD_Constants.Count_Effects + "/Count All Particle Systems", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_ParticleSystems() => HD_Operations.Count_AllComponentOfType<ParticleSystem>("Particle Systems");

        [MenuItem(HD_Constants.Count_Effects + "/Count All Particle System Force Fields", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_ParticleSystemForceFields() => HD_Operations.Count_AllComponentOfType<ParticleSystemForceField>("Particle System Force Fields");

        [MenuItem(HD_Constants.Count_Effects + "/Count All Trail Renderers", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_TrailRenderers() => HD_Operations.Count_AllComponentOfType<TrailRenderer>("Trail Renderers");

        [MenuItem(HD_Constants.Count_Effects + "/Count All Line Renderers", false, HD_Constants.MenuPriorityTen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_LineRenderers() => HD_Operations.Count_AllComponentOfType<LineRenderer>("Line Renderers");

        [MenuItem(HD_Constants.Count_Effects + "/Count All Halos", false, HD_Constants.MenuPriorityEleven + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Halos() => HD_Operations.Count_AllHalos();

#if UNITY_6000_5_OR_NEWER
#else
        [MenuItem(HD_Constants.Count_Effects + "/Count All Lens Flares", false, HD_Constants.MenuPriorityEleven + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_LensFlares() => HD_Operations.Count_AllComponentOfType<LensFlare>("Lens Flares");

        [MenuItem(HD_Constants.Count_Effects + "/Count All Projectors", false, HD_Constants.MenuPriorityEleven + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Projectors() => HD_Operations.Count_AllComponentOfType<Projector>("Projectors");
#endif

        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_VisualEffects() => HD_Operations.Count_AllComponentOfType<UnityEngine.VFX.VisualEffect>("Visual Effects");
        #endregion

        #region Lights
        [MenuItem(HD_Constants.Count_Light + "/Count All Lights", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Lights() => HD_Operations.Count_AllComponentOfType<Light>("Lights");

        [MenuItem(HD_Constants.Count_Light + "/Count All Directional Lights", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_DirectionalLights() => HD_Operations.Count_AllComponentOfType<Light>("Directional Lights", light => light.type == LightType.Directional);

        [MenuItem(HD_Constants.Count_Light + "/Count All Point Lights", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_PointLights() => HD_Operations.Count_AllComponentOfType<Light>("Point Lights", light => light.type == LightType.Point);

        [MenuItem(HD_Constants.Count_Light + "/Count All Spot Lights", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_SpotLights() => HD_Operations.Count_AllComponentOfType<Light>("Spot Lights", light => light.type == LightType.Spot);

        [MenuItem(HD_Constants.Count_Light + "/Count All Rectangle Area Lights", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_RectangleAreaLights() => HD_Operations.Count_AllComponentOfType<Light>("Rectangle Area Lights", light => light.type == LightType.Rectangle);

        [MenuItem(HD_Constants.Count_Light + "/Count All Disc Area Lights", false, HD_Constants.MenuPriorityEleven + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_DiscAreaLights() => HD_Operations.Count_AllComponentOfType<Light>("Disc Area Lights", light => light.type == LightType.Disc);

        [MenuItem(HD_Constants.Count_Light + "/Count All Reflection Probes", false, HD_Constants.MenuPriorityTwelve + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_ReflectionProbes() => HD_Operations.Count_AllComponentOfType<ReflectionProbe>("Reflection Probes");

        [MenuItem(HD_Constants.Count_Light + "/Count All Light Probe Groups", false, HD_Constants.MenuPriorityTwelve + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_LightProbeGroups() => HD_Operations.Count_AllComponentOfType<LightProbeGroup>("Light Probe Groups");

#if UNITY_6000_5_OR_NEWER
#else
        [MenuItem(HD_Constants.Count_Light + "/Count All Light Probe Proxy Volumes", false, HD_Constants.MenuPriorityTwelve + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_LightProbeProxyVolumes() => HD_Operations.Count_AllComponentOfType<LightProbeProxyVolume>("Light Probe Proxy Volumes");
#endif
        #endregion

        #region Video
        [MenuItem(HD_Constants.Count_Video + "/Count All Video Players", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_VideoPlayers() => HD_Operations.Count_AllComponentOfType<UnityEngine.Video.VideoPlayer>("Video Players");
        #endregion

        #region UI Toolkit
        [MenuItem(HD_Constants.Count_UIToolkit + "/Count All UI Documents", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_UIDocuments() => HD_Operations.Count_AllComponentOfType<UnityEngine.UIElements.UIDocument>("UI Documents");

        [MenuItem(HD_Constants.Count_UIToolkit + "/Count All Panel Event Handlers", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_PanelEventHandlers() => HD_Operations.Count_AllComponentOfType<UnityEngine.UIElements.PanelEventHandler>("Panel Event Handlers");

        [MenuItem(HD_Constants.Count_UIToolkit + "/Count All Panel Raycasters", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_PanelRaycasters() => HD_Operations.Count_AllComponentOfType<UnityEngine.UIElements.PanelRaycaster>("Panel Raycasters");
        #endregion

        #region Cameras
        [MenuItem(HD_Constants.Tools_Count + "/Count All Cameras", false, HD_Constants.MenuPriorityTen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Cameras() => HD_Operations.Count_AllComponentOfType<Camera>("Cameras");
        #endregion

        #region UI
        [MenuItem(HD_Constants.Count_UI + "/Count All Images", false, HD_Constants.MenuPriorityTen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Images() => HD_Operations.Count_AllComponentOfType<Image>("Images");

        [MenuItem(HD_Constants.Count_UI + "/Count All Texts - TextMeshPro", false, HD_Constants.MenuPriorityTen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_TextMeshPro() => HD_Operations.Count_AllTMPComponentIfAvailable<TMPro.TMP_Text>("Text - TextMeshPro");

        [MenuItem(HD_Constants.Count_UI + "/Count All Raw Images", false, HD_Constants.MenuPriorityTen + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_RawImages() => HD_Operations.Count_AllComponentOfType<RawImage>("Raw Images");

        [MenuItem(HD_Constants.Count_UI + "/Count All Toggles", false, HD_Constants.MenuPriorityEleven + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Toggles() => HD_Operations.Count_AllComponentOfType<Toggle>("Toggles");

        [MenuItem(HD_Constants.Count_UI + "/Count All Sliders", false, HD_Constants.MenuPriorityEleven + 5)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Sliders() => HD_Operations.Count_AllComponentOfType<Slider>("Sliders");

        [MenuItem(HD_Constants.Count_UI + "/Count All Scrollbars", false, HD_Constants.MenuPriorityEleven + 6)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Scrollbars() => HD_Operations.Count_AllComponentOfType<Scrollbar>("Scrollbars");

        [MenuItem(HD_Constants.Count_UI + "/Count All Scroll Views", false, HD_Constants.MenuPriorityEleven + 7)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_ScrollViews() => HD_Operations.Count_AllComponentOfType<ScrollRect>("Scroll Views");

        [MenuItem(HD_Constants.Count_UI + "/Count All Dropdowns - TextMeshPro", false, HD_Constants.MenuPriorityEleven + 8)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_DropdownTextMeshPro() => HD_Operations.Count_AllTMPComponentIfAvailable<TMPro.TMP_Dropdown>("Dropdowns - TextMeshPro");

        [MenuItem(HD_Constants.Count_UI + "/Count All Input Fields - TextMeshPro", false, HD_Constants.MenuPriorityEleven + 8)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_InputFieldTextMeshPro() => HD_Operations.Count_AllTMPComponentIfAvailable<TMPro.TMP_InputField>("Input Fields - TextMeshPro");

        [MenuItem(HD_Constants.Count_UI + "/Count All Canvases", false, HD_Constants.MenuPriorityTwelve + 8)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Canvases() => HD_Operations.Count_AllComponentOfType<Canvas>("Canvases");

        [MenuItem(HD_Constants.Count_UI + "/Count All Event Systems", false, HD_Constants.MenuPriorityTwelve + 8)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_EventSystems() => HD_Operations.Count_AllComponentOfType<UnityEngine.EventSystems.EventSystem>("Event Systems");

        [MenuItem(HD_Constants.Count_UI_Legacy + "/Count All Texts", false, HD_Constants.MenuPriorityThirteen + 8)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Texts() => HD_Operations.Count_AllComponentOfType<Text>("Texts");

        [MenuItem(HD_Constants.Count_UI_Legacy + "/Count All Buttons", false, HD_Constants.MenuPriorityThirteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Buttons() => HD_Operations.Count_AllComponentOfType<Button>("Buttons");

        [MenuItem(HD_Constants.Count_UI_Legacy + "/Count All Dropdowns", false, HD_Constants.MenuPriorityThirteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Dropdowns() => HD_Operations.Count_AllComponentOfType<Dropdown>("Dropdowns");

        [MenuItem(HD_Constants.Count_UI_Legacy + "/Count All Input Fields", false, HD_Constants.MenuPriorityThirteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_InputFields() => HD_Operations.Count_AllComponentOfType<InputField>("Input Fields");

        [MenuItem(HD_Constants.Count_UI + "/Count All Masks", false, HD_Constants.MenuPriorityFourteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Masks() => HD_Operations.Count_AllComponentOfType<Mask>("Masks");

        [MenuItem(HD_Constants.Count_UI + "/Count All Rect Masks 2D", false, HD_Constants.MenuPriorityFourteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_RectMasks2D() => HD_Operations.Count_AllComponentOfType<RectMask2D>("Rect Masks 2D");

        [MenuItem(HD_Constants.Count_UI + "/Count All Selectables", false, HD_Constants.MenuPriorityFourteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Selectables() => HD_Operations.Count_AllComponentOfType<Selectable>("Selectables");

        [MenuItem(HD_Constants.Count_UI + "/Count All Toggle Groups", false, HD_Constants.MenuPriorityFourteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_ToggleGroups() => HD_Operations.Count_AllComponentOfType<ToggleGroup>("Toggle Groups");

        [MenuItem(HD_Constants.Count_UI_Effects + "/Count All Outlines", false, HD_Constants.MenuPriorityFifteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_lOutlines() => HD_Operations.Count_AllComponentOfType<Outline>("Outlines");

        [MenuItem(HD_Constants.Count_UI_Effects + "/Count All Positions As UV1", false, HD_Constants.MenuPriorityFifteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_PositionsAsUV1() => HD_Operations.Count_AllComponentOfType<PositionAsUV1>("Positions As UV1");

        [MenuItem(HD_Constants.Count_UI_Effects + "/Count All Shadows", false, HD_Constants.MenuPriorityFifteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Shadows() => HD_Operations.Count_AllComponentOfType<Shadow>("Shadows");
        #endregion
        #endregion

        #region Lock
        #region General
        [MenuItem(HD_Constants.Lock_General + "/Lock Selected GameObjects", false, HD_Constants.MenuPriorityNine)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_SelectedGameObjects() => HD_Operations.Lock_SelectedGameObjects(true);

        [MenuItem(HD_Constants.Lock_General + "/Lock All GameObjects", false, HD_Constants.MenuPriorityTen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_GameObjects() => HD_Operations.Lock_AllGameObjects(true);

        [MenuItem(HD_Constants.Lock_General + "/Lock All Parent GameObjects", false, HD_Constants.MenuPriorityTen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_ParentGameObjects() => HD_Operations.Lock_AllParentGameObjects(true);

        [MenuItem(HD_Constants.Lock_General + "/Lock All Empty GameObjects", false, HD_Constants.MenuPriorityEleven)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_EmptyGameObjects() => HD_Operations.Lock_AllEmptyGameObjects(true);

        [MenuItem(HD_Constants.Lock_General + "/Lock All Active GameObjects", false, HD_Constants.MenuPriorityTwelve)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_ActiveGameObjects() => HD_Operations.Lock_AllActiveGameObjects(true);

        [MenuItem(HD_Constants.Lock_General + "/Lock All Inactive GameObjects", false, HD_Constants.MenuPriorityTwelve)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_InactiveGameObjects() => HD_Operations.Lock_AllInactiveGameObjects(true);

        [MenuItem(HD_Constants.Lock_General + "/Lock All Folders", false, HD_Constants.MenuPriorityThirteen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Folders() => HD_Operations.Lock_AllFolders(true);
        #endregion

        #region 2D Objects
        [MenuItem(HD_Constants.Lock_2D + "/Lock All Sprites", false, HD_Constants.MenuPriorityTen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Sprites() => HD_Operations.Lock_AllComponentOfType<SpriteRenderer>(true);

        [MenuItem(HD_Constants.Lock_2D + "/Lock All Sprite Masks", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_SpriteMasks() => HD_Operations.Lock_AllComponentOfType<SpriteMask>(true);

        [MenuItem(HD_Constants.Lock_2D_Sprites + "/Lock All 9-Sliced Sprites", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_9SlicedSprites() => HD_Operations.Lock_All2DSpritesByType("9-Sliced", true);

        [MenuItem(HD_Constants.Lock_2D_Sprites + "/Lock All Capsule Sprites", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_CapsuleSprites() => HD_Operations.Lock_All2DSpritesByType("Capsule", true);

        [MenuItem(HD_Constants.Lock_2D_Sprites + "/Lock All Circle Sprites", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_CircleSprites() => HD_Operations.Lock_All2DSpritesByType("Circle", true);

        [MenuItem(HD_Constants.Lock_2D_Sprites + "/Lock All Hexagon Flat-Top Sprites", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_HexagonFlatTopSprites() => HD_Operations.Lock_All2DSpritesByType("Hexagon Flat-Top", true);

        [MenuItem(HD_Constants.Lock_2D_Sprites + "/Lock All Hexagon Pointed-Top Sprites", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_HexagonPointedTopSprites() => HD_Operations.Lock_All2DSpritesByType("Hexagon Pointed-Top", true);

        [MenuItem(HD_Constants.Lock_2D_Sprites + "/Lock All Isometric Diamond Sprites", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_IsometricDiamondSprites() => HD_Operations.Lock_All2DSpritesByType("Isometric Diamond", true);

        [MenuItem(HD_Constants.Lock_2D_Sprites + "/Lock All Square Sprites", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_SquareSprites() => HD_Operations.Lock_All2DSpritesByType("Square", true);

        [MenuItem(HD_Constants.Lock_2D_Sprites + "/Lock All Triangle Sprites", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_TriangleSprites() => HD_Operations.Lock_All2DSpritesByType("Triangle", true);

        [MenuItem(HD_Constants.Lock_2D_Physics + "/Lock All Dynamic Sprites", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_PhysicsDynamicSprites() => HD_Operations.Lock_AllPhysicsDynamicSprites(true);

        [MenuItem(HD_Constants.Lock_2D_Physics + "/Lock All Static Sprites", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_PhysicsStaticSprites() => HD_Operations.Lock_AllPhysicsStaticSprites(true);
        #endregion

        #region 3D Objects
        [MenuItem(HD_Constants.Lock_3D + "/Lock All Mesh Filters", false, HD_Constants.MenuPriorityTen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_MeshFilters() => HD_Operations.Lock_AllComponentOfType<MeshFilter>(true);

        [MenuItem(HD_Constants.Lock_3D + "/Lock All Mesh Renderers", false, HD_Constants.MenuPriorityTen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_MeshRenderers() => HD_Operations.Lock_AllComponentOfType<MeshRenderer>(true);

        [MenuItem(HD_Constants.Lock_3D + "/Lock All Skinned Mesh Renderer", false, HD_Constants.MenuPriorityTen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_SkinnedMeshRenderers() => HD_Operations.Lock_AllComponentOfType<SkinnedMeshRenderer>(true);

        [MenuItem(HD_Constants.Lock_3D + "/Lock All Cubes", false, HD_Constants.MenuPriorityEleven)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_CubesObjects() => HD_Operations.Lock_All3DObjectsByType("Cube", true);

        [MenuItem(HD_Constants.Lock_3D + "/Lock All Spheres", false, HD_Constants.MenuPriorityEleven)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_SpheresObjects() => HD_Operations.Lock_All3DObjectsByType("Sphere", true);

        [MenuItem(HD_Constants.Lock_3D + "/Lock All Capsules", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_CapsulesObjects() => HD_Operations.Lock_All3DObjectsByType("Capsule", true);

        [MenuItem(HD_Constants.Lock_3D + "/Lock All Cylinders", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_CylindersObjects() => HD_Operations.Lock_All3DObjectsByType("Cylinder", true);

        [MenuItem(HD_Constants.Lock_3D + "/Lock All Planes", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_PlanesObjects() => HD_Operations.Lock_All3DObjectsByType("Plane", true);

        [MenuItem(HD_Constants.Lock_3D + "/Lock All Quads", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_QuadsObjects() => HD_Operations.Lock_All3DObjectsByType("Quad", true);

        [MenuItem(HD_Constants.Lock_3D + "/Lock All Texts - TextMeshPro", false, HD_Constants.MenuPriorityTwelve + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_TextMeshProObjects() => HD_Operations.Lock_All3DObjectsByType("TextMeshPro Mesh", true);

        [MenuItem(HD_Constants.Lock_3D_Legacy + "/Lock All Text Meshes", false, HD_Constants.MenuPriorityTwelve + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_TextMeshesObjects() => HD_Operations.Lock_AllComponentOfType<TextMesh>(true);

        [MenuItem(HD_Constants.Lock_3D + "/Lock All Terrains", false, HD_Constants.MenuPriorityThirteen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_TerrainsObjects() => HD_Operations.Lock_AllComponentOfType<Terrain>(true);

        [MenuItem(HD_Constants.Lock_3D + "/Lock All Trees", false, HD_Constants.MenuPriorityThirteen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_TreesObjects() => HD_Operations.Lock_AllComponentOfType<Tree>(true);

        [MenuItem(HD_Constants.Lock_3D + "/Lock All Wind Zones", false, HD_Constants.MenuPriorityThirteen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_WindZonesObjects() => HD_Operations.Lock_AllComponentOfType<WindZone>(true);
        #endregion

        #region Audio
        [MenuItem(HD_Constants.Lock_Audio + "/Lock All Audio Sources", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_AudioSources() => HD_Operations.Lock_AllComponentOfType<AudioSource>(true);

        [MenuItem(HD_Constants.Lock_Audio + "/Lock All Audio Reverb Zones", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_AudioReverbZones() => HD_Operations.Lock_AllComponentOfType<AudioReverbZone>(true);

        [MenuItem(HD_Constants.Lock_Audio + "/Lock All Audio Chorus Filters", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_AudioChorusFilters() => HD_Operations.Lock_AllComponentOfType<AudioChorusFilter>(true);

        [MenuItem(HD_Constants.Lock_Audio + "/Lock All Audio Distortion Filters", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_AudioDistortionFilters() => HD_Operations.Lock_AllComponentOfType<AudioDistortionFilter>(true);

        [MenuItem(HD_Constants.Lock_Audio + "/Lock All Audio Echo Filters", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_AudioEchoFilters() => HD_Operations.Lock_AllComponentOfType<AudioEchoFilter>(true);

        [MenuItem(HD_Constants.Lock_Audio + "/Lock All Audio High Pass Filters", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_AudioHighPassFilters() => HD_Operations.Lock_AllComponentOfType<AudioHighPassFilter>(true);

        [MenuItem(HD_Constants.Lock_Audio + "/Lock All Audio Listeners", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_AudioListeners() => HD_Operations.Lock_AllComponentOfType<AudioListener>(true);

        [MenuItem(HD_Constants.Lock_Audio + "/Lock All Audio Low Pass Filters", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_AudioLowPassFilters() => HD_Operations.Lock_AllComponentOfType<AudioLowPassFilter>(true);

        [MenuItem(HD_Constants.Lock_Audio + "/Lock All Audio Reverb Filters", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_AudioReverbFilters() => HD_Operations.Lock_AllComponentOfType<AudioReverbFilter>(true);
        #endregion

        #region Effects
        [MenuItem(HD_Constants.Lock_Effects + "/Lock All Particle Systems", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_ParticleSystems() => HD_Operations.Lock_AllComponentOfType<ParticleSystem>(true);

        [MenuItem(HD_Constants.Lock_Effects + "/Lock All Particle System Force Fields", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_ParticleSystemForceFields() => HD_Operations.Lock_AllComponentOfType<ParticleSystemForceField>(true);

        [MenuItem(HD_Constants.Lock_Effects + "/Lock All Trail Renderers", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_TrailRenderers() => HD_Operations.Lock_AllComponentOfType<TrailRenderer>(true);

        [MenuItem(HD_Constants.Lock_Effects + "/Lock All Line Renderers", false, HD_Constants.MenuPriorityTen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_LineRenderers() => HD_Operations.Lock_AllComponentOfType<LineRenderer>(true);

        [MenuItem(HD_Constants.Lock_Effects + "/Lock All Halos", false, HD_Constants.MenuPriorityEleven + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Halos() => HD_Operations.Lock_AllHalos(true);

#if UNITY_6000_5_OR_NEWER
#else
        [MenuItem(HD_Constants.Lock_Effects + "/Lock All Lens Flares", false, HD_Constants.MenuPriorityEleven + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_LensFlares() => HD_Operations.Lock_AllComponentOfType<LensFlare>(true);

        [MenuItem(HD_Constants.Lock_Effects + "/Lock All Projectors", false, HD_Constants.MenuPriorityEleven + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Projectors() => HD_Operations.Lock_AllComponentOfType<Projector>(true);
#endif

        [MenuItem(HD_Constants.Lock_Effects + "/Lock All Visual Effects", false, HD_Constants.MenuPriorityEleven + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_VisualEffects() => HD_Operations.Lock_AllComponentOfType<UnityEngine.VFX.VisualEffect>(true);
        #endregion

        #region Lights
        [MenuItem(HD_Constants.Lock_Light + "/Lock All Lights", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Lights() => HD_Operations.Lock_AllComponentOfType<Light>(true);

        [MenuItem(HD_Constants.Lock_Light + "/Lock All Directional Lights", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_DirectionalLights() => HD_Operations.Lock_AllComponentOfType<Light>(true, light => light.type == LightType.Directional);

        [MenuItem(HD_Constants.Lock_Light + "/Lock All Point Lights", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_PointLights() => HD_Operations.Lock_AllComponentOfType<Light>(true, light => light.type == LightType.Point);

        [MenuItem(HD_Constants.Lock_Light + "/Lock All Spot Lights", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_SpotLights() => HD_Operations.Lock_AllComponentOfType<Light>(true, light => light.type == LightType.Spot);

        [MenuItem(HD_Constants.Lock_Light + "/Lock All Rectangle Area Lights", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_RectangleAreaLights() => HD_Operations.Lock_AllComponentOfType<Light>(true, light => light.type == LightType.Rectangle);

        [MenuItem(HD_Constants.Lock_Light + "/Lock All Disc Area Lights", false, HD_Constants.MenuPriorityEleven + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_DiscAreaLights() => HD_Operations.Lock_AllComponentOfType<Light>(true, light => light.type == LightType.Disc);

        [MenuItem(HD_Constants.Lock_Light + "/Lock All Reflection Probes", false, HD_Constants.MenuPriorityTwelve + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_ReflectionProbes() => HD_Operations.Lock_AllComponentOfType<ReflectionProbe>(true);

        [MenuItem(HD_Constants.Lock_Light + "/Lock All Light Probe Groups", false, HD_Constants.MenuPriorityTwelve + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_LightProbeGroups() => HD_Operations.Lock_AllComponentOfType<LightProbeGroup>(true);

#if UNITY_6000_5_OR_NEWER
#else
        [MenuItem(HD_Constants.Lock_Light + "/Lock All Light Probe Proxy Volumes", false, HD_Constants.MenuPriorityTwelve + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_LightProbeProxyVolumes() => HD_Operations.Lock_AllComponentOfType<LightProbeProxyVolume>(true);
#endif
        #endregion

        #region Video
        [MenuItem(HD_Constants.Lock_Video + "/Lock All Video Players", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_VideoPlayers() => HD_Operations.Lock_AllComponentOfType<UnityEngine.Video.VideoPlayer>(true);
        #endregion

        #region UI Toolkit
        [MenuItem(HD_Constants.Lock_UIToolkit + "/Lock All UI Documents", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_UIDocuments() => HD_Operations.Lock_AllComponentOfType<UnityEngine.UIElements.UIDocument>(true);

        [MenuItem(HD_Constants.Lock_UIToolkit + "/Lock All Panel Event Handlers", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_PanelEventHandlers() => HD_Operations.Lock_AllComponentOfType<UnityEngine.UIElements.PanelEventHandler>(true);

        [MenuItem(HD_Constants.Lock_UIToolkit + "/Lock All Panel Raycasters", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_PanelRaycasters() => HD_Operations.Lock_AllComponentOfType<UnityEngine.UIElements.PanelRaycaster>(true);
        #endregion

        #region Cameras
        [MenuItem(HD_Constants.Tools_Lock + "/Lock All Cameras", false, HD_Constants.MenuPriorityTen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Cameras() => HD_Operations.Lock_AllComponentOfType<Camera>(true);
        #endregion

        #region UI
        [MenuItem(HD_Constants.Lock_UI + "/Lock All Images", false, HD_Constants.MenuPriorityTen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Images() => HD_Operations.Lock_AllComponentOfType<Image>(true);

        [MenuItem(HD_Constants.Lock_UI + "/Lock All Texts - TextMeshPro", false, HD_Constants.MenuPriorityTen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_TextMeshPro() => HD_Operations.Lock_AllTMPComponentIfAvailable<TMPro.TMP_Text>(true);

        [MenuItem(HD_Constants.Lock_UI + "/Lock All Raw Images", false, HD_Constants.MenuPriorityTen + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_RawImages() => HD_Operations.Lock_AllComponentOfType<RawImage>(true);

        [MenuItem(HD_Constants.Lock_UI + "/Lock All Toggles", false, HD_Constants.MenuPriorityEleven + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Toggles() => HD_Operations.Lock_AllComponentOfType<Toggle>(true);

        [MenuItem(HD_Constants.Lock_UI + "/Lock All Sliders", false, HD_Constants.MenuPriorityEleven + 5)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Sliders() => HD_Operations.Lock_AllComponentOfType<Slider>(true);

        [MenuItem(HD_Constants.Lock_UI + "/Lock All Scrollbars", false, HD_Constants.MenuPriorityEleven + 6)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Scrollbars() => HD_Operations.Lock_AllComponentOfType<Scrollbar>(true);

        [MenuItem(HD_Constants.Lock_UI + "/Lock All Scroll Views", false, HD_Constants.MenuPriorityEleven + 7)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_ScrollViews() => HD_Operations.Lock_AllComponentOfType<ScrollRect>(true);

        [MenuItem(HD_Constants.Lock_UI + "/Lock All Dropdowns - TextMeshPro", false, HD_Constants.MenuPriorityEleven + 8)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_DropdownTextMeshPro() => HD_Operations.Lock_AllTMPComponentIfAvailable<TMPro.TMP_Dropdown>(true);

        [MenuItem(HD_Constants.Lock_UI + "/Lock All Input Fields - TextMeshPro", false, HD_Constants.MenuPriorityEleven + 8)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_InputFieldTextMeshPro() => HD_Operations.Lock_AllTMPComponentIfAvailable<TMPro.TMP_InputField>(true);

        [MenuItem(HD_Constants.Lock_UI + "/Lock All Canvases", false, HD_Constants.MenuPriorityTwelve + 8)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Canvases() => HD_Operations.Lock_AllComponentOfType<Canvas>(true);

        [MenuItem(HD_Constants.Lock_UI + "/Lock All Event Systems", false, HD_Constants.MenuPriorityTwelve + 8)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_EventSystems() => HD_Operations.Lock_AllComponentOfType<UnityEngine.EventSystems.EventSystem>(true);

        [MenuItem(HD_Constants.Lock_UI_Legacy + "/Lock All Texts", false, HD_Constants.MenuPriorityThirteen + 8)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Texts() => HD_Operations.Lock_AllComponentOfType<Text>(true);

        [MenuItem(HD_Constants.Lock_UI_Legacy + "/Lock All Buttons", false, HD_Constants.MenuPriorityThirteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Buttons() => HD_Operations.Lock_AllComponentOfType<Button>(true);

        [MenuItem(HD_Constants.Lock_UI_Legacy + "/Lock All Dropdowns", false, HD_Constants.MenuPriorityThirteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Dropdowns() => HD_Operations.Lock_AllComponentOfType<Dropdown>(true);

        [MenuItem(HD_Constants.Lock_UI_Legacy + "/Lock All Input Fields", false, HD_Constants.MenuPriorityThirteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_InputFields() => HD_Operations.Lock_AllComponentOfType<InputField>(true);

        [MenuItem(HD_Constants.Lock_UI + "/Lock All Masks", false, HD_Constants.MenuPriorityFourteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Masks() => HD_Operations.Lock_AllComponentOfType<Mask>(true);

        [MenuItem(HD_Constants.Lock_UI + "/Lock All Rect Masks 2D", false, HD_Constants.MenuPriorityFourteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_RectMasks2D() => HD_Operations.Lock_AllComponentOfType<RectMask2D>(true);

        [MenuItem(HD_Constants.Lock_UI + "/Lock All Selectables", false, HD_Constants.MenuPriorityFourteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Selectables() => HD_Operations.Lock_AllComponentOfType<Selectable>(true);

        [MenuItem(HD_Constants.Lock_UI + "/Lock All Toggle Groups", false, HD_Constants.MenuPriorityFourteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_ToggleGroups() => HD_Operations.Lock_AllComponentOfType<ToggleGroup>(true);

        [MenuItem(HD_Constants.Lock_UI_Effects + "/Lock All Outlines", false, HD_Constants.MenuPriorityFifteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Outlines() => HD_Operations.Lock_AllComponentOfType<Outline>(true);

        [MenuItem(HD_Constants.Lock_UI_Effects + "/Lock All Positions As UV1", false, HD_Constants.MenuPriorityFifteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_PositionsAsUV1() => HD_Operations.Lock_AllComponentOfType<PositionAsUV1>(true);

        [MenuItem(HD_Constants.Lock_UI_Effects + "/Lock All Shadows", false, HD_Constants.MenuPriorityFifteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Shadows() => HD_Operations.Lock_AllComponentOfType<Shadow>(true);
        #endregion
        #endregion

        #region Unlock
        #region General
        [MenuItem(HD_Constants.Unlock_General + "/Unlock Selected GameObjects", false, HD_Constants.MenuPriorityNine)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_SelectedGameObjects() => HD_Operations.Lock_SelectedGameObjects(false);

        [MenuItem(HD_Constants.Unlock_General + "/Unlock All GameObjects", false, HD_Constants.MenuPriorityTen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_GameObjects() => HD_Operations.Lock_AllGameObjects(false);

        [MenuItem(HD_Constants.Unlock_General + "/Unlock All Parent GameObjects", false, HD_Constants.MenuPriorityTen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_ParentGameObjects() => HD_Operations.Lock_AllParentGameObjects(false);

        [MenuItem(HD_Constants.Unlock_General + "/Unlock All Empty GameObjects", false, HD_Constants.MenuPriorityEleven)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_EmptyGameObjects() => HD_Operations.Lock_AllEmptyGameObjects(false);

        [MenuItem(HD_Constants.Unlock_General + "/Unlock All Active GameObjects", false, HD_Constants.MenuPriorityTwelve)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_ActiveGameObjects() => HD_Operations.Lock_AllActiveGameObjects(false);

        [MenuItem(HD_Constants.Unlock_General + "/Unlock All Inactive GameObjects", false, HD_Constants.MenuPriorityTwelve)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_InactiveGameObjects() => HD_Operations.Lock_AllInactiveGameObjects(false);

        [MenuItem(HD_Constants.Unlock_General + "/Unlock All Folders", false, HD_Constants.MenuPriorityThirteen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Folders() => HD_Operations.Lock_AllFolders(false);
        #endregion

        #region 2D Objects
        [MenuItem(HD_Constants.Unlock_2D + "/Unlock All Sprites", false, HD_Constants.MenuPriorityTen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_UnlockAllSprites() => HD_Operations.Lock_AllComponentOfType<SpriteRenderer>(false);

        [MenuItem(HD_Constants.Unlock_2D + "/Unlock All Sprite Masks", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_UnlockAllSpriteMasks() => HD_Operations.Lock_AllComponentOfType<SpriteMask>(false);

        [MenuItem(HD_Constants.Unlock_2D_Sprites + "/Unlock All 9-Sliced Sprites", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_UnlockAll9SlicedSprites() => HD_Operations.Lock_All2DSpritesByType("9-Sliced", false);

        [MenuItem(HD_Constants.Unlock_2D_Sprites + "/Unlock All Capsule Sprites", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_UnlockAllCapsuleSprites() => HD_Operations.Lock_All2DSpritesByType("Capsule", false);

        [MenuItem(HD_Constants.Unlock_2D_Sprites + "/Unlock All Circle Sprites", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_UnlockAllCircleSprites() => HD_Operations.Lock_All2DSpritesByType("Circle", false);

        [MenuItem(HD_Constants.Unlock_2D_Sprites + "/Unlock All Hexagon Flat-Top Sprites", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_UnlockAllHexagonFlatTopSprites() => HD_Operations.Lock_All2DSpritesByType("Hexagon Flat-Top", false);

        [MenuItem(HD_Constants.Unlock_2D_Sprites + "/Unlock All Hexagon Pointed-Top Sprites", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_UnlockAllHexagonPointedTopSprites() => HD_Operations.Lock_All2DSpritesByType("Hexagon Pointed-Top", false);

        [MenuItem(HD_Constants.Unlock_2D_Sprites + "/Unlock All Isometric Diamond Sprites", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_UnlockAllIsometricDiamondSprites() => HD_Operations.Lock_All2DSpritesByType("Isometric Diamond", false);

        [MenuItem(HD_Constants.Unlock_2D_Sprites + "/Unlock All Square Sprites", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_UnlockAllSquareSprites() => HD_Operations.Lock_All2DSpritesByType("Square", false);

        [MenuItem(HD_Constants.Unlock_2D_Sprites + "/Unlock All Triangle Sprites", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_UnlockAllTriangleSprites() => HD_Operations.Lock_All2DSpritesByType("Triangle", false);

        [MenuItem(HD_Constants.Unlock_2D_Physics + "/Unlock All Dynamic Sprites", false, HD_Constants.MenuPriorityTwelve + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_UnlockAllPhysicsDynamicSprites() => HD_Operations.Lock_AllPhysicsDynamicSprites(false);

        [MenuItem(HD_Constants.Unlock_2D_Physics + "/Unlock All Static Sprites", false, HD_Constants.MenuPriorityTwelve + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_UnlockAllPhysicsStaticSprites() => HD_Operations.Lock_AllPhysicsStaticSprites(false);
        #endregion

        #region 3D Objects
        [MenuItem(HD_Constants.Unlock_3D + "/Unlock All Mesh Filters", false, HD_Constants.MenuPriorityTen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_MeshFilters() => HD_Operations.Lock_AllComponentOfType<MeshFilter>(false);

        [MenuItem(HD_Constants.Unlock_3D + "/Unlock All Mesh Renderers", false, HD_Constants.MenuPriorityTen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock__UnlockAllMeshRenderers() => HD_Operations.Lock_AllComponentOfType<MeshRenderer>(false);

        [MenuItem(HD_Constants.Unlock_3D + "/Unlock All Skinned Mesh Renderer", false, HD_Constants.MenuPriorityTen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_SkinnedMeshRenderers() => HD_Operations.Lock_AllComponentOfType<SkinnedMeshRenderer>(false);

        [MenuItem(HD_Constants.Unlock_3D + "/Unlock All Cubes", false, HD_Constants.MenuPriorityEleven)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_CubesObjects() => HD_Operations.Lock_All3DObjectsByType("Cube", false);

        [MenuItem(HD_Constants.Unlock_3D + "/Unlock All Spheres", false, HD_Constants.MenuPriorityEleven)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_SpheresObjects() => HD_Operations.Lock_All3DObjectsByType("Sphere", false);

        [MenuItem(HD_Constants.Unlock_3D + "/Unlock All Capsules", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_CapsulesObjects() => HD_Operations.Lock_All3DObjectsByType("Capsule", false);

        [MenuItem(HD_Constants.Unlock_3D + "/Unlock All Cylinders", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_CylindersObjects() => HD_Operations.Lock_All3DObjectsByType("Cylinder", false);

        [MenuItem(HD_Constants.Unlock_3D + "/Unlock All Planes", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_PlanesObjects() => HD_Operations.Lock_All3DObjectsByType("Plane", false);

        [MenuItem(HD_Constants.Unlock_3D + "/Unlock All Quads", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_QuadsObjects() => HD_Operations.Lock_All3DObjectsByType("Quad", false);

        [MenuItem(HD_Constants.Unlock_3D + "/Unlock All Texts - TextMeshPro", false, HD_Constants.MenuPriorityTwelve + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_TextMeshProObjects() => HD_Operations.Lock_All3DObjectsByType("TextMeshPro Mesh", false);

        [MenuItem(HD_Constants.Unlock_3D_Legacy + "/Unlock All Text Meshes", false, HD_Constants.MenuPriorityTwelve + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_TextMeshesObjects() => HD_Operations.Lock_AllComponentOfType<TextMesh>(false);

        [MenuItem(HD_Constants.Unlock_3D + "/Unlock All Terrains", false, HD_Constants.MenuPriorityThirteen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_TerrainsObjects() => HD_Operations.Lock_AllComponentOfType<Terrain>(false);

        [MenuItem(HD_Constants.Unlock_3D + "/Unlock All Trees", false, HD_Constants.MenuPriorityThirteen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_TreesObjects() => HD_Operations.Lock_AllComponentOfType<Tree>(false);

        [MenuItem(HD_Constants.Unlock_3D + "/Unlock All Wind Zones", false, HD_Constants.MenuPriorityThirteen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_WindZonesObjects() => HD_Operations.Lock_AllComponentOfType<WindZone>(false);
        #endregion

        #region Audio
        [MenuItem(HD_Constants.Unlock_Audio + "/Unlock All Audio Sources", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_AudioSources() => HD_Operations.Lock_AllComponentOfType<AudioSource>(false);

        [MenuItem(HD_Constants.Unlock_Audio + "/Unlock All Audio Reverb Zones", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_AudioReverbZones() => HD_Operations.Lock_AllComponentOfType<AudioReverbZone>(false);

        [MenuItem(HD_Constants.Unlock_Audio + "/Unlock All Audio Chorus Filters", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_AudioChorusFilters() => HD_Operations.Lock_AllComponentOfType<AudioChorusFilter>(false);

        [MenuItem(HD_Constants.Unlock_Audio + "/Unlock All Audio Distortion Filters", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_AudioDistortionFilters() => HD_Operations.Lock_AllComponentOfType<AudioDistortionFilter>(false);

        [MenuItem(HD_Constants.Unlock_Audio + "/Unlock All Audio Echo Filters", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_AudioEchoFilters() => HD_Operations.Lock_AllComponentOfType<AudioEchoFilter>(false);

        [MenuItem(HD_Constants.Unlock_Audio + "/Unlock All Audio High Pass Filters", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_AudioHighPassFilters() => HD_Operations.Lock_AllComponentOfType<AudioHighPassFilter>(false);

        [MenuItem(HD_Constants.Unlock_Audio + "/Unlock All Audio Listeners", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_AudioListeners() => HD_Operations.Lock_AllComponentOfType<AudioListener>(false);

        [MenuItem(HD_Constants.Unlock_Audio + "/Unlock All Audio Low Pass Filters", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_AudioLowPassFilters() => HD_Operations.Lock_AllComponentOfType<AudioLowPassFilter>(false);

        [MenuItem(HD_Constants.Unlock_Audio + "/Unlock All Audio Reverb Filters", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_AudioReverbFilters() => HD_Operations.Lock_AllComponentOfType<AudioReverbFilter>(false);
        #endregion

        #region Effects
        [MenuItem(HD_Constants.Unlock_Effects + "/Unlock All Particle Systems", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_ParticleSystems() => HD_Operations.Lock_AllComponentOfType<ParticleSystem>(false);

        [MenuItem(HD_Constants.Unlock_Effects + "/Unlock All Particle System Force Fields", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_ParticleSystemForceFields() => HD_Operations.Lock_AllComponentOfType<ParticleSystemForceField>(false);

        [MenuItem(HD_Constants.Unlock_Effects + "/Unlock All Trail Renderers", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_TrailRenderers() => HD_Operations.Lock_AllComponentOfType<TrailRenderer>(false);

        [MenuItem(HD_Constants.Unlock_Effects + "/Unlock All Line Renderers", false, HD_Constants.MenuPriorityTen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_LineRenderers() => HD_Operations.Lock_AllComponentOfType<LineRenderer>(false);

        [MenuItem(HD_Constants.Unlock_Effects + "/Unlock All Halos", false, HD_Constants.MenuPriorityEleven + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Halos() => HD_Operations.Lock_AllHalos(true);

#if UNITY_6000_5_OR_NEWER
#else
        [MenuItem(HD_Constants.Unlock_Effects + "/Unlock All Lens Flares", false, HD_Constants.MenuPriorityEleven + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_LensFlares() => HD_Operations.Lock_AllComponentOfType<LensFlare>(false);

        [MenuItem(HD_Constants.Unlock_Effects + "/Unlock All Projectors", false, HD_Constants.MenuPriorityEleven + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Projectors() => HD_Operations.Lock_AllComponentOfType<Projector>(false);
#endif

        [MenuItem(HD_Constants.Unlock_Effects + "/Unlock All Visual Effects", false, HD_Constants.MenuPriorityEleven + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_VisualEffects() => HD_Operations.Lock_AllComponentOfType<UnityEngine.VFX.VisualEffect>(true);
        #endregion

        #region Lights
        [MenuItem(HD_Constants.Unlock_Light + "/Unlock All Lights", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Lights() => HD_Operations.Lock_AllComponentOfType<Light>(false);

        [MenuItem(HD_Constants.Unlock_Light + "/Unlock All Directional Lights", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_DirectionalLights() => HD_Operations.Lock_AllComponentOfType<Light>(false, light => light.type == LightType.Directional);

        [MenuItem(HD_Constants.Unlock_Light + "/Unlock All Point Lights", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_PointLights() => HD_Operations.Lock_AllComponentOfType<Light>(false, light => light.type == LightType.Point);

        [MenuItem(HD_Constants.Unlock_Light + "/Unlock All Spot Lights", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_SpotLights() => HD_Operations.Lock_AllComponentOfType<Light>(false, light => light.type == LightType.Spot);

        [MenuItem(HD_Constants.Unlock_Light + "/Unlock All Rectangle Area Lights", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_RectangleAreaLights() => HD_Operations.Lock_AllComponentOfType<Light>(false, light => light.type == LightType.Rectangle);

        [MenuItem(HD_Constants.Unlock_Light + "/Unlock All Disc Area Lights", false, HD_Constants.MenuPriorityEleven + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_DiscAreaLights() => HD_Operations.Lock_AllComponentOfType<Light>(false, light => light.type == LightType.Disc);

        [MenuItem(HD_Constants.Unlock_Light + "/Unlock All Reflection Probes", false, HD_Constants.MenuPriorityTwelve + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_ReflectionProbes() => HD_Operations.Lock_AllComponentOfType<ReflectionProbe>(false);

        [MenuItem(HD_Constants.Unlock_Light + "/Unlock All Light Probe Groups", false, HD_Constants.MenuPriorityTwelve + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_LightProbeGroups() => HD_Operations.Lock_AllComponentOfType<LightProbeGroup>(false);

#if UNITY_6000_5_OR_NEWER
#else
        [MenuItem(HD_Constants.Unlock_Light + "/Unlock All Light Probe Proxy Volumes", false, HD_Constants.MenuPriorityTwelve + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_LightProbeProxyVolumes() => HD_Operations.Lock_AllComponentOfType<LightProbeProxyVolume>(false);
#endif
        #endregion

        #region Video
        [MenuItem(HD_Constants.Unlock_Video + "/Unlock All Video Players", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_VideoPlayers() => HD_Operations.Lock_AllComponentOfType<UnityEngine.Video.VideoPlayer>(false);
        #endregion

        #region UI Toolkit
        [MenuItem(HD_Constants.Unlock_UIToolkit + "/Unlock All UI Documents", false, HD_Constants.MenuPriorityTen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_UIDocuments() => HD_Operations.Lock_AllComponentOfType<UnityEngine.UIElements.UIDocument>(false);

        [MenuItem(HD_Constants.Unlock_UIToolkit + "/Unlock All Panel Event Handlers", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_PanelEventHandlers() => HD_Operations.Lock_AllComponentOfType<UnityEngine.UIElements.PanelEventHandler>(false);

        [MenuItem(HD_Constants.Unlock_UIToolkit + "/Unlock All Panel Raycasters", false, HD_Constants.MenuPriorityEleven + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_PanelRaycasters() => HD_Operations.Lock_AllComponentOfType<UnityEngine.UIElements.PanelRaycaster>(false);
        #endregion

        #region Cameras
        [MenuItem(HD_Constants.Tools_Unlock + "/Unlock All Cameras", false, HD_Constants.MenuPriorityTen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_AllCameras() => HD_Operations.Lock_AllComponentOfType<Camera>(false);
        #endregion

        #region UI
        [MenuItem(HD_Constants.Unlock_UI + "/Unlock All Images", false, HD_Constants.MenuPriorityTen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Images() => HD_Operations.Lock_AllComponentOfType<Image>(false);

        [MenuItem(HD_Constants.Unlock_UI + "/Unlock All Texts - TextMeshPro", false, HD_Constants.MenuPriorityTen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_TextMeshPro() => HD_Operations.Lock_AllTMPComponentIfAvailable<TMPro.TMP_Text>(false);

        [MenuItem(HD_Constants.Unlock_UI + "/Unlock All Raw Images", false, HD_Constants.MenuPriorityTen + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_RawImages() => HD_Operations.Lock_AllComponentOfType<RawImage>(false);

        [MenuItem(HD_Constants.Unlock_UI + "/Unlock All Toggles", false, HD_Constants.MenuPriorityEleven + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Toggles() => HD_Operations.Lock_AllComponentOfType<Toggle>(false);

        [MenuItem(HD_Constants.Unlock_UI + "/Unlock All Sliders", false, HD_Constants.MenuPriorityEleven + 5)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Sliders() => HD_Operations.Lock_AllComponentOfType<Slider>(false);

        [MenuItem(HD_Constants.Unlock_UI + "/Unlock All Scrollbars", false, HD_Constants.MenuPriorityEleven + 6)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Scrollbars() => HD_Operations.Lock_AllComponentOfType<Scrollbar>(false);

        [MenuItem(HD_Constants.Unlock_UI + "/Unlock All Scroll Views", false, HD_Constants.MenuPriorityEleven + 7)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_ScrollViews() => HD_Operations.Lock_AllComponentOfType<ScrollRect>(false);

        [MenuItem(HD_Constants.Unlock_UI + "/Unlock All Dropdowns - TextMeshPro", false, HD_Constants.MenuPriorityEleven + 8)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_DropdownTextMeshPro() => HD_Operations.Lock_AllTMPComponentIfAvailable<TMPro.TMP_Dropdown>(false);

        [MenuItem(HD_Constants.Unlock_UI + "/Unlock All Input Fields - TextMeshPro", false, HD_Constants.MenuPriorityEleven + 8)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_InputFieldTextMeshPro() => HD_Operations.Lock_AllTMPComponentIfAvailable<TMPro.TMP_InputField>(false);

        [MenuItem(HD_Constants.Unlock_UI + "/Unlock All Canvases", false, HD_Constants.MenuPriorityTwelve + 8)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Canvases() => HD_Operations.Lock_AllComponentOfType<Canvas>(false);

        [MenuItem(HD_Constants.Unlock_UI + "/Unlock All Event Systems", false, HD_Constants.MenuPriorityTwelve + 8)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_EventSystems() => HD_Operations.Lock_AllComponentOfType<UnityEngine.EventSystems.EventSystem>(false);

        [MenuItem(HD_Constants.Unlock_UI_Legacy + "/Unlock All Texts", false, HD_Constants.MenuPriorityThirteen + 8)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Texts() => HD_Operations.Lock_AllComponentOfType<Text>(false);

        [MenuItem(HD_Constants.Unlock_UI_Legacy + "/Unlock All Buttons", false, HD_Constants.MenuPriorityThirteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Buttons() => HD_Operations.Lock_AllComponentOfType<Button>(false);

        [MenuItem(HD_Constants.Unlock_UI_Legacy + "/Unlock All Dropdowns", false, HD_Constants.MenuPriorityThirteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Dropdowns() => HD_Operations.Lock_AllComponentOfType<Dropdown>(false);

        [MenuItem(HD_Constants.Unlock_UI_Legacy + "/Unlock All Input Fields", false, HD_Constants.MenuPriorityThirteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_InputFields() => HD_Operations.Lock_AllComponentOfType<InputField>(false);

        [MenuItem(HD_Constants.Unlock_UI + "/Unlock All Masks", false, HD_Constants.MenuPriorityFourteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Masks() => HD_Operations.Lock_AllComponentOfType<Mask>(false);

        [MenuItem(HD_Constants.Unlock_UI + "/Unlock All Rect Masks 2D", false, HD_Constants.MenuPriorityFourteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_RectMasks2D() => HD_Operations.Lock_AllComponentOfType<RectMask2D>(false);

        [MenuItem(HD_Constants.Unlock_UI + "/Unlock All Selectables", false, HD_Constants.MenuPriorityFourteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Selectables() => HD_Operations.Lock_AllComponentOfType<Selectable>(false);

        [MenuItem(HD_Constants.Unlock_UI + "/Unlock All Toggle Groups", false, HD_Constants.MenuPriorityFourteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_ToggleGroups() => HD_Operations.Lock_AllComponentOfType<ToggleGroup>(false);

        [MenuItem(HD_Constants.Unlock_UI_Effects + "/Unlock All Outlines", false, HD_Constants.MenuPriorityFifteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Outlines() => HD_Operations.Lock_AllComponentOfType<Outline>(false);

        [MenuItem(HD_Constants.Unlock_UI_Effects + "/Unlock All Positions As UV1", false, HD_Constants.MenuPriorityFifteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_PositionsAsUV1() => HD_Operations.Lock_AllComponentOfType<PositionAsUV1>(false);

        [MenuItem(HD_Constants.Unlock_UI_Effects + "/Unlock All Shadows", false, HD_Constants.MenuPriorityFifteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Shadows() => HD_Operations.Lock_AllComponentOfType<Shadow>(false);
        #endregion
        #endregion

        #region Rename
        [MenuItem(HD_Constants.Tools_Rename + "/Rename Selected GameObjects", false, HD_Constants.MenuPriorityTen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Rename)]
        public static void MenuItem_Rename_SelectedGameObjectst() => HD_Operations.Rename_SelectedGameObjects("rename", false);

        [MenuItem(HD_Constants.Tools_Rename + "/Rename Selected GameObjects with Auto-Indexing", false, HD_Constants.MenuPriorityTen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Rename)]
        public static void MenuItem_Rename_SelectedGameObjectsWithAutoIndex() => HD_Operations.Rename_SelectedGameObjects("rename with automatic indexing", true);

        [MenuItem(HD_Constants.Tools_Rename + "/Open Rename Tool Window", false, HD_Constants.MenuPriorityEleven)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Rename)]
        public static void MenuItem_Rename_OpenWindow() => HD_Operations.Rename_OpenRenameToolWindow();
        #endregion

        #region Select
        #region General
        [MenuItem(HD_Constants.Select_General + "/Select All GameObjects", false, HD_Constants.MenuPriorityTen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_GameObjects() => HD_Operations.Select_AllGameObjects();

        [MenuItem(HD_Constants.Select_General + "/Select All Parent GameObjects", false, HD_Constants.MenuPriorityTen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_ParentGameObjects() => HD_Operations.Select_AllParentGameObjects();

        [MenuItem(HD_Constants.Select_General + "/Select All Empty GameObjects", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_EmptyGameObjects() => HD_Operations.Select_AllEmptyGameObjects();

        [MenuItem(HD_Constants.Select_General + "/Select All Locked GameObjects", false, HD_Constants.MenuPriorityEleven + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_LockedGameObjects() => HD_Operations.Select_AllLockedGameObjects();

        [MenuItem(HD_Constants.Select_General + "/Select All Active GameObjects", false, HD_Constants.MenuPriorityTwelve + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_ActiveGameObjects() => HD_Operations.Select_AllActiveGameObjects();

        [MenuItem(HD_Constants.Select_General + "/Select All Inactive GameObjects", false, HD_Constants.MenuPriorityTwelve + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_InactiveGameObjects() => HD_Operations.Select_AllInactiveGameObjects();

        [MenuItem(HD_Constants.Select_General + "/Select All Folders", false, HD_Constants.MenuPriorityThirteen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Folders() => HD_Operations.Select_AllFolders();

        [MenuItem(HD_Constants.Select_General + "/Select All Separators", false, HD_Constants.MenuPriorityThirteen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Separators() => HD_Operations.Select_AllSeparators();

        [MenuItem(HD_Constants.Select_General + "/Select All GameObjects with Missing Scripts", false, HD_Constants.MenuPriorityThirteen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_GameObjectsWithMissingScripts() => HD_Operations.Select_AllGameObjectsWithMissingScripts();
        #endregion

        #region 2D Objects
        [MenuItem(HD_Constants.Select_2D + "/Select All Sprites", false, HD_Constants.MenuPriorityThirteen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Sprites() => HD_Operations.Select_AllComponentOfType<SpriteRenderer>();

        [MenuItem(HD_Constants.Select_2D + "/Select All Sprite Masks", false, HD_Constants.MenuPriorityThirteen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_SpriteMasks() => HD_Operations.Select_AllComponentOfType<SpriteMask>();

        [MenuItem(HD_Constants.Select_2D_Sprites + "/Select All 9-Sliced Sprites", false, HD_Constants.MenuPriorityFourteen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_9SlicedSprites() => HD_Operations.Select_All2DSpritesByType("9-Sliced");

        [MenuItem(HD_Constants.Select_2D_Sprites + "/Select All Capsule Sprites", false, HD_Constants.MenuPriorityFourteen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_CapsuleSprites() => HD_Operations.Select_All2DSpritesByType("Capsule");

        [MenuItem(HD_Constants.Select_2D_Sprites + "/Select All Circle Sprites", false, HD_Constants.MenuPriorityFourteen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_CircleSprites() => HD_Operations.Select_All2DSpritesByType("Circle");

        [MenuItem(HD_Constants.Select_2D_Sprites + "/Select All Hexagon Flat-Top Sprites", false, HD_Constants.MenuPriorityFourteen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_HexagonFlatTopSprites() => HD_Operations.Select_All2DSpritesByType("Hexagon Flat-Top");

        [MenuItem(HD_Constants.Select_2D_Sprites + "/Select All Hexagon Pointed-Top Sprites", false, HD_Constants.MenuPriorityFourteen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_HexagonPointedTopSprites() => HD_Operations.Select_All2DSpritesByType("Hexagon Pointed-Top");

        [MenuItem(HD_Constants.Select_2D_Sprites + "/Select All Isometric Diamond Sprites", false, HD_Constants.MenuPriorityFourteen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_IsometricDiamondSprites() => HD_Operations.Select_All2DSpritesByType("Isometric Diamond");

        [MenuItem(HD_Constants.Select_2D_Sprites + "/Select All Square Sprites", false, HD_Constants.MenuPriorityFourteen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_SquareSprites() => HD_Operations.Select_All2DSpritesByType("Square");

        [MenuItem(HD_Constants.Select_2D_Sprites + "/Select All Triangle Sprites", false, HD_Constants.MenuPriorityFourteen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_TriangleSprites() => HD_Operations.Select_All2DSpritesByType("Triangle");

        [MenuItem(HD_Constants.Select_2D_Physics + "/Select All Dynamic Sprites", false, HD_Constants.MenuPriorityFourteen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_PhysicsDynamicSprites() => HD_Operations.Select_AllPhysicsDynamicSprites();

        [MenuItem(HD_Constants.Select_2D_Physics + "/Select All Static Sprites", false, HD_Constants.MenuPriorityFourteen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_PhysicsStaticSprites() => HD_Operations.Select_AllPhysicsStaticSprites();
        #endregion

        #region 3D Objects
        [MenuItem(HD_Constants.Select_3D + "/Select All Mesh Filters", false, HD_Constants.MenuPriorityThirteen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_MeshFilters() => HD_Operations.Select_AllComponentOfType<MeshFilter>();

        [MenuItem(HD_Constants.Select_3D + "/Select All Mesh Renderers", false, HD_Constants.MenuPriorityThirteen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_MeshRenderers() => HD_Operations.Select_AllComponentOfType<MeshRenderer>();

        [MenuItem(HD_Constants.Select_3D + "/Select All Skinned Mesh Renderer", false, HD_Constants.MenuPriorityThirteen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_SkinnedMeshRenderers() => HD_Operations.Select_AllComponentOfType<SkinnedMeshRenderer>();

        [MenuItem(HD_Constants.Select_3D + "/Select All Cubes", false, HD_Constants.MenuPriorityFourteen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_CubesObjects() => HD_Operations.Select_All3DObjectsByType("Cube");

        [MenuItem(HD_Constants.Select_3D + "/Select All Spheres", false, HD_Constants.MenuPriorityFourteen + 1)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_SpheresObjects() => HD_Operations.Select_All3DObjectsByType("Sphere");

        [MenuItem(HD_Constants.Select_3D + "/Select All Capsules", false, HD_Constants.MenuPriorityFourteen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_CapsulesObjects() => HD_Operations.Select_All3DObjectsByType("Capsule");

        [MenuItem(HD_Constants.Select_3D + "/Select All Cylinders", false, HD_Constants.MenuPriorityFourteen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_CylindersObjects() => HD_Operations.Select_All3DObjectsByType("Cylinder");

        [MenuItem(HD_Constants.Select_3D + "/Select All Planes", false, HD_Constants.MenuPriorityFourteen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_PlanesObjects() => HD_Operations.Select_All3DObjectsByType("Plane");

        [MenuItem(HD_Constants.Select_3D + "/Select All Quads", false, HD_Constants.MenuPriorityFourteen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_QuadsObjects() => HD_Operations.Select_All3DObjectsByType("Quad");

        [MenuItem(HD_Constants.Select_3D + "/Select All Texts - TextMeshPro", false, HD_Constants.MenuPriorityFifteen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_TextMeshProObjects() => HD_Operations.Select_All3DObjectsByType("TextMeshPro Mesh");

        [MenuItem(HD_Constants.Select_3D_Legacy + "/Select All Text Meshes", false, HD_Constants.MenuPriorityFifteen + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_TextMeshesObjects() => HD_Operations.Select_AllComponentOfType<TextMesh>();

        [MenuItem(HD_Constants.Select_3D + "/Select All Terrains", false, HD_Constants.MenuPrioritySixteen + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_TerrainsObjects() => HD_Operations.Select_AllComponentOfType<Terrain>();

        [MenuItem(HD_Constants.Select_3D + "/Select All Trees", false, HD_Constants.MenuPrioritySixteen + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_TreesObjects() => HD_Operations.Select_AllComponentOfType<Tree>();

        [MenuItem(HD_Constants.Select_3D + "/Select All Wind Zones", false, HD_Constants.MenuPrioritySixteen + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_WindZonesObjects() => HD_Operations.Select_AllComponentOfType<WindZone>();
        #endregion

        #region Audio
        [MenuItem(HD_Constants.Select_Audio + "/Select All Audio Sources", false, HD_Constants.MenuPriorityThirteen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_AudioSources() => HD_Operations.Select_AllComponentOfType<AudioSource>();

        [MenuItem(HD_Constants.Select_Audio + "/Select All Audio Reverb Zones", false, HD_Constants.MenuPriorityThirteen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_AudioReverbZones() => HD_Operations.Select_AllComponentOfType<AudioReverbZone>();

        [MenuItem(HD_Constants.Select_Audio + "/Select All Audio Chorus Filters", false, HD_Constants.MenuPriorityFourteen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_AudioChorusFilters() => HD_Operations.Select_AllComponentOfType<AudioChorusFilter>();

        [MenuItem(HD_Constants.Select_Audio + "/Select All Audio Distortion Filters", false, HD_Constants.MenuPriorityFourteen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_AudioDistortionFilters() => HD_Operations.Select_AllComponentOfType<AudioDistortionFilter>();

        [MenuItem(HD_Constants.Select_Audio + "/Select All Audio Echo Filters", false, HD_Constants.MenuPriorityFourteen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_AudioEchoFilters() => HD_Operations.Select_AllComponentOfType<AudioEchoFilter>();

        [MenuItem(HD_Constants.Select_Audio + "/Select All Audio High Pass Filters", false, HD_Constants.MenuPriorityFourteen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_AudioHighPassFilters() => HD_Operations.Select_AllComponentOfType<AudioHighPassFilter>();

        [MenuItem(HD_Constants.Select_Audio + "/Select All Audio Listeners", false, HD_Constants.MenuPriorityFourteen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_AudioListeners() => HD_Operations.Select_AllComponentOfType<AudioListener>();

        [MenuItem(HD_Constants.Select_Audio + "/Select All Audio Low Pass Filters", false, HD_Constants.MenuPriorityFourteen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_AudioLowPassFilters() => HD_Operations.Select_AllComponentOfType<AudioLowPassFilter>();

        [MenuItem(HD_Constants.Select_Audio + "/Select All Audio Reverb Filters", false, HD_Constants.MenuPriorityFourteen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_AudioReverbFilters() => HD_Operations.Select_AllComponentOfType<AudioReverbFilter>();
        #endregion

        #region Effects
        [MenuItem(HD_Constants.Select_Effects + "/Select All Particle Systems", false, HD_Constants.MenuPriorityThirteen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_ParticleSystems() => HD_Operations.Select_AllComponentOfType<ParticleSystem>();

        [MenuItem(HD_Constants.Select_Effects + "/Select All Particle System Force Fields", false, HD_Constants.MenuPriorityThirteen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_ParticleSystemForceFields() => HD_Operations.Select_AllComponentOfType<ParticleSystemForceField>();

        [MenuItem(HD_Constants.Select_Effects + "/Select All Trail Renderers", false, HD_Constants.MenuPriorityThirteen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_TrailRenderers() => HD_Operations.Select_AllComponentOfType<TrailRenderer>();

        [MenuItem(HD_Constants.Select_Effects + "/Select All Line Renderers", false, HD_Constants.MenuPriorityThirteen + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_LineRenderers() => HD_Operations.Select_AllComponentOfType<LineRenderer>();

        [MenuItem(HD_Constants.Select_Effects + "/Select All Halos", false, HD_Constants.MenuPriorityFourteen + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Halos() => HD_Operations.Select_AllHalos();

#if UNITY_6000_5_OR_NEWER
#else
        [MenuItem(HD_Constants.Select_Effects + "/Select All Lens Flares", false, HD_Constants.MenuPriorityFourteen + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_LensFlares() => HD_Operations.Select_AllComponentOfType<LensFlare>();

        [MenuItem(HD_Constants.Select_Effects + "/Select All Projectors", false, HD_Constants.MenuPriorityFourteen + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_AllProjectors() => HD_Operations.Select_AllComponentOfType<Projector>();
#endif

        [MenuItem(HD_Constants.Select_Effects + "/Select All Visual Effects", false, HD_Constants.MenuPriorityFourteen + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_VisualEffects() => HD_Operations.Select_AllComponentOfType<UnityEngine.VFX.VisualEffect>();
        #endregion

        #region Lights
        [MenuItem(HD_Constants.Select_Light + "/Select All Lights", false, HD_Constants.MenuPriorityThirteen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Lights() => HD_Operations.Select_AllComponentOfType<Light>();

        [MenuItem(HD_Constants.Select_Light + "/Select All Directional Lights", false, HD_Constants.MenuPriorityFourteen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_DirectionalLights() => HD_Operations.Select_AllComponentOfType<Light>(light => light.type == LightType.Directional);

        [MenuItem(HD_Constants.Select_Light + "/Select All Point Lights", false, HD_Constants.MenuPriorityFourteen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_PointLights() => HD_Operations.Select_AllComponentOfType<Light>(light => light.type == LightType.Point);

        [MenuItem(HD_Constants.Select_Light + "/Select All Spot Lights", false, HD_Constants.MenuPriorityFourteen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_SpotLights() => HD_Operations.Select_AllComponentOfType<Light>(light => light.type == LightType.Spot);

        [MenuItem(HD_Constants.Select_Light + "/Select All Rectangle Area Lights", false, HD_Constants.MenuPriorityFourteen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_RectangleAreaLights() => HD_Operations.Select_AllComponentOfType<Light>(light => light.type == LightType.Rectangle);

        [MenuItem(HD_Constants.Select_Light + "/Select All Disc Area Lights", false, HD_Constants.MenuPriorityFourteen + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_DiscAreaLights() => HD_Operations.Select_AllComponentOfType<Light>(light => light.type == LightType.Disc);

        [MenuItem(HD_Constants.Select_Light + "/Select All Reflection Probes", false, HD_Constants.MenuPriorityFifteen + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_ReflectionProbes() => HD_Operations.Select_AllComponentOfType<ReflectionProbe>();

        [MenuItem(HD_Constants.Select_Light + "/Select All Light Probe Groups", false, HD_Constants.MenuPriorityFifteen + 5)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_LightProbeGroups() => HD_Operations.Select_AllComponentOfType<LightProbeGroup>();

#if UNITY_6000_5_OR_NEWER
#else
        [MenuItem(HD_Constants.Select_Light + "/Select All Light Probe Proxy Volumes", false, HD_Constants.MenuPriorityFifteen + 5)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_LightProbeProxyVolumes() => HD_Operations.Select_AllComponentOfType<LightProbeProxyVolume>();
#endif
        #endregion

        #region Video
        [MenuItem(HD_Constants.Select_Video + "/Select All Video Players", false, HD_Constants.MenuPriorityThirteen + 2)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_VideoPlayers() => HD_Operations.Select_AllComponentOfType<UnityEngine.Video.VideoPlayer>();
        #endregion

        #region UI Toolkit
        [MenuItem(HD_Constants.Select_UIToolkit + "/Select All UI Documents", false, HD_Constants.MenuPriorityThirteen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_UIDocuments() => HD_Operations.Select_AllComponentOfType<UnityEngine.UIElements.UIDocument>();

        [MenuItem(HD_Constants.Select_UIToolkit + "/Select All Panel Event Handlers", false, HD_Constants.MenuPriorityFourteen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_PanelEventHandlers() => HD_Operations.Select_AllComponentOfType<UnityEngine.UIElements.PanelEventHandler>();

        [MenuItem(HD_Constants.Select_UIToolkit + "/Select All Panel Raycasters", false, HD_Constants.MenuPriorityFourteen + 3)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_PanelRaycasters() => HD_Operations.Select_AllComponentOfType<UnityEngine.UIElements.PanelRaycaster>();
        #endregion

        #region Cameras
        [MenuItem(HD_Constants.Tools_Select + "/Select All Cameras", false, HD_Constants.MenuPriorityThirteen + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Cameras() => HD_Operations.Select_AllComponentOfType<Camera>();
        #endregion

        #region UI
        [MenuItem(HD_Constants.Select_UI + "/Select All Images", false, HD_Constants.MenuPriorityThirteen + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Images() => HD_Operations.Select_AllComponentOfType<Image>();

        [MenuItem(HD_Constants.Select_UI + "/Select All Texts - TextMeshPro", false, HD_Constants.MenuPriorityThirteen + 4)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_TextMeshPro() => HD_Operations.Select_AllTMPComponentIfAvailable<TMPro.TMP_Text>();

        [MenuItem(HD_Constants.Select_UI + "/Select All Raw Images", false, HD_Constants.MenuPriorityThirteen + 5)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_RawImages() => HD_Operations.Select_AllComponentOfType<RawImage>();

        [MenuItem(HD_Constants.Select_UI + "/Select All Toggles", false, HD_Constants.MenuPriorityFourteen + 5)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Toggles() => HD_Operations.Select_AllComponentOfType<Toggle>();

        [MenuItem(HD_Constants.Select_UI + "/Select All Sliders", false, HD_Constants.MenuPriorityFourteen + 6)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Sliders() => HD_Operations.Select_AllComponentOfType<Slider>();

        [MenuItem(HD_Constants.Select_UI + "/Select All Scrollbars", false, HD_Constants.MenuPriorityFourteen + 7)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Scrollbars() => HD_Operations.Select_AllComponentOfType<Scrollbar>();

        [MenuItem(HD_Constants.Select_UI + "/Select All Scroll Views", false, HD_Constants.MenuPriorityFourteen + 8)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_ScrollViews() => HD_Operations.Select_AllComponentOfType<ScrollRect>();

        [MenuItem(HD_Constants.Select_UI + "/Select All Dropdowns - TextMeshPro", false, HD_Constants.MenuPriorityFourteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_DropdownTextMeshPro() => HD_Operations.Select_AllTMPComponentIfAvailable<TMPro.TMP_Dropdown>();

        [MenuItem(HD_Constants.Select_UI + "/Select All Input Fields - TextMeshPro", false, HD_Constants.MenuPriorityFourteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_InputFieldTextMeshPro() => HD_Operations.Select_AllTMPComponentIfAvailable<TMPro.TMP_InputField>();

        [MenuItem(HD_Constants.Select_UI + "/Select All Canvases", false, HD_Constants.MenuPriorityFifteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Canvases() => HD_Operations.Select_AllComponentOfType<Canvas>();

        [MenuItem(HD_Constants.Select_UI + "/Select All Event Systems", false, HD_Constants.MenuPriorityFifteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_EventSystems() => HD_Operations.Select_AllComponentOfType<UnityEngine.EventSystems.EventSystem>();

        [MenuItem(HD_Constants.Select_UI_Legacy + "/Select All Texts", false, HD_Constants.MenuPrioritySixteen + 9)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Texts() => HD_Operations.Select_AllComponentOfType<Text>();

        [MenuItem(HD_Constants.Select_UI_Legacy + "/Select All Buttons", false, HD_Constants.MenuPrioritySixteen + 10)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Buttons() => HD_Operations.Select_AllComponentOfType<Button>();

        [MenuItem(HD_Constants.Select_UI_Legacy + "/Select All Dropdowns", false, HD_Constants.MenuPrioritySixteen + 10)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Dropdowns() => HD_Operations.Select_AllComponentOfType<Dropdown>();

        [MenuItem(HD_Constants.Select_UI_Legacy + "/Select All Input Fields", false, HD_Constants.MenuPrioritySixteen + 10)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_InputFields() => HD_Operations.Select_AllComponentOfType<InputField>();

        [MenuItem(HD_Constants.Select_UI + "/Select All Masks", false, HD_Constants.MenuPrioritySeventeen + 10)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Masks() => HD_Operations.Select_AllComponentOfType<Mask>();

        [MenuItem(HD_Constants.Select_UI + "/Select All Rect Masks 2D", false, HD_Constants.MenuPrioritySeventeen + 10)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_RectMasks2D() => HD_Operations.Select_AllComponentOfType<RectMask2D>();

        [MenuItem(HD_Constants.Select_UI + "/Select All Selectables", false, HD_Constants.MenuPrioritySeventeen + 10)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Selectables() => HD_Operations.Select_AllComponentOfType<Selectable>();

        [MenuItem(HD_Constants.Select_UI + "/Select All Toggle Groups", false, HD_Constants.MenuPrioritySeventeen + 10)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_ToggleGroups() => HD_Operations.Select_AllComponentOfType<ToggleGroup>();

        [MenuItem(HD_Constants.Select_UI_Effects + "/Select All Outlines", false, HD_Constants.MenuPriorityEigtheen + 10)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Outlines() => HD_Operations.Select_AllComponentOfType<Outline>();

        [MenuItem(HD_Constants.Select_UI_Effects + "/Select All Positions As UV1", false, HD_Constants.MenuPriorityEigtheen + 10)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_PositionsAsUV1() => HD_Operations.Select_AllComponentOfType<PositionAsUV1>();

        [MenuItem(HD_Constants.Select_UI_Effects + "/Select All Shadows", false, HD_Constants.MenuPriorityEigtheen + 10)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Shadows() => HD_Operations.Select_AllComponentOfType<Shadow>();
        #endregion
        #endregion

        #region Sort
        [MenuItem(HD_Constants.Tools_Sort + "/Sort Alphabetically Ascending", false, HD_Constants.MenuPriorityTen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_AlphabeticallyAscending() => HD_Operations.Sort_GameObjectChildren(HD_Operations.AlphanumericComparison, "sort its children alphabetically ascending");

        [MenuItem(HD_Constants.Tools_Sort + "/Sort Alphabetically Descending", false, HD_Constants.MenuPriorityTen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_AlphabeticallyDescending() => HD_Operations.Sort_GameObjectChildren((a, b) => -HD_Operations.AlphanumericComparison(a, b), "sort its children alphabetically descending");

        [MenuItem(HD_Constants.Tools_Sort + "/Sort Components Amount Ascending", false, HD_Constants.MenuPriorityEleven)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_ComponentsAmountAscending() => HD_Operations.Sort_GameObjectChildren((a, b) => a.GetComponents<Component>().Length.CompareTo(b.GetComponents<Component>().Length), "sort its children by components amount ascending");

        [MenuItem(HD_Constants.Tools_Sort + "/Sort Components Amount Descending", false, HD_Constants.MenuPriorityEleven)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_ComponentsAmountDescending() => HD_Operations.Sort_GameObjectChildren((a, b) => b.GetComponents<Component>().Length.CompareTo(a.GetComponents<Component>().Length), "sort its children by components amount descending");

        [MenuItem(HD_Constants.Tools_Sort + "/Sort Length Ascending", false, HD_Constants.MenuPriorityTwelve)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_LengthAscending() => HD_Operations.Sort_GameObjectChildren((a, b) => a.name.Length.CompareTo(b.name.Length), "sort its children by length ascending");

        [MenuItem(HD_Constants.Tools_Sort + "/Sort Length Descending", false, HD_Constants.MenuPriorityTwelve)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_LengthDescending() => HD_Operations.Sort_GameObjectChildren((a, b) => b.name.Length.CompareTo(a.name.Length), "sort its children by length descending");

        [MenuItem(HD_Constants.Tools_Sort + "/Sort Tag Alphabetically Ascending", false, HD_Constants.MenuPriorityThirteen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_TagAlphabeticallyAscending() => HD_Operations.Sort_GameObjectChildrenByTag(true, "sort its children by tag ascending");

        [MenuItem(HD_Constants.Tools_Sort + "/Sort Tag Alphabetically Descending", false, HD_Constants.MenuPriorityThirteen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_TagAlphabeticallyDescending() => HD_Operations.Sort_GameObjectChildrenByTag(false, "sort its children by tag descending");

        [MenuItem(HD_Constants.Tools_Sort + "/Sort Tag List Order Ascending", false, HD_Constants.MenuPriorityFourteen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_TagListOrderAscending() => HD_Operations.Sort_GameObjectChildrenByTagListOrder(true, "sort its children by tag list order ascending");

        [MenuItem(HD_Constants.Tools_Sort + "/Sort Tag List Order Descending", false, HD_Constants.MenuPriorityFourteen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_TagListOrderDescending() => HD_Operations.Sort_GameObjectChildrenByTagListOrder(false, "sort its children by tag list order descending");

        [MenuItem(HD_Constants.Tools_Sort + "/Sort Layer Alphabetically Ascending", false, HD_Constants.MenuPriorityFifteen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_LayerAlphabeticallyAscending() => HD_Operations.Sort_GameObjectChildrenByLayer(true, "sort its children by layer alphabetically ascending");

        [MenuItem(HD_Constants.Tools_Sort + "/Sort Layer Alphabetically Descending", false, HD_Constants.MenuPriorityFifteen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_LayerAlphabeticallyDescending() => HD_Operations.Sort_GameObjectChildrenByLayer(false, "sort its children by layer alphabetically descending");

        [MenuItem(HD_Constants.Tools_Sort + "/Sort Layer List Order Ascending", false, HD_Constants.MenuPrioritySixteen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_LayerListOrderAscending() => HD_Operations.Sort_GameObjectChildrenByLayerListOrder(true, "sort its children by layer list order ascending");

        [MenuItem(HD_Constants.Tools_Sort + "/Sort Layer List Order Descending", false, HD_Constants.MenuPrioritySixteen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_LayerListOrderDescending() => HD_Operations.Sort_GameObjectChildrenByLayerListOrder(false, "sort its children by layer list order descending");

        [MenuItem(HD_Constants.Tools_Sort + "/Sort Randomly", false, HD_Constants.MenuPrioritySeventeen)]
        [HD_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_Randomly() => HD_Operations.Sort_GameObjectChildrenRandomly("sort its children randomly");
        #endregion
        #endregion

        #region Refresh
        [MenuItem(HD_Constants.GroupRefresh + "/Refresh All GameObjects' Data", false, HD_Constants.MenuPriorityNineteen)]
        public static void RefreshAllGameObjectsData() => HD_Operations.RefreshAllGameObjectsData();

        [MenuItem(HD_Constants.GroupRefresh + "/Refresh Selected GameObject's Data", false, HD_Constants.MenuPriorityNineteen)]
        public static void RefreshSelectedGameObjectsData() => HD_Operations.RefreshSelectedGameObjectsData();

        [MenuItem(HD_Constants.GroupRefresh + "/Refresh Selected Main Icon", false, HD_Constants.MenuPriorityTwenty)]
        public static void RefreshSelectedMainIcon() => HD_Operations.RefreshMainIconForSelectedGameObject();

        [MenuItem(HD_Constants.GroupRefresh + "/Refresh Selected Component Icons", false, HD_Constants.MenuPriorityTwenty + 1)]
        public static void RefreshSelectedComponentIcons() => HD_Operations.RefreshComponentIconsForSelectedGameObjects();

        [MenuItem(HD_Constants.GroupRefresh + "/Refresh Selected Hierarchy Tree Icon", false, HD_Constants.MenuPriorityTwenty + 1)]
        public static void RefreshSelectedHierarchyTreeIcon() => HD_Operations.RefreshHierarchyTreeIconForSelectedGameObjects();

        [MenuItem(HD_Constants.GroupRefresh + "/Refresh Selected Tag", false, HD_Constants.MenuPriorityTwenty + 1)]
        public static void RefreshSelectedTag() => HD_Operations.RefreshTagForSelectedGameObjects();

        [MenuItem(HD_Constants.GroupRefresh + "/Refresh Selected Layer", false, HD_Constants.MenuPriorityTwenty + 2)]
        public static void RefreshSelectedLayer() => HD_Operations.RefreshLayerForSelectedGameObjects();
        #endregion

        #region General
        [MenuItem(HD_Constants.MenuBase + "/Expand All", false, HD_Constants.MenuPriorityTwentyOne + 2)]
        public static void GeneralExpandAll() => HD_Operations.ExpandAllGameObjects();

        [MenuItem(HD_Constants.MenuBase + "/Collapse All", false, HD_Constants.MenuPriorityTwentyOne + 3)]
        public static void GeneralCollapseAll() => HD_Operations.CollapseAllGameObjects();
        #endregion
    }
}
#endif