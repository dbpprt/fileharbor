import Vue from 'vue'
import Router from 'vue-router'

import MainLayout from '@/components/layout/MainLayout'
import PublicLayout from '@/components/layout/public-layout/PublicLayout'

import Hello from '@/components/Hello'
import About from '@/components/About'
import Login from '@/components/Login'
import Register from '@/components/Register'

Vue.use(Router)

export default new Router({
  routes: [
    { path: '/', redirect: '/app' },
    {
      path: '/app',
      component: MainLayout,
      children: [
        {
          path: '/app',
          component: Hello
        },
        {
          name: 'about',
          path: '/app/about',
          component: About
        }
      ]
    },
    {
      path: '/public',
      component: PublicLayout,
      children: [
        {
          path: '/public/login',
          component: Login
        },
        {
          path: '/public/register',
          component: Register
        }
      ]
    }
  ]
})
