import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/role.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login.component').then(
        (m) => m.LoginComponent
      ),
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/register/register.component').then(
        (m) => m.RegisterComponent
      ),
  },
  {
    path: '',
    loadComponent: () =>
      import('./layout/main-layout/main-layout.component').then(
        (m) => m.MainLayoutComponent
      ),
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then(
            (m) => m.DashboardComponent
          ),
      },
      {
        path: 'appointments',
        loadComponent: () =>
          import(
            './features/appointments/appointment-list/appointment-list.component'
          ).then((m) => m.AppointmentListComponent),
      },
      {
        path: 'appointments/calendar',
        loadComponent: () =>
          import(
            './features/appointments/appointment-calendar/appointment-calendar.component'
          ).then((m) => m.AppointmentCalendarComponent),
      },
      {
        path: 'appointments/:id',
        loadComponent: () =>
          import(
            './features/appointments/appointment-detail/appointment-detail.component'
          ).then((m) => m.AppointmentDetailComponent),
      },
      {
        path: 'customers',
        loadComponent: () =>
          import(
            './features/customers/customer-list/customer-list.component'
          ).then((m) => m.CustomerListComponent),
      },
      {
        path: 'services',
        loadComponent: () =>
          import(
            './features/services/service-list/service-list.component'
          ).then((m) => m.ServiceListComponent),
      },
      {
        path: 'vacancies',
        loadComponent: () =>
          import(
            './features/vacancies/vacancy-list/vacancy-list.component'
          ).then((m) => m.VacancyListComponent),
      },
      {
        path: 'users',
        loadComponent: () =>
          import('./features/users/user-list/user-list.component').then(
            (m) => m.UserListComponent
          ),
        canActivate: [adminGuard],
      },
      {
        path: 'finance',
        loadComponent: () =>
          import(
            './features/finance/finance-dashboard/finance-dashboard.component'
          ).then((m) => m.FinanceDashboardComponent),
        canActivate: [adminGuard],
      },
      {
        path: 'settings',
        loadComponent: () =>
          import('./features/settings/settings.component').then(
            (m) => m.SettingsComponent
          ),
      },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    ],
  },
  { path: '**', redirectTo: 'login' },
];
