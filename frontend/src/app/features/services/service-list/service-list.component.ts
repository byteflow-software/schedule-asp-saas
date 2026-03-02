import { Component, inject, OnInit , signal } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ServiceService } from '../../../core/services/service.service';
import { ServiceDto } from '../../../core/models/service.model';
import { CurrencyCentsPipe } from '../../../shared/pipes/currency-cents.pipe';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { ServiceFormComponent } from '../service-form/service-form.component';

@Component({
  selector: 'app-service-list',
  standalone: true,
  imports: [
    MatTableModule, MatButtonModule, MatIconModule,
    MatProgressSpinnerModule, MatTooltipModule, CurrencyCentsPipe,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <div>
          <h2>Serviços</h2>
          <p class="page-subtitle">Gerencie seus serviços</p>
        </div>
        <button mat-flat-button color="primary" (click)="openForm()">Novo Serviço</button>
      </div>

      @if (loading()) {
        <div class="loading"><mat-spinner></mat-spinner></div>
      } @else if (services().length === 0) {
        <div class="empty-state">
          <mat-icon>design_services</mat-icon>
          <h4>Nenhum serviço cadastrado</h4>
          <p>Comece adicionando seu primeiro serviço</p>
          <button mat-flat-button color="primary" (click)="openForm()">Adicionar Serviço</button>
        </div>
      } @else {
        <table mat-table [dataSource]="services()" class="full-width">
          <ng-container matColumnDef="name">
            <th mat-header-cell *matHeaderCellDef>Nome</th>
            <td mat-cell *matCellDef="let s">
              <div class="name-cell">
                <div class="avatar-sm">{{ s.name[0] }}</div>
                <span class="name-text">{{ s.name }}</span>
              </div>
            </td>
          </ng-container>
          <ng-container matColumnDef="durationMinutes">
            <th mat-header-cell *matHeaderCellDef>Duração</th>
            <td mat-cell *matCellDef="let s">{{ s.durationMinutes }} min</td>
          </ng-container>
          <ng-container matColumnDef="priceInCents">
            <th mat-header-cell *matHeaderCellDef>Preço</th>
            <td mat-cell *matCellDef="let s">{{ s.priceInCents | currencyCents }}</td>
          </ng-container>
          <ng-container matColumnDef="isActive">
            <th mat-header-cell *matHeaderCellDef>Status</th>
            <td mat-cell *matCellDef="let s">
              <span class="badge" [class.badge-active]="s.isActive" [class.badge-inactive]="!s.isActive">
                {{ s.isActive ? 'Ativo' : 'Inativo' }}
              </span>
            </td>
          </ng-container>
          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef></th>
            <td mat-cell *matCellDef="let s">
              <div class="actions">
                <button mat-button (click)="openForm(s)">Editar</button>
                <button mat-button class="danger-btn" (click)="confirmDelete(s)">Excluir</button>
              </div>
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
    .badge {
      display: inline-block; padding: 4px 12px; border-radius: 20px;
      font-size: 12px; font-weight: 600; line-height: 1;
    }
    .badge-active { background: #DCFCE7; color: #15803D; }
    .badge-inactive { background: #FEE2E2; color: #B91C1C; }
    .actions { display: flex; gap: 0; }
    .danger-btn { color: var(--danger, #EF4444) !important; }
  `],
})
export class ServiceListComponent implements OnInit {
  private serviceService = inject(ServiceService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  services = signal<ServiceDto[]>([]);
  columns = ['name', 'durationMinutes', 'priceInCents', 'isActive', 'actions'];
  loading = signal(true);

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.serviceService.getAll().subscribe({
      next: (result) => {
        this.services.set(result);
        this.loading.set(false);
      },
      error: () => { this.loading.set(false); },
    });
  }

  openForm(service?: ServiceDto): void {
    const dialogRef = this.dialog.open(ServiceFormComponent, { width: '500px', data: service || null });
    dialogRef.afterClosed().subscribe((result) => { if (result) this.load(); });
  }

  confirmDelete(service: ServiceDto): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Excluir Serviço', message: `Deseja realmente excluir "${service.name}"? Esta ação não pode ser desfeita.`, confirmText: 'Excluir' },
    });
    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.serviceService.delete(service.id).subscribe({
          next: () => { this.snackBar.open('Serviço excluído com sucesso.', 'OK', { duration: 3000, panelClass: ['snackbar-success'] }); this.load(); },
        });
      }
    });
  }
}
