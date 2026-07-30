-- Chạy file này ĐẦU TIÊN — bổ sung topics còn thiếu cho lớp 1-9 và 4 môn lớp 10
BEGIN;

INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES
    ((SELECT id FROM public.subjects WHERE grade_level_id = 1 AND code = 'MATH'), 'Các số đến 10', 'C1', 1),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 1 AND code = 'MATH'), 'Phép cộng, phép trừ trong phạm vi 10', 'C2', 2),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 1 AND code = 'MATH'), 'Các số đến 20', 'C3', 3),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 1 AND code = 'MATH'), 'Phép cộng, phép trừ trong phạm vi 20', 'C4', 4),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 1 AND code = 'MATH'), 'Các số đến 100', 'C5', 5),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 1 AND code = 'MATH'), 'Đo lường và thời gian', 'C6', 6),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 1 AND code = 'MATH'), 'Hình học: điểm, đoạn thẳng, hình vuông, hình tròn', 'C7', 7);

INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES
    ((SELECT id FROM public.subjects WHERE grade_level_id = 1 AND code = 'VIE'), 'Âm và chữ cái', 'C1', 1),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 1 AND code = 'VIE'), 'Vần đơn giản', 'C2', 2),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 1 AND code = 'VIE'), 'Từ và câu đơn giản', 'C3', 3),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 1 AND code = 'VIE'), 'Tập đọc: Gia đình em', 'C4', 4),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 1 AND code = 'VIE'), 'Tập đọc: Trường lớp em', 'C5', 5),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 1 AND code = 'VIE'), 'Kể chuyện: Con vật quanh em', 'C6', 6),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 1 AND code = 'VIE'), 'Luyện viết chính tả', 'C7', 7);

INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES
    ((SELECT id FROM public.subjects WHERE grade_level_id = 2 AND code = 'MATH'), 'Ôn tập các số đến 100', 'C1', 1),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 2 AND code = 'MATH'), 'Phép cộng, trừ có nhớ trong phạm vi 100', 'C2', 2),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 2 AND code = 'MATH'), 'Các số đến 1000', 'C3', 3),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 2 AND code = 'MATH'), 'Bảng nhân, bảng chia 2, 3, 4, 5', 'C4', 4),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 2 AND code = 'MATH'), 'Đo lường: độ dài, khối lượng, thời gian', 'C5', 5),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 2 AND code = 'MATH'), 'Hình học: hình chữ nhật, hình tứ giác', 'C6', 6),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 2 AND code = 'MATH'), 'Giải toán có lời văn', 'C7', 7);

INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES
    ((SELECT id FROM public.subjects WHERE grade_level_id = 2 AND code = 'VIE'), 'Tập đọc: Bạn bè', 'C1', 1),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 2 AND code = 'VIE'), 'Tập đọc: Thầy cô', 'C2', 2),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 2 AND code = 'VIE'), 'Từ ngữ chỉ sự vật, hoạt động', 'C3', 3),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 2 AND code = 'VIE'), 'Câu kể, câu hỏi', 'C4', 4),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 2 AND code = 'VIE'), 'Luyện từ và câu', 'C5', 5),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 2 AND code = 'VIE'), 'Tập làm văn: Kể về gia đình', 'C6', 6),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 2 AND code = 'VIE'), 'Chính tả nghe — viết', 'C7', 7);

INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES
    ((SELECT id FROM public.subjects WHERE grade_level_id = 3 AND code = 'MATH'), 'Các số đến 1000', 'C1', 1),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 3 AND code = 'MATH'), 'Phép cộng, trừ trong phạm vi 1000', 'C2', 2),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 3 AND code = 'MATH'), 'Bảng nhân, bảng chia 6, 7, 8, 9', 'C3', 3),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 3 AND code = 'MATH'), 'Các số đến 10000', 'C4', 4),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 3 AND code = 'MATH'), 'Phép nhân, chia số có nhiều chữ số', 'C5', 5),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 3 AND code = 'MATH'), 'Hình học: chu vi, diện tích hình chữ nhật, hình vuông', 'C6', 6),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 3 AND code = 'MATH'), 'Đo lường: đơn vị đo độ dài, khối lượng, thời gian', 'C7', 7);

INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES
    ((SELECT id FROM public.subjects WHERE grade_level_id = 3 AND code = 'VIE'), 'Tập đọc: Quê hương', 'C1', 1),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 3 AND code = 'VIE'), 'Từ loại: danh từ, động từ, tính từ', 'C2', 2),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 3 AND code = 'VIE'), 'Biện pháp so sánh, nhân hóa', 'C3', 3),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 3 AND code = 'VIE'), 'Câu cảm, câu khiến', 'C4', 4),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 3 AND code = 'VIE'), 'Tập làm văn: Kể chuyện', 'C5', 5),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 3 AND code = 'VIE'), 'Tập làm văn: Miêu tả đồ vật', 'C6', 6);

INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES
    ((SELECT id FROM public.subjects WHERE grade_level_id = 3 AND code = 'TNXH'), 'Con người và sức khỏe', 'C1', 1),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 3 AND code = 'TNXH'), 'Gia đình, trường học, cộng đồng', 'C2', 2),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 3 AND code = 'TNXH'), 'Thực vật và động vật', 'C3', 3),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 3 AND code = 'TNXH'), 'Trái Đất và bầu trời', 'C4', 4);

INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES
    ((SELECT id FROM public.subjects WHERE grade_level_id = 4 AND code = 'MATH'), 'Số tự nhiên và các phép tính', 'C1', 1),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 4 AND code = 'MATH'), 'Phân số', 'C2', 2),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 4 AND code = 'MATH'), 'Hình học: góc và đường thẳng', 'C3', 3),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 4 AND code = 'MATH'), 'Đại lượng và đo đại lượng', 'C4', 4),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 4 AND code = 'MATH'), 'Biểu đồ và số liệu thống kê', 'C5', 5),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 4 AND code = 'MATH'), 'Diện tích hình bình hành, hình thoi', 'C6', 6);

INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES
    ((SELECT id FROM public.subjects WHERE grade_level_id = 4 AND code = 'VIE'), 'Tập đọc: Con người Việt Nam', 'C1', 1),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 4 AND code = 'VIE'), 'Từ và cấu tạo từ', 'C2', 2),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 4 AND code = 'VIE'), 'Câu ghép', 'C3', 3),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 4 AND code = 'VIE'), 'Tập làm văn: Miêu tả cây cối', 'C4', 4),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 4 AND code = 'VIE'), 'Tập làm văn: Miêu tả con vật', 'C5', 5),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 4 AND code = 'VIE'), 'Kể chuyện được chứng kiến', 'C6', 6);

INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES
    ((SELECT id FROM public.subjects WHERE grade_level_id = 4 AND code = 'SCI'), 'Con người và sức khỏe', 'C1', 1),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 4 AND code = 'SCI'), 'Vật chất và năng lượng', 'C2', 2),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 4 AND code = 'SCI'), 'Thực vật và động vật', 'C3', 3),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 4 AND code = 'SCI'), 'Môi trường và tài nguyên thiên nhiên', 'C4', 4);

INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES
    ((SELECT id FROM public.subjects WHERE grade_level_id = 5 AND code = 'MATH'), 'Ôn tập và bổ sung về phân số', 'C1', 1),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 5 AND code = 'MATH'), 'Số thập phân', 'C2', 2),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 5 AND code = 'MATH'), 'Phép tính với số thập phân', 'C3', 3),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 5 AND code = 'MATH'), 'Hình học: diện tích, thể tích', 'C4', 4),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 5 AND code = 'MATH'), 'Tỉ số phần trăm', 'C5', 5),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 5 AND code = 'MATH'), 'Chuyển động đều', 'C6', 6);

INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES
    ((SELECT id FROM public.subjects WHERE grade_level_id = 5 AND code = 'VIE'), 'Tập đọc: Đất nước ngàn năm văn hiến', 'C1', 1),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 5 AND code = 'VIE'), 'Từ đồng nghĩa, từ trái nghĩa', 'C2', 2),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 5 AND code = 'VIE'), 'Câu ghép và quan hệ từ', 'C3', 3),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 5 AND code = 'VIE'), 'Tập làm văn: Tả người', 'C4', 4),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 5 AND code = 'VIE'), 'Tập làm văn: Tả cảnh', 'C5', 5),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 5 AND code = 'VIE'), 'Kể chuyện về Bác Hồ', 'C6', 6);

INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES
    ((SELECT id FROM public.subjects WHERE grade_level_id = 5 AND code = 'SCI'), 'Sự sinh sản và phát triển của sinh vật', 'C1', 1),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 5 AND code = 'SCI'), 'Vật chất và năng lượng', 'C2', 2),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 5 AND code = 'SCI'), 'Môi trường và tài nguyên', 'C3', 3),
    ((SELECT id FROM public.subjects WHERE grade_level_id = 5 AND code = 'SCI'), 'An toàn trong cuộc sống', 'C4', 4);
COMMIT;