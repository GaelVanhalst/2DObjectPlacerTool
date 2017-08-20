/*
 * Copyright (c) 2017 Gaël Vanhalst
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 *    1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 
 *    2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 
 *    3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System.Collections.Generic;
using Assets.Tools._2DObjectPlacer.Editor;
using UnityEditor;
using UnityEngine;

namespace Assets.Tools._2DObjectPlacer
{
    class ObjectPlacerGUI
    {
        private const string LightLayout2DPlacerToolKey = "LightLayout2DPlacerTool";
        private readonly ObjectPlacer _objectPlacer;
        private Vector2 _scrollPos;

        private bool _drawLight = false;

        private bool DrawLight
        {
            get { return _drawLight; }
            set
            {
                if (_drawLight != value)
                {
                    _drawLight = value;
                    EditorPrefs.SetBool(LightLayout2DPlacerToolKey, _drawLight);
                }
            }
        }

        private List<ObjectPlacerPrefabSetReordableList> _lists = new List<ObjectPlacerPrefabSetReordableList>();

        public ObjectPlacerGUI(ObjectPlacer objectPlacer)
        {
            _objectPlacer = objectPlacer;

            _lists.Clear();
            List<ObjectPlacerPrefabSet> prefabs = _objectPlacer.Prefabs;
            foreach (var prefabSet in prefabs)
            {
                _lists.Add(new ObjectPlacerPrefabSetReordableList(prefabSet));
            }

            _drawLight = EditorPrefs.GetBool(LightLayout2DPlacerToolKey, false);
        }

        public void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);


            _objectPlacer.PlaceMode = EditorGUILayout.ToggleLeft("Place mode", _objectPlacer.PlaceMode);
            EditorGUILayout.Space();
            GameObjectGUI();

            EditorGUILayout.Space();

            SettingsGUI();
            EditorGUILayout.EndScrollView();
        }
        
        private void GameObjectGUI()
        {
            EditorGUILayout.LabelField("Prefabs: ",EditorStyles.boldLabel);

            DrawLight = EditorGUILayout.Toggle("Light layout: ", DrawLight);

            ObjectPlacerPrefabSet newSet = EditorGUILayout.ObjectField("Add set :", null, typeof(ObjectPlacerPrefabSet), false) as ObjectPlacerPrefabSet;

            EditorGUILayout.Space();
            List<ObjectPlacerPrefabSet> prefabSets = _objectPlacer.Prefabs;
            if (newSet != null && !prefabSets.Contains(newSet))
            {
                prefabSets.Add(newSet);
                _lists.Add(new ObjectPlacerPrefabSetReordableList(newSet));
            }

            ObjectPlacerPrefabSet.ObjectPlacePrefab selectedPrefab = _objectPlacer.SelectedPrefab;
            if (DrawLight)
            {
                selectedPrefab= DrawLightLayout(prefabSets, selectedPrefab);
            }
            else
            {
                selectedPrefab = HeavyLayout(prefabSets, selectedPrefab);
            }

            
            //Check to make sure it is still in the list
            if (selectedPrefab != null)
            {
                if (!IsPrefabInSets(selectedPrefab, prefabSets))
                {
                    selectedPrefab = null;
                }
            }

            _objectPlacer.SelectedPrefab = selectedPrefab;
        }

        private bool IsPrefabInSets(ObjectPlacerPrefabSet.ObjectPlacePrefab prefab,
            List<ObjectPlacerPrefabSet> prefabSets)
        {
            foreach (var prefabSet in prefabSets)
            {
                foreach (var prefabInSet in prefabSet.Prefabs)
                {
                    if (prefab == prefabInSet)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        //Heavy layout has a preview of the prefab
        private ObjectPlacerPrefabSet.ObjectPlacePrefab HeavyLayout(List<ObjectPlacerPrefabSet> prefabSets, ObjectPlacerPrefabSet.ObjectPlacePrefab selectedPrefab)
        {
            for (int i = 0; i < prefabSets.Count;)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(string.Format("Set: {0}",prefabSets[i].name));
                if (GUILayout.Button("Remove"))
                {
                    _lists.RemoveAt(i);
                    prefabSets.RemoveAt(i);
                    continue;
                }
                EditorGUILayout.EndHorizontal();
                _lists[i].Draw(ref selectedPrefab);
                i++;
            }

            return selectedPrefab;
        }

        //Light layout allows for more objects being visible in the window
        private ObjectPlacerPrefabSet.ObjectPlacePrefab DrawLightLayout(List<ObjectPlacerPrefabSet> prefabSets, ObjectPlacerPrefabSet.ObjectPlacePrefab selectedPrefab)
        {
            for (int i = 0; i < prefabSets.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(string.Format("Set: {0}", prefabSets[i].name));
                if (GUILayout.Button("Remove"))
                {
                    _lists.RemoveAt(i);
                    prefabSets.RemoveAt(i);
                    continue;
                }
                EditorGUILayout.EndHorizontal();

                var set = prefabSets[i];
                for (int j = 0; j < set.Prefabs.Count; j++)
                {
                    string name = "None";
                    if (set.Prefabs[j].Prefab != null)
                    {
                        name = set.Prefabs[j].Prefab.name;
                    }

                    if (EditorGUILayout.ToggleLeft(name, set.Prefabs[j] == selectedPrefab))
                    {
                        selectedPrefab = set.Prefabs[j];
                    }
                }
            }

            return selectedPrefab;
        }

        private void SettingsGUI()
        {
            EditorGUILayout.LabelField("Settings: ", EditorStyles.boldLabel);
            _objectPlacer.CurrentPrefabSelectionMode = (ObjectPlacer.PrefabSelectionMode)EditorGUILayout.EnumPopup("Prefab selection mode: ", _objectPlacer.CurrentPrefabSelectionMode);
            _objectPlacer.ParentGameObject = EditorGUILayout.ObjectField("Parent gameobject: ",_objectPlacer.ParentGameObject, typeof(GameObject), true) as GameObject;

            _objectPlacer.OverwriteLayer = EditorGUILayout.Toggle("Overwrite layer", _objectPlacer.OverwriteLayer);
            if (_objectPlacer.OverwriteLayer)
            {
                RenderSortingLayers(ref _objectPlacer.Layer);
                _objectPlacer.OrderNumber = EditorGUILayout.IntField("Sorting order", _objectPlacer.OrderNumber);
            }
        }

        private void RenderSortingLayers(ref int currentLayer)
        {
            string[] sortingLayersString = GetSortingLayersAsString();
            SortingLayer[] layers = SortingLayer.layers;

            int layerId = currentLayer;

            int layerIndex = 0;

            for (layerIndex = 0; layerIndex < layers.Length; layerIndex++)
            {
                if (layerId == layers[layerIndex].id)
                {
                    break;
                }
            }

            int newLayerIndex = EditorGUILayout.Popup("Sorting layer", layerIndex, sortingLayersString);
            if (newLayerIndex != layerIndex)
            {
                currentLayer = layers[newLayerIndex].id;
            }
        }

        private string[] GetSortingLayersAsString()
        {
            SortingLayer[] layers = SortingLayer.layers;
            string[] layerNames = new string[layers.Length];

            for (int i = 0; i < layers.Length; i++)
            {
                layerNames[i] = layers[i].name;
            }
            return layerNames;
        }
    }
}
