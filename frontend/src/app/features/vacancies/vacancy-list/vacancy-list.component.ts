import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { VacancyService } from '../../../core/services/vacancy.service';
import { UserService } from '../../../core/services/user.service';
import { ServiceService } from '../../../core/services/service.service';
import { VacancyDto } from '../../../core/models/vacancy.model';
import { UserDto } from '../../../core/models/user.model';
import { ServiceDto } from '../../../core/models/service.model';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { VacancyFormComponent } from '../vacancy-form/vacancy-form.component';
import { DateFormatPipe } from '../../../shared/pipes/date-format.pipe';

@Component({
  selector: 'app-vacancy-list',
  standalone: true,
  imports: [
    FormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    DateFormatPipe,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <div>
          <h2>Vagas</h2>
          <p class="page-subtitle">Gerencie os hor\u00e1rios dispon\u00edveis</p>
        </div>
        <button mat-flat-button color="primary" (click)="openForm()">
          <mat-icon>add</mat-icon> Abrir Vagas
        </button>
      </div>

      <div class="filters">
        <mat-form-field appearance="outline">
          <mat-label>Profissional</mat-label>
          <mat-select [(ngModel)]="filterUserId" (selectionChange)="load()">
            <mat-option value="">Todos</mat-option>
            @for (u of users; track u.id) {
              <mat-option [value]="u.id">{{ u.fullName }}</mat-option>
            }
          </mat-select>
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Servi\u00e7o</mat-label>
          <mat-select [(ngModel)]="filterServiceId" (selectionChange)="load()">
            <mat-option value="">Todos</mat-option>
            @for (s of services; track s.id) {
              <mat-option [value]="s.id">{{ s.name }}</mat-option>
            }
          </mat-select>
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Data In\u00edcio</mat-label>
          <input matInput type="date" [(ngModel)]="fromDate" (change)="load()" />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Data Fim</mat-label>
          <input matInput type="date" [(ngModel)]="toDate" (change)="load()" />
        </mat-form-field>
      </div>

      @if (loading) {
        <div class="loading"><mat-spinner></mat-spinner></div>
      } @else if (vacancies.length === 0) {
        <div class="empty-state">
          <mat-icon>event_available</mat-icon>
          <h4>Nenhuma vaga encontrada</h4>
          <p>Abra novas vagas para que clientes possam agendar</p>
          <button mat-flat-button color="primary" (click)="openForm()">
            <mat-icon>add</mat-icon> Abrir Vagas
          </button>
        </div>
      } @else {
        <table mat-table [dataSource]="vacancies" class="full-width">
          <ng-container matColumnDef="userName">
            <th mat-header-cell *matHeaderCellDef>Profissional</th>
            <td mat-cell *matCellDef="let v">
              <div class="name-cell">
                <div class="avatar-sm">{{ v.userName[0] }}</div>
                <span class="name-text">{{ v.userName }}</span>
              </div>
            </td>
          </ng-container>
          <ng-container matColumnDef="serviceName">
            <th mat-header-cell *matHeaderCellDef>Servi\u00e7o</th>
            <td mat-cell *matCellDef="let v">{{ v.serviceName }}</td>
          </ng-container>
          <ng-container matColumnDef="startTime">
            <th mat-header-cell *matHeaderCellDef>Data</th>
            <td mat-cell *matCellDef="let v">{{ v.startTime | dateFormat }}</td>
          </ng-container>
          <ng-container matColumnDef="endTime">
            <th mat-header-cell *matHeaderCellDef>Hor\u00e1rio</th>
            <td mat-cell *matCellDef="let v">
              {{ v.startTime | dateFormat:'time' }} - {{ v.endTime | dateFormat:'time' }}
            </td>
          </ng-container>
          <ng-container matColumnDef="isBooked">
            <th mat-header-cell *matHeaderCellDef>Status</th>
            <td mat-cell *matCellDef="let v">
              <span class="status-badge" [class]="v.isBooked ? 'booked' : 'available'">
                {{ v.isBooked ? 'Agendada' : 'Dispon\u00edvel' }}
              </span>
            </td>
          </ng-container>
          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef></th>
            <td mat-cell *matCellDef="let v">
              @if (!v.isBooked) {
                <button mat-icon-button matTooltip="Excluir" class="danger-btn" (click)="confirmDelete(v)">
                  <mat-icon>delete_outline</mat-icon>
                </button>
              }
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="columns"></tr>
          <tr mat-row *matRowDef="let row; columns: columns;"></tr>
        </table>
      }
    </div>
  `,
  styles: [`
    .page-subtitle { font-size: 14px; color: var(--gray-500, #6B7280); margin: 4px 0 0; }
    .filters { display: flex; gap: 16px; margin-bottom: 16px; flex-wrap: wrap; }
    .loading { display: flex; justify-content: center; padding: 48px; }
    .empty-state {
      text-align: center; padding: 64px 24px;
      background: white; border-radius: 16px; border: 2px dashed var(--gray-200, #E5E7EB);
      mat-icon { font-size: 48px; width: 48px; height: 48px; color: var(--gray-300, #D1D5DB); }
      h4 { font-size: 16px; color: var(--gray-600, #4B5563); margin: 12px 0 4px; }
      p { font-size: 14px; color: var(--gray-400, #9CA3AF); margin: 0 0 16px; }
    }
    .name-cell { display: flex; align-items: center; gap: 12px; }
    .avatar-sm {
      width: 32px; height: 32px; border-radius: 8px;
      background: var(--primary-100, #E0E7FF); color: var(--primary, #4F46E5);
      display: flex; align-items: center; justify-content: center;
      font-weight: 600; font-size: 13px;
    }
    .name-text { font-weight: 500; }
    .status-badge {
      padding: 4px 12px;
      border-radius: 12px;
      font-size: 12px;
      font-weight: 500;
      text-transform: uppercase;
      &.booked { background: #e3f2fd; color: #1565c0; }
      &.available { background: #e8f5e9; color: #2e7d32; }
    }
    .danger-btn { color: var(--danger, #EF4444) !important; }
  `],
})
export class VacancyListComponent implements OnInit {
  private vacancyService = inject(VacancyService);
  private userService = inject(UserService);
  private serviceService = inject(ServiceService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  vacancies: VacancyDto[] = [];
  users: UserDto[] = [];
  services: ServiceDto[] = [];
  columns = ['userName', 'serviceName', 'startTime', 'endTime', 'isBooked', 'actions'];
  loading = true;

  filterUserId = '';
  filterServiceId = '';
  fromDate = '';
  toDate = '';

  ngOnInit(): void {
    const today = new Date();
    this.fromDate = today.toISOString().split('T')[0];
    const nextWeek = new Date(today);
    nextWeek.setDate(nextWeek.getDate() + 7);
    this.toDate = nextWeek.toISOString().split('T')[0];

    this.userService.getAll().subscribe({
      next: (users) => { this.users = users.filter(u => u.isActive); },
    });
    this.serviceService.getAll().subscribe({
      next: (services) => { this.services = services.filter(s => s.isActive); },
    });

    this.load();
  }

  load(): void {
    this.loading = true;
    const filters: Record<string, string | undefined> = {};
    if (this.filterUserId) filters['userId'] = this.filterUserId;
    if (this.filterServiceId) filters['serviceId'] = this.filterServiceId;
    if (this.fromDate) filters['from'] = `${this.fromDate}T00:00:00Z`;
    if (this.toDate) filters['to'] = `${this.toDate}T23:59:59Z`;

    this.vacancyService.getAll(filters).subscribe({
      next: (vacancies) => {
        this.vacancies = vacancies;
        this.loading = false;
      },
      error: () => { this.loading = false; },
    });
  }

  openForm(): void {
    const dialogRef = this.dialog.open(VacancyFormComponent, { width: '680px', maxHeight: '90vh' });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) this.load();
    });
  }

  confirmDelete(vacancy: VacancyDto): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Excluir Vaga',
        message: `Deseja realmente excluir a vaga de ${vacancy.userName} em ${new Date(vacancy.startTime).toLocaleDateString('pt-BR')}?`,
        confirmText: 'Excluir',
      },
    });
    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.vacancyService.delete(vacancy.id).subscribe({
          next: () => {
            this.snackBar.open('Vaga exclu\u00edda com sucesso.', 'OK', { duration: 3000, panelClass: ['snackbar-success'] });
            this.load();
          },
        });
      }
    });
  }
}
