import React, { useEffect, useState } from 'react'
import api from '../api'
import {
  Box,
  Button,
  Container,
  Typography,
  TextField,
  Select,
  MenuItem,
  OutlinedInput,
  Chip,
  Stack,
  IconButton,
  List,
  ListItem,
  ListItemText,
  FormControlLabel,
  Checkbox,
} from '@mui/material'
import DeleteIcon from '@mui/icons-material/Delete'
import EditIcon from '@mui/icons-material/Edit'
import { useAppSelector } from '../hooks'

type SurveyListItem = {
  id: number
  title: string
  description?: string
  isActive: boolean
  startDate?: string | null
  endDate?: string | null
}

type QuestionItem = {
  id: number
  text: string
}

type UserItem = {
  id: string
  email: string
  userName?: string
}

export default function SurveysAdmin() {
  const user = useAppSelector((s) => s.auth.user)
  const [surveys, setSurveys] = useState<SurveyListItem[]>([])
  const [questions, setQuestions] = useState<QuestionItem[]>([])
  const [users, setUsers] = useState<UserItem[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  // form state
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [selectedQuestionIds, setSelectedQuestionIds] = useState<number[]>([])
  const [selectedUserIds, setSelectedUserIds] = useState<string[]>([])
  const [start, setStart] = useState<string>('') // local datetime-local value
  const [end, setEnd] = useState<string>('')
  const [isActive, setIsActive] = useState(true)
  const [editingId, setEditingId] = useState<number | null>(null)

  // field-level validation state
  const [titleError, setTitleError] = useState<string | null>(null)
  const [dateError, setDateError] = useState<string | null>(null)

  useEffect(() => {
    loadAll()
  }, [])

  async function loadAll() {
    setLoading(true)
    setError(null)
    try {
      const [sRes, qRes, uRes] = await Promise.all([
        api.get<SurveyListItem[]>('/api/surveys'),
        api.get<QuestionItem[]>('/api/questions'),
        api.get<UserItem[]>('/api/users'),
      ])
      setSurveys(sRes.data)
      // map questions to minimal shape
      setQuestions(qRes.data.map((q) => ({ id: q.id, text: q.text })))
      setUsers(uRes.data)
    } catch (e: any) {
      setError(e?.response?.data || e.message)
    } finally {
      setLoading(false)
    }
  }

  function resetForm() {
    setTitle('')
    setDescription('')
    setSelectedQuestionIds([])
    setSelectedUserIds([])
    setStart('')
    setEnd('')
    setIsActive(true)
    setEditingId(null)
    setError(null)
    setTitleError(null)
    setDateError(null)
  }

  function toIsoOrNull(value: string) {
    if (!value) return null
    const d = new Date(value)
    return isNaN(d.getTime()) ? null : d.toISOString()
  }

  function buildPayload() {
    return {
      title,
      description,
      isActive,
      startDate: toIsoOrNull(start),
      endDate: toIsoOrNull(end),
      // send questionIds (existing question associations only)
      questionIds: selectedQuestionIds.length ? selectedQuestionIds : undefined,
      assignedUserIds: selectedUserIds.length ? selectedUserIds : undefined,
    }
  }

  function validate() {
    // clear previous field errors
    setTitleError(null)
    setDateError(null)
    setError(null)

    if (!title.trim()) {
      setTitleError('Title is required.')
      return false
    }
    if (start && end) {
      const s = new Date(start)
      const e = new Date(end)
      if (isNaN(s.getTime()) || isNaN(e.getTime())) {
        setDateError('Invalid start or end datetime.')
        return false
      }
      if (s > e) {
        setDateError('Start date must be before end date.')
        return false
      }
    }
    return true
  }

  async function handleSave() {
    // Clear general error, field errors will be set by validate()
    setError(null)
    if (!validate()) return
    try {
      if (editingId == null) {
        await api.post('/api/surveys', buildPayload())
      } else {
        await api.put(`/api/surveys/${editingId}`, buildPayload())
      }
      await loadAll()
      resetForm()
    } catch (e: any) {
      const msg = e?.response?.data
      if (msg && typeof msg === 'object') {
        // server may return { error: "..." } or validation shape
        if (msg.error && typeof msg.error === 'string') {
          setError(msg.error)
        } else if (msg.errors) {
          setError(JSON.stringify(msg.errors))
        } else {
          setError(JSON.stringify(msg))
        }
      } else if (typeof msg === 'string') {
        setError(msg)
      } else {
        setError(e.message)
      }
    }
  }

  function handleEdit(s: SurveyListItem) {
    setEditingId(s.id)
    setTitle(s.title)
    setDescription(s.description ?? '')
    setIsActive(!!s.isActive)
    setStart(s.startDate ? new Date(s.startDate).toISOString().slice(0, 16) : '')
    setEnd(s.endDate ? new Date(s.endDate).toISOString().slice(0, 16) : '')
    api
      .get<any>(`/api/surveys/${s.id}`)
      .then((res) => {
        setSelectedQuestionIds(res.data.questions?.map((q: any) => q.id) ?? [])
        setSelectedUserIds(res.data.assignedUserIds ?? [])
      })
      .catch((err) => setError(err?.response?.data || err.message))
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  async function handleDelete(id: number) {
    if (!confirm('Delete this survey?')) return
    try {
      await api.delete(`/api/surveys/${id}`)
      setSurveys((p) => p.filter((x) => x.id !== id))
    } catch (e: any) {
      setError(e?.response?.data || e.message)
    }
  }

  return (
    <Container>
      <Box sx={{ mt: 4 }}>
        <Typography variant="h4" gutterBottom>
          Surveys (Admin)
        </Typography>

        <Box sx={{ mb: 3, p: 2, border: '1px solid #ddd', borderRadius: 1 }}>
          <Typography variant="h6">{editingId ? 'Edit Survey' : 'Create Survey'}</Typography>

          <TextField
            label="Title"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            fullWidth
            sx={{ my: 1 }}
            error={!!titleError}
            helperText={titleError ?? ''}
          />

          <TextField
            label="Description"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            fullWidth
            multiline
            rows={3}
            sx={{ my: 1 }}
          />

          <Box sx={{ mt: 1 }}>
            <Typography variant="subtitle2">Select Questions</Typography>
            <Select
              multiple
              value={selectedQuestionIds}
              onChange={(e) => setSelectedQuestionIds(e.target.value as number[])}
              input={<OutlinedInput label="Questions" />}
              renderValue={(sel) => (
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                  {(sel as number[]).map((id) => {
                    const q = questions.find((x) => x.id === id)
                    return <Chip key={id} label={q ? q.text : id} />
                  })}
                </Box>
              )}
              fullWidth
              sx={{ mt: 1 }}
            >
              {questions.map((q) => (
                <MenuItem key={q.id} value={q.id}>
                  {q.text}
                </MenuItem>
              ))}
            </Select>
          </Box>

          <Box sx={{ mt: 2 }}>
            <Typography variant="subtitle2">Assign to Users</Typography>
            <Select
              multiple
              value={selectedUserIds}
              onChange={(e) => setSelectedUserIds(e.target.value as string[])}
              input={<OutlinedInput label="Users" />}
              renderValue={(sel) => (
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                  {(sel as string[]).map((id) => {
                    const u = users.find((x) => x.id === id)
                    return <Chip key={id} label={u ? u.email : id} />
                  })}
                </Box>
              )}
              fullWidth
              sx={{ mt: 1 }}
            >
              {users.map((u) => (
                <MenuItem key={u.id} value={u.id}>
                  {u.email}
                </MenuItem>
              ))}
            </Select>
          </Box>

          <Box sx={{ display: 'flex', gap: 2, mt: 2 }}>
            <TextField
              label="Start"
              type="datetime-local"
              value={start}
              onChange={(e) => setStart(e.target.value)}
              InputLabelProps={{ shrink: true }}
              error={!!dateError}
              helperText={dateError ?? ''}
            />
            <TextField
              label="End"
              type="datetime-local"
              value={end}
              onChange={(e) => setEnd(e.target.value)}
              InputLabelProps={{ shrink: true }}
              error={!!dateError}
              helperText={dateError ?? ''}
            />
            <FormControlLabel
              control={<Checkbox checked={isActive} onChange={(e) => setIsActive(e.target.checked)} />}
              label="Active"
            />
          </Box>

          {error && (
            <Typography color="error" sx={{ mt: 2 }}>
              {error}
            </Typography>
          )}

          <Box sx={{ mt: 2, display: 'flex', gap: 1 }}>
            <Button variant="contained" onClick={handleSave} disabled={user?.role !== 'Admin'}>
              {editingId ? 'Save' : 'Create'}
            </Button>
            <Button onClick={resetForm}>Reset</Button>
          </Box>
        </Box>

        <Typography variant="h6" sx={{ mb: 1 }}>
          Existing Surveys
        </Typography>

        {loading ? (
          <Typography>Loading...</Typography>
        ) : (
          <List>
            {surveys.map((s) => (
              <ListItem
                key={s.id}
                secondaryAction={
                  <Box>
                    <IconButton edge="end" onClick={() => handleEdit(s)} aria-label="edit" disabled={user?.role !== 'Admin'}>
                      <EditIcon />
                    </IconButton>
                    <IconButton edge="end" onClick={() => handleDelete(s.id)} aria-label="delete" disabled={user?.role !== 'Admin'}>
                      <DeleteIcon />
                    </IconButton>
                  </Box>
                }
              >
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