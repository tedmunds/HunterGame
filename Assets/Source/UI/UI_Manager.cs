using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * Maintains a list of registered variables, which other classes can listen to , to be notified when they change.
 * Basically performs a simple data binding
 */ 
public class UI_Manager {

    public delegate void BindingNotify();

    // Which variable in the boundObject is bound
    private struct BoundVarialbe {
        public object boundObject;
        public string boundVariable;
        public BindingNotify notify;
    }

    // maps bound variables to the name of the variable theya re watching
    private Dictionary<string, List<BoundVarialbe>> bindingMap;

    public UI_Manager() {
        bindingMap = new Dictionary<string, List<BoundVarialbe>>();
    }



    // Bind the varaible named varName, in the Object boundObject, to update whenever a registered variable of the targetVariable is updated
    // Can also have an optional notify function that willbe called whever the target is updated
    public void BindVariable(string varName, object boundObject, string targetVariable, BindingNotify optionalNotify = null) {
        BoundVarialbe newBinding = new BoundVarialbe();
        newBinding.boundObject = boundObject;
        newBinding.boundVariable = varName;
        newBinding.notify = optionalNotify;

        List<BoundVarialbe> boundList;
        if(bindingMap.TryGetValue(targetVariable, out boundList)) {
            // target variable is already being watched
            boundList.Add(newBinding);
        }
        else {
            // The target variable has not been registered
            boundList = new List<BoundVarialbe>();
            bindingMap.Add(targetVariable, boundList);

            boundList.Add(newBinding);
        }
    }


    public void ValueUpdated(string targetVariable, object newValue) {
        List<BoundVarialbe> boundList;
        if(bindingMap.TryGetValue(targetVariable, out boundList)) {
            // Update all bound variables
            foreach(BoundVarialbe binding in boundList) {
                System.Reflection.FieldInfo field = binding.boundObject.GetType().GetField(binding.boundVariable);
                if(field != null) {
                    // set the variable on the object to the new value
                    field.SetValue(binding.boundObject, newValue);

                    if(binding.notify != null) {
                        binding.notify();
                    }
                }
                else {
                    Debug.LogWarning("WARNING: " + binding.boundVariable + " is bound on " + binding.boundObject + ", but doesn't seem to exists or isn't public!");
                }
            }
        }
    }



}