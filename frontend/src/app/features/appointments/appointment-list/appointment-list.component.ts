import { Component, inject, OnInit , signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';
import { AppointmentService } from '../../../core/services/appointment.service';
import { AppointmentDto } from '../../../core/models/appointment.model';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { AppointmentFormComponent } from '../appointment-form/appointment-form.component';
import { DateFormatPipe } from '../../../shared/pipes/date-format.pipe';
import { StatusTranslatePipe } from '../../../shared/pipes/status.pipe';
import { CurrencyCentsPipe } from '../../../shared/pipes/currency-cents.pipe';

@Component({
  selector: 'app-appointment-list',
  standalone: true,
  imports: [
    FormsModule, MatTableModule, MatButtonModule, MatIconModule,
    MatFormFieldModule, MatInputModule, MatSelectModule,
    MatProgressSpinnerModule, MatTooltipModule, RouterLink,
    PaginationComponent, DateFormatPipe, StatusTranslatePipe, CurrencyCentsPipe,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <div>
          <h2>Agendamentos</h2>
          <p class="page-subtitle">Gerencie seus agendamentos</p>
        </div>
        <div class="header-actions">
          <a mat-button routerLink="/appointments/calendar">
            <mat-icon>calendar_view_week</mat-icon> Calendário
          </a>
          <button mat-flat-button color="primary" (click)="openForm()">
            <mat-icon>add</mat-icon> Novo Agendamento
          </button>
        </div>
      </div>

      <div class="filters">
        <mat-form-field>
          <mat-label>Data Início</mat-label>
          <input matInput type="date" [(ngModel)]="fromDate" (change)="load()" />
        </mat-form-field>
        <mat-form-field>
          <mat-label>Data Fim</mat-label>
          <input matInput type="date" [(ngModel)]="toDate" (change)="load()" />
        </mat-form-field>
        <mat-form-field>
          <mat-label>Status</mat-label>
          <mat-select [(ngModel)]="statusFilter" (selectionChange)="load()">
            <mat-option value="">Todos</mat-option>
            <mat-option value="PendingPayment">Pendente Pagamento</mat-option>
            <mat-option value="Confirmed">Confirmado</mat-option>
            <mat-option value="Completed">Concluído</mat-option>
            <mat-option value="Cancelled">Cancelado</mat-option>
          </mat-select>
        </mat-form-field>
      </div>

      @if (loading()) {
        <div class="loading"><mat-spinner></mat-spinner></div>
      } @else if (appointments().length === 0) {
        <div class="empty-state">
          <mat-icon>event_busy</mat-icon>
          <h4>Nenhum agendamento encontrado</h4>
          <p>Tente ajustar os filtros ou crie um novo agendamento</p>
        </div>
      } @else {
        <table mat-table [dataSource]="appointments()" class="full-width">
          <ng-container matColumnDef="startTime">
            <th mat-header-cell *matHeaderCellDef>Data/Hora</th>
            <td mat-cell *matCellDef="let a">{{ a.startTime | dateFormat:'full' }}</td>
          </ng-container>
          <ng-container matColumnDef="customerName">
            <th mat-header-cell *matHeaderCellDef>Cliente</th>
            <td mat-cell *matCellDef="let a">
              <div class="name-cell">
                <div class="avatar-sm">{{ a.customerName[0] }}</div>
                {{ a.customerName }}
              </div>
            </td>
          </ng-container>
          <ng-container matColumnDef="serviceName">
            <th mat-header-cell *matHeaderCellDef>Serviço</th>
            <td mat-cell *matCellDef="let a">{{ a.serviceName }}</td>
          </ng-container>
          <ng-container matColumnDef="userName">
            <th mat-header-cell *matHeaderCellDef>Profissional</th>
            <td mat-cell *matCellDef="let a">{{ a.userName }}</td>
          </ng-container>
          <ng-container matColumnDef="priceInCents">
            <th mat-header-cell *matHeaderCellDef>Valor</th>
            <td mat-cell *matCellDef="let a" class="price-cell">{{ a.priceInCents | currencyCents }}</td>
          </ng-container>
          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>Status</th>
            <td mat-cell *matCellDef="let a">
              <span class="status-badge" [attr.data-status]="a.status">{{ a.status | statusTranslate }}</span>
            </td>
          </ng-container>
          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef></th>
            <td mat-cell *matCellDef="let a">
              <div class="actions">
                <button mat-icon-button matTooltip="Ver Detalhes" (click)="viewDetail(a)">
                  <mat-icon>visibility</mat-icon>
                </button>
                @if (a.status === 'PendingPayment' || a.status === 'Confirmed') {
                  <button mat-icon-button matTooltip="Cancelar" class="danger-btn" (click)="confirmCancel(a)">
                    <mat-icon>cancel</mat-icon>
                  </button>
                }
              </div>
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="columns"></tr>
          <tr mat-row *matRowDef="let row; columns: columns;" class="clickable-row" (click)="viewDetail(row)"></tr>
        </table>

        <app-pagination
          [pageNumber]="pageNumber" [totalPages]="totalPages()" [totalCount]="totalCount()"
          [hasPreviousPage]="hasPreviousPage()" [hasNextPage]="hasNextPage()"
          (pageChange)="loadPage($event)" />
      }
    </div>
  `,
  styles: [`
    .page-subtitle { font-size: 14px; color: var(--gray-500, #6B7280); margin: 4px 0 0; }
    .header-actions { display: flex; gap: 8px; align-items: center; }
    .loading { display: flex; justify-content: center; padding: 48px; }
    .filters { display: flex; gap: 16px; margin-bottom: 16px; flex-wrap: wrap; }
    .empty-state {
      text-align: center; padding: 64px 24px;
      background: white; border-radius: 16px; border: 2px dashed var(--gray-200, #E5E7EB);
      mat-icon { font-size: 48px; width: 48px; height: 48px; color: var(--gray-300, #D1D5DB); }
      h4 { font-size: 16px; color: var(--gray-600, #4B5563); margin: 12px 0 4px; }
      p { font-size: 14px; color: var(--gray-400, #9CA3AF); margin: 0; }
    }
    .name-cell { display: flex; align-items: center; gap: 10px; }
    .avatar-sm {
      width: 30px; height: 30px; border-radius: 8px;
      background: var(--primary-100, #E0E7FF); color: var(--primary, #4F46E5);
      display: flex; align-items: center; justify-content: center;
      font-weight: 600; font-size: 12px;
    }
    .price-cell { font-weight: 600; color: #059669; }
    .status-badge {
      display: inline-flex; padding: 4px 12px; border-radius: 20px;
      font-size: 12px; font-weight: 600;
    }
    [data-status="PendingPayment"] { background: #FEF3C7; color: #92400E; }
    [data-status="Confirmed"] { background: #D1FAE5; color: #065F46; }
    [data-status="Completed"] { background: #DBEAFE; color: #1E40AF; }
    [data-status="Cancelled"] { background: #FEE2E2; color: #991B1B; }
    [data-status="NoShow"] { background: #F3E8FF; color: #6B21A8; }
    .actions { display: flex; gap: 0; }
    .danger-btn { color: var(--danger, #EF4444) !important; }
    .clickable-row { cursor: pointer; &:hover { background: var(--gray-50, #F9FAFB); } }
  `],
})
export class AppointmentListComponent implements OnInit {
  private appointmentService = inject(AppointmentService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private router = inject(Router);

  appointments = signal<AppointmentDto[]>([]);
  columns = ['startTime', 'customerName', 'serviceName', 'userName', 'priceInCents', 'status', 'actions'];
  loading = signal(true);
  fromDate = '';
  toDate = '';
  statusFilter = '';
  pageNumber = 1;
  totalPages = signal(1);
  totalCount = signal(0);
  hasPreviousPage = signal(false);
  hasNextPage = signal(false);

  ngOnInit(): void {
    const today = new Date();
    this.fromDate = today.toISOString().split('T')[0];
    const nextWeek = new Date(today);
    nextWeek.setDate(nextWeek.getDate() + 7);
    this.toDate = nextWeek.toISOString().split('T')[0];
    this.load();
  }

  load(): void {
    this.loading.set(true);
    const from = `${this.fromDate}T00:00:00Z`;
    const to = `${this.toDate}T23:59:59Z`;
    this.appointmentService.getAll({ from, to, pageNumber: this.pageNumber }).subscribe({
      next: (result) => {
        let items = result.items;
        if (this.statusFilter) {
          items = items.filter(a => a.status === this.statusFilter);
        }
        this.appointments.set(items);
        this.totalPages.set(result.totalPages);
        this.totalCount.set(result.totalCount);
        this.hasPreviousPage.set(result.hasPreviousPage);
        this.hasNextPage.set(result.hasNextPage);
        this.loading.set(false);
      },
      error: () => { this.loading.set(false); },
    });
  }

  loadPage(page: number): void { this.pageNumber = page; this.load(); }

  openForm(): void {
    const dialogRef = this.dialog.open(AppointmentFormComponent, {
      width: '700px',
      maxHeight: '90vh',
    });
    dialogRef.afterClosed().subscribe((result) => { if (result) this.load(); });
  }

  viewDetail(apt: AppointmentDto): void {
    this.router.navigate(['/appointments', apt.id]);
  }

  confirmCancel(apt: AppointmentDto): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Cancelar Agendamento',
        message: `Deseja cancelar o agendamento de ${apt.customerName}?`,
        confirmText: 'Cancelar Agendamento',
      },
    });
    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.appointmentService.cancel(apt.id).subscribe({
          next: () => {
            this.snackBar.open('Agendamento cancelado.', 'OK', { duration: 3000, panelClass: ['snackbar-success'] });
            this.load();
          },
        });
      }
    });
  }
}
