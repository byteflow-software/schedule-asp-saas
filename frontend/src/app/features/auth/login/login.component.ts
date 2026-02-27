import { Component, inject } from '@angular/core';
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
  selector: 'app-login',
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
          <p>Gerencie seus agendamentos de forma simples e profissional.</p>
          <div class="features">
            <div class="feature"><mat-icon>check_circle</mat-icon><span>Agendamentos inteligentes</span></div>
            <div class="feature"><mat-icon>check_circle</mat-icon><span>Gestão de clientes</span></div>
            <div class="feature"><mat-icon>check_circle</mat-icon><span>Multi-equipe</span></div>
          </div>
        </div>
      </div>
      <div class="auth-right">
        <div class="auth-form-container">
          <h2>Bem-vindo de volta</h2>
          <p class="subtitle">Entre na sua conta para continuar</p>
          <form [formGroup]="form" (ngSubmit)="onSubmit()">
            <mat-form-field class="full-width" appearance="outline">
              <mat-label>Email</mat-label>
              <mat-icon matPrefix>email</mat-icon>
              <input matInput formControlName="email" type="email" placeholder="seu@email.com" />
            </mat-form-field>
            <mat-form-field class="full-width" appearance="outline">
              <mat-label>Senha</mat-label>
              <mat-icon matPrefix>lock</mat-icon>
              <input matInput formControlName="password" [type]="hidePassword ? 'password' : 'text'" />
              <button mat-icon-button matSuffix type="button" (click)="hidePassword = !hidePassword">
                <mat-icon>{{ hidePassword ? 'visibility_off' : 'visibility' }}</mat-icon>
              </button>
            </mat-form-field>
            <button mat-flat-button color="primary" class="full-width submit-btn" type="submit"
              [disabled]="form.invalid || loading">
              @if (loading) {
                <mat-spinner diameter="20"></mat-spinner>
              } @else {
                Entrar
              }
            </button>
          </form>
          <div class="auth-footer">
            <span>Não tem conta?</span>
            <a routerLink="/register">Criar conta grátis</a>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .auth-page {
      display: flex;
      min-height: 100vh;
    }
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
      width: 56px;
      height: 56px;
      border-radius: 16px;
      background: rgba(255,255,255,0.15);
      backdrop-filter: blur(10px);
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 28px;
      font-weight: 700;
      margin-bottom: 24px;
    }
    .brand-content h1 {
      font-size: 40px;
      font-weight: 700;
      margin: 0 0 12px;
      letter-spacing: -1px;
    }
    .brand-content p {
      font-size: 18px;
      color: rgba(255,255,255,0.7);
      line-height: 1.6;
      margin-bottom: 36px;
    }
    .features { display: flex; flex-direction: column; gap: 12px; }
    .feature {
      display: flex;
      align-items: center;
      gap: 10px;
      font-size: 15px;
      color: rgba(255,255,255,0.85);
      mat-icon { font-size: 20px; width: 20px; height: 20px; color: #818CF8; }
    }
    .auth-right {
      flex: 1;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 48px;
      background: white;
    }
    .auth-form-container {
      width: 100%;
      max-width: 400px;
    }
    h2 {
      font-size: 28px;
      font-weight: 700;
      color: #111827;
      margin: 0 0 8px;
    }
    .subtitle {
      font-size: 15px;
      color: #6B7280;
      margin: 0 0 32px;
    }
    form { display: flex; flex-direction: column; gap: 4px; }
    .submit-btn {
      height: 48px;
      font-size: 15px;
      font-weight: 600;
      margin-top: 8px;
      border-radius: 10px !important;
    }
    .auth-footer {
      text-align: center;
      margin-top: 24px;
      font-size: 14px;
      color: #6B7280;
      a {
        color: #4F46E5;
        font-weight: 600;
        text-decoration: none;
        margin-left: 4px;
        &:hover { text-decoration: underline; }
      }
    }
    mat-spinner { margin: 0 auto; }
    @media (max-width: 768px) {
      .auth-left { display: none; }
    }
  `],
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);
  loading = false;
  hidePassword = true;

  form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  onSubmit(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.authService.login(this.form.getRawValue()).subscribe({
      next: () => { this.router.navigate(['/dashboard']); },
      error: (err) => {
        this.loading = false;
        const msg = err.status === 401 ? 'Email ou senha incorretos.' : 'Erro ao fazer login. Tente novamente.';
        this.snackBar.open(msg, 'OK', { duration: 5000, panelClass: ['snackbar-error'] });
      },
    });
  }
}
