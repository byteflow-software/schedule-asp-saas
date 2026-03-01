import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDividerModule } from '@angular/material/divider';
import { TenantService } from '../../../core/services/tenant.service';
import { TenantDto } from '../../../core/models/tenant.model';

@Component({
  selector: 'app-tenant-info',
  standalone: true,
  imports: [
    ReactiveFormsModule, FormsModule, MatCardModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatDividerModule,
  ],
  template: `
    @if (loading()) {
      <div class="loading"><mat-spinner></mat-spinner></div>
    } @else {
      <form [formGroup]="form">
        <!-- Dados da Empresa -->
        <div class="section-card">
          <div class="section-header">
            <div class="section-icon business">
              <mat-icon>business</mat-icon>
            </div>
            <div>
              <h3>Dados da Empresa</h3>
              <p>Informações básicas do seu negócio</p>
            </div>
          </div>
          <div class="form-grid">
            <mat-form-field appearance="outline">
              <mat-label>Nome da Empresa</mat-label>
              <mat-icon matPrefix>store</mat-icon>
              <input matInput formControlName="name" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>CPF/CNPJ</mat-label>
              <mat-icon matPrefix>badge</mat-icon>
              <input matInput formControlName="cpfCnpj" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Email</mat-label>
              <mat-icon matPrefix>email</mat-icon>
              <input matInput formControlName="email" type="email" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Telefone</mat-label>
              <mat-icon matPrefix>phone</mat-icon>
              <input matInput formControlName="phone" />
            </mat-form-field>
            <mat-form-field appearance="outline" class="span-2">
              <mat-label>URL do Logo</mat-label>
              <mat-icon matPrefix>image</mat-icon>
              <input matInput formControlName="logoUrl" />
            </mat-form-field>
          </div>
        </div>

        <!-- Endereço -->
        <div class="section-card">
          <div class="section-header">
            <div class="section-icon location">
              <mat-icon>location_on</mat-icon>
            </div>
            <div>
              <h3>Endereço</h3>
              <p>Localização do seu estabelecimento</p>
            </div>
          </div>
          <div class="form-grid">
            <mat-form-field appearance="outline" class="span-2">
              <mat-label>Endereço</mat-label>
              <mat-icon matPrefix>home</mat-icon>
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
          </div>
        </div>

        <div class="save-bar">
          <button mat-flat-button color="primary" class="btn-action" [disabled]="form.invalid || saving()" (click)="saveInfo()">
            @if (saving()) {
              <mat-spinner diameter="18"></mat-spinner>
              <span>Salvando...</span>
            } @else {
              <mat-icon>save</mat-icon>
              <span>Salvar Alterações</span>
            }
          </button>
        </div>
      </form>

      <!-- Integração Asaas -->
      <div class="section-card">
        <div class="section-header">
          <div class="section-icon payment">
            <mat-icon>account_balance</mat-icon>
          </div>
          <div>
            <h3>Integração de Pagamentos</h3>
            <p>Gerencie sua conexão com o Asaas</p>
          </div>
          <div class="header-badge">
            @if (tenant()?.hasAsaasIntegration) {
              <div class="integration-badge connected">
                <mat-icon>check_circle</mat-icon> Conectado
              </div>
            } @else {
              <div class="integration-badge disconnected">
                <mat-icon>error_outline</mat-icon> Desconectado
              </div>
            }
          </div>
        </div>

        @if (tenant()?.hasAsaasIntegration) {
          <div class="asaas-info-grid">
            <div class="info-item">
              <span class="info-label">Status</span>
              <span class="info-value connected">
                <mat-icon>verified</mat-icon> Ativo
              </span>
            </div>
            <div class="info-item">
              <span class="info-label">Wallet ID</span>
              <span class="info-value mono">{{ tenant()?.asaasWalletId || '—' }}</span>
            </div>
          </div>
        }

        <mat-divider></mat-divider>

        <div class="token-section">
          <p class="token-description">
            @if (tenant()?.hasAsaasIntegration) {
              Para alterar o token, insira a nova chave abaixo e valide.
            } @else {
              Insira seu token de API do Asaas para habilitar cobranças automáticas.
            }
          </p>
          <div class="token-form">
            <mat-form-field appearance="outline" class="token-field">
              <mat-label>Token Asaas (API Key)</mat-label>
              <mat-icon matPrefix>key</mat-icon>
              <input matInput [type]="showToken() ? 'text' : 'password'" [(ngModel)]="asaasApiKey"
                [ngModelOptions]="{standalone: true}" placeholder="$aact_..." />
              <button mat-icon-button matSuffix (click)="showToken.set(!showToken())">
                <mat-icon>{{ showToken() ? 'visibility_off' : 'visibility' }}</mat-icon>
              </button>
            </mat-form-field>
            <button mat-flat-button color="primary" class="btn-action validate-btn"
              [disabled]="!asaasApiKey || validatingAsaas()" (click)="validateAsaas()">
              @if (validatingAsaas()) {
                <mat-spinner diameter="18"></mat-spinner>
                <span>Validando...</span>
              } @else {
                <mat-icon>verified</mat-icon>
                <span>Validar Token</span>
              }
            </button>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    :host { display: block; }

    .section-card {
      background: white;
      border-radius: var(--radius-lg, 12px);
      border: 1px solid var(--gray-200, #E5E7EB);
      box-shadow: var(--shadow-sm, 0 1px 2px rgba(0,0,0,0.05));
      padding: 28px;
      margin-bottom: 20px;
    }

    .section-header {
      display: flex;
      align-items: center;
      gap: 16px;
      margin-bottom: 24px;

      h3 {
        margin: 0;
        font-size: 18px;
        font-weight: 700;
        color: var(--gray-900, #111827);
      }
      p {
        margin: 4px 0 0;
        font-size: 13px;
        color: var(--gray-500, #6B7280);
      }
    }

    .section-icon {
      width: 44px;
      height: 44px;
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;

      mat-icon { color: white; font-size: 22px; width: 22px; height: 22px; }

      &.business { background: linear-gradient(135deg, #4F46E5, #7C3AED); }
      &.location { background: linear-gradient(135deg, #0EA5E9, #06B6D4); }
      &.payment { background: linear-gradient(135deg, #F59E0B, #F97316); }
    }

    .header-badge { margin-left: auto; }

    .integration-badge {
      display: inline-flex;
      align-items: center;
      gap: 4px;
      padding: 4px 12px;
      border-radius: 20px;
      font-size: 12px;
      font-weight: 600;
      line-height: 1;

      mat-icon { font-size: 14px; width: 14px; height: 14px; }

      &.connected { background: #D1FAE5; color: #065F46; }
      &.disconnected { background: #FEE2E2; color: #991B1B; }
    }

    .form-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 8px 16px;
    }

    .span-2 { grid-column: span 2; }

    .btn-action {
      height: 44px;
      min-width: 160px;
      font-size: 14px;
      font-weight: 600;
      padding: 0 20px;
      border-radius: 8px;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
      white-space: nowrap;

      mat-icon { font-size: 18px; width: 18px; height: 18px; }
      mat-spinner { display: inline-block; }
    }

    .save-bar {
      display: flex;
      justify-content: flex-end;
      margin-bottom: 20px;
    }

    .asaas-info-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
      margin-bottom: 20px;
    }

    .info-item {
      background: var(--gray-50, #F9FAFB);
      border-radius: var(--radius, 8px);
      padding: 14px 16px;
      border: 1px solid var(--gray-100, #F3F4F6);

      .info-label {
        display: block;
        font-size: 11px;
        font-weight: 600;
        text-transform: uppercase;
        letter-spacing: 0.5px;
        color: var(--gray-400, #9CA3AF);
        margin-bottom: 6px;
      }

      .info-value {
        display: flex;
        align-items: center;
        gap: 6px;
        font-size: 14px;
        font-weight: 600;
        color: var(--gray-800, #1F2937);

        mat-icon { font-size: 18px; width: 18px; height: 18px; }

        &.connected { color: #059669; }
        &.mono { font-family: 'SF Mono', 'Fira Code', monospace; font-size: 13px; font-weight: 500; }
      }
    }

    mat-divider { margin: 20px 0 !important; }

    .token-section {
      .token-description {
        font-size: 13px;
        color: var(--gray-500, #6B7280);
        margin: 0 0 16px;
        line-height: 1.5;
      }
    }

    .token-form {
      display: flex;
      align-items: center;
      gap: 12px;

      .token-field { flex: 1; }
    }

    @media (max-width: 768px) {
      .form-grid { grid-template-columns: 1fr; }
      .span-2 { grid-column: span 1; }
      .asaas-info-grid { grid-template-columns: 1fr; }
      .token-form {
        flex-direction: column;
        align-items: stretch;
        .token-field { width: 100%; }
        .validate-btn { width: 100%; }
      }
    }
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
