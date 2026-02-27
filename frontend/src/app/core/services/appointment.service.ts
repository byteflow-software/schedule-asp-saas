import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AppointmentDto,
  CreateAppointmentRequest,
  UpdateAppointmentRequest,
} from '../models/appointment.model';
import { PaginatedList } from '../models/paginated.model';

@Injectable({ providedIn: 'root' })
export class AppointmentService {
  private readonly apiUrl = `${environment.apiUrl}/api/appointments`;

  constructor(private http: HttpClient) {}

  getAll(filters: {
    from?: string;
    to?: string;
    userId?: string;
    customerId?: string;
    pageNumber?: number;
    pageSize?: number;
  } = {}): Observable<PaginatedList<AppointmentDto>> {
    let params = new HttpParams();
    if (filters.from) params = params.set('from', filters.from);
    if (filters.to) params = params.set('to', filters.to);
    if (filters.userId) params = params.set('userId', filters.userId);
    if (filters.customerId) params = params.set('customerId', filters.customerId);
    if (filters.pageNumber) params = params.set('pageNumber', filters.pageNumber);
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize);
    return this.http.get<PaginatedList<AppointmentDto>>(this.apiUrl, { params });
  }

  getById(id: string): Observable<AppointmentDto> {
    return this.http.get<AppointmentDto>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateAppointmentRequest): Observable<AppointmentDto> {
    return this.http.post<AppointmentDto>(this.apiUrl, request);
  }

  update(id: string, request: UpdateAppointmentRequest): Observable<AppointmentDto> {
    return this.http.put<AppointmentDto>(`${this.apiUrl}/${id}`, request);
  }

  confirm(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/confirm`, {});
  }

  cancel(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/cancel`, {});
  }

  markDone(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/done`, {});
  }
}
