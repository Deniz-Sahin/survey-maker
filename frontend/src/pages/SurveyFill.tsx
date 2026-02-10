import React, { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import {
  Box,
  Button,
  Container,
  Typography,
  TextField,
  RadioGroup,
  FormControlLabel,
  Radio,
  CircularProgress,
  Alert,
} from '@mui/material'
import api from '../api'
import { useAppSelector } from '../hooks'

type OptionDto = { id?: number | null; text: string }
type QuestionDto = {
  id: number
  text: string
  isMultipleChoice: boolean
  options: OptionDto[]
}

type SurveyDto = {
  id: number
  title: string
  description?: string
  isActive: boolean
  startDate?: string | null
  endDate?: string | null
  questions: QuestionDto[]
}

export default function SurveyFill() {
  const { id } = useParams()
  const navigate = useNavigate()
  const token = useAppSelector((s) => s.auth.token)
  const [survey, setSurvey] = useState<SurveyDto | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)
  const [successMsg, setSuccessMsg] = useState<string | null>(null)

  // answers: questionId -> optionId | text
  const [selectedOptions, setSelectedOptions] = useState<Record<number, number | null>>({})
  const [textAnswers, setTextAnswers] = useState<Record<number, string>>({})

  useEffect(() => {
    if (!id) return
    loadSurvey()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id, token])

  async function loadSurvey() {
    setLoading(true)
    setError(null)
    try {
      const res = await api.get<SurveyDto>(`/api/surveys/${id}`)
      setSurvey(res.data)
      // initialize answer state
      const optState: Record<number, number | null> = {}
      const txtState: Record<number, string> = {}
      res.data.questions.forEach((q) => {
        optState[q.id] = null
        txtState[q.id] = ''
      })
      setSelectedOptions(optState)
      setTextAnswers(txtState)
    } catch (e: any) {
      setError(e?.response?.data || e.message)
    } finally {
      setLoading(false)
    }
  }

  function handleSelectOption(questionId: number, optionId: number) {
    setSelectedOptions((p) => ({ ...p, [questionId]: optionId }))
  }

  function handleTextChange(questionId: number, value: string) {
    setTextAnswers((p) => ({ ...p, [questionId]: value }))
  }

  function validateBeforeSubmit() {
    if (!survey) return false
    for (const q of survey.questions) {
      if (q.isMultipleChoice) {
        if (!selectedOptions[q.id]) {
          setError(`Please select an option for: "${q.text}"`)
          return false
        }
      } else {
        if (!textAnswers[q.id] || textAnswers[q.id].trim() === '') {
          setError(`Please provide an answer for: "${q.text}"`)
          return false
        }
      }
    }
    return true
  }

  async function handleSubmit() {
    setError(null)
    setSuccessMsg(null)
    if (!validateBeforeSubmit()) return
    if (!survey) return
    setSubmitting(true)
    try {
      const answers = survey.questions.map((q) => {
        if (q.isMultipleChoice) {
          return { questionId: q.id, optionId: selectedOptions[q.id] ?? null, textAnswer: null }
        }
        return { questionId: q.id, optionId: null, textAnswer: textAnswers[q.id] ?? '' }
      })
      await api.post(`/api/surveys/${survey.id}/responses`, { answers })
      setSuccessMsg('Survey submitted. Thank you.')
      // optionally navigate away after a short delay
      setTimeout(() => navigate('/user-surveys'), 1200)
    } catch (e: any) {
      setError(e?.response?.data || e.message)
    } finally {
      setSubmitting(false)
    }
  }

  if (loading) return <CircularProgress />

  if (error)
    return (
      <Container maxWidth="lg">
        <Box sx={{ mt: 4, maxWidth: 1000, mx: 'auto' }}>
          <Alert severity="error">{String(error)}</Alert>
        </Box>
      </Container>
    )

  if (!survey)
    return (
      <Container maxWidth="lg">
        <Box sx={{ mt: 4, maxWidth: 1000, mx: 'auto' }}>
          <Typography>No survey found.</Typography>
        </Box>
      </Container>
    )

  return (
    <Container maxWidth="lg">
      <Box sx={{ mt: 4, maxWidth: 1000, mx: 'auto', width: '100%' }}>
        <Typography variant="h4" gutterBottom>
          {survey.title}
        </Typography>
        {survey.description && (
          <Typography color="text.secondary" sx={{ mb: 2 }}>
            {survey.description}
          </Typography>
        )}

        {survey.questions.map((q) => (
          <Box key={q.id} sx={{ mb: 3, p: 2, border: '1px solid #eee', borderRadius: 1, width: '100%' }}>
            <Typography variant="subtitle1">{q.text}</Typography>
            {q.isMultipleChoice ? (
              <RadioGroup
                value={selectedOptions[q.id] ?? ''}
                onChange={(_, v) => handleSelectOption(q.id, Number(v))}
                name={`q-${q.id}`}
              >
                {q.options.map((o) => (
                  <FormControlLabel
                    key={o.id ?? o.text}
                    value={o.id ?? ''}
                    control={<Radio />}
                    label={o.text}
                    sx={{ display: 'block', width: '100%' }}
                  />
                ))}
              </RadioGroup>
            ) : (
              <TextField
                fullWidth
                multiline
                rows={3}
                value={textAnswers[q.id] ?? ''}
                onChange={(e) => handleTextChange(q.id, e.target.value)}
                sx={{ mt: 1 }}
              />
            )}
          </Box>
        ))}

        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {String(error)}
          </Alert>
        )}
        {successMsg && (
          <Alert severity="success" sx={{ mb: 2 }}>
            {successMsg}
          </Alert>
        )}

        <Box sx={{ display: 'flex', gap: 1, justifyContent: 'flex-start' }}>
          <Button variant="contained" onClick={handleSubmit} disabled={submitting}>
            Submit
          </Button>
          <Button onClick={() => navigate(-1)} disabled={submitting}>
            Cancel
          </Button>
        </Box>
      </Box>
    </Container>
  )
}