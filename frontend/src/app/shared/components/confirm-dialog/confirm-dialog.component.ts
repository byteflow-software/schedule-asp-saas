import { Component, inject } from '@angular/core';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
}

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <div class="dialog-content">
      <div class="warning-icon">
        <mat-icon>warning_amber</mat-icon>
      </div>
      <h2>{{ data.title }}</h2>
      <p>{{ data.message }}</p>
      <div class="dialog-actions">
        <button mat-button (click)="dialogRef.close(false)">
          {{ data.cancelText || 'Cancelar' }}
        </button>
        <button mat-flat-button color="warn" (click)="dialogRef.close(true)">
          {{ data.confirmText || 'Confirmar' }}
        </button>
      </div>
    </div>
  `,
  styles: [`
    .dialog-content { text-align: center; padding: 24px; }
    .warning-icon {
      width: 56px; height: 56px; border-radius: 50%;
      background: #FEF3C7; display: flex;
      align-items: center; justify-content: center; margin: 0 auto 16px;
      mat-icon { font-size: 28px; width: 28px; height: 28px; color: #D97706; }
    }
    h2 { font-size: 18px; font-weight: 700; color: #111827; margin: 0 0 8px; }
    p { font-size: 14px; color: #6B7280; margin: 0 0 24px; line-height: 1.5; }
    .dialog-actions { display: flex; gap: 12px; justify-content: center; }
  `],
})
export class ConfirmDialogComponent {
  data = inject<ConfirmDialogData>(MAT_DIALOG_DATA);
  dialogRef = inject(MatDialogRef<ConfirmDialogComponent>);
}
