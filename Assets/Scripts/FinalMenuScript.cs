using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class ResultsClass
{
    public List<string> names;
    public List<float> times; 

    public ResultsClass()
    {
        names = new List<string>();
        times = new List<float>();
    }
}

public class FinalMenuScript : MonoBehaviour {
    private float finalTime;
    public int game_mode;
    public Text timerText;
    public Text tableTitle;
    public Text tableRecords;
    public InputField inputName;
    public ResultsClass resultsData;
    string dataPath;

    void Start()
    {
        finalTime = PlayerPrefs.GetFloat("Time");
        game_mode = PlayerPrefs.GetInt("GameMode");
        string minutes = ((int)finalTime / 60).ToString();
        string seconds = (finalTime % 60).ToString("f1");   // the "f" parameter defines how many decimals you want
        timerText.text = "Congratulations! You did it in " + minutes + " minuts and " + seconds + " seconds!";
        if (game_mode == 1)
        {
            dataPath = Path.Combine(Application.persistentDataPath, "Results_mode1.txt");
            tableTitle.text = "Best Records as dog";
        }
        else
        {
            dataPath = Path.Combine(Application.persistentDataPath, "Results_mode2.txt");
            tableTitle.text = "Best Records as shepherd";
        }
        
        resultsData = LoadGames(dataPath);
        string table_text = "";

        if (resultsData.times.Count > 0) {
            List<float> sorted_times = new List<float>(resultsData.times);
            sorted_times.Sort();
            for (int i = 0; i < Mathf.Min(5, resultsData.times.Count); i++)
            {
                float t = sorted_times[i];
                int ind_n = resultsData.times.FindIndex(x => x==t);
                string n = resultsData.names[ind_n];
                table_text = table_text + i.ToString() + " - " + n + " -- Time record: " + t.ToString() + "\n";
            }
        }

        tableRecords.text = table_text;
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene(0);
    }

    public void SaveGame()
    {
        if (!string.IsNullOrEmpty(inputName.text))
        {
            resultsData.names.Add(inputName.text);
            resultsData.times.Add(finalTime);
            SaveGames(resultsData, dataPath);
            SceneManager.LoadScene(0);
        }
        else
        {
            SceneManager.LoadScene(1);
        }
    }

    static void SaveGames(ResultsClass data, string path)
    {
        string jsonString = JsonUtility.ToJson(data);

        using (StreamWriter streamWriter = File.CreateText(path))
        {
            streamWriter.Write(jsonString);
        }
    }

    static ResultsClass LoadGames(string path)
    {
        if (File.Exists(path)) { 
            using (StreamReader streamReader = File.OpenText(path))
            {
                string jsonString = streamReader.ReadToEnd();
                return JsonUtility.FromJson<ResultsClass>(jsonString);
            }
        }
        else
        {
            return new ResultsClass();
        }
    }
}
