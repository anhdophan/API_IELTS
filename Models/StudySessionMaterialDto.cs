public class StudySessionMaterialDto
{
    public DateTime Date { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    public string Material { get; set; } // Leave null if it's an exam
    public int? ExamId { get; set; }     // Set if this session is an exam
}