using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR

using UnityEditor;

#if PVR_CCK_AVATARS

using PVR.CCK.Avatars;

namespace Voy.PVRScripts.PVRAvatarMenuEditor
{
    public class PVRAvatarMenuEditor : EditorWindow
{
        const string VERSION = "1.0.0";
        const string CREDIT = "PoligonVR Avatar Menu Editor by VoyVivika";
        const string GITLINK = "(InsertLinkHere)";

        const float maxWidthMagicNumber = (200 * 3) + 90 + 8;

        const string upArrow = "↑";
        const string downArrow = "↓";

        PVR_AvatarParameters pvrParams;
        PVR_AvatarMenu pvrMenu;

        int? deleteIdx = null;
        int? upIdx = null;
        int? downIdx = null;

        Vector2 ScrollView;

        string[] paramNames = null;

        GUILayoutOption[] layoutDefault =
        { 
            GUILayout.ExpandWidth(false), 
            GUILayout.MaxWidth(200) 
        };
        GUILayoutOption[] layoutTypes = 
        {
            GUILayout.ExpandWidth(false),
            GUILayout.MaxWidth(90) 
        };
        GUILayoutOption[] layoutGroup =
        {
            GUILayout.ExpandWidth(false),
            GUILayout.MaxWidth(maxWidthMagicNumber)
        };

        [MenuItem("Voy/PVR Avatar Menu Editor")]
        public static void ShowUI()
        {
            EditorWindow wnd = GetWindow<PVRAvatarMenuEditor>();
            wnd.titleContent = new GUIContent("Voy's PVR Avatar Menu Editor");
        }


        public void OnGUI()
        {
            EditorGUILayout.HelpBox(CREDIT + "\n" + VERSION + " " + GITLINK, MessageType.Info);
            EditorGUILayout.Space();
            pvrMenu = (PVR_AvatarMenu)EditorGUILayout.ObjectField(GUIContent.none, pvrMenu, typeof(PVR_AvatarMenu), false);
            EditorGUILayout.Space();
            pvrParams = (PVR_AvatarParameters)EditorGUILayout.ObjectField(GUIContent.none, pvrParams, typeof(PVR_AvatarParameters), false);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (pvrParams != null)
            {
                if(paramNames == null)
                {
                    paramNames = getParameterNames();
                }
            }

            if (pvrMenu != null)
            {
                if (upIdx != null)
                {
                    int oldIdx = (int)upIdx;
                    int newIdx = oldIdx - 1;

                    moveItem(oldIdx, newIdx);

                    upIdx = null;
                }

                if (downIdx != null)
                {
                    int oldIdx = (int)downIdx;
                    int newIdx = oldIdx + 1;

                    moveItem(oldIdx, newIdx);

                    downIdx = null;
                }

                if (deleteIdx != null)
                {
                    pvrMenu.items.RemoveAt((int)deleteIdx);
                    deleteIdx = null;
                }


                ScrollView = EditorGUILayout.BeginScrollView(ScrollView);
                for(int idx = 0; idx < pvrMenu.items.Count; idx++)
                {
                    EditorGUILayout.BeginHorizontal(layoutGroup);

                    pvrMenu.items[idx].name = EditorGUILayout.TextField(pvrMenu.items[idx].name, layoutDefault);

                    pvrMenu.items[idx].icon = (Texture2D)EditorGUILayout.ObjectField(pvrMenu.items[idx].icon, typeof(Texture2D), layoutDefault);

                    pvrMenu.items[idx].type = (PVR_AvatarMenu.Item.Type)EditorGUILayout.EnumPopup(pvrMenu.items[idx].type, layoutTypes);

                    if (pvrMenu.items[idx].type == PVR_AvatarMenu.Item.Type.SubMenu)
                    {
                        pvrMenu.items[idx].subMenu = (PVR_AvatarMenu)EditorGUILayout.ObjectField(pvrMenu.items[idx].subMenu, typeof(PVR_AvatarMenu), layoutDefault);
                    }
                    else
                    {
                        DrawParameters(idx);
                    }

                    DrawCommonControls(idx);

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal(layoutGroup);

                    switch (pvrMenu.items[idx].type)
                    {
                        case PVR_AvatarMenu.Item.Type.Slider:
                            DrawSliderGUI(idx);
                            break;
                        case PVR_AvatarMenu.Item.Type.Button:
                            DrawButtonToggleGUI(idx);
                            break;
                        case PVR_AvatarMenu.Item.Type.Toggle:
                            DrawButtonToggleGUI(idx);
                            break;
                        default:
                            break;
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                }
                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space();
                if (GUILayout.Button("Create/Add", layoutGroup))
                {
                    PVR_AvatarMenu.Item item = new PVR_AvatarMenu.Item();

                    item.icon = pvrMenu.items[pvrMenu.items.Count - 1].icon;
                    item.maxValue = pvrMenu.items[pvrMenu.items.Count - 1].maxValue;
                    item.minValue = pvrMenu.items[pvrMenu.items.Count - 1].minValue;
                    item.name = pvrMenu.items[pvrMenu.items.Count - 1].name;
                    item.parameter = pvrMenu.items[pvrMenu.items.Count - 1].parameter;
                    item.subMenu = pvrMenu.items[pvrMenu.items.Count - 1].subMenu;
                    item.tooltip = pvrMenu.items[pvrMenu.items.Count - 1].tooltip;
                    item.type = pvrMenu.items[pvrMenu.items.Count - 1].type;
                    item.value = pvrMenu.items[pvrMenu.items.Count - 1].value;

                    pvrMenu.items.Add(item);
                };
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }

            int? findParameter(string parameter)
            {
                if (pvrParams != null)
                {
                    for (int i = 0; i < pvrParams.parameters.Length; i++)
                    {
                        if (pvrParams.parameters[i].parameterName == parameter) return i;
                    }
                }
                return null;
            }

            PVR_AvatarParameters.Parameter.ParameterType? getParameterType(string parameter)
            {
                int? paramIdx = findParameter(parameter);
                if (paramIdx != null)
                {
                    return pvrParams.parameters[(int)paramIdx].parameterType;
                }
                return null;
            }

            string[] getParameterNames()
            {
                if (pvrParams == null) return null;
                List<string> parameters = new List<string>();
                foreach (PVR_AvatarParameters.Parameter param in pvrParams.parameters)
                {
                    parameters.Add(param.parameterName);
                }
                return parameters.ToArray();
            }

            void refreshData()
            {
                paramNames = getParameterNames();
            }

            void moveItem(int oldIdx, int newIdx)
            {
                if (newIdx >= pvrMenu.items.Count) return;
                PVR_AvatarMenu.Item removedItem = pvrMenu.items[oldIdx];
                pvrMenu.items.RemoveAt(oldIdx);
                pvrMenu.items.Insert(newIdx, removedItem);
            }

            void DrawParameters(int idx)
            {
                bool ParameterExists = findParameter(pvrMenu.items[idx].parameter) != null;

                if (pvrParams != null && pvrMenu.items[idx].parameter == "")
                {
                    pvrMenu.items[idx].parameter = paramNames[0];
                }

                if (pvrParams != null && paramNames != null && ParameterExists)
                {
                    pvrMenu.items[idx].parameter = paramNames[EditorGUILayout.Popup((int)findParameter(pvrMenu.items[idx].parameter), paramNames, layoutDefault)];
                }
                else
                {
                    pvrMenu.items[idx].parameter = EditorGUILayout.TextField(pvrMenu.items[idx].parameter, layoutDefault);
                }
            }

            void DrawButtonToggleGUI(int idx)
            {
                PVR_AvatarParameters.Parameter.ParameterType? type = getParameterType(pvrMenu.items[idx].parameter);

                if (type != null && type == PVR_AvatarParameters.Parameter.ParameterType.Bool) return;

                EditorGUILayout.LabelField("Value:", GUILayout.MaxWidth(64));

                if (type != null && type == PVR_AvatarParameters.Parameter.ParameterType.Int)
                {
                    pvrMenu.items[idx].value = (float)EditorGUILayout.IntField((int)pvrMenu.items[idx].value);

                    if (pvrMenu.items[idx].value > 255) pvrMenu.items[idx].value = 255;
                    if (pvrMenu.items[idx].value < 0) pvrMenu.items[idx].value = 0;
                }
                else
                {
                    pvrMenu.items[idx].value = EditorGUILayout.FloatField(pvrMenu.items[idx].value);

                    if (type != null && type == PVR_AvatarParameters.Parameter.ParameterType.Float)
                    {
                        if (pvrMenu.items[idx].value > 1f) pvrMenu.items[idx].value = 1;
                        if (pvrMenu.items[idx].value < -1f) pvrMenu.items[idx].value = -1;
                    }
                }
            }

            void DrawSliderGUI(int idx)
            {
                PVR_AvatarParameters.Parameter.ParameterType? type = getParameterType(pvrMenu.items[idx].parameter);
                if (type == null) return;

                switch(type)
                {
                    case PVR_AvatarParameters.Parameter.ParameterType.Float:
                        float StringMinVal = (float)((int)(pvrMenu.items[idx].minValue * 100) * 0.01f);
                        float StringMaxVal = (float)((int)(pvrMenu.items[idx].maxValue * 100) * 0.01f);
                        EditorGUILayout.LabelField("Range of " + StringMinVal.ToString() + " to " + StringMaxVal.ToString(), GUILayout.MaxWidth(140));
                        EditorGUILayout.MinMaxSlider(ref pvrMenu.items[idx].minValue, ref pvrMenu.items[idx].maxValue, -1, 1);
                        break;
                    case PVR_AvatarParameters.Parameter.ParameterType.Int:
                        EditorGUILayout.LabelField("Minimum:", GUILayout.MaxWidth(64));
                        pvrMenu.items[idx].minValue = (float)EditorGUILayout.IntField((int)pvrMenu.items[idx].minValue);
                        EditorGUILayout.LabelField("Maximum:", GUILayout.MaxWidth(64));
                        pvrMenu.items[idx].maxValue = (float)EditorGUILayout.IntField((int)pvrMenu.items[idx].maxValue);

                        if (pvrMenu.items[idx].minValue < 0) pvrMenu.items[idx].minValue = 0;
                        if (pvrMenu.items[idx].maxValue > 255) pvrMenu.items[idx].maxValue = 255;
                        break;
                    case PVR_AvatarParameters.Parameter.ParameterType.Bool:
                        // I have no idea if sliders technically work on booleans, and I don't care to check
                        // I assume they do and just round to true or false, if they don't then this shouldn't have an in-game effect
                        pvrMenu.items[idx].minValue = 0;
                        pvrMenu.items[idx].maxValue = 1;

                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginVertical(layoutGroup);

                        EditorGUILayout.HelpBox("This Parameter is a Bool but this Menu Option is a Slider. You may want to use a Toggle instead.", MessageType.Warning);
                        if (GUILayout.Button("Change to Toggle")) pvrMenu.items[idx].type = PVR_AvatarMenu.Item.Type.Toggle;

                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginHorizontal(layoutGroup);
                        break;
                }
            }

            void DrawCommonControls(int idx)
            {
                if (GUILayout.Button(upArrow)) upIdx = idx;
                if (GUILayout.Button(downArrow)) downIdx = idx;
                if (GUILayout.Button("Delete")) deleteIdx = idx;
            }
    }
    }
}

#endif

#endif