import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import NotesPage from '../../pages/home/HomePage.vue'
import LoginPage from '../../pages/notes/LoginPage.vue'
import RegisterPage from '../../pages/notes/RegisterPage.vue'
import PrivacyPolicyPage from '../../pages/info/PrivacyPolicyPage.vue'
import TermsOfUsePage from '../../pages/info/TermsOfUsePage.vue'
import { useAuthStore } from '../../entities/session/model/store/auth'
import NoteDetailsPage from '../../pages/notes/NoteDetailsPage.vue'
import TrashPage from '../../pages/notes/TrashPage.vue'
import RemindersPage from '../../pages/notes/RemindersPage.vue'

const routes: RouteRecordRaw[] = [
  { path: '/', redirect: '/home' },
  { path: '/home', component: NotesPage, meta: { requiresAuth: true } },
  { path: '/trash', component: TrashPage, meta: { requiresAuth: true } },
  { path: '/reminders', component: RemindersPage, meta: { requiresAuth: true } },
  { path: '/login', component: LoginPage },
  { path: '/register', component: RegisterPage },
  { path: '/privacy-policy', component: PrivacyPolicyPage },
  { path: '/terms-of-use', component: TermsOfUsePage },
  { path: '/note/:id', component: NoteDetailsPage, meta: { requiresAuth: true } },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach((to) => {
  const auth = useAuthStore()

  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return '/login'
  }

  if (auth.isAuthenticated && (to.path === '/login' || to.path === '/register')) {
    return '/home'
  }
})

export default router
