// gen_question_template.cs
// Sinh file mẫu import câu hỏi hàng loạt (question_import_template.xlsx).
// Định dạng 11 cột phải khớp đúng thứ tự BulkImportService.cs kỳ vọng.
// Chạy: dotnet run gen_question_template.cs
#:property PublishAot=false
#:package ClosedXML@0.104.2

using ClosedXML.Excel;

// Đường dẫn output: seed/question_import_template.xlsx (thư mục gốc seed, đi lên 2 cấp từ tools/GenTemplate)
var scriptDir = AppContext.BaseDirectory;
var outPath = Path.GetFullPath(Path.Combine(
    Environment.GetEnvironmentVariable("OUT_DIR") ?? Directory.GetCurrentDirectory(),
    "question_import_template.xlsx"));

using var wb = new XLWorkbook();

// ── Sheet 1: Câu hỏi ─────────────────────────────────────────────
var ws = wb.Worksheets.Add("Câu hỏi");

string[] headers =
[
    "Content", "QuestionTypeId", "DifficultyLevelId", "TopicId", "CognitiveLevelId",
    "Explanation", "AnswerA", "AnswerB", "AnswerC", "AnswerD", "CorrectAnswers"
];
for (int c = 0; c < headers.Length; c++)
    ws.Cell(1, c + 1).Value = headers[c];

var head = ws.Range(1, 1, 1, headers.Length);
head.Style.Font.Bold = true;
head.Style.Font.FontColor = XLColor.White;
head.Style.Fill.BackgroundColor = XLColor.FromHtml("#1677FF");
head.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

// Dữ liệu mẫu: [Content, TypeId, DiffId, TopicId, CogId, Explanation, A, B, C, D, Correct]
object[][] rows =
[
    ["2 + 3 = ?", 1, 1, "", 1, "2 cộng 3 bằng 5.", "4", "5", "6", "7", "B"],
    ["Chọn các số chẵn trong dãy sau:", 2, 2, "", 2, "Số chẵn chia hết cho 2.", "2", "3", "4", "5", "A,C"],
    ["4 + 2 = 5. Đúng hay sai?", 3, 1, "", 1, "4 + 2 = 6 nên khẳng định là Sai.", "Đúng", "Sai", "", "", "B"],
    ["Việt Nam có bao nhiêu mùa trong năm ở miền Bắc?", 1, 2, "", 3, "Miền Bắc có 4 mùa: xuân, hạ, thu, đông.", "2", "3", "4", "5", "C"],
];
for (int r = 0; r < rows.Length; r++)
    for (int c = 0; c < rows[r].Length; c++)
    {
        var cell = ws.Cell(r + 2, c + 1);
        switch (rows[r][c])
        {
            case int i: cell.Value = i; break;
            case string s: cell.Value = s; break;
        }
    }

ws.SheetView.FreezeRows(1);
ws.Column(1).Width = 45;   // Content
ws.Column(6).Width = 35;   // Explanation
foreach (var c in new[] { 2, 3, 4, 5, 11 }) ws.Column(c).Width = 16;
foreach (var c in new[] { 7, 8, 9, 10 }) ws.Column(c).Width = 12;
ws.Range(1, 1, rows.Length + 1, headers.Length).Style.Alignment.WrapText = true;

// ── Sheet 2: Hướng dẫn ───────────────────────────────────────────
var g = wb.Worksheets.Add("Hướng dẫn");
int row = 1;

void Title(string t)
{
    var cell = g.Cell(row, 1);
    cell.Value = t;
    cell.Style.Font.Bold = true;
    cell.Style.Font.FontSize = 13;
    row++;
}
void Line(string a, string b = "", bool bold = false)
{
    g.Cell(row, 1).Value = a;
    g.Cell(row, 2).Value = b;
    if (bold)
    {
        g.Cell(row, 1).Style.Font.Bold = true;
        g.Cell(row, 2).Style.Font.Bold = true;
    }
    row++;
}

Title("CÁCH ĐIỀN FILE IMPORT CÂU HỎI");
row++;
Line("Cột", "Ý nghĩa / Bắt buộc", bold: true);
Line("Content", "Nội dung câu hỏi — BẮT BUỘC.");
Line("QuestionTypeId", "Loại câu hỏi (số) — BẮT BUỘC. Xem bảng bên dưới.");
Line("DifficultyLevelId", "Độ khó (số) — tùy chọn. Để trống sẽ lấy 'Độ khó mặc định' chọn ở form import.");
Line("TopicId", "Chủ đề (số) — tùy chọn. Để trống sẽ lấy 'Chủ đề mặc định' chọn ở form import.");
Line("CognitiveLevelId", "Cấp độ nhận thức Bloom (số) — tùy chọn. Để trống sẽ lấy mặc định ở form.");
Line("Explanation", "Giải thích đáp án — tùy chọn.");
Line("AnswerA..AnswerD", "Nội dung 4 đáp án. Để trống đáp án không dùng (vd true_false chỉ cần A, B).");
Line("CorrectAnswers", "Chữ cái đáp án đúng: 'B' hoặc nhiều đáp án 'A,C'. Câu trắc nghiệm phải có ít nhất 1 đáp án đúng.");
row++;

Title("BẢNG TRA: QuestionTypeId");
Line("Id", "Loại câu hỏi", bold: true);
Line("1", "Trắc nghiệm 1 đáp án (multiple_choice)");
Line("2", "Trắc nghiệm nhiều đáp án (multiple_select)");
Line("3", "Đúng/Sai (true_false)");
Line("4", "Điền vào chỗ trống (fill_blank)");
Line("5", "Tự luận (essay)");
Line("6", "Nối cột (matching)");
row++;

Title("BẢNG TRA: DifficultyLevelId");
Line("Id", "Độ khó", bold: true);
Line("1", "Dễ");
Line("2", "Trung bình");
Line("3", "Khó");
Line("4", "Rất khó");
row++;

Title("BẢNG TRA: CognitiveLevelId (Bloom)");
Line("Id", "Cấp độ nhận thức", bold: true);
Line("1", "Nhớ (Remember)");
Line("2", "Hiểu (Understand)");
Line("3", "Vận dụng (Apply)");
Line("4", "Phân tích (Analyze)");
Line("5", "Đánh giá (Evaluate)");
Line("6", "Sáng tạo (Create)");
row++;

Title("TopicId");
Line("Chủ đề phụ thuộc môn/lớp nên không cố định. Để trống để dùng 'Chủ đề mặc định' ở form,");
Line("hoặc tra Id trong màn Danh mục > Chủ đề của hệ thống.");

g.Column(1).Width = 22;
g.Column(2).Width = 70;

wb.SaveAs(outPath);
Console.WriteLine($"Đã sinh: {outPath}");
