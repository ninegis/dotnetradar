import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi } from '@/api'

export const useUserStore = defineStore('user', () => {
  const token = ref<string>(localStorage.getItem('token') || '')
  const userInfo = ref<any>(null)
  
  const isLoggedIn = computed(() => !!token.value)
  
  const login = async (username: string, password: string) => {
    try {
      const response: any = await authApi.login(username, password)
      console.log('Login response:', response) // 调试日志
      
      if (response && response.success) {
        // 后端返回的是 response.data.token，不是 response.data.data.token
        token.value = response.data.token
        userInfo.value = response.data.user
        localStorage.setItem('token', response.data.token)
        console.log('Token saved:', response.data.token) // 调试日志
        return true
      } else {
        console.log('Login failed:', response) // 调试日志
        return false
      }
    } catch (error) {
      console.error('Login error:', error) // 调试日志
      return false
    }
  }
  
  const logout = () => {
    token.value = ''
    userInfo.value = null
    localStorage.removeItem('token')
  }
  
  const loadUserInfo = async () => {
    if (token.value) {
      try {
        const response: any = await authApi.getCurrentUser()
        if (response.success) {
          userInfo.value = response.data
        }
      } catch (error) {
        logout()
      }
    }
  }
  
  return {
    token,
    userInfo,
    isLoggedIn,
    login,
    logout,
    loadUserInfo
  }
})

export const useAppStore = defineStore('app', () => {
  const sidebarCollapsed = ref(false)
  const currentProject = ref<any>(null)
  
  const toggleSidebar = () => {
    sidebarCollapsed.value = !sidebarCollapsed.value
  }
  
  const setCurrentProject = (project: any) => {
    currentProject.value = project
    localStorage.setItem('currentProject', JSON.stringify(project))
  }
  
  const loadCurrentProject = () => {
    const saved = localStorage.getItem('currentProject')
    if (saved) {
      currentProject.value = JSON.parse(saved)
    }
  }
  
  return {
    sidebarCollapsed,
    currentProject,
    toggleSidebar,
    setCurrentProject,
    loadCurrentProject
  }
})


