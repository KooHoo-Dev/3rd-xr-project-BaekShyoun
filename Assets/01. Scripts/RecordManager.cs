using System;
using System.Collections.Generic;
using UnityEngine;

namespace Whack_A_Mole
{
    [Serializable]
    public class GameRecord
    {
        public int score;
        public int bestCombo;
        public string date;
        public string id;
    }

    [Serializable]
    public class RecordList
    {
        public List<GameRecord> records = new List<GameRecord>();
    }

    public static class RecordManager
    {
        private const string saveKey = "NeonHammer.Records";
        public static GameRecord LastRecord { get; private set; }

        public static List<GameRecord> LoadRecords()
        {
            // 기록이 없으면 빈 목록으로 시작.
            string json = PlayerPrefs.GetString(saveKey, "");
            if (string.IsNullOrEmpty(json)) return new List<GameRecord>();
            try
            {
                RecordList data = JsonUtility.FromJson<RecordList>(json);
                return data?.records ?? new List<GameRecord>();
            }
            catch (ArgumentException)
            {
                return new List<GameRecord>();
            }
        }

        public static void SaveRecord(int score, int bestCombo)
        {
            LastRecord = new GameRecord
            {
                score = score,
                bestCombo = bestCombo,
                date = DateTime.Now.ToString("MM.dd HH:mm"),
                id = Guid.NewGuid().ToString()
            };
            RecordList data = new RecordList { records = LoadRecords() };
            data.records.Add(LastRecord);
            // 점수 우선, 동점이면 최고 콤보가 높은 순서. 완전 동점이면 최신 기록 우선.
            data.records.Sort((a, b) =>
            {
                if (a.id == b.id) return 0;
                int result = b.score.CompareTo(a.score);
                if (result == 0) result = b.bestCombo.CompareTo(a.bestCombo);
                if (result == 0 && a.id == LastRecord.id) return -1;
                if (result == 0 && b.id == LastRecord.id) return 1;
                return result;
            });
            if (data.records.Count > 10) data.records.RemoveRange(10, data.records.Count - 10);
            PlayerPrefs.SetString(saveKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }
    }
}
