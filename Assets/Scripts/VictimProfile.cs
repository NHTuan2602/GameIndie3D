using UnityEngine;

[CreateAssetMenu(fileName = "NanNhanMoi", menuName = "Kịch Bản Lừa Đảo/Tạo Nạn Nhân")]
public class VictimProfile : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string victimName = "Tên nạn nhân";
    public string jobOrAge = "Công việc / Tuổi";

    [Header("Chỉ số yêu cầu & Phần thưởng")]
    public int staminaCost = 20;
    public float potentialReward = 5000f; // DÒNG MỚI THÊM: Số tiền lừa được

    public enum Difficulty { De, TrungBinh, Kho }
    public Difficulty difficultyLevel;

    [Header("KỊCH BẢN 5 HIỆP")]
    public ScamRound[] rounds = new ScamRound[5];
}