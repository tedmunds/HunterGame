using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("GameState")]
public class GameStateUtility {

	[XmlElement("PlayerState")]
	public PlayerState playerState;

	[XmlElement("TimeOfDay")]
	public float timeOfDay;


	public GameStateUtility() {
		playerState = new PlayerState();
	}
	
	// Generate a file path name for a new save with the input name
	public static string CreateSavePath(string saveName) {
		return Path.Combine(Application.dataPath, "SaveGame/"+saveName+".xml");
	}

	public static void SaveGameState(string saveName, GameStateUtility gameState) {
		if(gameState == null) {
			Debug.Log("ERROR! Cannot save a null game state");
			return;
		}

		string filePath = CreateSavePath(saveName);

		XmlSerializer serializer = new XmlSerializer(typeof(GameStateUtility));

		try {
			FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
			serializer.Serialize(stream, gameState);
			//stream.Close();
		}
		catch(IOException e) {
			Debug.Log("ERROR! Couldn't save at "+filePath+" :: "+e.Message);

			return;
		}
	}
	
	public static GameStateUtility LoadGameState(string saveName) {
		XmlSerializer serializer = new XmlSerializer(typeof(GameStateUtility));

		string filePath = CreateSavePath(saveName);

		try {
			FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			return serializer.Deserialize(stream) as GameStateUtility;
		}
		catch(IOException e) {
			Debug.Log("ERROR! could not load game state from "+filePath+" :: "+e.Message);
			return null;
		}
	}
}
