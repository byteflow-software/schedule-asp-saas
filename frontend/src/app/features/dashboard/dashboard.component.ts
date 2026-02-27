import { Component, inject, OnInit } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';
import { AppointmentService } from '../../core/services/appointment.service';
import { CustomerService } from '../../core/services/customer.service';
import { TenantService } from '../../core/services/tenant.service';
import { DateFormatPipe } from '../../shared/pipes/date-format.pipe';
import { StatusTranslatePipe } from '../../shared/pipes/status.pipe';
import { AppointmentDto } from '../../core/models/appointment.model';
import { TenantDto } from '../../core/models/tenant.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [MatCardModule, MatIconModule, MatProgressSpinnerModule, MatButtonModule, RouterLink, DateFormatPipe, StatusTranslatePipe],
  template: `
    <div class="page-container">
      <div class="page-header">
        <div>
          <h2>Dashboard</h2>
          <p class="page-subtitle">Visão geral do seu negócio</p>
        </div>
        <a mat-flat-button color="primary" routerLink="/appointments/calendar">
          <mat-icon>add</mat-icon> Novo Agendamento
        </a>
      </div>

      @if (loading) {
        <div class="loading"><mat-spinner></mat-spinner></div>
      } @else {
        <div class="stats-grid">
          <div class="stat-card stat-indigo">
            <div class="stat-icon-wrap"><mat-icon>calendar_month</mat-icon></div>
            <div class="stat-info">
              <span class="stat-value">{{ todayAppointments.length }}</span>
              <span class="stat-label">Agendamentos Hoje</span>
            </div>
          </div>
          <div class="stat-card stat-emerald">
            <div class="stat-icon-wrap"><mat-icon>people_alt</mat-icon></div>
            <div class="stat-info">
              <span class="stat-value">{{ totalCustomers }}</span>
              <span class="stat-label">Total de Clientes</span>
            </div>
          </div>
          <div class="stat-card stat-amber">
            <div class="stat-icon-wrap"><mat-icon>event_available</mat-icon></div>
            <div class="stat-info">
              <span class="stat-value">{{ weekAppointments }}</span>
              <span class="stat-label">Agendamentos na Semana</span>
            </div>
          </div>
        </div>

        <div class="section-header">
          <h3>Agendamentos de Hoje</h3>
          <a mat-button routerLink="/appointments" class="see-all">Ver todos <mat-icon>arrow_forward</mat-icon></a>
        </div>

        @if (todayAppointments.length === 0) {
          <div class="empty-state">
            <mat-icon>event_available</mat-icon>
            <h4>Nenhum agendamento para hoje</h4>
            <p>Que tal criar um novo agendamento?</p>
          </div>
        } @else {
          <div class="today-list">
            @for (apt of todayAppointments; track apt.id) {
              <div class="today-item">
                <div class="time-col">
                  <span class="time-start">{{ apt.startTime | dateFormat:'time' }}</span>
                  <span class="time-end">{{ apt.endTime | dateFormat:'time' }}</span>
                </div>
                <div class="divider-col">
                  <div class="dot"></div>
                  <div class="line"></div>
                </div>
                <div class="info-col">
                  <span class="customer-name">{{ apt.customerName }}</span>
                  <span class="staff-name">com {{ apt.userName }}</span>
                </div>
                <span class="status-badge" [attr.data-status]="apt.status">{{ apt.status | statusTranslate }}</span>
              </div>
            }
          </div>
        }
      }
    </div>
  `,
  styles: [`
    .page-subtitle { font-size: 14px; color: var(--gray-500, #6B7280); margin: 4px 0 0; }
    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: 20px;
      margin-bottom: 36px;
    }
    .stat-card {
      display: flex; align-items: center; gap: 20px;
      padding: 24px; border-radius: 16px; color: white;
      position: relative; overflow: hidden;
      &::after {
        content: ''; position: absolute; right: -20px; top: -20px;
        width: 100px; height: 100px; border-radius: 50%;
        background: rgba(255,255,255,0.1);
      }
    }
    .stat-indigo { background: linear-gradient(135deg, #4F46E5, #7C3AED); }
    .stat-emerald { background: linear-gradient(135deg, #059669, #10B981); }
    .stat-amber { background: linear-gradient(135deg, #D97706, #F59E0B); }
    .stat-icon-wrap {
      width: 52px; height: 52px; border-radius: 14px;
      background: rgba(255,255,255,0.2); display: flex;
      align-items: center; justify-content: center;
      mat-icon { font-size: 28px; width: 28px; height: 28px; }
    }
    .stat-info { display: flex; flex-direction: column; }
    .stat-value { font-size: 32px; font-weight: 700; line-height: 1; }
    .stat-label { font-size: 13px; opacity: 0.85; margin-top: 4px; }
    .section-header {
      display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px;
      h3 { font-size: 18px; font-weight: 600; color: var(--gray-800, #1F2937); margin: 0; }
    }
    .see-all {
      display: flex; align-items: center; gap: 4px;
      font-size: 13px; color: var(--primary, #4F46E5);
      mat-icon { font-size: 16px; width: 16px; height: 16px; }
    }
    .empty-state {
      text-align: center; padding: 48px 24px;
      background: white; border-radius: 16px; border: 2px dashed var(--gray-200, #E5E7EB);
      mat-icon { font-size: 48px; width: 48px; height: 48px; color: var(--gray-300, #D1D5DB); }
      h4 { font-size: 16px; color: var(--gray-600, #4B5563); margin: 12px 0 4px; }
      p { font-size: 14px; color: var(--gray-400, #9CA3AF); margin: 0; }
    }
    .today-list {
      background: white; border-radius: 16px; overflow: hidden;
      box-shadow: 0 1px 3px rgba(0,0,0,0.1); border: 1px solid var(--gray-100, #F3F4F6);
    }
    .today-item {
      display: flex; align-items: center; gap: 16px; padding: 16px 24px;
      border-bottom: 1px solid var(--gray-100, #F3F4F6);
      transition: background 0.15s;
      &:last-child { border-bottom: none; }
      &:hover { background: var(--gray-50, #F9FAFB); }
    }
    .time-col { display: flex; flex-direction: column; min-width: 60px; text-align: center; }
    .time-start { font-size: 15px; font-weight: 600; color: var(--gray-800, #1F2937); }
    .time-end { font-size: 12px; color: var(--gray-400, #9CA3AF); }
    .divider-col { display: flex; flex-direction: column; align-items: center; gap: 2px; }
    .dot { width: 10px; height: 10px; border-radius: 50%; background: var(--primary, #4F46E5); }
    .line { width: 2px; height: 20px; background: var(--gray-200, #E5E7EB); }
    .info-col { flex: 1; }
    .customer-name { display: block; font-size: 14px; font-weight: 600; color: var(--gray-800, #1F2937); }
    .staff-name { font-size: 13px; color: var(--gray-500, #6B7280); }
  `],
})
export class DashboardComponent implements OnInit {
  private appointmentService = inject(AppointmentService);
  private customerService = inject(CustomerService);
  private tenantService = inject(TenantService);

  loading = true;
  todayAppointments: AppointmentDto[] = [];
  totalCustomers = 0;
  weekAppointments = 0;
  tenant: TenantDto | null = null;

  ngOnInit(): void {
    const today = new Date();
    const y = today.getFullYear();
    const m = String(today.getMonth() + 1).padStart(2, '0');
    const d = String(today.getDate()).padStart(2, '0');
    const from = `${y}-${m}-${d}T00:00:00Z`;
    const to = `${y}-${m}-${d}T23:59:59Z`;

    this.appointmentService.getAll({ from, to, pageSize: 50 }).subscribe({
      next: (result) => { this.todayAppointments = result.items; },
      error: () => {},
    });
    // Week appointments count
    const weekEnd = new Date(today);
    weekEnd.setDate(weekEnd.getDate() + 7);
    const wy = weekEnd.getFullYear();
    const wm = String(weekEnd.getMonth() + 1).padStart(2, '0');
    const wd = String(weekEnd.getDate()).padStart(2, '0');
    const weekTo = `${wy}-${wm}-${wd}T23:59:59Z`;
    this.appointmentService.getAll({ from, to: weekTo, pageSize: 1 }).subscribe({
      next: (result) => { this.weekAppointments = result.totalCount; },
      error: () => {},
    });
    this.customerService.getAll(undefined, 1, 1).subscribe({
      next: (result) => { this.totalCustomers = result.totalCount; },
      error: () => {},
    });
    this.tenantService.getMyTenant().subscribe({
      next: (tenant) => { this.tenant = tenant; this.loading = false; },
      error: () => { this.loading = false; },
    });
  }
}
