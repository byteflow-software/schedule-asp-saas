import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [MatButtonModule, MatIconModule, MatMenuModule],
  template: `
    <header class="header">
      <div class="header-left">
        <h1 class="greeting">Olá, {{ firstName() }}</h1>
      </div>
      <div class="header-right">
        <div class="user-pill" [matMenuTriggerFor]="menu">
          <div class="avatar">{{ initials() }}</div>
          <div class="user-details">
            <span class="user-name">{{ authService.currentUser()?.fullName }}</span>
            <span class="user-role">{{ authService.currentUser()?.role }}</span>
          </div>
          <mat-icon class="dropdown-icon">expand_more</mat-icon>
        </div>
        <mat-menu #menu="matMenu">
          <button mat-menu-item routerLink="/settings">
            <mat-icon>tune</mat-icon>
            <span>Configurações</span>
          </button>
          <button mat-menu-item (click)="authService.logout()">
            <mat-icon>logout</mat-icon>
            <span>Sair</span>
          </button>
        </mat-menu>
      </div>
    </header>
  `,
  styles: [`
    .header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 16px 32px;
      background: white;
      border-bottom: 1px solid var(--gray-200, #E5E7EB);
      position: sticky;
      top: 0;
      z-index: 100;
    }
    .greeting {
      font-size: 18px;
      font-weight: 600;
      color: var(--gray-800, #1F2937);
      margin: 0;
    }
    .header-right {
      display: flex;
      align-items: center;
      gap: 12px;
    }
    .user-pill {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 6px 12px 6px 6px;
      border-radius: 50px;
      cursor: pointer;
      transition: background 0.2s;
      &:hover { background: var(--gray-100, #F3F4F6); }
    }
    .avatar {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      background: linear-gradient(135deg, #4F46E5, #7C3AED);
      color: white;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: 600;
      font-size: 14px;
    }
    .user-details {
      display: flex;
      flex-direction: column;
    }
    .user-name {
      font-size: 13px;
      font-weight: 600;
      color: var(--gray-800, #1F2937);
      line-height: 1.2;
    }
    .user-role {
      font-size: 11px;
      color: var(--gray-500, #6B7280);
    }
    .dropdown-icon {
      font-size: 18px;
      width: 18px;
      height: 18px;
      color: var(--gray-400, #9CA3AF);
    }
  `],
})
export class HeaderComponent {
  authService = inject(AuthService);

  firstName() {
    return this.authService.currentUser()?.fullName?.split(' ')[0] ?? '';
  }

  initials() {
    const name = this.authService.currentUser()?.fullName ?? '';
    const parts = name.split(' ').filter(Boolean);
    if (parts.length >= 2) return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    return (parts[0]?.[0] ?? '').toUpperCase();
  }
}
