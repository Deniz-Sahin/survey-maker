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

export default function LoginPage() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const dispatch = useAppDispatch()
  const navigate = useNavigate()

  const submit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      const res = await api.post('/api/auth/login', { email, password })
      const token: string = res.data.token
      if (!token) throw new Error('No token returned')

      // decode token to extract user info (email and role)
      const payload = parseJwt(token) || {}
      // possible claim names: "role", "roles", or claim URI
      let role: string | undefined
      if (payload.role) {
        role = Array.isArray(payload.role) ? payload.role[0] : payload.role
      } else if (payload.roles) {
        role = Array.isArray(payload.roles) ? payload.roles[0] : payload.roles
      } else {
        // check common claim URIs
        role =
          payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
          payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role']
      }

      const userEmail = payload.email || email

      // set axios default via interceptor (api instance uses store for header)
      dispatch(
        setCredentials({
          user: { email: userEmail, role: role ?? 'User' },
          token,
        }),
      )

      navigate('/')
    } catch (err) {
      // TODO: show friendly error UI
      console.error('Login failed', err)
      alert('Login failed')
    }
  }

  return (
    <Container maxWidth="xs">
      <Box sx={{ mt: 8 }}>
        <Typography variant="h5" component="h1" gutterBottom>
          Login
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
            autoComplete="current-password"
          />
          <Button type="submit" fullWidth variant="contained" sx={{ mt: 2 }}>
            Sign in
          </Button>
        </Box>
      </Box>
    </Container>
  )
}