import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import NotesPage from '../../pages/home/HomePage.vue'
import LoginPage from '../../pages/auth/LoginPage.vue'
import RegisterPage from '../../pages/auth/RegisterPage.vue'
import PrivacyPolicyPage from '../../pages/info/PrivacyPolicyPage.vue'
import TermsOfUsePage from '../../pages/info/TermsOfUsePage.vue'
import { useAuthStore } from '../../entities/session/model/store/auth'
import NoteDetailsPage from '../../pages/notes/NoteDetailsPage.vue'
import TrashPage from '../../pages/notes/TrashPage.vue'
import RemindersPage from '../../pages/notes/RemindersPage.vue'
import SharedNotesPage from '../../pages/notes/SharedNotesPage.vue'
import UserProfilePage from '../../pages/user-profile/UserProfilePage.vue'
import LandingPage from '../../pages/LandingPage.vue'

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: LandingPage
  },
  {
    path: '/home',
    component: NotesPage,
    meta: { requiresAuth: true }
  },
  {
    path: '/shared-notes',
    component: SharedNotesPage,
    meta: { requiresAuth: true }
  },
  {
    path: '/trash',
    component: TrashPage,
    meta: { requiresAuth: true }
  },
  {
    path: '/reminders',
    component: RemindersPage,
    meta: { requiresAuth: true }
  },
  {
    path: '/profile',
    component: UserProfilePage,
    meta: { requiresAuth: true }
  },
  {
    path: '/login',
    component: LoginPage
  },
  {
    path: '/register',
    component: RegisterPage
  },
  {
    path: '/privacy-policy',
    component: PrivacyPolicyPage
  },
  {
    path: '/terms-of-use',
    component: TermsOfUsePage
  },
  {
    path: '/note/:id',
    component: NoteDetailsPage,
    meta: { requiresAuth: true }
  },
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

  if (auth.isAuthenticated && (to.path === '/login' || to.path === '/register' || to.path === '/')) {
    return '/home'
  }
})

export default router
