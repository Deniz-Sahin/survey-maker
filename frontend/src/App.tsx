import React, { useEffect } from 'react'
import { Provider } from 'react-redux'
import { store } from './store'
import { BrowserRouter, Routes, Route, Link, Navigate } from 'react-router-dom'
import CssBaseline from '@mui/material/CssBaseline'
import { ThemeProvider, createTheme, Box, Toolbar, AppBar, Typography, Button } from '@mui/material'
import Sidebar from './components/Sidebar'
import LoginPage from './pages/Login'
import RegisterPage from './pages/Register'
import SurveysList from './pages/SurveysList'
import QuestionsCreate from './pages/QuestionsCreate'
import SurveysAdmin from './pages/SurveysAdmin'
import SurveyFill from './pages/SurveyFill'
import SurveySubmissionsAdmin from './pages/SurveySubmissionsAdmin'
import { useAppSelector, useAppDispatch } from './hooks'
import { logout } from './features/auth/authSlice'
import { setAuthToken, clearAuthToken } from './api'

const theme = createTheme()

function TopBar() {
  const user = useAppSelector((s) => s.auth.user)
  const dispatch = useAppDispatch()

  return (
    <AppBar position="fixed" sx={{ zIndex: (t) => t.zIndex.drawer + 1 }}>
      <Toolbar>
        <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
          Survey Maker
        </Typography>
        {!user ? (
          <>
            <Button color="inherit" component={Link} to="/login">
              Login
            </Button>
            <Button color="inherit" component={Link} to="/register">
              Register
            </Button>
          </>
        ) : (
          <>
            <Typography sx={{ mr: 2 }}>{user.email} ({user.role})</Typography>
            <Button color="inherit" onClick={() => dispatch(logout())}>
              Logout
            </Button>
          </>
        )}
      </Toolbar>
    </AppBar>
  )
}

/**
 * Protect routes by redirecting unauthenticated users to /login
 */
function ProtectedRoute({ children }: { children: React.ReactElement }) {
  const user = useAppSelector((s) => s.auth.user)
  if (!user) return <Navigate to="/login" replace />
  return children
}

/**
 * Sync auth token from Redux into the centralized axios instance.
 * Must be rendered inside the Redux Provider.
 */
function AuthTokenSync() {
  const token = useAppSelector((s) => s.auth.token)

  useEffect(() => {
    if (token) setAuthToken(token)
    else clearAuthToken()
  }, [token])

  return null
}

function AppRouter() {
  const user = useAppSelector((s) => s.auth.user)

  return (
    <BrowserRouter>
      <TopBar />
      {user && <Sidebar />}
      <Box component="main" sx={{ ml: user ? '240px' : 0, p: 3 }}>
        <Toolbar />
        <Routes>
          <Route
            path="/"
            element={
              <ProtectedRoute>
                <SurveysList />
              </ProtectedRoute>
            }
          />
          <Route
            path="/user-surveys"
            element={
              <ProtectedRoute>
                <SurveysList />
              </ProtectedRoute>
            }
          />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />

          <Route
            path="/surveys"
            element={
              <ProtectedRoute>
                <SurveysAdmin />
              </ProtectedRoute>
            }
          />
          <Route
            path="/surveys/:id"
            element={
              <ProtectedRoute>
                <SurveyFill />
              </ProtectedRoute>
            }
          />

          <Route
            path="/questions/create"
            element={
              <ProtectedRoute>
                <QuestionsCreate />
              </ProtectedRoute>
            }
          />
          <Route
            path="/surveys/submissions"
            element={
              <ProtectedRoute>
                <SurveySubmissionsAdmin />
              </ProtectedRoute>
            }
          />
          {/* Catch-all */}
          <Route
            path="*"
            element={user ? <Navigate to="/user-surveys" replace /> : <Navigate to="/login" replace />}
          />
        </Routes>
      </Box>
    </BrowserRouter>
  )
}

export default function App() {
  return (
    <Provider store={store}>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <AuthTokenSync />
        <AppRouter />
      </ThemeProvider>
    </Provider>
  )
}