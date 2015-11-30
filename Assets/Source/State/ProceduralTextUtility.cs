using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("TextUtility")]
public class ProceduralTextUtility {

    private const bool bReusesWords = false;

    [XmlArray("Verbs")]
    [XmlArrayItem("v")]
    public string[] verbs;

    [XmlArray("Titles")]
    [XmlArrayItem("t")]
    public string[] title;

    [XmlArray("Nouns")]
    [XmlArrayItem("n")]
    public string[] nouns;

    [XmlArray("Places")]
    [XmlArrayItem("pl")]
    public string[] places;

    [XmlArray("Qualities")]
    [XmlArrayItem("q")]
    public string[] qualities;

    [XmlArray("Adjectives")]
    [XmlArrayItem("a")]
    public string[] adjectives;

    // Tracks which of each list have been used
    private List<string> verbsList;
    private List<string> titleLists;
    private List<string> nounsList;
    private List<string> placesList;
    private List<string> qualitiesList;
    private List<string> adjectivesList;


    public void InitializeTextUtility() {
        verbsList = new List<string>(verbs);
        titleLists = new List<string>(title);
        nounsList = new List<string>(nouns);
        placesList = new List<string>(places);
        qualitiesList = new List<string>(qualities);
        adjectivesList = new List<string>(adjectives);
    }

    /** Fills if the input prefix title and suffix approprietly selected random ones */
    public void GenerateCompleteName(ref string pref, ref string title, ref string suff) {
        int verbIdx = Random.Range(0, verbsList.Count);
        int titleIdx = Random.Range(0, titleLists.Count);
        int nounIdx = Random.Range(0, nounsList.Count);
        int adjIdx = Random.Range(0, adjectivesList.Count);
        int placeIdx = Random.Range(0, placesList.Count);
        int qualIdx = Random.Range(0, qualitiesList.Count);
        
        pref = adjectivesList[adjIdx];

        title = " " + titleLists[titleIdx];

        int secondAdjIdx = Random.Range(0, adjectivesList.Count);
        if(secondAdjIdx == adjIdx) {
            secondAdjIdx += 1;
            if(secondAdjIdx >= adjectivesList.Count) {
                secondAdjIdx = 0;
            }
        }

        bool bOfPlaceName = Random.Range(0.0f, 1.0f) > 0.5f;

        if(bOfPlaceName) {
            suff = ", " + qualitiesList[qualIdx] + " of " + placesList[placeIdx];
        }
        else {
            suff = " the " + adjectivesList[secondAdjIdx] + " " + nounsList[nounIdx];
        }

        // Ensure that we dont reuse words by removing them from the sets
        if(!bReusesWords) {
            verbsList.RemoveAt(verbIdx);
            titleLists.RemoveAt(titleIdx);
            adjectivesList.RemoveAt(adjIdx);
            
            if(bOfPlaceName) {
                placesList.RemoveAt(placeIdx);
                qualitiesList.RemoveAt(qualIdx);
            }
            else {
                if(secondAdjIdx > adjIdx && secondAdjIdx > 0) {
                    secondAdjIdx -= 1;
                }
                adjectivesList.RemoveAt(secondAdjIdx);
                nounsList.RemoveAt(nounIdx);
            }
        }
    }



    public static ProceduralTextUtility LoadTextTypes(string sourceFile) {
        XmlSerializer serializer = new XmlSerializer(typeof(ProceduralTextUtility));

        string filePath = Path.Combine(Application.dataPath, "Data/" + sourceFile + ".xml");

        try {
            FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            ProceduralTextUtility textUtility = serializer.Deserialize(stream) as ProceduralTextUtility;
            textUtility.InitializeTextUtility();

            return textUtility;
        }
        catch(IOException e) {
            Debug.Log("ERROR! could find procedural text source file " + filePath + " :: " + e.Message);
            return null;
        }
    }

}
