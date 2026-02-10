import React, { useState } from 'react'
import { Box, Button, Container, TextField, Typography } from '@mui/material'
import { useAppDispatch } from '../hooks'
import { setCredentials } from '../features/auth/authSlice'
import { useNavigate } from 'react-router-dom'
import api from '../api/axios'

function parseJwt(token: string): any {
  try {
    const base64Url = token.split('.')[1]
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join(''),
    )
    return JSON.parse(jsonPayload)
  } catch {
    return null
  }
}

export default function RegisterPage() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [role, setRole] = useState<'User' | 'Admin'>('User')
  const dispatch = useAppDispatch()
  const navigate = useNavigate()

  const submit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      // register
      await api.post('/api/auth/register', { email, password, role })

      // login after successful register to get JWT
      const res = await api.post('/api/auth/login', { email, password })
      const token: string = res.data.token
      if (!token) throw new Error('No token returned')

      const payload = parseJwt(token) || {}

      let parsedRole: string | undefined
      if (payload.role) {
        parsedRole = Array.isArray(payload.role) ? payload.role[0] : payload.role
      } else if (payload.roles) {
        parsedRole = Array.isArray(payload.roles) ? payload.roles[0] : payload.roles
      } else {
        parsedRole =
          payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
          payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role']
      }

      const userEmail = payload.email || email

      dispatch(
        setCredentials({
          user: { email: userEmail, role: parsedRole ?? role },
          token,
        }),
      )

      navigate('/')
    } catch (err) {
      console.error('Register failed', err)
      alert('Register failed')
    }
  }

  return (
    <Container maxWidth="xs">
      <Box sx={{ mt: 8 }}>
        <Typography variant="h5" component="h1" gutterBottom>
          Register
        </Typography>
        <Box component="form" onSubmit={submit} noValidate>
          <TextField
            margin="normal"
            required
            fullWidth
            label="Email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            autoComplete="email"
          />
          <TextField
            margin="normal"
            required
            fullWidth
            label="Password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            autoComplete="new-password"
          />
          <Button type="submit" fullWidth variant="contained" sx={{ mt: 2 }}>
            Register
          </Button>
        </Box>
      </Box>
    </Container>
  )
}