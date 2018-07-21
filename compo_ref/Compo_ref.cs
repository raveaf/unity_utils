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

using System;
using UnityEngine;

namespace raveaf.unity_utils {

/// <summary>
/// Attribute to link serialized fields of MonoBehaviours to Components in the scene while post processing the scene
/// </summary>            
/// <param name="search_name">The name of the Component you want to link; if empty, the name of the field is used instead</param> 
[AttributeUsage(AttributeTargets.Field , AllowMultiple = false)]
public class Compo_ref : PropertyAttribute {
    
    private string search_name;            
    private Search_in search_in;
    private bool children_as_list;

    /// <summary>
    /// Attribute to link serialized fields of MonoBehaviours to Components in the scene while post processing the scene
    /// </summary>            
    /// <param name="search_name">The name of the Component you want to link; if empty, the name of the field is used instead</param>            
    public Compo_ref (string search_name = "", Search_in search_in = Search_in.Children, bool children_as_list = false) {
        this.search_name = search_name;
        this.search_in = search_in;
        this.children_as_list = children_as_list;
    }    


    public string get_search_name() {
        return search_name;
    }

    public Search_in get_search_in() {
        return search_in;
    }

    public bool get_children_as_list() {
        return children_as_list;
    }
} 

public enum Search_in {
    Children,
    From_root,
    Scene,
    Self
}

/// <summary>
/// Shortcut to the Compo_ref Attribute with Search_in.Self
/// </summary>
[AttributeUsage(AttributeTargets.Field , AllowMultiple = false)]
public class Compo_ref_self  : Compo_ref {

    /// <summary>
    /// Shortcut to the Compo_ref Attribute with Search_in.Self
    /// </summary>
    public Compo_ref_self (bool children_as_list = false) : base ("", Search_in.Self, children_as_list) {}
}

}