import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CustomerService } from '../../../core/services/customer.service';
import { CustomerDto } from '../../../core/models/customer.model';

@Component({
  selector: 'app-customer-form',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule],
  template: `
    <div class="dialog-header">
      <h2 mat-dialog-title>{{ data ? 'Editar' : 'Novo' }} Cliente</h2>
      <p class="dialog-subtitle">{{ data ? 'Atualize as informações do cliente' : 'Preencha os dados do novo cliente' }}</p>
    </div>
    <mat-dialog-content>
      <form [formGroup]="form" class="form-fields">
        <mat-form-field class="full-width" appearance="outline">
          <mat-label>Nome Completo</mat-label>
          <mat-icon matPrefix>person</mat-icon>
          <input matInput formControlName="fullName" placeholder="João Silva" />
        </mat-form-field>
        <mat-form-field class="full-width" appearance="outline">
          <mat-label>CPF/CNPJ</mat-label>
          <mat-icon matPrefix>badge</mat-icon>
          <input matInput formControlName="cpfCnpj" placeholder="000.000.000-00" />
        </mat-form-field>
        <mat-form-field class="full-width" appearance="outline">
          <mat-label>Email</mat-label>
          <mat-icon matPrefix>email</mat-icon>
          <input matInput formControlName="email" type="email" placeholder="joao@email.com" />
        </mat-form-field>
        <mat-form-field class="full-width" appearance="outline">
          <mat-label>Telefone</mat-label>
          <mat-icon matPrefix>phone</mat-icon>
          <input matInput formControlName="phone" placeholder="(11) 99999-9999" />
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="dialogRef.close()">Cancelar</button>
      <button mat-flat-button color="primary" [disabled]="form.invalid || saving" (click)="save()">
{{ saving ? 'Salvando...' : (data ? 'Atualizar' : 'Criar Cliente') }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .dialog-header { padding: 24px 24px 0; }
    h2 { margin: 0; font-size: 20px; font-weight: 700; color: #111827; }
    .dialog-subtitle { margin: 4px 0 0; font-size: 14px; color: #6B7280; }
    .form-fields { display: flex; flex-direction: column; gap: 4px; min-width: 420px; padding-top: 8px; }
  `],
})
export class CustomerFormComponent {
  data = inject<CustomerDto | null>(MAT_DIALOG_DATA);
  dialogRef = inject(MatDialogRef<CustomerFormComponent>);
  private fb = inject(FormBuilder);
  private customerService = inject(CustomerService);
  private snackBar = inject(MatSnackBar);
  saving = false;

  form = this.fb.nonNullable.group({
    fullName: [this.data?.fullName ?? '', Validators.required],
    cpfCnpj: [this.data?.cpfCnpj ?? '', Validators.required],
    email: [this.data?.email ?? '', [Validators.required, Validators.email]],
    phone: [this.data?.phone ?? ''],
  });

  save(): void {
    if (this.form.invalid) return;
    this.saving = true;
    const values = this.form.getRawValue();
    const request$ = this.data
      ? this.customerService.update({ id: this.data.id, ...values })
      : this.customerService.create(values);
    request$.subscribe({
      next: () => {
        this.snackBar.open(`Cliente ${this.data ? 'atualizado' : 'criado'} com sucesso!`, 'OK', { duration: 3000, panelClass: ['snackbar-success'] });
        this.dialogRef.close(true);
      },
      error: () => { this.saving = false; },
    });
  }
}
