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

export const router = createBrowserRouter([
  { path: '/',         element: <Navigate to="/login" replace /> },
  { path: '/login',    element: <LoginPage /> },
  { path: '/register', element: <RegisterPage /> },

  /* ── Student pages (no sidebar) ── */
  { path: '/student/exam',      element: <ExamCoverPage /> },
  { path: '/student/exam/take', element: <ExamTakingPage /> },

  /* ── Admin / Teacher app ── */
  {
    path: '/app',
    element: <AppLayout />,
    children: [
      { index: true,            element: <Navigate to="/app/dashboard" replace /> },
      { path: 'dashboard',      element: <DashboardPage /> },
      { path: 'questions',      element: <QuestionBankPage /> },
      { path: 'questions/add',  element: <AddQuestionPage /> },
      { path: 'exams',          element: <ExamTemplatePage /> },
      { path: 'exams/create',   element: <CreateExamTemplatePage /> },
      { path: 'category',       element: <CategoryPage /> },
      { path: 'generate',       element: <Placeholder title="Sinh đề thi" /> },
      { path: 'users',          element: <Placeholder title="Người dùng" /> },
    ],
  },
])
