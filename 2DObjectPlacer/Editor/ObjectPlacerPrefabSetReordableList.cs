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

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Assets.Tools._2DObjectPlacer.Editor
{
    public class ObjectPlacerPrefabSetReordableList
    {
        private const float SizePreviewTexture = 40.0f;
        private const float OffsetBorders = 5.0f;

        private ReorderableList _list;
        private ObjectPlacerPrefabSet _set;

        private bool _showSelection = false;
        private ObjectPlacerPrefabSet.ObjectPlacePrefab _selectedPrefab = null;

        public ObjectPlacerPrefabSetReordableList(ObjectPlacerPrefabSet set)
        {
            _set = set;
            _list = new ReorderableList(set.Prefabs, typeof(ObjectPlacerPrefabSet.ObjectPlacePrefab),true,true,true,true);

            _list.drawHeaderCallback = rect => {
                EditorGUI.LabelField(rect, string.Format("Prefab set: {0}",_set.name));
            };

            _list.drawElementCallback = DrawElememt;

            _list.elementHeightCallback = (index) => {
                return GetHeight();
            };

            _list.drawElementBackgroundCallback = (rect, index, active, focused) =>
            {
                rect.height = GetHeight();
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, new Color(0.33f, 0.66f, 1f, 0.66f));
                tex.Apply();
                if (active)
                    GUI.DrawTexture(rect, tex as Texture);
            };
        }

        private float GetHeight()
        {
            return SizePreviewTexture + 2* OffsetBorders;
        }

        private void DrawElememt(Rect rect, int index, bool isActive, bool isFocused)
        {
            Rect rectPreviewTexture = new Rect(rect.xMax - SizePreviewTexture - OffsetBorders, rect.y + OffsetBorders, SizePreviewTexture, SizePreviewTexture);
            float startXRectPrefab = rect.x + OffsetBorders;
            Rect rectPrefab = new Rect(startXRectPrefab, rect.y+ OffsetBorders,(rectPreviewTexture.xMin - OffsetBorders) - startXRectPrefab, EditorGUIUtility.singleLineHeight);
            //Rect rectChance = new Rect(rectPrefab.x, rectPrefab.yMax + EditorGUIUtility.standardVerticalSpacing,rectPrefab.width, EditorGUIUtility.singleLineHeight);
            Rect rectSelection = new Rect(rectPrefab.x, rectPrefab.yMax + EditorGUIUtility.standardVerticalSpacing, rectPrefab.width, EditorGUIUtility.singleLineHeight);

            ObjectPlacerPrefabSet.ObjectPlacePrefab prefab = _list.list[index] as ObjectPlacerPrefabSet.ObjectPlacePrefab;
            prefab.Prefab = EditorGUI.ObjectField(rectPrefab, prefab.Prefab, typeof (GameObject), false) as GameObject;
            //prefab.Chance = Mathf.Max(1,EditorGUI.IntField(rectChance, "Chance: ", prefab.Chance));

            if (_showSelection)
            {
                if (EditorGUI.ToggleLeft(rectSelection,"Selected", _selectedPrefab == prefab))
                {
                    _selectedPrefab = prefab;
                }
            }
            else
            {
                _selectedPrefab = null;
            }


            Texture texturePreview = null;
            if (prefab.Prefab != null)
            {
                texturePreview = AssetPreview.GetAssetPreview(prefab.Prefab);
            }

            if (texturePreview != null)
            {
                EditorGUI.DrawTextureTransparent(rectPreviewTexture, texturePreview);
            }
            else
            {
                EditorGUI.DrawRect(rectPreviewTexture, Color.gray);
            }
        }

        public void Draw()
        {
            Undo.RecordObject(_set, "Changed values prefab set");
            _showSelection = false;
            _list.DoLayoutList();
        }

        public void Draw(ref ObjectPlacerPrefabSet.ObjectPlacePrefab currentlySelected)
        {
            Undo.RecordObject(_set, "Changed values prefab set");
            _showSelection = true;
            _selectedPrefab = currentlySelected;
            _list.DoLayoutList();
            currentlySelected = _selectedPrefab;
        }
    }
}
