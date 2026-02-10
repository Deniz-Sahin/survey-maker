import React from 'react'
import { Drawer, List, ListItemButton, ListItemIcon, ListItemText, Toolbar, Divider } from '@mui/material'
import InboxIcon from '@mui/icons-material/MoveToInbox'
import CreateIcon from '@mui/icons-material/Create'
import EditIcon from '@mui/icons-material/Edit'
import ListAltIcon from '@mui/icons-material/ListAlt'
import { NavLink } from 'react-router-dom'
import { useAppSelector } from '../hooks'

const drawerWidth = 240

export default function Sidebar() {
  const user = useAppSelector((s) => s.auth.user)

  const navItemsForUser = [
      { text: 'Surveys', to: '/user-surveys', icon: <InboxIcon /> },
  ]

  const navItemsForAdmin = [
    { text: 'Surveys (list / edit)', to: '/surveys', icon: <EditIcon /> },
      { text: 'Create Question', to: '/questions/create', icon: <CreateIcon /> },
      { text: 'Survey Submissions', to: '/surveys/submissions', icon: <ListAltIcon /> },
  ]

  const items = user?.role === 'Admin' ? navItemsForAdmin : navItemsForUser

  return (
    <Drawer
      variant="permanent"
      sx={{
        width: drawerWidth,
        flexShrink: 0,
        '& .MuiDrawer-paper': { width: drawerWidth, boxSizing: 'border-box' },
      }}
    >
      <Toolbar />
      <Divider />
      <List>
        {items.map((it) => (
          <ListItemButton key={it.to} component={NavLink} to={it.to}>
            <ListItemIcon>{it.icon}</ListItemIcon>
            <ListItemText primary={it.text} />
          </ListItemButton>
        ))}
      </List>
    </Drawer>
  )
}