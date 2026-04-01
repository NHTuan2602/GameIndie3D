using UnityEngine;

[CreateAssetMenu(fileName = "NanNhanMoi", menuName = "Kịch Bản Lừa Đảo/Tạo Nạn Nhân")]
public class VictimProfile : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string victimName = "Tên nạn nhân";
    public string jobOrAge = "Công việc / Tuổi";
    public Sprite avatar;

    [Header("Phân loại Nạn nhân")]
    [Tooltip("Tích vào đây nếu đây là giang hồ mạng/người nổi tiếng vào trêu")]
    public bool isTroll = false;

    [Header("Chỉ số yêu cầu & Phần thưởng")]
    public int staminaCost = 20;
    public int potentialReward = 5000000;

    [Tooltip("Số điểm Đạo đức bị trừ khi lừa. Troll thì để 0.")]
    public int karmaPenalty = 20;

    public enum Difficulty { De, TrungBinh, Kho }
    public Difficulty difficultyLevel;

    [Header("KỊCH BẢN 5 HIỆP")]
    public ScamRound[] rounds = new ScamRound[5];
}