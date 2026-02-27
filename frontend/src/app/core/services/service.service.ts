import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ServiceDto, CreateServiceRequest, UpdateServiceRequest } from '../models/service.model';

@Injectable({ providedIn: 'root' })
export class ServiceService {
  private readonly apiUrl = `${environment.apiUrl}/api/services`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ServiceDto[]> {
    return this.http.get<ServiceDto[]>(this.apiUrl);
  }

  getById(id: string): Observable<ServiceDto> {
    return this.http.get<ServiceDto>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateServiceRequest): Observable<ServiceDto> {
    return this.http.post<ServiceDto>(this.apiUrl, request);
  }

  update(request: UpdateServiceRequest): Observable<ServiceDto> {
    return this.http.put<ServiceDto>(`${this.apiUrl}/${request.id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
