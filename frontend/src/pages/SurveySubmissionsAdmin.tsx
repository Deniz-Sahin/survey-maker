import React, { useEffect, useState } from 'react'
import {
  Box,
  Button,
  Container,
  Typography,
  Select,
  MenuItem,
  OutlinedInput,
  TextField,
  List,
  ListItem,
  ListItemText,
  Divider,
} from '@mui/material'
import api from '../api'
import { useAppSelector } from '../hooks'

type SurveyItem = { id: number; title: string }
type SubmissionAnswer = { questionId: number; optionId?: number | null; textAnswer?: string | null }
type Submission = {
  responseId: number
  userId?: string | null
  userEmail?: string | null
  submittedAt: string
  answers: SubmissionAnswer[]
}
type SubmissionsResult = {
  surveyId: number
  surveyTitle: string
  responses: Submission[]
  nonResponders: { id: string; email?: string | null }[]
}

export default function SurveySubmissionsAdmin() {
  const user = useAppSelector((s) => s.auth.user)
  const [surveys, setSurveys] = useState<SurveyItem[]>([])
  const [selectedSurvey, setSelectedSurvey] = useState<number | null>(null)
  const [data, setData] = useState<SubmissionsResult | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [filterText, setFilterText] = useState('')
  const [filterStatus, setFilterStatus] = useState<'all' | 'responded' | 'notresponded'>('all')

  useEffect(() => {
    loadSurveys()
  }, [])

  async function loadSurveys() {
    setError(null)
    try {
      const res = await api.get<SurveyItem[]>('/api/surveys')
      setSurveys(res.data)
    } catch (e: any) {
      setError(e?.response?.data || e.message)
    }
  }

  async function loadSubmissions(surveyId: number) {
    setLoading(true)
    setError(null)
    try {
      const res = await api.get<SubmissionsResult>(`/api/surveys/${surveyId}/submissions`)
      // normalize date strings
      res.data.responses.forEach((r: any) => {
        r.submittedAt = new Date(r.submittedAt).toString()
      })
      setData(res.data)
    } catch (e: any) {
      setError(e?.response?.data || e.message)
      setData(null)
    } finally {
      setLoading(false)
    }
  }

  function handleSurveyChange(id: number) {
    setSelectedSurvey(id)
    setData(null)
    loadSubmissions(id)
  }

  function filteredResponses() {
    if (!data) return []
    const q = filterText.trim().toLowerCase()
    const allResponses = data.responses
    let items = allResponses
    if (filterStatus === 'responded') {
      items = items
    } else if (filterStatus === 'notresponded') {
      items = []
    }
    if (!q) return items
    return items.filter(
      (r) =>
        (r.userEmail && r.userEmail.toLowerCase().includes(q)) ||
        (r.userId && r.userId.toLowerCase().includes(q)),
    )
  }

  function filteredNonResponders() {
    if (!data) return []
    const q = filterText.trim().toLowerCase()
    const items = data.nonResponders
    if (!q) return items
    return items.filter((u) => (u.email || u.id).toLowerCase().includes(q))
  }

  return (
    <Container>
      <Box sx={{ mt: 4 }}>
        <Typography variant="h4" gutterBottom>
          Survey Submissions (Admin)
        </Typography>

        <Box sx={{ mb: 2, display: 'flex', gap: 2, alignItems: 'center' }}>
          <Select
            value={selectedSurvey ?? ''}
            onChange={(e) => handleSurveyChange(Number(e.target.value))}
            displayEmpty
            input={<OutlinedInput />}
            sx={{ minWidth: 300 }}
          >
            <MenuItem value="">Select survey...</MenuItem>
            {surveys.map((s) => (
              <MenuItem key={s.id} value={s.id}>
                {s.title}
              </MenuItem>
            ))}
          </Select>

          <TextField
            placeholder="Filter by user email or id"
            value={filterText}
            onChange={(e) => setFilterText(e.target.value)}
            size="small"
          />

          <Select value={filterStatus} onChange={(e) => setFilterStatus(e.target.value as any)} size="small">
            <MenuItem value="all">All</MenuItem>
            <MenuItem value="responded">Responded</MenuItem>
            <MenuItem value="notresponded">Not Responded</MenuItem>
          </Select>
        </Box>

        {error && (
          <Typography color="error" sx={{ mb: 2 }}>
            {String(error)}
          </Typography>
        )}

        {!selectedSurvey && <Typography>Select a survey to see submissions.</Typography>}

        {selectedSurvey && !data && !loading && <Typography>No submission data loaded.</Typography>}

        {loading && <Typography>Loading...</Typography>}

        {data && (
          <Box>
            <Typography variant="h6" sx={{ mt: 2 }}>
              Responses ({data.responses.length})
            </Typography>
            <List>
              {filteredResponses().map((r) => (
                <Box key={r.responseId}>
                  <ListItem>
                    <ListItemText
                      primary={r.userEmail ?? r.userId ?? 'Anonymous'}
                      secondary={`Submitted at: ${new Date(r.submittedAt).toLocaleString()}`}
                    />
                  </ListItem>
                  <Box sx={{ pl: 4, pb: 2 }}>
                    {r.answers.map((a, idx) => (
                      <Box key={idx} sx={{ mb: 1 }}>
                        <Typography variant="body2">QuestionId: {a.questionId}</Typography>
                        <Typography variant="body2">OptionId: {a.optionId ?? '-'}</Typography>
                        <Typography variant="body2">Text: {a.textAnswer ?? '-'}</Typography>
                      </Box>
                    ))}
                  </Box>
                  <Divider />
                </Box>
              ))}
            </List>

            <Typography variant="h6" sx={{ mt: 3 }}>
              Not Responded ({data.nonResponders.length})
            </Typography>
            <List>
              {filterStatus !== 'responded' &&
                filteredNonResponders().map((u) => (
                  <ListItem key={u.id}>
                    <ListItemText primary={u.email ?? u.id} secondary={u.id} />
                  </ListItem>
                ))}
            </List>
          </Box>
        )}

        <Box sx={{ mt: 2 }}>
          <Button onClick={() => { setSelectedSurvey(null); setData(null); setFilterText(''); }}>
            Clear
          </Button>
        </Box>
      </Box>
    </Container>
  )
}