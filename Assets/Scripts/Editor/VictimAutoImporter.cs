using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

public class VictimAutoImporter : EditorWindow
{
    private TextAsset textFile;
    private string savePath = "Assets/TaiNguyenMang/Victims";

    [MenuItem("Tools/Tự Động Nhập Nạn Nhân")]
    public static void ShowWindow()
    {
        GetWindow<VictimAutoImporter>("Nhập Nạn Nhân");
    }

    void OnGUI()
    {
        GUILayout.Label("CÔNG CỤ TẠO SCRIPTABLE OBJECT TỰ ĐỘNG (V2)", EditorStyles.boldLabel);
        GUILayout.Label("- Tự nhận diện Tên, Tiền, Độ khó, Nghiệp.");
        GUILayout.Label("- Tự bật Pop-up nếu có ghi chú: Popup: \"...\"");
        GUILayout.Space(10);

        textFile = (TextAsset)EditorGUILayout.ObjectField("File Text (Kịch bản):", textFile, typeof(TextAsset), false);
        savePath = EditorGUILayout.TextField("Thư mục lưu:", savePath);

        GUILayout.Space(10);
        if (GUILayout.Button("BẮT ĐẦU QUÉT & TẠO FILE", GUILayout.Height(40)))
        {
            if (textFile == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Vui lòng kéo file Text kịch bản vào ô trống!", "OK");
                return;
            }
            ParseAndCreateAssets(textFile.text);
        }
    }

    void ParseAndCreateAssets(string content)
    {
        if (!AssetDatabase.IsValidFolder(savePath))
        {
            string[] folders = savePath.Split('/');
            string currentPath = folders[0];
            for (int i = 1; i < folders.Length; i++)
            {
                if (!AssetDatabase.IsValidFolder(currentPath + "/" + folders[i]))
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                currentPath += "/" + folders[i];
            }
        }

        string[] lines = content.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

        VictimProfile currentVictim = null;
        int roundIndex = 0;
        int count = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // 1. Quét tìm Tên và Thủ đoạn
            Match matchName = Regex.Match(line, @"^\d+\.\s+(.*?)\s+-\s+Thủ đoạn:\s+(.*)");
            if (matchName.Success)
            {
                if (currentVictim != null) SaveAsset(currentVictim, count);

                currentVictim = ScriptableObject.CreateInstance<VictimProfile>();
                currentVictim.victimName = matchName.Groups[1].Value.Trim();
                currentVictim.jobOrAge = matchName.Groups[2].Value.Trim();
                currentVictim.rounds = new ScamRound[5];
                for (int r = 0; r < 5; r++) currentVictim.rounds[r] = new ScamRound();
                roundIndex = 0;
                count++;
                continue;
            }

            if (currentVictim == null) continue;

            // 2. Quét tìm Mức độ, Tiền, Nghiệp
            Match matchStats = Regex.Match(line, @"Mức độ:\s+(.*?)\.\s+Tiền:\s+(\d+)M\.\s+Nghiệp:\s+(-?\d+)");
            if (matchStats.Success)
            {
                string diff = matchStats.Groups[1].Value.Trim().ToLower();
                if (diff.Contains("dễ") || diff.Contains("de")) currentVictim.difficultyLevel = VictimProfile.Difficulty.De;
                else if (diff.Contains("khó") || diff.Contains("kho")) currentVictim.difficultyLevel = VictimProfile.Difficulty.Kho;
                else currentVictim.difficultyLevel = VictimProfile.Difficulty.TrungBinh;

                currentVictim.potentialReward = int.Parse(matchStats.Groups[2].Value) * 1000000;
                currentVictim.karmaPenalty = Mathf.Abs(int.Parse(matchStats.Groups[3].Value));
                continue;
            }

            // 3. Quét tìm kịch bản (Bao gồm cả Popup Cản trở)
            // Cấu trúc: Hiệp X - V: "..." P: "..." [Popup: "..."]
            Match matchRound = Regex.Match(line, @"Hiệp\s+\d+\s+-\s+V:\s+""(.*?)""\s*P:\s+""(.*?)""(?:\s*Popup:\s+""(.*?)"")?");
            if (matchRound.Success && roundIndex < 5)
            {
                currentVictim.rounds[roundIndex].victimMessage = matchRound.Groups[1].Value;
                currentVictim.rounds[roundIndex].scriptToType = matchRound.Groups[2].Value;
                currentVictim.rounds[roundIndex].timeLimit = 15f;

                // Kiểm tra xem có lấy được Group 3 (Popup) không
                if (matchRound.Groups.Count > 3 && !string.IsNullOrEmpty(matchRound.Groups[3].Value))
                {
                    currentVictim.rounds[roundIndex].hasDistraction = true;
                    currentVictim.rounds[roundIndex].distractionMessage = matchRound.Groups[3].Value;
                }
                else
                {
                    currentVictim.rounds[roundIndex].hasDistraction = false;
                    currentVictim.rounds[roundIndex].distractionMessage = "";
                }

                roundIndex++;
            }
        }

        if (currentVictim != null) SaveAsset(currentVictim, count);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Thành Công V2", $"Đã quét và tạo/cập nhật {count} Nạn Nhân kèm theo thông số Cản Trở!", "OK");
    }

    void SaveAsset(VictimProfile victim, int index)
    {
        string safeName = Regex.Replace(victim.victimName, @"[^a-zA-Z0-9\s]", "");
        string assetPath = $"{savePath}/Victim_{index:00}_{safeName}.asset";
        AssetDatabase.CreateAsset(victim, assetPath);
    }
}