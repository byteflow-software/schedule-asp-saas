import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { VacancyDto, CreateVacancyRequest, BulkCreateVacancyRequest } from '../models/vacancy.model';

@Injectable({ providedIn: 'root' })
export class VacancyService {
  private readonly apiUrl = `${environment.apiUrl}/api/vacancies`;

  constructor(private http: HttpClient) {}

  getAll(filters: {
    userId?: string;
    serviceId?: string;
    from?: string;
    to?: string;
    available?: boolean;
  } = {}): Observable<VacancyDto[]> {
    let params = new HttpParams();
    if (filters.userId) params = params.set('userId', filters.userId);
    if (filters.serviceId) params = params.set('serviceId', filters.serviceId);
    if (filters.from) params = params.set('from', filters.from);
    if (filters.to) params = params.set('to', filters.to);
    if (filters.available !== undefined) params = params.set('available', filters.available);
    return this.http.get<VacancyDto[]>(this.apiUrl, { params });
  }

  create(request: CreateVacancyRequest): Observable<VacancyDto> {
    return this.http.post<VacancyDto>(this.apiUrl, request);
  }

  bulkCreate(request: BulkCreateVacancyRequest): Observable<VacancyDto[]> {
    return this.http.post<VacancyDto[]>(`${this.apiUrl}/bulk`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
