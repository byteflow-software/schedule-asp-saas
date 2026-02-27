import { Component, inject , signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    ReactiveFormsModule, RouterLink, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule, MatProgressSpinnerModule,
  ],
  template: `
    <div class="auth-page">
      <div class="auth-left">
        <div class="brand-content">
          <div class="brand-logo">S</div>
          <h1>Scheduly</h1>
          <p>Comece agora e organize seus agendamentos em minutos.</p>
          <div class="steps">
            <div class="step">
              <div class="step-num">1</div>
              <div><strong>Crie sua empresa</strong><br><span>Defina o nome da empresa</span></div>
            </div>
            <div class="step">
              <div class="step-num">2</div>
              <div><strong>Adicione sua equipe</strong><br><span>Convide seus colaboradores</span></div>
            </div>
            <div class="step">
              <div class="step-num">3</div>
              <div><strong>Comece a agendar</strong><br><span>Gerencie seus clientes</span></div>
            </div>
          </div>
        </div>
      </div>
      <div class="auth-right">
        <div class="auth-form-container">
          <h2>Criar conta</h2>
          <p class="subtitle">Preencha os dados para começar</p>
          <form [formGroup]="form" (ngSubmit)="onSubmit()">
            <mat-form-field class="full-width" appearance="outline">
              <mat-label>Nome da Empresa</mat-label>
              <mat-icon matPrefix>business</mat-icon>
              <input matInput formControlName="tenantName" placeholder="Sua Empresa Ltda" />
            </mat-form-field>
            <mat-form-field class="full-width" appearance="outline">
              <mat-label>Seu Nome Completo</mat-label>
              <mat-icon matPrefix>person</mat-icon>
              <input matInput formControlName="fullName" placeholder="João Silva" />
            </mat-form-field>
            <mat-form-field class="full-width" appearance="outline">
              <mat-label>Email</mat-label>
              <mat-icon matPrefix>email</mat-icon>
              <input matInput formControlName="email" type="email" placeholder="seu@email.com" />
            </mat-form-field>
            <mat-form-field class="full-width" appearance="outline">
              <mat-label>Senha</mat-label>
              <mat-icon matPrefix>lock</mat-icon>
              <input matInput formControlName="password" [type]="hidePassword ? 'password' : 'text'" placeholder="Mínimo 8 caracteres" />
              <button mat-icon-button matSuffix type="button" (click)="hidePassword = !hidePassword">
                <mat-icon>{{ hidePassword ? 'visibility_off' : 'visibility' }}</mat-icon>
              </button>
              @if (form.controls.password.hasError('minlength') && form.controls.password.touched) {
                <mat-hint class="error-hint">Mínimo 8 caracteres</mat-hint>
              }
            </mat-form-field>
            <button mat-flat-button color="primary" class="full-width submit-btn" type="submit"
              [disabled]="form.invalid || loading()">
              @if (loading()) {
                <mat-spinner diameter="20"></mat-spinner>
              } @else {
                Criar Conta
              }
            </button>
          </form>
          <div class="auth-footer">
            <span>Já tem conta?</span>
            <a routerLink="/login">Entrar</a>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .auth-page { display: flex; min-height: 100vh; }
    .auth-left {
      flex: 1;
      background: linear-gradient(135deg, #1E1B4B 0%, #3730A3 50%, #4F46E5 100%);
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 48px;
      color: white;
    }
    .brand-content { max-width: 420px; }
    .brand-logo {
      width: 56px; height: 56px; border-radius: 16px;
      background: rgba(255,255,255,0.15); backdrop-filter: blur(10px);
      display: flex; align-items: center; justify-content: center;
      font-size: 28px; font-weight: 700; margin-bottom: 24px;
    }
    .brand-content h1 { font-size: 40px; font-weight: 700; margin: 0 0 12px; letter-spacing: -1px; }
    .brand-content p { font-size: 18px; color: rgba(255,255,255,0.7); line-height: 1.6; margin-bottom: 36px; }
    .steps { display: flex; flex-direction: column; gap: 20px; }
    .step {
      display: flex; align-items: flex-start; gap: 14px;
      font-size: 14px; color: rgba(255,255,255,0.8);
      strong { color: white; font-size: 15px; }
      span { color: rgba(255,255,255,0.55); font-size: 13px; }
    }
    .step-num {
      width: 32px; height: 32px; border-radius: 50%;
      background: rgba(255,255,255,0.15); display: flex; flex-shrink: 0;
      align-items: center; justify-content: center; font-weight: 700; font-size: 14px;
    }
    .auth-right {
      flex: 1; display: flex; align-items: center; justify-content: center;
      padding: 48px; background: white;
    }
    .auth-form-container { width: 100%; max-width: 420px; }
    h2 { font-size: 28px; font-weight: 700; color: #111827; margin: 0 0 8px; }
    .subtitle { font-size: 15px; color: #6B7280; margin: 0 0 32px; }
    form { display: flex; flex-direction: column; gap: 4px; }
    .submit-btn { height: 48px; font-size: 15px; font-weight: 600; margin-top: 8px; border-radius: 10px !important; }
    .error-hint { color: var(--danger, #EF4444); }
    .auth-footer {
      text-align: center; margin-top: 24px; font-size: 14px; color: #6B7280;
      a { color: #4F46E5; font-weight: 600; text-decoration: none; margin-left: 4px; &:hover { text-decoration: underline; } }
    }
    mat-spinner { margin: 0 auto; }
    @media (max-width: 768px) { .auth-left { display: none; } }
  `],
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);
  loading = signal(false);
  hidePassword = true;

  form = this.fb.nonNullable.group({
    tenantName: ['', Validators.required],
    fullName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  onSubmit(): void {
    if (this.form.invalid) return;
    this.loading.set(true);
    const formValues = this.form.getRawValue();
    this.authService.register(formValues).subscribe({
      next: () => {
        this.authService.login({ email: formValues.email, password: formValues.password }).subscribe({
          next: () => {
            this.snackBar.open('Conta criada com sucesso!', 'OK', { duration: 3000, panelClass: ['snackbar-success'] });
            this.router.navigate(['/dashboard']);
          },
          error: () => {
            this.snackBar.open('Conta criada! Faça login.', 'OK', { duration: 3000, panelClass: ['snackbar-success'] });
            this.router.navigate(['/login']);
          },
        });
      },
      error: () => { this.loading.set(false); },
    });
  }
}
