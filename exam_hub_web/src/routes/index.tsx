import { createBrowserRouter, Navigate } from 'react-router-dom'
import LoginPage from '../pages/auth/LoginPage'
import RegisterPage from '../pages/auth/RegisterPage'
import AppLayout from '../layouts/AppLayout'
import DashboardPage from '../pages/dashboard/DashboardPage'
import CategoryPage from '../pages/category/CategoryPage'
import QuestionBankPage from '../pages/questions/QuestionBankPage'
import AddQuestionPage from '../pages/questions/AddQuestionPage'
import ExamTemplatePage from '../pages/exams/ExamTemplatePage'
import CreateExamTemplatePage from '../pages/exams/CreateExamTemplatePage'
import ExamCoverPage from '../pages/student/ExamCoverPage'
import ExamTakingPage from '../pages/student/ExamTakingPage'
import { Placeholder } from '../components/Placeholder'
import UserPage from '../pages/user/UserPage'
import { ProtectedRoute } from './ProtectedRoute'
import { ROUTES } from './paths'

export { ROUTES }

export const router = createBrowserRouter([
    { path: ROUTES.HOME,     element: <Navigate to={ROUTES.LOGIN} replace /> },
    { path: ROUTES.LOGIN,    element: <LoginPage /> },
    { path: ROUTES.REGISTER, element: <RegisterPage /> },

    /* ── Student pages (no sidebar, no auth guard) ── */
    { path: ROUTES.STUDENT_EXAM,      element: <ExamCoverPage /> },
    { path: ROUTES.STUDENT_EXAM_TAKE, element: <ExamTakingPage /> },

    { path: ROUTES.FORBIDDEN, element: <Placeholder title="403 — Không có quyền truy cập" /> },

    /* ── Protected admin / teacher app ── */
    {
        element: <ProtectedRoute />,
        children: [
            {
                path: ROUTES.APP,
                element: <AppLayout />,
                children: [
                    { index: true,                              element: <Navigate to={ROUTES.DASHBOARD} replace /> },
                    { path: 'dashboard',                        element: <DashboardPage /> },
                    { path: 'questions',                        element: <QuestionBankPage /> },
                    { path: 'questions/add',                    element: <AddQuestionPage /> },
                    { path: 'exams',                            element: <ExamTemplatePage /> },
                    { path: 'exams/create',                     element: <CreateExamTemplatePage /> },
                    { path: 'category',                         element: <CategoryPage /> },
                    { path: 'generate',                         element: <Placeholder title="Sinh đề thi" /> },
                    { path: 'users',                            element: <UserPage /> },
                ],
            },
        ],
    },
])
