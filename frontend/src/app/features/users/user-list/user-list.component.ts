import { Component, inject, OnInit } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { UserService } from '../../../core/services/user.service';
import { UserDto } from '../../../core/models/user.model';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { UserFormComponent } from '../user-form/user-form.component';
import { DateFormatPipe } from '../../../shared/pipes/date-format.pipe';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [
    MatTableModule, MatButtonModule, MatIconModule,
    MatProgressSpinnerModule, MatTooltipModule, DateFormatPipe,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <div>
          <h2>Equipe</h2>
          <p class="page-subtitle">Gerencie os membros da equipe</p>
        </div>
        <button mat-flat-button color="primary" (click)="openForm()">
          <mat-icon>person_add</mat-icon> Novo Membro
        </button>
      </div>

      @if (loading) {
        <div class="loading"><mat-spinner></mat-spinner></div>
      } @else {
        <table mat-table [dataSource]="users" class="full-width">
          <ng-container matColumnDef="fullName">
            <th mat-header-cell *matHeaderCellDef>Membro</th>
            <td mat-cell *matCellDef="let u">
              <div class="name-cell">
                <div class="avatar-sm" [class.inactive-avatar]="!u.isActive">{{ u.fullName[0] }}</div>
                <div>
                  <span class="name-text">{{ u.fullName }}</span>
                  <span class="email-text">{{ u.email }}</span>
                </div>
              </div>
            </td>
          </ng-container>
          <ng-container matColumnDef="role">
            <th mat-header-cell *matHeaderCellDef>Função</th>
            <td mat-cell *matCellDef="let u">
              <span class="role-badge" [class]="u.role.toLowerCase()">{{ u.role }}</span>
            </td>
          </ng-container>
          <ng-container matColumnDef="isActive">
            <th mat-header-cell *matHeaderCellDef>Status</th>
            <td mat-cell *matCellDef="let u">
              <span class="status-dot" [class.active]="u.isActive" [class.inactive]="!u.isActive"></span>
              {{ u.isActive ? 'Ativo' : 'Inativo' }}
            </td>
          </ng-container>
          <ng-container matColumnDef="createdAt">
            <th mat-header-cell *matHeaderCellDef>Cadastro</th>
            <td mat-cell *matCellDef="let u">{{ u.createdAt | dateFormat }}</td>
          </ng-container>
          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef></th>
            <td mat-cell *matCellDef="let u">
              @if (u.isActive) {
                <button mat-icon-button matTooltip="Desativar" class="danger-btn" (click)="confirmDeactivate(u)">
                  <mat-icon>block</mat-icon>
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
    .name-cell { display: flex; align-items: center; gap: 12px; }
    .avatar-sm {
      width: 36px; height: 36px; border-radius: 10px;
      background: var(--primary-100, #E0E7FF); color: var(--primary, #4F46E5);
      display: flex; align-items: center; justify-content: center;
      font-weight: 600; font-size: 14px;
    }
    .inactive-avatar { background: var(--gray-200, #E5E7EB); color: var(--gray-500, #6B7280); }
    .name-text { display: block; font-weight: 500; font-size: 14px; color: var(--gray-800, #1F2937); }
    .email-text { display: block; font-size: 12px; color: var(--gray-500, #6B7280); }
    .role-badge {
      padding: 4px 12px; border-radius: 20px; font-size: 12px; font-weight: 600;
      &.admin { background: #EDE9FE; color: #6D28D9; }
      &.staff { background: #ECFDF5; color: #059669; }
    }
    .status-dot {
      display: inline-block; width: 8px; height: 8px; border-radius: 50%; margin-right: 6px;
      &.active { background: var(--success, #10B981); }
      &.inactive { background: var(--gray-300, #D1D5DB); }
    }
    .danger-btn { color: var(--danger, #EF4444) !important; }
  `],
})
export class UserListComponent implements OnInit {
  private userService = inject(UserService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  users: UserDto[] = [];
  columns = ['fullName', 'role', 'isActive', 'createdAt', 'actions'];
  loading = true;

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.userService.getAll().subscribe({
      next: (users) => { this.users = users; this.loading = false; },
      error: () => {
        this.loading = false;
        this.snackBar.open('Erro ao carregar equipe.', 'OK', { duration: 3000 });
      },
    });
  }

  openForm(): void {
    const dialogRef = this.dialog.open(UserFormComponent, { width: '500px' });
    dialogRef.afterClosed().subscribe((result) => { if (result) this.load(); });
  }

  confirmDeactivate(user: UserDto): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Desativar Membro', message: `Deseja desativar "${user.fullName}"? O membro não poderá mais acessar o sistema.`, confirmText: 'Desativar' },
    });
    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.userService.deactivate(user.id).subscribe({
          next: () => { this.snackBar.open('Membro desativado com sucesso.', 'OK', { duration: 3000, panelClass: ['snackbar-success'] }); this.load(); },
        });
      }
    });
  }
}
