import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TransactionDto, TransactionSummaryDto } from '../models/transaction.model';
import { PaginatedList } from '../models/paginated.model';

@Injectable({ providedIn: 'root' })
export class TransactionService {
  private readonly apiUrl = `${environment.apiUrl}/api/transactions`;

  constructor(private http: HttpClient) {}

  getAll(filters: {
    from?: string;
    to?: string;
    status?: string;
    customerId?: string;
    pageNumber?: number;
    pageSize?: number;
  } = {}): Observable<PaginatedList<TransactionDto>> {
    let params = new HttpParams();
    if (filters.from) params = params.set('from', filters.from);
    if (filters.to) params = params.set('to', filters.to);
    if (filters.status) params = params.set('status', filters.status);
    if (filters.customerId) params = params.set('customerId', filters.customerId);
    if (filters.pageNumber) params = params.set('pageNumber', filters.pageNumber);
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize);
    return this.http.get<PaginatedList<TransactionDto>>(this.apiUrl, { params });
  }

  getById(id: string): Observable<TransactionDto> {
    return this.http.get<TransactionDto>(`${this.apiUrl}/${id}`);
  }

  getSummary(from?: string, to?: string): Observable<TransactionSummaryDto> {
    let params = new HttpParams();
    if (from) params = params.set('from', from);
    if (to) params = params.set('to', to);
    return this.http.get<TransactionSummaryDto>(`${this.apiUrl}/summary`, { params });
  }

  pay(id: string, paymentMethod: string): Observable<TransactionDto> {
    return this.http.patch<TransactionDto>(`${this.apiUrl}/${id}/pay`, { paymentMethod });
  }

  cancel(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/cancel`, {});
  }
}
