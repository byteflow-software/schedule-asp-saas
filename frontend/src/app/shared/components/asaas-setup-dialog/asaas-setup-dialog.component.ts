import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router } from '@angular/router';
import { TenantService } from '../../../core/services/tenant.service';

@Component({
  selector: 'app-asaas-setup-dialog',
  standalone: true,
  imports: [
    FormsModule, MatDialogModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule, MatProgressSpinnerModule,
  ],
  template: `
    <div class="setup-dialog">
      @if (!success()) {
        <div class="dialog-icon">
          <mat-icon>account_balance</mat-icon>
        </div>
        <h2>Configure a Integração de Pagamentos</h2>
        <p class="subtitle">Para criar agendamentos com cobrança automática, é necessário configurar sua conta Asaas.</p>

        <div class="steps">
          <div class="step">
            <div class="step-number">1</div>
            <div class="step-text">
              Acesse <a href="https://www.asaas.com" target="_blank">asaas.com</a> e faça login na sua conta
            </div>
          </div>
          <div class="step">
            <div class="step-number">2</div>
            <div class="step-text">
              Vá em <strong>Minha Conta</strong> → <strong>Integrações</strong> → <strong>Chave de API</strong>
            </div>
          </div>
          <div class="step">
            <div class="step-number">3</div>
            <div class="step-text">
              Copie o <strong>Token de API</strong> e cole no campo abaixo
            </div>
          </div>
        </div>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Token Asaas (API Key)</mat-label>
          <mat-icon matPrefix>key</mat-icon>
          <input matInput [type]="showToken() ? 'text' : 'password'" [(ngModel)]="apiKey"
            placeholder="$aact_..." />
          <button mat-icon-button matSuffix (click)="showToken.set(!showToken())">
            <mat-icon>{{ showToken() ? 'visibility_off' : 'visibility' }}</mat-icon>
          </button>
        </mat-form-field>

        @if (error()) {
          <div class="error-msg">
            <mat-icon>error</mat-icon> {{ error() }}
          </div>
        }

        <div class="dialog-actions">
          <button mat-flat-button color="primary" class="full-width validate-btn"
            [disabled]="!apiKey || validating()" (click)="validate()">
            @if (validating()) {
              <mat-spinner diameter="20"></mat-spinner> Validando...
            } @else {
              <mat-icon>verified</mat-icon> Validar e Salvar
            }
          </button>
          <button mat-button class="settings-link" (click)="goToSettings()">
            <mat-icon>settings</mat-icon> Ir para Configurações
          </button>
        </div>
      } @else {
        <div class="success-state">
          <div class="success-icon">
            <mat-icon>check_circle</mat-icon>
          </div>
          <h2>Integração Configurada!</h2>
          <p class="subtitle">Sua conta Asaas foi conectada com sucesso. Agora você pode criar agendamentos com cobrança automática.</p>
          <button mat-flat-button color="primary" (click)="dialogRef.close(true)">
            <mat-icon>arrow_forward</mat-icon> Continuar
          </button>
        </div>
      }
    </div>
  `,
  styles: [`
    .setup-dialog {
      padding: 32px;
      text-align: center;
      max-width: 480px;
    }
    .dialog-icon {
      width: 64px; height: 64px; border-radius: 16px;
      background: linear-gradient(135deg, #4F46E5, #7C3AED);
      display: flex; align-items: center; justify-content: center;
      margin: 0 auto 20px;
      mat-icon { color: white; font-size: 32px; width: 32px; height: 32px; }
    }
    h2 { margin: 0 0 8px; font-size: 22px; font-weight: 700; color: #111827; }
    .subtitle { font-size: 14px; color: #6B7280; margin: 0 0 24px; line-height: 1.5; }
    .steps {
      text-align: left;
      background: #F9FAFB; border-radius: 12px; padding: 16px;
      margin-bottom: 24px; border: 1px solid #E5E7EB;
    }
    .step {
      display: flex; align-items: flex-start; gap: 12px; padding: 8px 0;
      &:not(:last-child) { border-bottom: 1px solid #E5E7EB; }
    }
    .step-number {
      min-width: 28px; height: 28px; border-radius: 8px;
      background: #4F46E5; color: white; font-weight: 700; font-size: 13px;
      display: flex; align-items: center; justify-content: center;
    }
    .step-text {
      font-size: 14px; color: #374151; line-height: 1.5; padding-top: 3px;
      a { color: #4F46E5; font-weight: 600; }
      strong { color: #111827; }
    }
    .full-width { width: 100%; }
    .error-msg {
      display: flex; align-items: center; gap: 8px; justify-content: center;
      color: #DC2626; font-size: 14px; margin-bottom: 16px;
      mat-icon { font-size: 18px; width: 18px; height: 18px; }
    }
    .dialog-actions { display: flex; flex-direction: column; gap: 8px; }
    .validate-btn {
      height: 48px; font-size: 15px;
      mat-spinner { display: inline-block; margin-right: 8px; }
    }
    .settings-link { color: #6B7280; }
    .success-state { padding: 16px 0; }
    .success-icon {
      width: 72px; height: 72px; border-radius: 50%;
      background: #D1FAE5; display: flex; align-items: center; justify-content: center;
      margin: 0 auto 20px;
      mat-icon { color: #059669; font-size: 40px; width: 40px; height: 40px; }
    }
  `],
})
export class AsaasSetupDialogComponent {
  dialogRef = inject(MatDialogRef<AsaasSetupDialogComponent>);
  private tenantService = inject(TenantService);
  private router = inject(Router);

  apiKey = '';
  showToken = signal(false);
  validating = signal(false);
  error = signal('');
  success = signal(false);

  validate(): void {
    if (!this.apiKey) return;
    this.validating.set(true);
    this.error.set('');
    this.tenantService.validateAsaasKey(this.apiKey).subscribe({
      next: () => {
        this.validating.set(false);
        this.success.set(true);
      },
      error: () => {
        this.validating.set(false);
        this.error.set('Token inválido. Verifique se copiou corretamente.');
      },
    });
  }

  goToSettings(): void {
    this.dialogRef.close(false);
    this.router.navigate(['/settings']);
  }
}
