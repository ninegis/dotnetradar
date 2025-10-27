import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi } from '@/api'

export const useUserStore = defineStore('user', () => {
  const token = ref<string>(localStorage.getItem('token') || '')
  const userInfo = ref<any>(null)
  
  const isLoggedIn = computed(() => !!token.value)
  
  const login = async (username: string, password: string) => {
    const response: any = await authApi.login(username, password)
    if (response.success) {
      token.value = response.data.token
      userInfo.value = response.data.user
      localStorage.setItem('token', response.data.token)
      return true
    }
    return false
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


