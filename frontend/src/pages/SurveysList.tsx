import React, { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Box, Container, Typography, List, ListItem, ListItemText, CircularProgress } from '@mui/material'
import api from '../api'
import { useAppSelector } from '../hooks'

type SurveyListItem = {
  id: number
  title: string
  description?: string
  isActive: boolean
  startDate?: string | null
  endDate?: string | null
}

export default function SurveysList() {
  const token = useAppSelector((s) => s.auth.token)
  const [surveys, setSurveys] = useState<SurveyListItem[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const navigate = useNavigate()

  useEffect(() => {
    loadAssignedPending()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token])

  async function loadAssignedPending() {
    setLoading(true)
    setError(null)
    try {
      // New backend endpoint returns only surveys assigned to current user that are not yet submitted
      const sRes = await api.get<SurveyListItem[]>('/api/surveys/assigned')
      setSurveys(sRes.data)
    } catch (e: any) {
      setError(e?.response?.data || e.message)
    } finally {
      setLoading(false)
    }
  }

  return (
    <Container>
      <Box sx={{ mt: 4 }}>
        <Typography variant="h4" gutterBottom>
          Your Surveys
        </Typography>

        {loading ? (
          <CircularProgress />
        ) : error ? (
          <Typography color="error">{String(error)}</Typography>
        ) : surveys.length === 0 ? (
          <Typography color="text.secondary">No pending surveys assigned to you.</Typography>
        ) : (
          <List>
            {surveys.map((s) => (
              <ListItem key={s.id} onClick={() => navigate(`/surveys/${s.id}`)}>
                <ListItemText
                  primary={`${s.title} ${s.isActive ? '' : '(Passive)'}`}
                  secondary={
                    <>
                      <Typography component="span" variant="body2">
                        {s.description}
                      </Typography>
                      <Box component="div" sx={{ mt: 0.5 }}>
                        {s.startDate ? `Start: ${new Date(s.startDate).toLocaleString()}` : 'Start: -'} —{' '}
                        {s.endDate ? `End: ${new Date(s.endDate).toLocaleString()}` : 'End: -'}
                      </Box>
                    </>
                  }
                />
              </ListItem>
            ))}
          </List>
        )}
      </Box>
    </Container>
  )
}