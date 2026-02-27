import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ServiceService } from '../../../core/services/service.service';
import { ServiceDto } from '../../../core/models/service.model';

@Component({
  selector: 'app-service-form',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, MatSlideToggleModule],
  template: `
    <div class="dialog-header">
      <h2 mat-dialog-title>{{ data ? 'Editar' : 'Novo' }} Serviço</h2>
      <p class="dialog-subtitle">{{ data ? 'Atualize as informações do serviço' : 'Preencha os dados do novo serviço' }}</p>
    </div>
    <mat-dialog-content>
      <form [formGroup]="form" class="form-fields">
        <mat-form-field class="full-width" appearance="outline">
          <mat-label>Nome</mat-label>
          <mat-icon matPrefix>design_services</mat-icon>
          <input matInput formControlName="name" placeholder="Corte de Cabelo" />
        </mat-form-field>
        <mat-form-field class="full-width" appearance="outline">
          <mat-label>Descrição</mat-label>
          <mat-icon matPrefix>description</mat-icon>
          <textarea matInput formControlName="description" rows="3" placeholder="Descrição do serviço (opcional)"></textarea>
        </mat-form-field>
        <div class="row">
          <mat-form-field class="half-width" appearance="outline">
            <mat-label>Duração (min)</mat-label>
            <mat-icon matPrefix>schedule</mat-icon>
            <input matInput type="number" formControlName="durationMinutes" placeholder="30" />
          </mat-form-field>
          <mat-form-field class="half-width" appearance="outline">
            <mat-label>Preço (R$)</mat-label>
            <mat-icon matPrefix>attach_money</mat-icon>
            <input matInput type="number" formControlName="priceReais" placeholder="50.00" step="0.01" />
          </mat-form-field>
        </div>
        @if (data) {
          <mat-slide-toggle formControlName="isActive" color="primary">Serviço ativo</mat-slide-toggle>
        }
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="dialogRef.close()">Cancelar</button>
      <button mat-flat-button color="primary" [disabled]="form.invalid || saving" (click)="save()">
        {{ saving ? 'Salvando...' : (data ? 'Atualizar' : 'Criar Serviço') }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .dialog-header { padding: 24px 24px 0; }
    h2 { margin: 0; font-size: 20px; font-weight: 700; color: #111827; }
    .dialog-subtitle { margin: 4px 0 0; font-size: 14px; color: #6B7280; }
    .form-fields { display: flex; flex-direction: column; gap: 4px; min-width: 420px; padding-top: 8px; }
    .row { display: flex; gap: 16px; }
    .half-width { flex: 1; }
  `],
})
export class ServiceFormComponent {
  data = inject<ServiceDto | null>(MAT_DIALOG_DATA);
  dialogRef = inject(MatDialogRef<ServiceFormComponent>);
  private fb = inject(FormBuilder);
  private serviceService = inject(ServiceService);
  private snackBar = inject(MatSnackBar);
  saving = false;

  form = this.fb.nonNullable.group({
    name: [this.data?.name ?? '', Validators.required],
    description: [this.data?.description ?? ''],
    durationMinutes: [this.data?.durationMinutes ?? 30, [Validators.required, Validators.min(5)]],
    priceReais: [this.data ? this.data.priceInCents / 100 : 0, [Validators.required, Validators.min(0)]],
    isActive: [this.data?.isActive ?? true],
  });

  save(): void {
    if (this.form.invalid) return;
    this.saving = true;
    const { name, description, durationMinutes, priceReais, isActive } = this.form.getRawValue();
    const priceInCents = Math.round(priceReais * 100);

    const request$ = this.data
      ? this.serviceService.update({ id: this.data.id, name, description, durationMinutes, priceInCents, isActive })
      : this.serviceService.create({ name, description, durationMinutes, priceInCents });

    request$.subscribe({
      next: () => {
        this.snackBar.open(`Serviço ${this.data ? 'atualizado' : 'criado'} com sucesso!`, 'OK', { duration: 3000, panelClass: ['snackbar-success'] });
        this.dialogRef.close(true);
      },
      error: () => { this.saving = false; },
    });
  }
}
