import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TenantDto, UpdateTenantRequest } from '../models/tenant.model';

@Injectable({ providedIn: 'root' })
export class TenantService {
  private readonly apiUrl = `${environment.apiUrl}/api/tenants`;

  constructor(private http: HttpClient) {}

  getMyTenant(): Observable<TenantDto> {
    return this.http.get<TenantDto>(`${this.apiUrl}/me`);
  }

  updateTenant(data: UpdateTenantRequest): Observable<TenantDto> {
    return this.http.put<TenantDto>(`${this.apiUrl}/me`, data);
  }

  validateAsaasKey(apiKey: string): Observable<TenantDto> {
    return this.http.post<TenantDto>(`${this.apiUrl}/me/validate-asaas`, { apiKey });
  }

  checkWebhookStatus(): Observable<WebhookStatusDto> {
    return this.http.get<WebhookStatusDto>(`${this.apiUrl}/me/webhook-status`);
  }
}

export interface WebhookStatusDto {
  webhookUrl: string;
  configured: boolean;
  webhooks: { url: string; enabled: boolean; name: string }[];
}
