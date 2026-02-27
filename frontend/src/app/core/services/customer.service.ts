import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CustomerDto,
  CreateCustomerRequest,
  UpdateCustomerRequest,
} from '../models/customer.model';
import { PaginatedList } from '../models/paginated.model';

@Injectable({ providedIn: 'root' })
export class CustomerService {
  private readonly apiUrl = `${environment.apiUrl}/api/customers`;

  constructor(private http: HttpClient) {}

  getAll(search?: string, pageNumber = 1, pageSize = 10): Observable<PaginatedList<CustomerDto>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    if (search) params = params.set('search', search);
    return this.http.get<PaginatedList<CustomerDto>>(this.apiUrl, { params });
  }

  getById(id: string): Observable<CustomerDto> {
    return this.http.get<CustomerDto>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateCustomerRequest): Observable<CustomerDto> {
    return this.http.post<CustomerDto>(this.apiUrl, request);
  }

  update(request: UpdateCustomerRequest): Observable<CustomerDto> {
    return this.http.put<CustomerDto>(`${this.apiUrl}/${request.id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
