/*
   Copyright 2018 Raviv Elon

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Reflection;
using UnityEngine.SceneManagement;
using System;
using raveaf.unity_utils;

public class Compo_link_processor {

    //This is necessary for the Component_refs to work in play mode properly
    [DidReloadScripts]
    public static void process_after_script_reload () {
        process();
    }
    
    [PostProcessScene]
    public static void process () {                

        MonoBehaviour[] objects =  Resources.FindObjectsOfTypeAll<MonoBehaviour>();
        
        //Looping through all the objects
        for (int i = 0; i < objects.Length; i++) {
            MonoBehaviour mono_behaviour = objects[i];

            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            
            //looping through all the fields of the current object
            foreach (var field in mono_behaviour.GetType().GetFields(flags) ) {

                Func<string> create_debug_string = () => "(" + field.Name + " in " + mono_behaviour.GetType() + ", game object: " + mono_behaviour.gameObject.name +")";

                object[] array =  Attribute.GetCustomAttributes(field, typeof( Compo_ref  ), true );

                if (array.Length <= 0) {
                    //no annotation found
                    continue;
                }

                if ( ! field.IsPublic && Attribute.GetCustomAttributes(field, typeof( SerializeField  ), true ).Length <= 0 ) {
                    Debug.LogError("The field is not a field to serialize " + create_debug_string() );
                    continue;
                }

                Compo_ref attr = (Compo_ref) array[0];
            
                object value = null;
                string search_name = attr.get_search_name().Equals("") ? field.Name : attr.get_search_name();
                Type search_type = field.FieldType;

                if (attr.get_children_as_list() ) {

                    if ( ! search_type.IsGenericType || search_type.GetGenericTypeDefinition() != typeof(List<>)) {
                        Debug.LogError("The field is not a list " + create_debug_string() );
                        continue;
                    }

                    search_type = typeof(Transform);
                }

                if ( ! search_type.IsSubclassOf(typeof(Component) ) && ! search_type.IsInterface ) {
                    Debug.LogError("Wrong type of field " + create_debug_string() );
                    continue;
                }

                //finding the component
                switch (attr.get_search_in() ) {
                    case Search_in.Self:
                        value = mono_behaviour.GetComponent(search_type);
                        break;
                    case Search_in.Children:
                        value = child_by_name(mono_behaviour, search_type, search_name);
                        break;
                    case Search_in.From_root:
                        value = child_by_name(mono_behaviour.transform.root, search_type, search_name);
                        break;
                
                    case Search_in.Scene:
                        List<GameObject> root_objects = new List<GameObject>();

                        SceneManager.GetActiveScene().GetRootGameObjects(root_objects);

                        foreach (GameObject root in root_objects) {
                            var target = child_by_name(root.transform, search_type, search_name);
                            if (target != null) {
                                value = target;
                                break;
                            }
                        }

                        break;                
                }
            
                if (value == null || value.Equals(null) ) {
                    Debug.LogError("Compo_link: Search unsuccessful " + create_debug_string() );
                    continue;
                }

                if (attr.get_children_as_list() ) {
                    //creating and filling the list for children as list
                    IList list = (IList) Activator.CreateInstance(field.FieldType);

                    Transform parent = (Transform) value;
                    Type generic_type = field.FieldType.GetGenericArguments()[0];

                    for(int j = 0; j < parent.childCount; j++) {
                        Component compo = parent.GetChild(j).GetComponent(generic_type);

                        if (compo != null) {
                            list.Add(compo);
                        }
                    }

                    if (list.Count <= 0) {
                        Debug.LogWarning("Compo_link: No " + generic_type + " children found " + create_debug_string() );
                    }

                    field.SetValue(mono_behaviour, list);    
                } else {
                    field.SetValue(mono_behaviour, value);    
                }            
            } 
        }
    }

    /// <summary>
    /// Find components by name even if they are inactive.
    /// </summary>
    static object child_by_name(Component component, Type type, string name)  {
        foreach (Component t in component.GetComponentsInChildren(type,true) ) {
            if (t.gameObject.name.Trim().Equals(name.Trim() ) ) {
                return t;
            }                
        }

        return null;
    }

}

/// <summary>
/// Used to hide the fields in the inspector without having to add the additional HideInInspector annotation.
/// </summary>
[CustomPropertyDrawer(typeof(Compo_ref),true)]
public class Hide_compo_link : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return 0;
    }

}


