using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Text;

public class Leaderboards : MonoBehaviour
{
    public const int RANK_INVALID = -1; // The rank is invalid

    private static string _Url = "http://localhost";
    private static string _Port = "8080";

    // Submits users score to the server.
    // 
    // @param which             Leaderboard index to submit score to
    // @param score             Score to submit
    // @param userName          Name of user to submit score for
    // @param OnScoreSubmitted  Callback to call when operation completes or fails, a bool is passed to the callback which
    //                          is true if an error occurred or false if not
    //
    public void SubmitScore(int which, int score, string userName, Action<bool> OnScoreSubmitted)
    {
        StartCoroutine(SubmitScoreToServer(which, score, userName, OnScoreSubmitted));
    }

    private IEnumerator SubmitScoreToServer(int which, int score, string userName, Action<bool> OnScoreSubmitted)
    {
        Debug.Log("Submitting score");

        // Create a form that will contain our data
        WWWForm form = new WWWForm();
        StringBuilder sb = new StringBuilder("m=score");
        sb.Append("&w=");
        sb.Append(which.ToString());
        sb.Append("&s=");
        sb.Append(score.ToString());
        sb.Append("&n=");
        sb.Append(userName);
        byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
        form.AddField("d", Convert.ToBase64String(bytes));

        // Create a POST web request with our form data
        UnityWebRequest www = UnityWebRequest.Post(_Url + ":" + _Port, form);
        // Send the request and yield until the send completes
        yield return www.Send();

        if (www.isError)
        {
            // There was an error
            Debug.Log(www.error);
            if (OnScoreSubmitted != null)
                OnScoreSubmitted(true);
        }
        else
        {
            if (www.responseCode == 200)
            {
                // Response code 200 signifies that the server had no issues with the data we sent
                Debug.Log("Score send complete!");
                Debug.Log("Response:" + www.downloadHandler.text);
                if (OnScoreSubmitted != null)
                    OnScoreSubmitted(false);
            }
            else
            {
                // Any other response signifies that there was an issue with the data we sent
                Debug.Log("Score send error response code:" + www.responseCode.ToString());
                if (OnScoreSubmitted != null)
                    OnScoreSubmitted(true);
            }
        }
    }

    // Submits a collection of scores to the server.
    // 
    // @param scores            Scores to submit, once score per leaderboard
    // @param userName          Name of user to submit score for
    // @param OnScoresSubmitted Callback to call when operation completes or fails, a bool is passed to the callback which
    //                          is true if an error occurred or false if not
    //
    public void SubmitScores(int[] scores, string userName, Action<bool> OnScoresSubmitted)
    {
        StartCoroutine(SubmitScoresToServer(scores, userName, OnScoresSubmitted));
    }

    private IEnumerator SubmitScoresToServer(int[] scores, string userName, Action<bool> OnScoresSubmitted)
    {
        Debug.Log("Submitting scores");

        WWWForm form = new WWWForm();
        StringBuilder sb = new StringBuilder("m=scores");
        sb.Append("&a=");
        for (int t = 0; t < scores.Length; t++)
        {
            sb.Append(scores[t].ToString());
            if (t < scores.Length - 1)
                sb.Append(",");
        }
        sb.Append("&n=");
        sb.Append(userName);
        byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
        form.AddField("d", Convert.ToBase64String(bytes));

        UnityWebRequest www = UnityWebRequest.Post(_Url + ":" + _Port, form);
        yield return www.Send();

        if (www.isError)
        {
            Debug.Log(www.error);
            if (OnScoresSubmitted != null)
                OnScoresSubmitted(true);
        }
        else
        {
            if (www.responseCode == 200)
            {
                Debug.Log("Scores sent complete!");
                Debug.Log("Response:" + www.downloadHandler.text);
                if (OnScoresSubmitted != null)
                    OnScoresSubmitted(false);
            }
            else
            {
                Debug.Log("Scores sent error response code:" + www.responseCode.ToString());
                if (OnScoresSubmitted != null)
                    OnScoresSubmitted(true);
            }
        }
    }

    // Gets the user rank from the server.
    // 
    // @param which             Leaderboard index to submit score to
    // @param score             Score to submit
    // @param userName          Name of user to submit score for
    // @param OnScoreSubmitted  Callback to call when operation completes or fails, an int is passed to the callback which
    //                          represents the users rank
    //
    public void GetRank(int which, string userName, Action<int> OnRankRetrieved)
    {
        StartCoroutine(GetRankFromServer(which, userName, OnRankRetrieved));
    }

    private IEnumerator GetRankFromServer(int which, string userName, Action<int> OnRankRetrieved)
    {
        Debug.Log("Getting rank");

        WWWForm form = new WWWForm();
        StringBuilder sb = new StringBuilder("m=rank");
        sb.Append("&w=");
        sb.Append(which.ToString());
        sb.Append("&n=");
        sb.Append(userName);
        byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
        form.AddField("d", Convert.ToBase64String(bytes));

        UnityWebRequest www = UnityWebRequest.Post(_Url + ":" + _Port, form);
        yield return www.Send();

        if (www.isError)
        {
            Debug.Log(www.error);
            if (OnRankRetrieved != null)
                OnRankRetrieved(RANK_INVALID);
        }
        else
        {
            if (www.responseCode == 200)
            {
                Debug.Log("Get rank complete!");
                Debug.Log("Response:" + www.downloadHandler.text);
                if (OnRankRetrieved != null)
                {
                    int rank = -1;
                    int.TryParse(www.downloadHandler.text, out rank);
                    OnRankRetrieved(rank);
                }
            }
            else
            {
                Debug.Log("Get rank error response code:" + www.responseCode.ToString());
                if (OnRankRetrieved != null)
                    OnRankRetrieved(RANK_INVALID);
            }
        }
    }

    // Gets the users ranks from the server.
    // 
    // @param userName          Name of user to submit score for
    // @param OnRanksRetrieved  Callback to call when operation completes or fails, callback has an array of ints, with
    //                          each element representing a leaerboard rank
    //
    public void GetRanks(string userName, Action<int[]> OnRanksRetrieved)
    {
        StartCoroutine(GetRanksFromServer(userName, OnRanksRetrieved));
    }

    private IEnumerator GetRanksFromServer(string userName, Action<int[]> OnRanksRetrieved)
    {
        Debug.Log("Getting ranks");

        WWWForm form = new WWWForm();
        StringBuilder sb = new StringBuilder("m=ranks");
        sb.Append("&n=");
        sb.Append(userName);
        byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
        form.AddField("d", Convert.ToBase64String(bytes));

        UnityWebRequest www = UnityWebRequest.Post(_Url + ":" + _Port, form);
        yield return www.Send();

        if (www.isError)
        {
            Debug.Log(www.error);
            if (OnRanksRetrieved != null)
                OnRanksRetrieved(null);
        }
        else
        {
            if (www.responseCode == 200)
            {
                Debug.Log("Get ranks complete!");
                Debug.Log("Response:" + www.downloadHandler.text);
                if (OnRanksRetrieved != null)
                {
                    string[] sranks = www.downloadHandler.text.Split(',');
                    int[] ranks = new int[sranks.Length];
                    int i = 0;
                    foreach (string s in sranks)
                    {
                        ranks[i] = -1;
                        int.TryParse(sranks[i], out ranks[i]);
                        i++;
                    }
                    OnRanksRetrieved(ranks);
                }
            }
            else
            {
                Debug.Log("Get ranks error response code:" + www.responseCode.ToString());
                if (OnRanksRetrieved != null)
                    OnRanksRetrieved(null);
            }
        }
    }
}
