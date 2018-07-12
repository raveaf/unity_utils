﻿/*
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
using UnityEditor.Callbacks;
using System.Reflection;
using UnityEngine.SceneManagement;
using System;

namespace raveaf.unity_utils {

public class Compo_link_processor {

    [PostProcessScene]
    public static void process () {
        MonoBehaviour[] objects =  Resources.FindObjectsOfTypeAll<MonoBehaviour>();

        for (int i = 0; i < objects.Length; i++) {
            MonoBehaviour mono_behaviour = objects[i];

            var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            foreach (var field in mono_behaviour.GetType().GetFields(flags) ) {

                object[] array =  field.GetCustomAttributes(typeof( Compo_link  ), false);

                if (array.Length <= 0) {
                    continue;
                }

                Compo_link attr = (Compo_link) array[0];
            
                object value = null;
                string search_name = attr.search_name.Equals("") ? field.Name : attr.search_name;
                Type search_type = field.FieldType;

                if (attr.children_as_list) {

                    if ( ! search_type.IsGenericType || search_type.GetGenericTypeDefinition() != typeof(List<>)) {
                        Debug.LogError("The field is not a list " + debug_string(field, mono_behaviour) );
                        continue;
                    }

                    search_type = typeof(Transform);
                }

                if ( ! search_type.IsSubclassOf(typeof(Component) ) && ! search_type.IsInterface ) {
                    Debug.LogError("Wrong type of field " + debug_string(field, mono_behaviour) );
                    continue;
                }


                switch (attr.search_in) {
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
                    Debug.LogError("Compo_link: Search unsuccessful " + debug_string(field, mono_behaviour) );
                    continue;
                }

                if (attr.children_as_list) {
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
                        Debug.LogWarning("Compo_link: No " + generic_type + " children found " + debug_string(field, mono_behaviour) );
                    }

                    field.SetValue(mono_behaviour, list);    
                } else {
                    field.SetValue(mono_behaviour, value);    
                }            
            } 
        }
    }


    static object child_by_name(Component component, Type type , string name)  {
        foreach (Component t in component.GetComponentsInChildren(type,true) ) {
            if (t.gameObject.name.Trim().Equals(name.Trim() ) ) {
                return t;
            }                
        }

        return null;
    }

    static string debug_string (FieldInfo prop, MonoBehaviour mono_behaviour) {
        return "(" + prop.Name + " in " + mono_behaviour.GetType() + ", game object: " + mono_behaviour.gameObject.name +")";
    }

}

}
