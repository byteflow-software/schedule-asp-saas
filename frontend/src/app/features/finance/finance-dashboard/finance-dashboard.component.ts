import { Component, inject, OnInit , signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { TransactionService } from '../../../core/services/transaction.service';
import { TransactionDto, TransactionSummaryDto } from '../../../core/models/transaction.model';
import { PaginatedList } from '../../../core/models/paginated.model';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { CurrencyCentsPipe } from '../../../shared/pipes/currency-cents.pipe';
import { StatusTranslatePipe } from '../../../shared/pipes/status.pipe';
import { DateFormatPipe } from '../../../shared/pipes/date-format.pipe';

@Component({
  selector: 'app-payment-method-dialog',
  standalone: true,
  imports: [MatButtonModule],
  template: `
    <div class="dialog-container">
      <h3>Selecione o Método de Pagamento</h3>
      <div class="method-buttons">
        <button mat-flat-button color="primary" (click)="select('pix')">PIX</button>
        <button mat-flat-button color="primary" (click)="select('cartao')">Cartão</button>
        <button mat-flat-button color="primary" (click)="select('dinheiro')">Dinheiro</button>
      </div>
      <button mat-button (click)="dialogRef.close()">Cancelar</button>
    </div>
  `,
  styles: [`
    .dialog-container {
      padding: 24px;
      text-align: center;
    }
    h3 {
      margin: 0 0 20px;
      font-size: 18px;
      font-weight: 600;
      color: var(--gray-800, #1F2937);
    }
    .method-buttons {
      display: flex;
      gap: 12px;
      justify-content: center;
      margin-bottom: 16px;
    }
    .method-buttons button {
      min-width: 120px;
    }
  `],
})
export class PaymentMethodDialogComponent {
  dialogRef = inject(MatDialogRef<PaymentMethodDialogComponent>);

  select(method: string): void {
    this.dialogRef.close(method);
  }
}

@Component({
  selector: 'app-finance-dashboard',
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
    MatDialogModule,
    PaginationComponent,
    CurrencyCentsPipe,
    StatusTranslatePipe,
    DateFormatPipe,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <div>
          <h2>Financeiro</h2>
          <p class="page-subtitle">Visão geral das transações</p>
        </div>
      </div>

      @if (loading()) {
        <div class="loading"><mat-spinner></mat-spinner></div>
      } @else {
        <div class="stats-grid">
          <div class="stat-card stat-emerald">
            <div class="stat-icon-wrap"><mat-icon>trending_up</mat-icon></div>
            <div class="stat-info">
              <span class="stat-value">{{ summary().totalRevenueCents | currencyCents }}</span>
              <span class="stat-label">Receita Total</span>
            </div>
          </div>
          <div class="stat-card stat-amber">
            <div class="stat-icon-wrap"><mat-icon>schedule</mat-icon></div>
            <div class="stat-info">
              <span class="stat-value">{{ summary().totalPendingCents | currencyCents }}</span>
              <span class="stat-label">Pendente</span>
            </div>
          </div>
          <div class="stat-card stat-indigo">
            <div class="stat-icon-wrap"><mat-icon>check_circle</mat-icon></div>
            <div class="stat-info">
              <span class="stat-value">{{ summary().totalPaidCents | currencyCents }}</span>
              <span class="stat-label">Recebido</span>
            </div>
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
              <mat-option value="Pending">Pendente</mat-option>
              <mat-option value="Paid">Pago</mat-option>
              <mat-option value="Cancelled">Cancelado</mat-option>
              <mat-option value="Refunded">Estornado</mat-option>
            </mat-select>
          </mat-form-field>
        </div>

        <table mat-table [dataSource]="transactions()" class="full-width">
          <ng-container matColumnDef="referenceNumber">
            <th mat-header-cell *matHeaderCellDef>Referência</th>
            <td mat-cell *matCellDef="let t">{{ t.referenceNumber }}</td>
          </ng-container>
          <ng-container matColumnDef="customerName">
            <th mat-header-cell *matHeaderCellDef>Cliente</th>
            <td mat-cell *matCellDef="let t">{{ t.customerName }}</td>
          </ng-container>
          <ng-container matColumnDef="description">
            <th mat-header-cell *matHeaderCellDef>Descrição</th>
            <td mat-cell *matCellDef="let t">{{ t.description }}</td>
          </ng-container>
          <ng-container matColumnDef="amountInCents">
            <th mat-header-cell *matHeaderCellDef>Valor</th>
            <td mat-cell *matCellDef="let t">{{ t.amountInCents | currencyCents }}</td>
          </ng-container>
          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>Status</th>
            <td mat-cell *matCellDef="let t">
              <span class="status-badge" [class]="t.status.toLowerCase()">{{ t.status | statusTranslate }}</span>
            </td>
          </ng-container>
          <ng-container matColumnDef="createdAt">
            <th mat-header-cell *matHeaderCellDef>Data</th>
            <td mat-cell *matCellDef="let t">{{ t.createdAt | dateFormat:'short' }}</td>
          </ng-container>
          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef>Ações</th>
            <td mat-cell *matCellDef="let t">
              @if (t.invoiceUrl) {
                <a mat-button color="accent" [href]="t.invoiceUrl" target="_blank">Ver Cobrança</a>
              }
              @if (t.status === 'Pending') {
                <button mat-button color="primary" (click)="openPaymentDialog(t)">Registrar Pagamento</button>
                <button mat-button color="warn" (click)="cancelTransaction(t)">Cancelar</button>
              }
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="columns"></tr>
          <tr mat-row *matRowDef="let row; columns: columns;"></tr>
        </table>

        <app-pagination
          [pageNumber]="pageNumber"
          [totalPages]="totalPages()"
          [totalCount]="totalCount()"
          [hasPreviousPage]="hasPreviousPage()"
          [hasNextPage]="hasNextPage()"
          (pageChange)="loadPage($event)" />
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
    .stat-value { font-size: 28px; font-weight: 700; line-height: 1; }
    .stat-label { font-size: 13px; opacity: 0.85; margin-top: 4px; }
    .filters { display: flex; gap: 16px; margin-bottom: 16px; flex-wrap: wrap; }
    .loading { display: flex; justify-content: center; padding: 48px; }
    .status-badge {
      padding: 4px 12px;
      border-radius: 12px;
      font-size: 12px;
      text-transform: uppercase;
      font-weight: 500;
      &.pending { background: #fef3c7; color: #92400e; }
      &.paid { background: #d1fae5; color: #065f46; }
      &.cancelled { background: #fce4ec; color: #c62828; }
      &.refunded { background: #ede9fe; color: #5b21b6; }
    }
  `],
})
export class FinanceDashboardComponent implements OnInit {
  private transactionService = inject(TransactionService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  loading = signal(true);
  transactions = signal<TransactionDto[]>([]);
  summary = signal<TransactionSummaryDto>({ totalRevenueCents: 0, totalPendingCents: 0, totalPaidCents: 0, count: 0 });
  columns = ['referenceNumber', 'customerName', 'description', 'amountInCents', 'status', 'createdAt', 'actions'];

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
    const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
    this.fromDate = firstDay.toISOString().split('T')[0];
    this.toDate = today.toISOString().split('T')[0];
    this.load();
  }

  load(): void {
    this.loading.set(true);
    const from = this.fromDate ? `${this.fromDate}T00:00:00Z` : undefined;
    const to = this.toDate ? `${this.toDate}T23:59:59Z` : undefined;

    this.transactionService.getSummary(from, to).subscribe({
      next: (summary) => { this.summary.set(summary); },
      error: () => {},
    });

    this.transactionService.getAll({
      from,
      to,
      status: this.statusFilter || undefined,
      pageNumber: this.pageNumber,
      pageSize: 10,
    }).subscribe({
      next: (result) => {
        this.transactions.set(result.items);
        this.totalPages.set(result.totalPages);
        this.totalCount.set(result.totalCount);
        this.hasPreviousPage.set(result.hasPreviousPage);
        this.hasNextPage.set(result.hasNextPage);
        this.loading.set(false);
      },
      error: () => { this.loading.set(false); },
    });
  }

  loadPage(page: number): void {
    this.pageNumber = page;
    this.load();
  }

  openPaymentDialog(transaction: TransactionDto): void {
    const dialogRef = this.dialog.open(PaymentMethodDialogComponent, { width: '400px' });
    dialogRef.afterClosed().subscribe((method) => {
      if (method) {
        this.transactionService.pay(transaction.id, method).subscribe({
          next: () => {
            this.snackBar.open('Pagamento registrado com sucesso.', 'OK', { duration: 3000, panelClass: ['snackbar-success'] });
            this.load();
          },
          error: () => {
            this.snackBar.open('Erro ao registrar pagamento.', 'OK', { duration: 3000, panelClass: ['snackbar-error'] });
          },
        });
      }
    });
  }

  cancelTransaction(transaction: TransactionDto): void {
    this.transactionService.cancel(transaction.id).subscribe({
      next: () => {
        this.snackBar.open('Transação cancelada.', 'OK', { duration: 3000, panelClass: ['snackbar-success'] });
        this.load();
      },
      error: () => {
        this.snackBar.open('Erro ao cancelar transação.', 'OK', { duration: 3000, panelClass: ['snackbar-error'] });
      },
    });
  }
}
