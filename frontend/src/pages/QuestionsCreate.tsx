import React, { useEffect, useState } from 'react'
import axios from 'axios'
import {
  Box,
  Button,
  Container,
  Typography,
  TextField,
  FormControlLabel,
  Checkbox,
  Select,
  MenuItem,
  OutlinedInput,
  Chip,
  Stack,
  IconButton,
  List,
  ListItem,
  ListItemText,
} from '@mui/material'
import DeleteIcon from '@mui/icons-material/Delete'
import EditIcon from '@mui/icons-material/Edit'
import { useAppSelector } from '../hooks'

type SurveyItem = { id: number; title: string }
type OptionDto = { id?: number | null; text: string }
type QuestionListItem = {
  id: number
  text: string
  isMultipleChoice: boolean
  options: OptionDto[]
  surveys: { id: number; title: string }[]
}

export default function QuestionsCreate() {
  const token = useAppSelector((s) => s.auth.token)
  const [surveys, setSurveys] = useState<SurveyItem[]>([])
  const [questions, setQuestions] = useState<QuestionListItem[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  // form state
  const [text, setText] = useState('')
  const [isMultipleChoice, setIsMultipleChoice] = useState(false)
  const [options, setOptions] = useState<OptionDto[]>([{ text: '' }, { text: '' }])
  const [selectedSurveyIds, setSelectedSurveyIds] = useState<number[]>([])
  const [editingId, setEditingId] = useState<number | null>(null)

  const api = axios.create({
    baseURL: 'http://localhost:5000',
    headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  })

  async function loadAll() {
    setLoading(true)
    setError(null)
    try {
      const [sRes, qRes] = await Promise.all([
        api.get<SurveyItem[]>('/api/surveys'),
        api.get<QuestionListItem[]>('/api/questions'),
      ])
      setSurveys(sRes.data)
      setQuestions(qRes.data)
    } catch (e: any) {
      setError(e?.response?.data || e.message)
    } finally {
      setLoading(false)
    }
  }

  function setOptionText(index: number, value: string) {
    setOptions((prev) => {
      const copy = [...prev]
      copy[index] = { ...copy[index], text: value }
      return copy
    })
  }

  function addOption() {
    if (options.length >= 4) return
    setOptions((p) => [...p, { text: '' }])
  }

  function removeOption(idx: number) {
    if (options.length <= 1) return
    setOptions((p) => p.filter((_, i) => i !== idx))
  }

  function resetForm() {
    setText('')
    setIsMultipleChoice(false)
    setOptions([{ text: '' }, { text: '' }])
    setSelectedSurveyIds([])
    setEditingId(null)
    setError(null)
  }

  function buildCreatePayload() {
    return {
      text,
      isMultipleChoice,
      surveyIds: selectedSurveyIds.length ? selectedSurveyIds : undefined,
      options: options.filter((o) => o.text.trim()).map((o) => ({ text: o.text.trim() })),
    }
  }

  function validateBeforeSend() {
    if (!text.trim()) {
      setError('Question text is required.')
      return false
    }
    const opts = options.filter((o) => o.text.trim())
    if (isMultipleChoice) {
      if (opts.length < 2 || opts.length > 4) {
        setError('Multiple choice questions must have between 2 and 4 options.')
        return false
      }
    } else if (opts.length > 0) {
      setError('Non-multiple-choice questions must not have options.')
      return false
    }
    return true
  }

  async function handleCreateOrUpdate() {
    setError(null)
    if (!validateBeforeSend()) return

    try {
      if (editingId == null) {
        const res = await api.post('/api/questions', buildCreatePayload())
        // created -> reload
        await loadAll()
        resetForm()
      } else {
        await api.put(`/api/questions/${editingId}`, buildCreatePayload())
        await loadAll()
        resetForm()
      }
    } catch (e: any) {
      const msg = e?.response?.data
      // try to read model state errors
      if (msg && typeof msg === 'object' && msg.errors) {
        setError(JSON.stringify(msg.errors))
      } else if (typeof msg === 'string') {
        setError(msg)
      } else {
        setError(e.message)
      }
    }
  }

  async function handleEdit(q: QuestionListItem) {
    setEditingId(q.id)
    setText(q.text)
    setIsMultipleChoice(q.isMultipleChoice)
    setOptions(q.options.length ? q.options.map((o) => ({ text: o.text })) : [{ text: '' }, { text: '' }])
    setSelectedSurveyIds(q.surveys.map((s) => s.id))
    setError(null)
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  async function handleDelete(id: number) {
    if (!confirm('Delete this question?')) return
    try {
      await api.delete(`/api/questions/${id}`)
      setQuestions((p) => p.filter((x) => x.id !== id))
    } catch (e: any) {
      setError(e?.response?.data || e.message)
    }
  }

  return (
    <Container>
      <Box sx={{ mt: 4 }}>
        <Typography variant="h4" gutterBottom>
          Questions
        </Typography>

        <Box sx={{ mb: 3, p: 2, border: '1px solid #ddd', borderRadius: 1 }}>
          <Typography variant="h6">{editingId ? 'Edit Question' : 'Create Question'}</Typography>

          <TextField
            label="Question text"
            value={text}
            onChange={(e) => setText(e.target.value)}
            fullWidth
            sx={{ my: 2 }}
          />

          <FormControlLabel
            control={
              <Checkbox
                checked={isMultipleChoice}
                onChange={(e) => {
                  setIsMultipleChoice(e.target.checked)
                  // if switching to MC ensure at least 2 options
                  if (e.target.checked && options.length < 2) setOptions([{ text: '' }, { text: '' }])
                  if (!e.target.checked) setOptions([{ text: '' }])
                }}
              />
            }
            label="Is multiple choice"
          />

          {isMultipleChoice && (
            <Box sx={{ mt: 2 }}>
              <Typography variant="subtitle2">Options (2 to 4)</Typography>
              <Stack spacing={1} sx={{ mt: 1 }}>
                {options.map((o, idx) => (
                  <Box key={idx} sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
                    <TextField
                      value={o.text}
                      onChange={(e) => setOptionText(idx, e.target.value)}
                      placeholder={`Option ${idx + 1}`}
                      fullWidth
                    />
                    <IconButton
                      onClick={() => removeOption(idx)}
                      disabled={options.length <= 2}
                      aria-label="remove option"
                      size="small"
                    >
                      <DeleteIcon fontSize="small" />
                    </IconButton>
                  </Box>
                ))}
                <Button onClick={addOption} disabled={options.length >= 4}>
                  Add option
                </Button>
              </Stack>
            </Box>
          )}

          {error && (
            <Typography color="error" sx={{ mt: 2 }}>
              {typeof error === 'string' ? error : JSON.stringify(error)}
            </Typography>
          )}

          <Box sx={{ mt: 2, display: 'flex', gap: 1 }}>
            <Button variant="contained" onClick={handleCreateOrUpdate}>
              {editingId ? 'Save' : 'Create'}
            </Button>
            <Button onClick={resetForm}>Reset</Button>
          </Box>
        </Box>

        <Typography variant="h6" sx={{ mb: 1 }}>
          Existing Questions
        </Typography>

        {loading ? (
          <Typography>Loading...</Typography>
        ) : (
          <List>
            {questions.map((q) => (
              <ListItem key={q.id} secondaryAction={
                <Box>
                  <IconButton edge="end" onClick={() => handleEdit(q)} aria-label="edit">
                    <EditIcon />
                  </IconButton>
                  <IconButton edge="end" onClick={() => handleDelete(q.id)} aria-label="delete">
                    <DeleteIcon />
                  </IconButton>
                </Box>
              }>
                <ListItemText
                  primary={q.text}
                  secondary={
                    <>
                      <Typography component="span" variant="body2">
                        {q.isMultipleChoice ? 'Multiple choice' : 'Text'} — Surveys: {q.surveys.map(s => s.title).join(', ')}
                      </Typography>
                      {q.options && q.options.length > 0 && (
                        <Box component="div" sx={{ mt: 0.5 }}>
                          Options: {q.options.map((o) => o.text).join(' | ')}
                        </Box>
                      )}
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