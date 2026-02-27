import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TenantDto } from '../models/tenant.model';

@Injectable({ providedIn: 'root' })
export class TenantService {
  private readonly apiUrl = `${environment.apiUrl}/api/tenants`;

  constructor(private http: HttpClient) {}

  getMyTenant(): Observable<TenantDto> {
    return this.http.get<TenantDto>(`${this.apiUrl}/me`);
  }
}
