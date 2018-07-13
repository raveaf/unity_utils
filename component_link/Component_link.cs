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
/// Attribute to link public, serialized fields of MonoBehaviours to Components in the scene while post processing the scene
/// </summary>            
/// <param name="search_name">The name of the Component you want to link; if empty, the name of the field is used instead</param> 
[AttributeUsage(AttributeTargets.Field , AllowMultiple = false)]
public class Component_link : PropertyAttribute {
    
    public string search_name;
    public Search_in search_in;
    public bool children_as_list;

    /// <summary>
    /// Attribute to link public, serialized fields of MonoBehaviours to Components in the scene while post processing the scene
    /// </summary>            
    /// <param name="search_name">The name of the Component you want to link; if empty, the name of the field is used instead</param>            
    public Component_link (string search_name = "", Search_in search_in = Search_in.Children, bool children_as_list = false) {
        this.search_name = search_name;
        this.search_in = search_in;
        this.children_as_list = children_as_list;
    }    
} 

public enum Search_in {
    Children,
    From_root,
    Scene,
    Self
}

/// <summary>
/// Shortcut to the Component_link Attribute with Search_in.Self
/// </summary>
[AttributeUsage(AttributeTargets.Field , AllowMultiple = false)]
public class Component_link_self  : Component_link {

    /// <summary>
    /// Shortcut to the Component_link Attribute with Search_in.Self
    /// </summary>
    public Component_link_self (bool children_as_list = false) {        
        this.search_in = Search_in.Self;
        this.children_as_list = children_as_list;
    }
}

}