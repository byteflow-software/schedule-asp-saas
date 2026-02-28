import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { SidebarComponent } from '../../shared/components/sidebar/sidebar.component';
import { HeaderComponent } from '../../shared/components/header/header.component';
import { TenantService } from '../../core/services/tenant.service';
import { AsaasSetupDialogComponent } from '../../shared/components/asaas-setup-dialog/asaas-setup-dialog.component';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet, SidebarComponent, HeaderComponent],
  template: `
    <div class="layout">
      <app-sidebar></app-sidebar>
      <div class="main-content">
        <app-header></app-header>
        <div class="content">
          <router-outlet></router-outlet>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .layout {
      display: flex;
      height: 100vh;
    }
    .main-content {
      flex: 1;
      display: flex;
      flex-direction: column;
      overflow: hidden;
      background: var(--gray-50, #F9FAFB);
    }
    .content {
      flex: 1;
      overflow-y: auto;
    }
  `],
})
export class MainLayoutComponent implements OnInit {
  private tenantService = inject(TenantService);
  private dialog = inject(MatDialog);

  ngOnInit(): void {
    this.tenantService.getMyTenant().subscribe({
      next: (tenant) => {
        if (!tenant.hasAsaasIntegration) {
          this.dialog.open(AsaasSetupDialogComponent, {
            width: '520px',
            disableClose: true,
          });
        }
      },
    });
  }
}
