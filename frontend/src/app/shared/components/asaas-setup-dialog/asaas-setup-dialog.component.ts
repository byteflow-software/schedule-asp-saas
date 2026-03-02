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
            <mat-icon>error</mat-icon>
            <span>{{ error() }}</span>
          </div>
        }

        <div class="dialog-actions">
          <button mat-flat-button color="primary" class="action-btn primary-btn"
            [disabled]="!apiKey || validating()" (click)="validate()">
            @if (validating()) {
              <mat-spinner diameter="18"></mat-spinner>
              <span>Validando...</span>
            } @else {
              <span>Validar e Salvar</span>
            }
          </button>
          <button mat-button class="action-btn secondary-btn" (click)="goToSettings()">
            <span>Ir para Configurações</span>
          </button>
        </div>
      } @else {
        <div class="success-state">
          <div class="success-icon">
            <mat-icon>check_circle</mat-icon>
          </div>
          <h2>Integração Configurada!</h2>
          <p class="subtitle">Sua conta Asaas foi conectada. Agora configure o Webhook para receber confirmações de pagamento automaticamente.</p>

          <div class="steps">
            <div class="step">
              <div class="step-number">4</div>
              <div class="step-text">
                No painel Asaas, vá em <strong>Minha Conta</strong> → <strong>Integrações</strong> → <strong>Webhooks</strong>
              </div>
            </div>
            <div class="step">
              <div class="step-number">5</div>
              <div class="step-text">
                Crie um novo webhook com a URL abaixo:
                <div class="url-copy-box">
                  <code>{{ webhookUrl() }}</code>
                  <button mat-button class="copy-btn" (click)="copyWebhookUrl()">
                    {{ copied() ? 'Copiado!' : 'Copiar' }}
                  </button>
                </div>
              </div>
            </div>
            <div class="step">
              <div class="step-number">6</div>
              <div class="step-text">
                Selecione os eventos: <strong>Cobrança confirmada</strong>, <strong>Cobrança recebida</strong> e <strong>Cobrança estornada</strong>
              </div>
            </div>
          </div>

          @if (webhookStatus()) {
            <div class="webhook-result" [class]="webhookStatus()">
              <mat-icon>{{ webhookStatus() === 'ok' ? 'check_circle' : 'error_outline' }}</mat-icon>
              <span>{{ webhookStatus() === 'ok' ? 'Webhook configurado corretamente!' : 'Webhook não encontrado. Configure e tente novamente.' }}</span>
            </div>
          }

          <div class="dialog-actions">
            <button mat-flat-button color="primary" class="action-btn primary-btn"
              [disabled]="checkingWebhook()" (click)="checkWebhook()">
              @if (checkingWebhook()) {
                <mat-spinner diameter="18"></mat-spinner>
                <span>Verificando...</span>
              } @else {
                <span>Verificar Webhook</span>
              }
            </button>
            <button mat-button class="action-btn secondary-btn" (click)="dialogRef.close(true)">
              <span>Pular e Continuar</span>
            </button>
          </div>
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
      width: 56px;
      height: 56px;
      border-radius: 14px;
      background: linear-gradient(135deg, #4F46E5, #7C3AED);
      display: flex;
      align-items: center;
      justify-content: center;
      margin: 0 auto 20px;

      mat-icon {
        color: white;
        font-size: 28px;
        width: 28px;
        height: 28px;
      }
    }

    h2 {
      margin: 0 0 8px;
      font-size: 20px;
      font-weight: 700;
      color: #111827;
    }

    .subtitle {
      font-size: 14px;
      color: #6B7280;
      margin: 0 0 24px;
      line-height: 1.5;
    }

    .steps {
      text-align: left;
      background: #F9FAFB;
      border-radius: 12px;
      padding: 16px;
      margin-bottom: 24px;
      border: 1px solid #E5E7EB;
    }

    .step {
      display: flex;
      align-items: flex-start;
      gap: 12px;
      padding: 10px 0;

      &:not(:last-child) {
        border-bottom: 1px solid #E5E7EB;
      }
    }

    .step-number {
      min-width: 26px;
      height: 26px;
      border-radius: 8px;
      background: #4F46E5;
      color: white;
      font-weight: 700;
      font-size: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }

    .step-text {
      font-size: 13px;
      color: #374151;
      line-height: 1.5;
      padding-top: 2px;

      a {
        color: #4F46E5;
        font-weight: 600;
        text-decoration: none;

        &:hover { text-decoration: underline; }
      }

      strong { color: #111827; }
    }

    .full-width { width: 100%; }

    .error-msg {
      display: flex;
      align-items: center;
      gap: 8px;
      justify-content: center;
      color: #DC2626;
      font-size: 13px;
      margin: -8px 0 16px;

      mat-icon {
        font-size: 18px;
        width: 18px;
        height: 18px;
      }
    }

    .dialog-actions {
      display: flex;
      flex-direction: column;
      gap: 8px;
      margin-top: 4px;
    }

    .action-btn {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
      width: 100%;
      border-radius: 10px !important;

      mat-icon {
        font-size: 20px;
        width: 20px;
        height: 20px;
      }

      span {
        font-size: 14px;
        font-weight: 600;
      }
    }

    .primary-btn {
      height: 46px;

      mat-spinner {
        display: inline-block;
        --mdc-circular-progress-active-indicator-color: white;
      }
    }

    .secondary-btn {
      height: 40px;
      color: #6B7280;

      span { font-weight: 500; }
    }

    .success-state {
      padding: 16px 0;
      text-align: center;
    }

    .url-copy-box {
      display: flex;
      align-items: center;
      gap: 8px;
      background: #F3F4F6;
      border: 1px solid #E5E7EB;
      border-radius: 8px;
      padding: 8px 12px;
      margin-top: 8px;

      code {
        flex: 1;
        font-size: 11px;
        color: #374151;
        word-break: break-all;
        text-align: left;
      }

      .copy-btn {
        min-width: 70px;
        font-size: 12px;
        font-weight: 600;
        color: #4F46E5;
      }
    }

    .webhook-result {
      display: flex;
      align-items: center;
      gap: 8px;
      justify-content: center;
      font-size: 13px;
      font-weight: 500;
      margin-bottom: 16px;
      padding: 10px 16px;
      border-radius: 8px;

      mat-icon { font-size: 18px; width: 18px; height: 18px; }

      &.ok { background: #D1FAE5; color: #065F46; }
      &.error { background: #FEE2E2; color: #991B1B; }
    }

    .success-icon {
      width: 64px;
      height: 64px;
      border-radius: 50%;
      background: #D1FAE5;
      display: flex;
      align-items: center;
      justify-content: center;
      margin: 0 auto 20px;

      mat-icon {
        color: #059669;
        font-size: 32px;
        width: 32px;
        height: 32px;
      }
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

  webhookUrl = signal('');
  copied = signal(false);
  checkingWebhook = signal(false);
  webhookStatus = signal<'ok' | 'error' | ''>('');

  validate(): void {
    if (!this.apiKey) return;
    this.validating.set(true);
    this.error.set('');
    this.tenantService.validateAsaasKey(this.apiKey).subscribe({
      next: () => {
        this.validating.set(false);
        this.success.set(true);
        this.loadWebhookUrl();
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

  private loadWebhookUrl(): void {
    this.tenantService.checkWebhookStatus().subscribe({
      next: (result) => { this.webhookUrl.set(result.webhookUrl); },
      error: () => { this.webhookUrl.set(`${window.location.origin}/api/webhooks/asaas`); },
    });
  }

  copyWebhookUrl(): void {
    navigator.clipboard.writeText(this.webhookUrl());
    this.copied.set(true);
    setTimeout(() => this.copied.set(false), 2000);
  }

  checkWebhook(): void {
    this.checkingWebhook.set(true);
    this.webhookStatus.set('');
    this.tenantService.checkWebhookStatus().subscribe({
      next: (result) => {
        this.checkingWebhook.set(false);
        this.webhookStatus.set(result.configured ? 'ok' : 'error');
      },
      error: () => {
        this.checkingWebhook.set(false);
        this.webhookStatus.set('error');
      },
    });
  }
}
