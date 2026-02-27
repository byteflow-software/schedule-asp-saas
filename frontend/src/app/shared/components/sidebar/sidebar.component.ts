import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, MatIconModule],
  template: `
    <nav class="sidebar">
      <div class="logo">
        <div class="logo-icon">S</div>
        <span class="logo-text">Scheduly</span>
      </div>
      <div class="nav-section">
        <span class="nav-label">MENU</span>
        <a routerLink="/dashboard" routerLinkActive="active" class="nav-item">
          <mat-icon>space_dashboard</mat-icon>
          <span>Dashboard</span>
        </a>
        <a routerLink="/appointments" routerLinkActive="active" class="nav-item">
          <mat-icon>calendar_month</mat-icon>
          <span>Agendamentos</span>
        </a>
        <a routerLink="/customers" routerLinkActive="active" class="nav-item">
          <mat-icon>people_alt</mat-icon>
          <span>Clientes</span>
        </a>
        <a routerLink="/services" routerLinkActive="active" class="nav-item">
          <mat-icon>design_services</mat-icon>
          <span>Serviços</span>
        </a>
        <a routerLink="/vacancies" routerLinkActive="active" class="nav-item">
          <mat-icon>event_available</mat-icon>
          <span>Vagas</span>
        </a>
      </div>
      @if (authService.isAdmin()) {
        <div class="nav-section">
          <span class="nav-label">ADMIN</span>
          <a routerLink="/users" routerLinkActive="active" class="nav-item">
            <mat-icon>badge</mat-icon>
            <span>Equipe</span>
          </a>
          <a routerLink="/finance" routerLinkActive="active" class="nav-item">
            <mat-icon>account_balance</mat-icon>
            <span>Financeiro</span>
          </a>
        </div>
      }
      <div class="nav-section">
        <span class="nav-label">SISTEMA</span>
        <a routerLink="/settings" routerLinkActive="active" class="nav-item">
          <mat-icon>tune</mat-icon>
          <span>Configurações</span>
        </a>
      </div>
      <div class="sidebar-footer">
        <div class="role-badge">
          <mat-icon>verified</mat-icon>
          <span>{{ authService.currentUser()?.role }}</span>
        </div>
      </div>
    </nav>
  `,
  styles: [`
    .sidebar {
      width: 260px;
      height: 100vh;
      background: linear-gradient(180deg, #1E1B4B 0%, #312E81 50%, #3730A3 100%);
      color: white;
      display: flex;
      flex-direction: column;
      padding: 0;
      overflow-y: auto;
    }
    .logo {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 24px 24px 32px;
    }
    .logo-icon {
      width: 36px;
      height: 36px;
      border-radius: 10px;
      background: linear-gradient(135deg, #818CF8, #6366F1);
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: 700;
      font-size: 18px;
      box-shadow: 0 2px 8px rgba(99, 102, 241, 0.4);
    }
    .logo-text {
      font-size: 22px;
      font-weight: 700;
      letter-spacing: -0.5px;
    }
    .nav-section {
      padding: 0 16px;
      margin-bottom: 8px;
    }
    .nav-label {
      display: block;
      font-size: 10px;
      font-weight: 600;
      letter-spacing: 1.2px;
      color: rgba(255,255,255,0.35);
      padding: 8px 12px 8px;
      margin-top: 8px;
    }
    .nav-item {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 10px 12px;
      border-radius: 10px;
      color: rgba(255,255,255,0.65);
      text-decoration: none;
      font-size: 14px;
      font-weight: 500;
      transition: all 0.2s ease;
      margin-bottom: 2px;

      mat-icon {
        font-size: 20px;
        width: 20px;
        height: 20px;
        opacity: 0.7;
      }

      &:hover {
        background: rgba(255,255,255,0.08);
        color: rgba(255,255,255,0.9);
        mat-icon { opacity: 0.9; }
      }

      &.active {
        background: rgba(255,255,255,0.15);
        color: white;
        font-weight: 600;
        mat-icon { opacity: 1; }
        box-shadow: 0 1px 3px rgba(0,0,0,0.2);
      }
    }
    .sidebar-footer {
      margin-top: auto;
      padding: 16px 24px 24px;
    }
    .role-badge {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 10px 14px;
      background: rgba(255,255,255,0.1);
      border-radius: 10px;
      font-size: 13px;
      font-weight: 500;
      color: rgba(255,255,255,0.8);
      mat-icon { font-size: 18px; width: 18px; height: 18px; color: #818CF8; }
    }
  `],
})
export class SidebarComponent {
  authService = inject(AuthService);
}
