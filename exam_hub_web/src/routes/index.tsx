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
import GeneratePage from '../pages/exams/GeneratePage'
import ExamListPage from '../pages/exams/ExamListPage'
import ExamSessionListPage from '../pages/exams/ExamSessionListPage'
import ExamCoverPage from '../pages/student/ExamCoverPage'
import ExamTakingPage from '../pages/student/ExamTakingPage'
import ExamResultPage from '../pages/student/ExamResultPage'
import StudentExamListPage from '../pages/student/StudentExamListPage'
import StudentLayout from '../layouts/StudentLayout'
import AppProfilePage from '../pages/profile/AppProfilePage'
import StudentProfilePage from '../pages/profile/StudentProfilePage'
import NoRolePage from '../pages/auth/NoRolePage'
import { Placeholder } from '../components/Placeholder'
import UserPage from '../pages/user/UserPage'
import SchoolListPage from '../pages/school/SchoolListPage'
import SchoolDetailPage from '../pages/school/SchoolDetailPage'
import CohortDetailPage from '../pages/school/CohortDetailPage'
import { ProtectedRoute } from './ProtectedRoute'
import { ROUTES } from './paths'

export { ROUTES }

export const router = createBrowserRouter([
    { path: ROUTES.HOME,     element: <Navigate to={ROUTES.LOGIN} replace /> },
    { path: ROUTES.LOGIN,    element: <LoginPage /> },
    { path: ROUTES.REGISTER, element: <RegisterPage /> },

    /* ── Student portal (with header layout) ── */
    {
        element: <StudentLayout />,
        children: [
            { path: ROUTES.STUDENT_EXAMS,   element: <StudentExamListPage /> },
            { path: ROUTES.STUDENT_PROFILE, element: <StudentProfilePage /> },
        ],
    },

    /* ── Student exam-taking flow (full screen, no header) ── */
    { path: ROUTES.STUDENT_EXAM,      element: <ExamCoverPage /> },
    { path: ROUTES.STUDENT_EXAM_TAKE, element: <ExamTakingPage /> },
    { path: '/student/exam/result',   element: <ExamResultPage /> },

    { path: ROUTES.FORBIDDEN, element: <Placeholder title="403 — Không có quyền truy cập" /> },
    { path: ROUTES.NO_ROLE,   element: <NoRolePage /> },

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
                    { path: 'questions/:id/edit',               element: <AddQuestionPage /> },
                    { path: 'exams',                            element: <ExamTemplatePage /> },
                    { path: 'exams/create',                     element: <CreateExamTemplatePage /> },
                    { path: 'exams/:id/edit',                   element: <CreateExamTemplatePage /> },
                    { path: 'category',                         element: <CategoryPage /> },
                    { path: 'generate',                         element: <GeneratePage /> },
                    { path: 'exam-list',                        element: <ExamListPage /> },
                    { path: 'exam-sessions',                    element: <ExamSessionListPage /> },
                    { path: 'users',                            element: <UserPage /> },
                    { path: 'schools',      element: <SchoolListPage /> },
                    { path: 'schools/:id',  element: <SchoolDetailPage /> },
                    { path: 'cohorts/:id',  element: <CohortDetailPage /> },
                    { path: 'profile',      element: <AppProfilePage /> },
                ],
            },
        ],
    },
])
