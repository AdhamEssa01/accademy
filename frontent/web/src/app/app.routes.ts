import { Routes } from '@angular/router';
import { PublicLayoutComponent } from './core/layout/public-layout/public-layout.component';
import { AppShellComponent } from './core/layout/app-shell/app-shell.component';
import { authGuard } from './core/auth/auth.guard';
import { roleGuard } from './core/auth/role.guard';
import { Roles } from './core/auth/roles';

export const routes: Routes = [
  {
    path: '',
    component: PublicLayoutComponent,
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/public/landing-page/landing-page.component').then((m) => m.LandingPageComponent),
      },
      {
        path: 'about',
        loadComponent: () => import('./features/public/about/about.component').then((m) => m.AboutComponent),
      },
      {
        path: 'login',
        loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent),
      },
      {
        path: 'register',
        loadComponent: () => import('./features/auth/register/register.component').then((m) => m.RegisterComponent),
      },
    ],
  },
  {
    path: 'app',
    component: AppShellComponent,
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/dashboards/dashboard-home/dashboard-home.component').then(
            (m) => m.DashboardHomeComponent
          ),
      },
      {
        path: 'admin',
        loadComponent: () =>
          import('./features/dashboards/admin-dashboard/admin-dashboard.component').then(
            (m) => m.AdminDashboardComponent
          ),
        canActivate: [roleGuard],
        data: { roles: [Roles.Admin] },
      },
      {
        path: 'admin/academy',
        loadComponent: () =>
          import('./features/admin/academy/academy-settings.component').then((m) => m.AcademySettingsComponent),
        canActivate: [roleGuard],
        data: { roles: [Roles.Admin] },
      },
      {
        path: 'admin/branches',
        loadComponent: () =>
          import('./features/admin/branches/branches.component').then((m) => m.BranchesComponent),
        canActivate: [roleGuard],
        data: { roles: [Roles.Admin] },
      },
      {
        path: 'admin/programs',
        loadComponent: () =>
          import('./features/admin/programs/programs.component').then((m) => m.ProgramsComponent),
        canActivate: [roleGuard],
        data: { roles: [Roles.Admin] },
      },
      {
        path: 'admin/courses',
        loadComponent: () =>
          import('./features/admin/courses/courses.component').then((m) => m.CoursesComponent),
        canActivate: [roleGuard],
        data: { roles: [Roles.Admin] },
      },
      {
        path: 'admin/levels',
        loadComponent: () => import('./features/admin/levels/levels.component').then((m) => m.LevelsComponent),
        canActivate: [roleGuard],
        data: { roles: [Roles.Admin] },
      },
      {
        path: 'admin/groups',
        loadComponent: () => import('./features/admin/groups/groups.component').then((m) => m.GroupsComponent),
        canActivate: [roleGuard],
        data: { roles: [Roles.Admin] },
      },
      {
        path: 'admin/sessions',
        loadComponent: () =>
          import('./features/admin/sessions/sessions.component').then((m) => m.SessionsComponent),
        canActivate: [roleGuard],
        data: { roles: [Roles.Admin] },
      },
      {
        path: 'admin/students',
        loadComponent: () =>
          import('./features/admin/students/students.component').then((m) => m.StudentsComponent),
        canActivate: [roleGuard],
        data: { roles: [Roles.Admin] },
      },
      {
        path: 'admin/students/:id',
        loadComponent: () =>
          import('./features/admin/students/student-details.component').then((m) => m.StudentDetailsComponent),
        canActivate: [roleGuard],
        data: { roles: [Roles.Admin] },
      },
      {
        path: 'admin/attendance',
        loadComponent: () =>
          import('./features/admin/attendance/admin-attendance.component').then((m) => m.AdminAttendanceComponent),
        canActivate: [roleGuard],
        data: { roles: [Roles.Admin, Roles.Instructor] },
      },
      {
        path: 'admin/achievements',
        loadComponent: () =>
          import('./features/admin/achievements/achievements.component').then((m) => m.AchievementsComponent),
        canActivate: [roleGuard],
        data: { roles: [Roles.Admin] },
      },
      {
        path: 'instructor',
        loadComponent: () =>
          import('./features/dashboards/instructor-dashboard/instructor-dashboard.component').then(
            (m) => m.InstructorDashboardComponent
          ),
        canActivate: [roleGuard],
        data: { roles: [Roles.Instructor] },
      },
      {
        path: 'instructor/groups',
        loadComponent: () =>
          import('./features/instructor/groups/instructor-groups.component').then((m) => m.InstructorGroupsComponent),
        canActivate: [roleGuard],
        data: { roles: [Roles.Instructor] },
      },
      {
        path: 'instructor/sessions',
        loadComponent: () =>
          import('./features/instructor/sessions/instructor-sessions.component').then(
            (m) => m.InstructorSessionsComponent
          ),
        canActivate: [roleGuard],
        data: { roles: [Roles.Instructor] },
      },
      {
        path: 'instructor/attendance',
        loadComponent: () =>
          import('./features/instructor/attendance/instructor-attendance.component').then(
            (m) => m.InstructorAttendanceComponent
          ),
        canActivate: [roleGuard],
        data: { roles: [Roles.Instructor] },
      },
      {
        path: 'instructor/assignments',
        loadComponent: () =>
          import('./features/instructor/assignments/instructor-assignments.component').then(
            (m) => m.InstructorAssignmentsComponent
          ),
        canActivate: [roleGuard],
        data: { roles: [Roles.Instructor] },
      },
      {
        path: 'parent',
        loadComponent: () =>
          import('./features/dashboards/parent-dashboard/parent-dashboard.component').then(
            (m) => m.ParentDashboardComponent
          ),
        canActivate: [roleGuard],
        data: { roles: [Roles.Parent] },
      },
      {
        path: 'parent/children',
        loadComponent: () =>
          import('./features/parent/children/parent-children.component').then((m) => m.ParentChildrenComponent),
        canActivate: [roleGuard],
        data: { roles: [Roles.Parent] },
      },
      {
        path: 'parent/attendance',
        loadComponent: () =>
          import('./features/parent/attendance/parent-attendance.component').then(
            (m) => m.ParentAttendanceComponent
          ),
        canActivate: [roleGuard],
        data: { roles: [Roles.Parent] },
      },
      {
        path: 'parent/assignments',
        loadComponent: () =>
          import('./features/parent/assignments/parent-assignments.component').then(
            (m) => m.ParentAssignmentsComponent
          ),
        canActivate: [roleGuard],
        data: { roles: [Roles.Parent] },
      },
      {
        path: 'parent/announcements',
        loadComponent: () =>
          import('./features/parent/announcements/parent-announcements.component').then(
            (m) => m.ParentAnnouncementsComponent
          ),
        canActivate: [roleGuard],
        data: { roles: [Roles.Parent] },
      },
    ],
  },
  {
    path: 'forbidden',
    loadComponent: () => import('./features/errors/forbidden/forbidden.component').then((m) => m.ForbiddenComponent),
  },
  {
    path: 'not-found',
    loadComponent: () => import('./features/errors/not-found/not-found.component').then((m) => m.NotFoundComponent),
  },
  { path: '**', redirectTo: 'not-found' },
];
