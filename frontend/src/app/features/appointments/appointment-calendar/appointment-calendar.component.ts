import { Component, inject, OnInit , signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RouterLink } from '@angular/router';
import { AppointmentService } from '../../../core/services/appointment.service';
import { AppointmentDto } from '../../../core/models/appointment.model';
import { AppointmentFormComponent } from '../appointment-form/appointment-form.component';

@Component({
  selector: 'app-appointment-calendar',
  standalone: true,
  imports: [MatButtonModule, MatIconModule, MatProgressSpinnerModule, RouterLink],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h2>Calendário Semanal</h2>
        <div>
          <a mat-button routerLink="/appointments">Lista</a>
          <button mat-flat-button color="primary" (click)="openForm()">Novo</button>
        </div>
      </div>

      <div class="week-nav">
        <button mat-button (click)="prevWeek()">Anterior</button>
        <span class="week-label">{{ weekLabel }}</span>
        <button mat-button (click)="nextWeek()">Próxima</button>
        <button mat-button (click)="goToday()">Hoje</button>
      </div>

      @if (loading()) {
        <div class="loading"><mat-spinner></mat-spinner></div>
      } @else {
        <div class="calendar-grid">
          <div class="time-col">
            <div class="day-header"></div>
            @for (hour of hours; track hour) {
              <div class="time-slot">{{ hour }}:00</div>
            }
          </div>
          @for (day of weekDays; track day.date) {
            <div class="day-col">
              <div class="day-header" [class.today]="isToday(day.date)">
                <div class="day-name">{{ day.name }}</div>
                <div class="day-number">{{ day.date.getDate() }}</div>
              </div>
              <div class="day-body">
                @for (apt of getAppointmentsForDay(day.date); track apt.id) {
                  <div class="event"
                    [class]="apt.status.toLowerCase()"
                    [style.top.px]="getEventTop(apt)"
                    [style.height.px]="getEventHeight(apt)">
                    <div class="event-time">{{ formatTime(apt.startTime) }}</div>
                    <div class="event-name">{{ apt.customerName }}</div>
                  </div>
                }
              </div>
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .loading { display: flex; justify-content: center; padding: 48px; }
    .week-nav { display: flex; align-items: center; gap: 8px; margin-bottom: 16px; }
    .week-label { font-size: 16px; font-weight: 500; }
    .calendar-grid {
      display: grid;
      grid-template-columns: 60px repeat(7, 1fr);
      border: 1px solid #e0e0e0;
      border-radius: 8px;
      overflow: hidden;
    }
    .day-header {
      padding: 8px;
      text-align: center;
      background: #f5f5f5;
      border-bottom: 1px solid #e0e0e0;
      min-height: 48px;
      display: flex;
      flex-direction: column;
      justify-content: center;
      &.today { background: #e3f2fd; }
    }
    .day-name { font-size: 11px; text-transform: uppercase; color: #666; }
    .day-number { font-size: 18px; font-weight: 500; }
    .time-slot {
      height: 60px;
      padding: 4px;
      font-size: 11px;
      color: #999;
      border-bottom: 1px solid #f0f0f0;
    }
    .day-col { border-left: 1px solid #e0e0e0; }
    .day-body {
      position: relative;
      height: calc(60px * 12);
    }
    .event {
      position: absolute;
      left: 2px;
      right: 2px;
      border-radius: 4px;
      padding: 2px 4px;
      font-size: 11px;
      overflow: hidden;
      cursor: pointer;
      &.pendingpayment { background: #FEF3C7; color: #92400E; border-left: 3px solid #D97706; }
      &.confirmed { background: #D1FAE5; color: #065F46; border-left: 3px solid #059669; }
      &.completed { background: #DBEAFE; color: #1E40AF; border-left: 3px solid #3B82F6; }
      &.cancelled { background: #FEE2E2; color: #991B1B; border-left: 3px solid #EF4444; }
      &.noshow { background: #F3E8FF; color: #6B21A8; border-left: 3px solid #7C3AED; }
    }
    .event-time { font-weight: 500; }
    .event-name { white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
  `],
})
export class AppointmentCalendarComponent implements OnInit {
  private appointmentService = inject(AppointmentService);
  private dialog = inject(MatDialog);

  loading = signal(true);
  appointments = signal<AppointmentDto[]>([]);
  weekStart = this.getWeekStart(new Date());
  weekDays: { name: string; date: Date }[] = [];
  weekLabel = '';
  hours = [8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19];

  private dayNames = ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb'];

  ngOnInit(): void {
    this.buildWeek();
    this.load();
  }

  buildWeek(): void {
    this.weekDays = [];
    for (let i = 0; i < 7; i++) {
      const d = new Date(this.weekStart);
      d.setDate(d.getDate() + i);
      this.weekDays.push({ name: this.dayNames[d.getDay()], date: d });
    }
    const end = new Date(this.weekStart);
    end.setDate(end.getDate() + 6);
    this.weekLabel = `${this.weekStart.getDate()}/${this.weekStart.getMonth() + 1} - ${end.getDate()}/${end.getMonth() + 1}/${end.getFullYear()}`;
  }

  load(): void {
    this.loading.set(true);
    const fromDate = this.weekStart;
    const endDate = new Date(this.weekStart);
    endDate.setDate(endDate.getDate() + 6);
    const from = `${fromDate.getFullYear()}-${String(fromDate.getMonth() + 1).padStart(2, '0')}-${String(fromDate.getDate()).padStart(2, '0')}T00:00:00Z`;
    const to = `${endDate.getFullYear()}-${String(endDate.getMonth() + 1).padStart(2, '0')}-${String(endDate.getDate()).padStart(2, '0')}T23:59:59Z`;
    this.appointmentService.getAll({ from, to, pageSize: 100 }).subscribe({
      next: (result) => {
        this.appointments.set(result.items);
        this.loading.set(false);
      },
      error: () => { this.loading.set(false); },
    });
  }

  prevWeek(): void {
    this.weekStart.setDate(this.weekStart.getDate() - 7);
    this.weekStart = new Date(this.weekStart);
    this.buildWeek();
    this.load();
  }

  nextWeek(): void {
    this.weekStart.setDate(this.weekStart.getDate() + 7);
    this.weekStart = new Date(this.weekStart);
    this.buildWeek();
    this.load();
  }

  goToday(): void {
    this.weekStart = this.getWeekStart(new Date());
    this.buildWeek();
    this.load();
  }

  isToday(date: Date): boolean {
    const today = new Date();
    return date.toDateString() === today.toDateString();
  }

  getAppointmentsForDay(date: Date): AppointmentDto[] {
    return this.appointments().filter((a) => {
      const aptDate = new Date(a.startTime);
      return aptDate.toDateString() === date.toDateString();
    });
  }

  getEventTop(apt: AppointmentDto): number {
    const start = new Date(apt.startTime);
    const hours = start.getHours() - 8;
    const minutes = start.getMinutes();
    return hours * 60 + minutes;
  }

  getEventHeight(apt: AppointmentDto): number {
    const start = new Date(apt.startTime);
    const end = new Date(apt.endTime);
    return Math.max(20, (end.getTime() - start.getTime()) / 60000);
  }

  formatTime(isoString: string): string {
    const d = new Date(isoString);
    return `${d.getHours().toString().padStart(2, '0')}:${d.getMinutes().toString().padStart(2, '0')}`;
  }

  openForm(): void {
    const dialogRef = this.dialog.open(AppointmentFormComponent, { width: '700px', maxHeight: '90vh' });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) this.load();
    });
  }

  private getWeekStart(date: Date): Date {
    const d = new Date(date);
    const day = d.getDay();
    d.setDate(d.getDate() - day);
    d.setHours(0, 0, 0, 0);
    return d;
  }
}
