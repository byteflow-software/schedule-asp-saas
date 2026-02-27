import { Component, inject, OnInit , signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { AppointmentService } from '../../../core/services/appointment.service';
import { TransactionService } from '../../../core/services/transaction.service';
import { AppointmentDto } from '../../../core/models/appointment.model';
import { CurrencyCentsPipe } from '../../../shared/pipes/currency-cents.pipe';
import { StatusTranslatePipe } from '../../../shared/pipes/status.pipe';
import { DateFormatPipe } from '../../../shared/pipes/date-format.pipe';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-appointment-detail',
  standalone: true,
  imports: [
    MatCardModule, MatButtonModule, MatIconModule, MatChipsModule,
    MatProgressSpinnerModule, CurrencyCentsPipe, StatusTranslatePipe, DateFormatPipe,
  ],
  template: `
    <div class="page-container">
      @if (loading()) {
        <div class="loading"><mat-spinner></mat-spinner></div>
      } @else if (appointment) {
        <div class="page-header">
          <div>
            <button mat-icon-button (click)="goBack()"><mat-icon>arrow_back</mat-icon></button>
            <h2>Detalhes do Agendamento</h2>
          </div>
          <div class="header-actions">
            @if (appointment.status === 'PendingPayment') {
              <button mat-stroked-button color="primary" (click)="confirmPayment()">
                <mat-icon>payment</mat-icon> Registrar Pagamento
              </button>
            }
            @if (appointment.status === 'Confirmed') {
              <button mat-stroked-button color="primary" (click)="markDone()">
                <mat-icon>check_circle</mat-icon> Marcar Concluído
              </button>
            }
            @if (appointment.status !== 'Cancelled' && appointment.status !== 'Completed') {
              <button mat-stroked-button color="warn" (click)="cancelAppointment()">
                <mat-icon>cancel</mat-icon> Cancelar
              </button>
            }
          </div>
        </div>

        <div class="detail-grid">
          <div class="detail-card">
            <h3><mat-icon>info</mat-icon> Informações</h3>
            <div class="info-grid">
              <div class="info-item">
                <span class="label">Status</span>
                <span class="status-badge" [attr.data-status]="appointment.status">
                  {{ appointment.status | statusTranslate }}
                </span>
              </div>
              <div class="info-item">
                <span class="label">Serviço</span>
                <span class="value">{{ appointment.serviceName }}</span>
              </div>
              <div class="info-item">
                <span class="label">Valor</span>
                <span class="value price">{{ appointment.priceInCents | currencyCents }}</span>
              </div>
              <div class="info-item">
                <span class="label">Criado em</span>
                <span class="value">{{ appointment.createdAt | dateFormat }}</span>
              </div>
            </div>
          </div>

          <div class="detail-card">
            <h3><mat-icon>schedule</mat-icon> Horário</h3>
            <div class="info-grid">
              <div class="info-item">
                <span class="label">Início</span>
                <span class="value">{{ appointment.startTime | dateFormat }}</span>
              </div>
              <div class="info-item">
                <span class="label">Fim</span>
                <span class="value">{{ appointment.endTime | dateFormat }}</span>
              </div>
            </div>
          </div>

          <div class="detail-card">
            <h3><mat-icon>person</mat-icon> Cliente</h3>
            <div class="info-grid">
              <div class="info-item">
                <span class="label">Nome</span>
                <span class="value">{{ appointment.customerName }}</span>
              </div>
            </div>
          </div>

          <div class="detail-card">
            <h3><mat-icon>badge</mat-icon> Profissional</h3>
            <div class="info-grid">
              <div class="info-item">
                <span class="label">Nome</span>
                <span class="value">{{ appointment.userName }}</span>
              </div>
            </div>
          </div>

          @if (appointment.notes) {
            <div class="detail-card full-width">
              <h3><mat-icon>notes</mat-icon> Observações</h3>
              <p class="notes-text">{{ appointment.notes }}</p>
            </div>
          }

          @if (appointment.transaction) {
            <div class="detail-card full-width">
              <h3><mat-icon>receipt</mat-icon> Cobrança</h3>
              <div class="info-grid">
                <div class="info-item">
                  <span class="label">Referência</span>
                  <span class="value mono">{{ appointment.transaction.referenceNumber }}</span>
                </div>
                <div class="info-item">
                  <span class="label">Valor</span>
                  <span class="value price">{{ appointment.transaction.amountInCents | currencyCents }}</span>
                </div>
                <div class="info-item">
                  <span class="label">Status</span>
                  <span class="status-badge" [attr.data-status]="appointment.transaction.status">
                    {{ appointment.transaction.status | statusTranslate }}
                  </span>
                </div>
                @if (appointment.transaction.paymentMethod) {
                  <div class="info-item">
                    <span class="label">Forma de Pagamento</span>
                    <span class="value">{{ appointment.transaction.paymentMethod }}</span>
                  </div>
                }
                @if (appointment.transaction.paidAt) {
                  <div class="info-item">
                    <span class="label">Pago em</span>
                    <span class="value">{{ appointment.transaction.paidAt | dateFormat }}</span>
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
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px;
      > div { display: flex; align-items: center; gap: 8px; }
      h2 { margin: 0; }
    }
    .header-actions { display: flex; gap: 8px; }
    .detail-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 16px; }
    .detail-card {
      background: white; border-radius: 16px; padding: 24px; border: 1px solid #F3F4F6;
      box-shadow: 0 1px 3px rgba(0,0,0,0.06);
      h3 { margin: 0 0 16px; font-size: 16px; font-weight: 600; color: #374151; display: flex; align-items: center; gap: 8px;
        mat-icon { font-size: 20px; width: 20px; height: 20px; color: #6B7280; }
      }
      &.full-width { grid-column: 1 / -1; }
    }
    .info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
    .info-item { display: flex; flex-direction: column; gap: 4px; }
    .label { font-size: 12px; font-weight: 500; color: #9CA3AF; text-transform: uppercase; letter-spacing: 0.5px; }
    .value { font-size: 15px; font-weight: 500; color: #111827; }
    .price { font-weight: 700; color: #059669; font-size: 18px; }
    .mono { font-family: monospace; letter-spacing: 0.5px; }
    .notes-text { font-size: 14px; color: #4B5563; line-height: 1.6; margin: 0; }
    .status-badge {
      display: inline-flex; padding: 4px 12px; border-radius: 20px; font-size: 12px; font-weight: 600;
      width: fit-content;
    }
    [data-status="PendingPayment"], [data-status="Pending"] { background: #FEF3C7; color: #92400E; }
    [data-status="Confirmed"], [data-status="Paid"] { background: #D1FAE5; color: #065F46; }
    [data-status="Completed"] { background: #DBEAFE; color: #1E40AF; }
    [data-status="Cancelled"] { background: #FEE2E2; color: #991B1B; }
    [data-status="NoShow"] { background: #F3E8FF; color: #6B21A8; }
    [data-status="Refunded"] { background: #F3E8FF; color: #6B21A8; }
  `],
})
export class AppointmentDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private appointmentService = inject(AppointmentService);
  private transactionService = inject(TransactionService);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);

  appointment: AppointmentDto | null = null;
  loading = signal(true);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) this.loadAppointment(id);
  }

  loadAppointment(id: string): void {
    this.loading.set(true);
    this.appointmentService.getById(id).subscribe({
      next: (a) => { this.appointment = a; this.loading.set(false); },
      error: () => { this.loading.set(false); this.router.navigate(['/appointments']); },
    });
  }

  goBack(): void { this.router.navigate(['/appointments']); }

  confirmPayment(): void {
    if (!this.appointment?.transaction) return;
    const txId = this.appointment.transaction.id;
    const dialogRef = this.dialog.open(PaymentMethodDialogComponent, { width: '360px' });
    dialogRef.afterClosed().subscribe((method: string) => {
      if (method) {
        this.transactionService.pay(txId, method).subscribe({
          next: () => {
            this.snackBar.open('Pagamento registrado!', 'OK', { duration: 3000, panelClass: ['snackbar-success'] });
            this.loadAppointment(this.appointment!.id);
          },
        });
      }
    });
  }

  markDone(): void {
    if (!this.appointment) return;
    this.appointmentService.markDone(this.appointment.id).subscribe({
      next: () => {
        this.snackBar.open('Agendamento concluído!', 'OK', { duration: 3000, panelClass: ['snackbar-success'] });
        this.loadAppointment(this.appointment!.id);
      },
    });
  }

  cancelAppointment(): void {
    if (!this.appointment) return;
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Cancelar Agendamento', message: 'Deseja realmente cancelar este agendamento? A cobrança também será cancelada.', confirmText: 'Cancelar Agendamento' },
    });
    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.appointmentService.cancel(this.appointment!.id).subscribe({
          next: () => {
            this.snackBar.open('Agendamento cancelado.', 'OK', { duration: 3000 });
            this.loadAppointment(this.appointment!.id);
          },
        });
      }
    });
  }
}

import { MatDialogModule, MatDialogRef as DialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-payment-method-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <h2 mat-dialog-title>Forma de Pagamento</h2>
    <mat-dialog-content>
      <div class="method-grid">
        <button mat-stroked-button (click)="select('pix')" class="method-btn">
          <mat-icon>qr_code</mat-icon> PIX
        </button>
        <button mat-stroked-button (click)="select('cartao')" class="method-btn">
          <mat-icon>credit_card</mat-icon> Cartão
        </button>
        <button mat-stroked-button (click)="select('dinheiro')" class="method-btn">
          <mat-icon>payments</mat-icon> Dinheiro
        </button>
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancelar</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .method-grid { display: flex; gap: 12px; padding: 8px 0; }
    .method-btn { flex: 1; display: flex; flex-direction: column; align-items: center; gap: 8px;
      padding: 20px 16px; border-radius: 12px; font-size: 14px; font-weight: 500;
      mat-icon { font-size: 28px; width: 28px; height: 28px; }
    }
  `],
})
export class PaymentMethodDialogComponent {
  dialogRef = inject(DialogRef<PaymentMethodDialogComponent>);
  select(method: string): void { this.dialogRef.close(method); }
}
