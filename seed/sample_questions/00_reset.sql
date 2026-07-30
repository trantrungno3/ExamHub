-- ============================================================
-- RESET: xóa dữ liệu mẫu đã sinh trước đó (nếu có) trước khi insert lại
-- Chỉ xóa đúng phần dữ liệu do bộ script "Ngân hàng câu hỏi mẫu - sinh tự động" tạo ra
-- và các topic thuộc lớp 1, 2, 3, 4, 5 — KHÔNG đụng tới dữ liệu khác (vd topics lớp 10 có sẵn trong schema gốc)
-- An toàn khi chạy lại nhiều lần.
-- ============================================================
BEGIN;

-- 1) Gỡ tham chiếu trong đề thi (nếu đã lỡ dùng các câu hỏi mẫu này để tạo đề)
DELETE FROM public.exam_questions
WHERE question_id IN (
    SELECT id FROM public.questions WHERE source = 'Ngân hàng câu hỏi mẫu - sinh tự động'
);

-- 2) Xóa đáp án của các câu hỏi mẫu (question_answers có ON DELETE CASCADE nên bước này
--    thật ra sẽ tự động xảy ra ở bước 3, nhưng xóa tường minh trước cho rõ ràng)
DELETE FROM public.question_answers
WHERE question_id IN (
    SELECT id FROM public.questions WHERE source = 'Ngân hàng câu hỏi mẫu - sinh tự động'
);

-- 3) Xóa các câu hỏi mẫu
DELETE FROM public.questions
WHERE source = 'Ngân hàng câu hỏi mẫu - sinh tự động';

-- 4) Xóa các topics đã tạo cho lớp 1, 2, 3, 4, 5 (không đụng topics lớp 10 có sẵn trong schema gốc)
DELETE FROM public.topics
WHERE subject_id IN (
    SELECT id FROM public.subjects WHERE grade_level_id IN (1, 2, 3, 4, 5)
);

COMMIT;
