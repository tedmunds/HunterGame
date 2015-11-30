using UnityEngine;
using System.Collections.Generic;

public class TextureLoader : MonoBehaviour {

    [SerializeField]
    private List<Sprite> textures;

    private Dictionary<string, Sprite> textureDictionary;

    private void Start() {
        // Map the lists into the texture dictionaries     
        textureDictionary = new Dictionary<string, Sprite>();

        foreach(Sprite tex in textures) {
            if(tex == null) {
                continue;
            }
            textureDictionary.Add(tex.name, tex);
        }

    }



    public bool GetTexture(string name, out Sprite texture) {
        return textureDictionary.TryGetValue(name, out texture);
    }

}