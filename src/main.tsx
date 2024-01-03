import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.tsx'
import './index.css'
import { createBrowserRouter, RouterProvider } from 'react-router-dom'

import Login from './pages/auth/Login.tsx'
import Register from './pages/auth/Register.tsx'
import Error404 from './Error404.tsx'
import ForgottenPassword from './pages/auth/ForgottenPassword.tsx'
import AccountConfig from './pages/auth/AccountConfig.tsx'
import Users from './pages/auth/Users.tsx'
import UserBlogs from './pages/blog/UserBlogs.tsx'
import CreatePost from './pages/post/CreatePost.tsx'
import UpdatePost from './pages/post/UpdatePost.tsx'
import PostView from './pages/post/PostView.tsx'
import BlogView from './pages/blog/BlogView.tsx'
import SearchPublicPost from './pages/post/SearchPublicPost.tsx'
import SearchPublicPostInBlog from './pages/post/SearchPublicPostInBlog.tsx'
import Home from './pages/blog/Home.tsx'

const router = createBrowserRouter([
  {
    path: '/',
    element: <App />,
    errorElement: <Error404/>,
    children: [
      {
        path: '',
        element: <Home />
      },
      {
        path: 'login',
        element: <Login />
      },
      {
        path: 'register',
        element: <Register />
      },
      {
        path: 'forgotten-password',
        element: <ForgottenPassword />
      },
      {
        path: 'account-config',
        element: <AccountConfig />
      },
      {
        path: 'users',
        element: <Users />
      },
      {
        path: 'user-blogs',
        element: <UserBlogs />
      },
      {
        path: 'blog/:blogId/:blogTitle/preview',
        element: <BlogView />
      },
      {
        path: 'blog/:blogId/:blogTitle',
        element: <BlogView />
      },
      {
        path: 'blog/:blogId/:blogTitle/create-post',
        element: <CreatePost />
      },
      {
        path: 'blog/:blogId/:blogTitle/update-post/:postId',
        element: <UpdatePost />
      },
      {
        path: 'blog/:blogId/:blogTitle/:postId/:postTitle',
        element: <PostView />
      },
      {
        path: 'blog/:blogId/:blogTitle/:postId/:postTitle/preview',
        element: <PostView />
      },
      {
        path: 'blog/:blogId/:blogTitle/search',
        element: <SearchPublicPostInBlog />
      },
      {
        path: 'search',
        element: <SearchPublicPost />
      }
    ]
  }
])

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <RouterProvider router={router} />
  </React.StrictMode>,
)
