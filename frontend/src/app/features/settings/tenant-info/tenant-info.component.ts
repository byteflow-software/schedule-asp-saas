import { Component, inject, OnInit , signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TenantService } from '../../../core/services/tenant.service';
import { TenantDto } from '../../../core/models/tenant.model';

@Component({
  selector: 'app-tenant-info',
  standalone: true,
  imports: [MatCardModule, MatProgressSpinnerModule],
  template: `
    <mat-card>
      <mat-card-header>
        <mat-card-title>Informações da Empresa</mat-card-title>
      </mat-card-header>
      <mat-card-content>
        @if (loading()) {
          <mat-spinner diameter="32"></mat-spinner>
        } @else if (tenant) {
          <div class="info-grid">
            <div class="info-item">
              <span class="label">Nome</span>
              <span class="value">{{ tenant.name }}</span>
            </div>
            <div class="info-item">
              <span class="label">Slug</span>
              <span class="value">{{ tenant.slug }}</span>
            </div>
            <div class="info-item">
              <span class="label">Status</span>
              <span class="value" [class.active]="tenant.isActive" [class.inactive]="!tenant.isActive">
                {{ tenant.isActive ? 'Ativo' : 'Inativo' }}
              </span>
            </div>
          </div>
        }
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; padding: 16px 0; }
    .info-item { display: flex; flex-direction: column; gap: 4px; }
    .label { font-size: 12px; color: #666; text-transform: uppercase; }
    .value { font-size: 16px; }
    .active { color: #2e7d32; }
    .inactive { color: #c62828; }
  `],
})
export class TenantInfoComponent implements OnInit {
  private tenantService = inject(TenantService);
  tenant: TenantDto | null = null;
  loading = signal(true);

  ngOnInit(): void {
    this.tenantService.getMyTenant().subscribe({
      next: (tenant) => {
        this.tenant = tenant;
        this.loading.set(false);
      },
      error: () => { this.loading.set(false); },
    });
  }
}
