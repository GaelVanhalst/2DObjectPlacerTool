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

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Assets.Tools._2DObjectPlacer
{
    public class ObjectPlacer : EditorWindow
    {
        private ObjectPlacerInput _objectPlacerInput = null;
        private ObjectPlacerGUI _objectPlacerGui = null;
        public bool PlaceMode
        {
            get { return _inPlaceMode; }
            set
            {
                if (_inPlaceMode != value)
                {
                    _inPlaceMode = value;

                    if (_inPlaceMode)
                    {
                        UpdatePreviewedObject();
                    }
                    else
                    {
                        RemovePreviewedObject();
                    }
                }
                Repaint();
            }
        }
        private bool _inPlaceMode;

        public PrefabSelectionMode CurrentPrefabSelectionMode = PrefabSelectionMode.KeepSelection;

        public bool HasPreviewObject
        {
            get { return _currentPreviewObject != null; }
        }

        public List<ObjectPlacerPrefabSet> Prefabs = new List<ObjectPlacerPrefabSet>();
        private ObjectPlacerPrefabSet.ObjectPlacePrefab _selectedPrefab;
        private int _prefabHashcode = 0;
        public ObjectPlacerPrefabSet.ObjectPlacePrefab SelectedPrefab
        {
            get { return _selectedPrefab; }
            set
            {
                bool gameObjectIsNull = value == null || value.Prefab == null;
                //Selected prefab is different or the prefab inside is different
                if (_selectedPrefab != value ||
                    (!gameObjectIsNull && value.Prefab.GetHashCode() != _prefabHashcode)
                    || (gameObjectIsNull && _prefabHashcode != -1)
                    )
                {
                    _selectedPrefab = value;
                    UpdatePreviewedObject();
                    Repaint();
                }
            }
        }

        public int IndexSelectedPrefab
        {
            get
            {
                if (SelectedPrefab != null)
                {
                    int index = 0;
                    foreach (var prefabSet in Prefabs)
                    {
                        foreach (var prefab in prefabSet.Prefabs)
                        {
                            if (prefab == SelectedPrefab)
                            {
                                return index;
                            }
                            index++;
                        }
                    }
                }
                return -1;
            }
            set
            {
                int count = GetPrefabCount();
                if (count == 0)
                {
                    SelectedPrefab = null;
                }
                else
                {
                    int normalizedIndex = value;
                    while (normalizedIndex<0)
                    {
                        normalizedIndex += count;
                    }

                    normalizedIndex %= count;
                    SelectedPrefab = GetPrefab(normalizedIndex);
                }
            }
        }

        public GameObject ParentGameObject;

        public bool OverwriteLayer;
        public int Layer;
        public int OrderNumber;

        private GameObject _currentPreviewObject;

        private Vector2 _positionObject;
        private float _rotationObject;
        private float _scaleObject = 1;
        private int _mirrorObject = 1;

        [MenuItem("Window/Object placer tool")]
        private static void Init()
        {
            var window = GetWindow<ObjectPlacer>();
            window.name = "Object Placer";
            window.Show();
        }

        private void OnEnable()
        {
            if (_objectPlacerInput != null)
            {
                _objectPlacerInput.Disable();
            }
            _objectPlacerInput = new ObjectPlacerInput(this);
            _objectPlacerGui = new ObjectPlacerGUI(this);

            EditorSceneManager.sceneClosing -= OnSceneClosing;
            EditorSceneManager.sceneClosing += OnSceneClosing;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;

            UpdatePreviewedObject();
        }

        private void OnDisable()
        {
            _objectPlacerInput.Disable();
            _objectPlacerInput = null;
            _objectPlacerGui = null;

            EditorSceneManager.sceneClosing -= OnSceneClosing;
            EditorSceneManager.sceneOpened -= OnSceneOpened;

            RemovePreviewedObject();
        }

        private void OnGUI()
        {
            _objectPlacerGui.OnGUI();
        }

        #region SceneUpdate

        private void OnSceneClosing(Scene scene, bool removingscene)
        {
            RemovePreviewedObject();
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            UpdatePreviewedObject();
        }

        #endregion

        private void UpdatePreviewedObject()
        {
            RemovePreviewedObject();

            if (SelectedPrefab != null && SelectedPrefab.Prefab != null)
            {
                _prefabHashcode = SelectedPrefab.Prefab.GetHashCode();
                if (PlaceMode)
                {
                    _currentPreviewObject = PrefabUtility.InstantiatePrefab(SelectedPrefab.Prefab) as GameObject;
                    if (_currentPreviewObject != null)
                    {
                        Transform[] transforms = _currentPreviewObject.GetComponentsInChildren<Transform>(true);
                        foreach (var transform in transforms)
                        {
                            transform.gameObject.hideFlags = HideFlags.HideAndDontSave;
                        }

                        UpdateTransform();
                    }
                }
                else
                {
                    _prefabHashcode = -1;
                }
            }
        }

        #region updateTransforms

        private void UpdateTransform()
        {
            UpdatePositionPreviewObject();

        }

        private void UpdatePositionPreviewObject()
        {
            Vector3 pos = _positionObject;
            pos.z = _currentPreviewObject.transform.position.z;
            _currentPreviewObject.transform.position = pos;
        }

        private void UpdateRotationPreviewObject()
        {
            _currentPreviewObject.transform.rotation = Quaternion.Euler(0, 0, _rotationObject);
        }

        private void UpdateScalePreviewObject()
        {
            Vector3 scale = _selectedPrefab.Prefab.transform.localScale;
            scale *= _scaleObject;
            scale.x *= _mirrorObject;
            _currentPreviewObject.transform.localScale = scale;
        }

        #endregion

        private void RemovePreviewedObject()
        {
            if (_currentPreviewObject != null)
            {
                DestroyImmediate(_currentPreviewObject);
            }
            _currentPreviewObject = null;
        }

        public void SelectNextPrefabInLine()
        {
            if (Prefabs.Count > 0)
            {
                switch (CurrentPrefabSelectionMode)
                {
                    case PrefabSelectionMode.KeepSelection:
                        break;
                    case PrefabSelectionMode.GoToNext:
                        IndexSelectedPrefab ++;
                        break;
                    case PrefabSelectionMode.GetRandom:
                        IndexSelectedPrefab = Random.Range(0, GetPrefabCount());
                        break;
                    case PrefabSelectionMode.GetRandomNoRepeat:
                        int random = Random.Range(0, GetPrefabCount() -1);
                        if (random >= IndexSelectedPrefab)
                        {
                            random++;
                        }
                        IndexSelectedPrefab = random;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public ObjectPlacerPrefabSet.ObjectPlacePrefab GetPrefab(int index)
        {
            for (int i = 0; i < Prefabs.Count; i++)
            {
                if (index < Prefabs[i].Prefabs.Count)
                {
                    return Prefabs[i].Prefabs[index];
                }
                index -= Prefabs[i].Prefabs.Count;
            }

            return null;
        }

        public int GetPrefabCount()
        {
            int count = 0;
            for (int i = 0; i < Prefabs.Count; i++)
            {
                count += Prefabs[i].Prefabs.Count;
            }

            return count;
        }

        public void PlaceObject()
        {
            GameObject spawnedPrefab = PrefabUtility.InstantiatePrefab(SelectedPrefab.Prefab) as GameObject;
            spawnedPrefab.transform.position = _currentPreviewObject.transform.position;
            spawnedPrefab.transform.rotation = _currentPreviewObject.transform.rotation;
            spawnedPrefab.transform.localScale = _currentPreviewObject.transform.localScale;

            if (OverwriteLayer)
            {
                SetLayerAndOrderGameObject(spawnedPrefab,Layer,OrderNumber);
            }

            Undo.RegisterCreatedObjectUndo(spawnedPrefab,string.Empty);

            if (ParentGameObject != null)
            {
                Undo.SetTransformParent(spawnedPrefab.transform,ParentGameObject.transform,string.Empty);
            }

            Undo.SetCurrentGroupName("Placed object");

            Selection.activeGameObject = spawnedPrefab;

            SelectNextPrefabInLine();
        }

        private void SetLayerAndOrderGameObject(GameObject gameObject, int layer, int order)
        {
            SortingGroup[] groups = gameObject.GetComponents<SortingGroup>();
            foreach (var group in groups)
            {
                group.sortingLayerID = layer;
                group.sortingOrder = order;
            }

            Renderer[] renderers = gameObject.GetComponents<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.sortingLayerID = layer;
                renderer.sortingOrder = order;
            }

            if (groups.Length == 0)
            {
                int childCount = gameObject.transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    SetLayerAndOrderGameObject(gameObject.transform.GetChild(i).gameObject,layer,order);
                }
            }
        }

        #region TransformControls

        public void SetPositionObject(Vector2 position)
        {
            _positionObject = position;
            UpdatePositionPreviewObject();
        }

        public void RotatePreviewObject(float angle)
        {
            _rotationObject += angle;
            UpdateRotationPreviewObject();
        }

        public void ScalePreviewObject(float addedScale)
        {
            _scaleObject += addedScale;
            UpdateScalePreviewObject();
        }

        public void MirrorPreviewObject()
        {
            _mirrorObject *= -1;
            UpdateScalePreviewObject();
        }

        public void ResetPreviewObject()
        {
            _mirrorObject = 1;
            _scaleObject = 1;
            _rotationObject = 0;
            UpdateTransform();
        }

        #endregion

        public enum PrefabSelectionMode
        {
            KeepSelection,
            GoToNext,
            GetRandom,
            GetRandomNoRepeat,
        }
    }
}
