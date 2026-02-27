import { Component, inject, OnInit , signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { CustomerService } from '../../../core/services/customer.service';
import { CustomerDto } from '../../../core/models/customer.model';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { CustomerFormComponent } from '../customer-form/customer-form.component';
import { DateFormatPipe } from '../../../shared/pipes/date-format.pipe';

@Component({
  selector: 'app-customer-list',
  standalone: true,
  imports: [
    FormsModule, MatTableModule, MatButtonModule, MatIconModule,
    MatFormFieldModule, MatInputModule, MatProgressSpinnerModule,
    MatTooltipModule, PaginationComponent, DateFormatPipe,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <div>
          <h2>Clientes</h2>
          <p class="page-subtitle">Gerencie sua base de clientes</p>
        </div>
        <button mat-flat-button color="primary" (click)="openForm()">
          <mat-icon>person_add</mat-icon> Novo Cliente
        </button>
      </div>

      <div class="search-bar">
        <mat-icon class="search-icon">search</mat-icon>
        <input type="text" [(ngModel)]="search" (input)="onSearch()"
          placeholder="Buscar por nome ou email..." class="search-input" />
        @if (search) {
          <button class="clear-btn" (click)="search = ''; onSearch()">
            <mat-icon>close</mat-icon>
          </button>
        }
      </div>

      @if (loading()) {
        <div class="loading"><mat-spinner></mat-spinner></div>
      } @else if (customers.length === 0) {
        <div class="empty-state">
          <mat-icon>people_outline</mat-icon>
          <h4>{{ search ? 'Nenhum cliente encontrado' : 'Nenhum cliente cadastrado' }}</h4>
          <p>{{ search ? 'Tente outro termo de busca' : 'Comece adicionando seu primeiro cliente' }}</p>
          @if (!search) {
            <button mat-flat-button color="primary" (click)="openForm()">
              <mat-icon>add</mat-icon> Adicionar Cliente
            </button>
          }
        </div>
      } @else {
        <table mat-table [dataSource]="customers" class="full-width">
          <ng-container matColumnDef="fullName">
            <th mat-header-cell *matHeaderCellDef>Nome</th>
            <td mat-cell *matCellDef="let c">
              <div class="name-cell">
                <div class="avatar-sm">{{ c.fullName[0] }}</div>
                <span class="name-text">{{ c.fullName }}</span>
              </div>
            </td>
          </ng-container>
          <ng-container matColumnDef="email">
            <th mat-header-cell *matHeaderCellDef>Email</th>
            <td mat-cell *matCellDef="let c">{{ c.email }}</td>
          </ng-container>
          <ng-container matColumnDef="phone">
            <th mat-header-cell *matHeaderCellDef>Telefone</th>
            <td mat-cell *matCellDef="let c">{{ c.phone || '-' }}</td>
          </ng-container>
          <ng-container matColumnDef="createdAt">
            <th mat-header-cell *matHeaderCellDef>Cadastro</th>
            <td mat-cell *matCellDef="let c">{{ c.createdAt | dateFormat }}</td>
          </ng-container>
          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef></th>
            <td mat-cell *matCellDef="let c">
              <div class="actions">
                <button mat-icon-button matTooltip="Editar" (click)="openForm(c)">
                  <mat-icon>edit</mat-icon>
                </button>
                <button mat-icon-button matTooltip="Excluir" class="danger-btn" (click)="confirmDelete(c)">
                  <mat-icon>delete_outline</mat-icon>
                </button>
              </div>
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="columns"></tr>
          <tr mat-row *matRowDef="let row; columns: columns;"></tr>
        </table>

        <app-pagination
          [pageNumber]="pageNumber" [totalPages]="totalPages" [totalCount]="totalCount"
          [hasPreviousPage]="hasPreviousPage" [hasNextPage]="hasNextPage"
          (pageChange)="loadPage($event)" />
      }
    </div>
  `,
  styles: [`
    .page-subtitle { font-size: 14px; color: var(--gray-500, #6B7280); margin: 4px 0 0; }
    .search-bar {
      display: flex; align-items: center; gap: 12px;
      background: white; border: 1px solid var(--gray-200, #E5E7EB);
      border-radius: 12px; padding: 0 16px; margin-bottom: 20px;
      transition: border-color 0.2s, box-shadow 0.2s;
      &:focus-within {
        border-color: var(--primary, #4F46E5);
        box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
      }
    }
    .search-icon { color: var(--gray-400, #9CA3AF); font-size: 20px; }
    .search-input {
      flex: 1; border: none; outline: none; padding: 14px 0;
      font-size: 14px; background: transparent; font-family: inherit;
      color: var(--gray-800, #1F2937);
      &::placeholder { color: var(--gray-400, #9CA3AF); }
    }
    .clear-btn {
      border: none; background: none; cursor: pointer; padding: 4px;
      color: var(--gray-400, #9CA3AF); display: flex;
      &:hover { color: var(--gray-600, #4B5563); }
    }
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
    .actions { display: flex; gap: 0; }
    .danger-btn { color: var(--danger, #EF4444) !important; }
  `],
})
export class CustomerListComponent implements OnInit {
  private customerService = inject(CustomerService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  customers: CustomerDto[] = [];
  columns = ['fullName', 'email', 'phone', 'createdAt', 'actions'];
  search = '';
  loading = signal(true);
  pageNumber = 1;
  totalPages = 1;
  totalCount = 0;
  hasPreviousPage = false;
  hasNextPage = false;
  private searchTimeout: ReturnType<typeof setTimeout> | null = null;

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.customerService.getAll(this.search, this.pageNumber).subscribe({
      next: (result) => {
        this.customers = result.items;
        this.totalPages = result.totalPages;
        this.totalCount = result.totalCount;
        this.hasPreviousPage = result.hasPreviousPage;
        this.hasNextPage = result.hasNextPage;
        this.loading.set(false);
      },
      error: () => { this.loading.set(false); },
    });
  }

  onSearch(): void {
    if (this.searchTimeout) clearTimeout(this.searchTimeout);
    this.searchTimeout = setTimeout(() => { this.pageNumber = 1; this.load(); }, 400);
  }

  loadPage(page: number): void { this.pageNumber = page; this.load(); }

  openForm(customer?: CustomerDto): void {
    const dialogRef = this.dialog.open(CustomerFormComponent, { width: '500px', data: customer || null });
    dialogRef.afterClosed().subscribe((result) => { if (result) this.load(); });
  }

  confirmDelete(customer: CustomerDto): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Excluir Cliente', message: `Deseja realmente excluir "${customer.fullName}"? Esta ação não pode ser desfeita.`, confirmText: 'Excluir' },
    });
    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.customerService.delete(customer.id).subscribe({
          next: () => { this.snackBar.open('Cliente excluído com sucesso.', 'OK', { duration: 3000, panelClass: ['snackbar-success'] }); this.load(); },
        });
      }
    });
  }
}
