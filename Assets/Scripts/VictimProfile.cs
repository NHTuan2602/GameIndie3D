using UnityEngine;

[CreateAssetMenu(fileName = "NanNhanMoi", menuName = "Kịch Bản Lừa Đảo/Tạo Nạn Nhân")]
public class VictimProfile : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string victimName = "Tên nạn nhân";
    public string jobOrAge = "Công việc / Tuổi";
    public Sprite avatar; // THÊM LUÔN LỖ CẮM AVATAR VÀO ĐÂY ĐỂ ĐỒNG BỘ GIAO DIỆN MESSENGER!

    [Header("Chỉ số yêu cầu & Phần thưởng")]
    public int staminaCost = 20;

    // ĐÃ FIX: Bỏ chữ 'f' đi để đúng chuẩn số nguyên (int)
    public int potentialReward = 5000000;

    // ĐÃ THÊM: Chỉ số Nghiệp chướng (Karma Penalty)
    [Tooltip("Số điểm Đạo đức bị trừ khi lừa THÀNH CÔNG người này. Người nghèo/già thì để cao (30-50), người giàu/ác thì để thấp (5-10).")]
    public int karmaPenalty = 20;

    public enum Difficulty { De, TrungBinh, Kho }
    public Difficulty difficultyLevel;

    [Header("KỊCH BẢN 5 HIỆP")]
    public ScamRound[] rounds = new ScamRound[5];
}