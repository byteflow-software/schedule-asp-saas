import { Component } from '@angular/core';
import { TenantInfoComponent } from './tenant-info/tenant-info.component';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [TenantInfoComponent],
  template: `
    <div class="page-container">
      <h2>Configurações</h2>
      <div class="settings-grid">
        <app-tenant-info></app-tenant-info>
      </div>
    </div>
  `,
  styles: [`.settings-grid { display: flex; flex-direction: column; gap: 24px; }`],
})
export class SettingsComponent {}
