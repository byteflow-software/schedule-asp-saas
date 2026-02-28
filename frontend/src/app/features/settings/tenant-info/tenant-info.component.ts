import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TenantService } from '../../../core/services/tenant.service';
import { TenantDto } from '../../../core/models/tenant.model';

@Component({
  selector: 'app-tenant-info',
  standalone: true,
  imports: [
    ReactiveFormsModule, FormsModule, MatCardModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule, MatProgressSpinnerModule,
  ],
  template: `
    @if (loading()) {
      <div class="loading"><mat-spinner></mat-spinner></div>
    } @else {
      <mat-card>
        <mat-card-header>
          <mat-card-title>Informações da Empresa</mat-card-title>
          <mat-card-subtitle>Dados cadastrais da sua empresa</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <form [formGroup]="form" class="form-grid">
            <mat-form-field appearance="outline">
              <mat-label>Nome da Empresa</mat-label>
              <input matInput formControlName="name" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>CPF/CNPJ</mat-label>
              <input matInput formControlName="cpfCnpj" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Email</mat-label>
              <input matInput formControlName="email" type="email" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Telefone</mat-label>
              <input matInput formControlName="phone" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Endereço</mat-label>
              <input matInput formControlName="address" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Número</mat-label>
              <input matInput formControlName="addressNumber" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Complemento</mat-label>
              <input matInput formControlName="complement" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Bairro</mat-label>
              <input matInput formControlName="neighborhood" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Cidade</mat-label>
              <input matInput formControlName="city" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Estado</mat-label>
              <input matInput formControlName="state" maxlength="2" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>CEP</mat-label>
              <input matInput formControlName="postalCode" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>URL do Logo</mat-label>
              <input matInput formControlName="logoUrl" />
            </mat-form-field>
          </form>
          <div class="actions">
            <button mat-flat-button color="primary" [disabled]="form.invalid || saving()" (click)="saveInfo()">
              <mat-icon>save</mat-icon> {{ saving() ? 'Salvando...' : 'Salvar' }}
            </button>
          </div>
        </mat-card-content>
      </mat-card>

      <mat-card class="asaas-card">
        <mat-card-header>
          <mat-card-title>Integração Asaas</mat-card-title>
          <mat-card-subtitle>Configure a integração de pagamentos com o Asaas</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <div class="asaas-status">
            @if (tenant()?.hasAsaasIntegration) {
              <div class="status-badge success">
                <mat-icon>check_circle</mat-icon> Integrado
                @if (tenant()?.asaasWalletId) {
                  <span class="wallet-id">WalletId: {{ tenant()!.asaasWalletId }}</span>
                }
              </div>
            } @else {
              <div class="status-badge warning">
                <mat-icon>warning</mat-icon> Não configurado
              </div>
            }
          </div>

          <div class="asaas-form">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Token Asaas (API Key)</mat-label>
              <input matInput [type]="showToken() ? 'text' : 'password'" [(ngModel)]="asaasApiKey" [ngModelOptions]="{standalone: true}" />
              <button mat-icon-button matSuffix (click)="showToken.set(!showToken())">
                <mat-icon>{{ showToken() ? 'visibility_off' : 'visibility' }}</mat-icon>
              </button>
            </mat-form-field>
            <button mat-flat-button color="primary" [disabled]="!asaasApiKey || validatingAsaas()" (click)="validateAsaas()">
              <mat-icon>verified</mat-icon> {{ validatingAsaas() ? 'Validando...' : 'Validar Token' }}
            </button>
          </div>
        </mat-card-content>
      </mat-card>
    }
  `,
  styles: [`
    .loading { display: flex; justify-content: center; padding: 48px; }
    mat-card { margin-bottom: 24px; }
    .form-grid {
      display: grid; grid-template-columns: 1fr 1fr; gap: 8px 16px; padding: 16px 0;
    }
    .actions { display: flex; justify-content: flex-end; padding-top: 8px; }
    .asaas-card { margin-top: 24px; }
    .asaas-status { margin: 16px 0; }
    .status-badge {
      display: inline-flex; align-items: center; gap: 8px;
      padding: 8px 16px; border-radius: 8px; font-weight: 500;
      &.success { background: #D1FAE5; color: #065F46; }
      &.warning { background: #FEF3C7; color: #92400E; }
      .wallet-id { font-size: 12px; opacity: 0.8; margin-left: 8px; }
    }
    .asaas-form {
      display: flex; align-items: flex-start; gap: 16px; margin-top: 16px;
      mat-form-field { flex: 1; }
    }
    .full-width { width: 100%; }
  `],
})
export class TenantInfoComponent implements OnInit {
  private tenantService = inject(TenantService);
  private fb = inject(FormBuilder);
  private snackBar = inject(MatSnackBar);

  tenant = signal<TenantDto | null>(null);
  loading = signal(true);
  saving = signal(false);
  validatingAsaas = signal(false);
  showToken = signal(false);
  asaasApiKey = '';

  form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    cpfCnpj: [''],
    email: [''],
    phone: [''],
    address: [''],
    addressNumber: [''],
    complement: [''],
    neighborhood: [''],
    city: [''],
    state: [''],
    postalCode: [''],
    logoUrl: [''],
  });

  ngOnInit(): void {
    this.tenantService.getMyTenant().subscribe({
      next: (tenant) => {
        this.tenant.set(tenant);
        this.form.patchValue({
          name: tenant.name,
          cpfCnpj: tenant.cpfCnpj ?? '',
          email: tenant.email ?? '',
          phone: tenant.phone ?? '',
          address: tenant.address ?? '',
          addressNumber: tenant.addressNumber ?? '',
          complement: tenant.complement ?? '',
          neighborhood: tenant.neighborhood ?? '',
          city: tenant.city ?? '',
          state: tenant.state ?? '',
          postalCode: tenant.postalCode ?? '',
          logoUrl: tenant.logoUrl ?? '',
        });
        this.loading.set(false);
      },
      error: () => { this.loading.set(false); },
    });
  }

  saveInfo(): void {
    if (this.form.invalid) return;
    this.saving.set(true);
    this.tenantService.updateTenant(this.form.getRawValue()).subscribe({
      next: (tenant) => {
        this.tenant.set(tenant);
        this.saving.set(false);
        this.snackBar.open('Dados atualizados com sucesso!', 'OK', { duration: 3000, panelClass: ['snackbar-success'] });
      },
      error: () => {
        this.saving.set(false);
        this.snackBar.open('Erro ao atualizar dados.', 'OK', { duration: 3000, panelClass: ['snackbar-error'] });
      },
    });
  }

  validateAsaas(): void {
    if (!this.asaasApiKey) return;
    this.validatingAsaas.set(true);
    this.tenantService.validateAsaasKey(this.asaasApiKey).subscribe({
      next: (tenant) => {
        this.tenant.set(tenant);
        this.validatingAsaas.set(false);
        this.asaasApiKey = '';
        this.snackBar.open('Token Asaas validado com sucesso!', 'OK', { duration: 3000, panelClass: ['snackbar-success'] });
      },
      error: () => {
        this.validatingAsaas.set(false);
        this.snackBar.open('Token Asaas inválido.', 'OK', { duration: 3000, panelClass: ['snackbar-error'] });
      },
    });
  }
}
