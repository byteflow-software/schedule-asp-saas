import { Component, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [MatButtonModule, MatIconModule],
  template: `
    <div class="pagination">
      <button mat-icon-button
        [disabled]="!hasPreviousPage()"
        (click)="pageChange.emit(pageNumber() - 1)">
        <mat-icon>chevron_left</mat-icon>
      </button>
      <span class="page-info">Página {{ pageNumber() }} de {{ totalPages() }}</span>
      <button mat-icon-button
        [disabled]="!hasNextPage()"
        (click)="pageChange.emit(pageNumber() + 1)">
        <mat-icon>chevron_right</mat-icon>
      </button>
      <span class="total-info">({{ totalCount() }} itens)</span>
    </div>
  `,
  styles: [`
    .pagination {
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 16px 0;
      gap: 8px;
    }
    .page-info { font-size: 14px; }
    .total-info { font-size: 12px; color: #666; }
  `],
})
export class PaginationComponent {
  pageNumber = input.required<number>();
  totalPages = input.required<number>();
  totalCount = input.required<number>();
  hasPreviousPage = input.required<boolean>();
  hasNextPage = input.required<boolean>();
  pageChange = output<number>();
}
